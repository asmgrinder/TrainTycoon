using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static GameManager;
using Random = UnityEngine.Random;

public class PathManager : PathBase
{
    public LuggageController Luggage;

    public ToiletController Toilet;

    [SerializeField] Vector3[] toiletPath;

    void Awake()
    {
        initialize();
    }


    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.CanRespawnPassengers
            && null != PassengerPrefab)
        {
            for (int i = 0; i < scs.Length; i++)
            {
                if (null == IsSeatBusy(i))
                {
                    float xdisp = Random.Range(0, 1.5f);
                    float zdisp = Random.Range(0, -1.5f);
                    Vector3 pos = transform.position + WayOut[0] + WayOut[1] + new Vector3(xdisp, 0, zdisp);
                    GameObject passenger = Instantiate(PassengerPrefab, pos, Quaternion.identity);
                    if (null != passenger
                        && passenger.TryGetComponent<PassengerController>(out PassengerController pc))
                    {
                        pc.PathMan = this;
                        pc.SeatIndex = i;
                        MarkSeatBusy(i, pc);
                    }
                }
            }
        }
    }

    public Vector3[][] GetPassengerWay(int SeatIndex)
    {
        List<Vector3>[] result = new List<Vector3>[] { new (), new (), new(), new(), new() };
        if (WayOut.Length >= 2
            && null != scs
            && SeatIndex < scs.Length
            && scs[SeatIndex].WayOut.Length >= 2)
        {
            Vector3 p1 = transform.position + WayOut[0];    // wagon entrance
            Vector3 p2 = scs[SeatIndex].transform.position; // seat
            Vector3 p3 = p2 + scs[SeatIndex].WayOut[0];
            Vector3 p4 = p3 + scs[SeatIndex].WayOut[1];     // seat entry
            Vector3 p5 = transform.position - WayOut[0];    // wagon exit
            p1.z = p5.z = p4.z;

            result[0].Add(p1);
            result[0].Add(p4);
            result[0].Add(p3);
            result[0].Add(p2);  // seat

            result[1].Add(p3);
            result[1].Add(p4);

            result[1].Add(Luggage.transform.position);

            result[2].Add(p5);
            result[2].Add(p5 + WayOut[1]);

            // path from seat to toilet
            result[3].Add(p3);
            result[3].Add(p4);
            for (int i = 0; i < toiletPath.Length; i++)
            {
                result[3].Add(toiletPath[i]);
            }
            //result[3].Add(toiletPath[0]);
            //result[3].Add(toiletPath[1]);
            //result[3].Add(toiletPath[2]);

            // path from toilet to seat
            for (int i = toiletPath.Length - 2; i >= 0; i--)
            {
                result[4].Add(toiletPath[i]);
            }
            //result[4].Add(toiletPath[1]);
            //result[4].Add(toiletPath[0]);
            result[4].Add(p4);
            result[4].Add(p3);
            result[4].Add(p2);
        }
        // convert to jagged array
        Vector3[][] r = new Vector3[result.Length][];
        for (int i = 0; i < result.Length; i++)
        {
            r[i] = result[i].ToArray();
        }
        return r;
    }

    public void GetRouteToToilet(out Vector3[] To, out Vector3[] From)
    {
        To = new Vector3[] { toiletPath[0], toiletPath[1] };
        From = new Vector3[] { toiletPath[0] };
    }

    public int GetRouteToTicketsOrLuggage(Vector3 Pos, out Vector3 Point1, out Vector3 Point2, bool Luggage = false)
    {
        float minDist2 = 0;
        int seatIndex = -1;
        for (int i = 0; i < scs.Length; i ++)
        {
            PassengerController pc = scs[i].Passenger as PassengerController;
            if (null != pc
                && (pc.TicketsRequest && !Luggage
                    || pc.LuggageRequest && Luggage))
            {
                float dist2 = (scs[i].transform.position - Pos).sqrMagnitude;
                if (seatIndex < 0
                    || minDist2 > dist2)
                {
                    minDist2 = dist2;
                    seatIndex = i;
                }
            }
        }
        if (seatIndex >= 0)
        {
            Point2 = scs[seatIndex].transform.position + scs[seatIndex].WayOut[0];
            Point1 = Point2 + scs[seatIndex].WayOut[1];
            Point2 += scs[seatIndex].ZAdd * Vector3.forward;
            return seatIndex;
        }
        Point1 = Point2 = Vector3.zero;
        return -1;
    }
}
