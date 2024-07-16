using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper2Controller : MovingCharController
{
    public BlanketController BlanketZone;

    Vector3 HelperBase;

    int level = 0;

    private IEnumerator updateEnum = null;

    private void Awake()
    {
        initialize();
        HelperBase = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        (updateEnum = updateEnum ?? update().GetEnumerator())?.MoveNext();

        processAnimation();
    }

    IEnumerable<int> update()
    {
        yield return 0;
        PathManager2 pathMan = PathMan as PathManager2;
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
                    int seatIndex = -1;
                    if (0 <= (seatIndex = pathMan.GetRouteToBlanket(transform.position, out Vector3 point)))
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
                        Transform blanket = null;
                        foreach (Transform child in luggageMountPoint)
                        {
                            if (child.CompareTag("blanket"))
                            {
                                blanket = child;
                                break;
                            }
                        }
                        if (null == blanket)
                        {
                            way[2] = new Vector3[] { BlanketZone.transform.position - BlanketZone.transform.position.y * Vector3.up };
                            foreach (NextPathPoint nextPoint in nextPathPoint(2))
                            {
                                transform.position = nextPoint.point;
                                transform.LookAt(transform.position + nextPoint.lookDir);
                                yield return 0;
                            }
                            while (luggageMountPoint.childCount < MaxLuggage)
                            {
                                if (BlanketZone.CanTakeBlanket)
                                {
                                    BlanketZone.CreateAndTransferBlanket(this as PlayerBase);
                                }
                                yield return 0;
                            }
                        }
                        way[2] = new Vector3[] { point };
                        foreach (NextPathPoint nextPoint in nextPathPoint(2))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + nextPoint.lookDir);
                            yield return 0;
                        }
                        SeatController sc = pathMan.GetSeat(seatIndex);
                        if (null != sc)
                        {
                            processBlanket(sc);
                        }
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

    public void IncLevel()
    {
        level++;
    }
}
