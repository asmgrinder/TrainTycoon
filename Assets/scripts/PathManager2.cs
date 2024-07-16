using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.PlayerSettings;

public class PathManager2 : PathBase
{
    private void Awake()
    {
        initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (respawnPassengers
            && GameManager.Instance.CanRespawnPassengers
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
                        && passenger.TryGetComponent<Passenger2Controller>(out Passenger2Controller pc))
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
        List<Vector3>[] result = new List<Vector3>[] { new(), new() };
        if (WayOut.Length >= 2
            && null != scs
            && SeatIndex < scs.Length
            && scs[SeatIndex].WayOut.Length > 0)
        {
            Vector3 p1 = transform.position + WayOut[0];    // wagon entrance
            Vector3 p2 = scs[SeatIndex].transform.position; // seat
            Vector3 p3 = p2 + scs[SeatIndex].WayOut[0];     // seat entry
            Vector3 p4 = transform.position - WayOut[0];
            Vector3 p5 = p4 + WayOut[1];    // wagon exit
            p1.z = p4.z = p3.z;

            result[0].Add(p1);
            result[0].Add(p3);
            result[0].Add(p2);

            result[1].Add(p3);
            result[1].Add(p4);
            result[1].Add(p5);
        }
        // convert to jagged array
        Vector3[][] r = new Vector3[result.Length][];
        for (int i = 0; i < result.Length; i++)
        {
            r[i] = result[i].ToArray();
        }
        return r;
    }

    public int GetRouteToBlanket(Vector3 Pos, out Vector3 Point)
    {
        float minDist2 = 0;
        int seatIndex = -1;
        for (int i = 0; i < scs.Length; i++)
        {
            Passenger2Controller pc = scs[i].Passenger as Passenger2Controller;
            if (null != pc
                && pc.BlanketRequest)
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
        Point = (seatIndex >= 0)
                ? scs[seatIndex].transform.position + scs[seatIndex].WayOut[0]
                : Vector3.zero;
        return (seatIndex >= 0) ? seatIndex : -1;
    }
}
