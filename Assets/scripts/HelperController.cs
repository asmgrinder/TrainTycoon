using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperController : MovingCharController
{
    public LuggageController LuggageZone;

    Vector3 HelperBase;

    //Rigidbody rb;
    
    int level = 0;

    private IEnumerator updateEnum = null;

    private void Awake()
    {
        initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        HelperBase = transform.position;

        //rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        (updateEnum = updateEnum ?? update().GetEnumerator())?.MoveNext();

        processAnimation();
    }

    IEnumerable<int> update()
    {
        PathManager pathMan = PathMan as PathManager;
        if (null != pathMan)
        {
            way = new Vector3[4][];
            way[0] = new Vector3[] { HelperBase.x * Vector3.right };
            way[1] = new Vector3[] { HelperBase.x * Vector3.right, HelperBase };

            bool startPosition = true;
            do
            {
                if (level > 0)
                {
                    if (pathMan.Toilet.NeedCleaning)
                    {
                        if (startPosition)
                        {
                            startPosition = false;
                            foreach (NextPathPoint nextPoint in nextPathPoint(0))
                            {
                                transform.position = nextPoint.point;
                                transform.LookAt(transform.position + nextPoint.lookDir);
                                yield return 0;
                            }
                        }
                        pathMan.GetRouteToToilet(out Vector3[] to, out Vector3[] from);
                        way[2] = to;
                        way[3] = from;
                        foreach (NextPathPoint nextPoint in nextPathPoint(2))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + nextPoint.lookDir);
                            yield return 0;
                        }
                        while (pathMan.Toilet.NeedCleaning)
                        {
                            pathMan.Toilet.NeedCleaning = false;
                            yield return 0;
                        }
                        foreach (NextPathPoint nextPoint in nextPathPoint(3))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + nextPoint.lookDir);
                            yield return 0;
                        }
                    }
                    else if (0 <= pathMan.GetRouteToTicketsOrLuggage(transform.position, out Vector3 p1, out Vector3 p2, false)
                        || 0 <= pathMan.GetRouteToTicketsOrLuggage(transform.position, out p1, out p2, true))
                    {
                        float searchR = (p1 - p2).magnitude * 1.2f;
                        way[2] = new Vector3[] { p1, p2 };
                        way[3] = new Vector3[] { p1 };
                        if (startPosition)
                        {
                            startPosition = false;
                            foreach (NextPathPoint nextPoint in nextPathPoint(0))
                            {
                                transform.position = nextPoint.point;
                                transform.LookAt(transform.position + nextPoint.lookDir);
                                yield return 0;
                            }
                        }

                        foreach (NextPathPoint nextPoint in nextPathPoint(2))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + nextPoint.lookDir);
                            yield return 0;
                        }
                        // process tickets and luggage for nearby passengers
                        SeatController[] seats = PathMan.FindSeatsInRadius(transform.position, searchR);
                        while (null != seats
                                && seats.Length > 0
                                && luggageMountPoint.childCount < MaxLuggage
                                && 0 <= Array.FindIndex(seats,
                                                    seat => (seat.Passenger as PassengerController).TicketsRequest
                                                            || (seat.Passenger as PassengerController).LuggageRequest))
                        {
                            foreach (SeatController seat in seats)
                            {
                                processTicketsAndLuggage(seat);
                            }
                            yield return 0;
                        }

                        foreach (NextPathPoint nextPoint in nextPathPoint(3))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + nextPoint.lookDir);
                            yield return 0;
                        }
                        if (luggageMountPoint.childCount >= MaxLuggage
                            && null != LuggageZone)
                        {
                            way[2] = new Vector3[] { (PathMan as PathManager).Luggage.transform.position };
                            foreach (NextPathPoint nextPoint in nextPathPoint(2))
                            {
                                transform.position = nextPoint.point;
                                transform.LookAt(transform.position + nextPoint.lookDir);
                                yield return 0;
                            }
                            LuggageZone.TransferLuggage(gameObject);
                        }
                    }
                    else if (luggageMountPoint.childCount > 0
                            && null != LuggageZone)
                    {
                        way[2] = new Vector3[] { pathMan.Luggage.transform.position };
                        foreach (NextPathPoint nextPoint in nextPathPoint(2))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + nextPoint.lookDir);
                            yield return 0;
                        }
                        LuggageZone.TransferLuggage(gameObject);
                    }
                    else if (!startPosition)
                    {
                        startPosition = true;
                        foreach (NextPathPoint nextPoint in nextPathPoint(1))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + (nextPoint.lastStretch ? -1 : 1) * nextPoint.lookDir);
                            yield return 0;
                        }
                    }
                }
                yield return 0;
            }
            while (true);
        }
    }

    //private void FixedUpdate()
    //{
    //    if (startPosition
    //        || false == anim.GetBool(isWalkingHash))
    //    {
    //        rb.velocity = Vector3.zero;
    //        rb.angularVelocity = Vector3.zero;
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        processTicketsAndLuggage(other.GetComponent<SeatController>());
    }

    public void IncLevel()
    {
        level++;
    }
}
