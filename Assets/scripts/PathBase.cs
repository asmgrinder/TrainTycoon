using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathBase : MonoBehaviour
{
    [SerializeField] protected Vector3[] WayOut;
    [SerializeField] protected GameObject PassengerPrefab;

    protected bool respawnPassengers = false;

    public int SeatsCount => (null != scs) ? scs.Length : 0;

    protected SeatController[] scs;

    protected void initialize()
    {
        scs = GetComponentsInChildren<SeatController>();
    }

    public void StartRespawnPassengers()
    {
        respawnPassengers = true;
    }

    public SeatController GetSeat(int Index)
    {
        return (Index >= 0 && Index < scs.Length) ? scs[Index] : null;
    }

    public MovingCharController IsSeatBusy(int Index)
    {
        return (Index >= 0 && Index < scs.Length) ? scs[Index].Passenger : null;
    }

    public void MarkSeatBusy(int Index, MovingCharController Passenger)
    {
        if (Index >= 0
            && Index < scs.Length)
        {
            scs[Index].Passenger = Passenger;
        }
    }

    public SeatController[] FindSeatsInRadius(Vector3 Pos, float R)
    {
        SeatController[] seats = Array.FindAll(scs, sc => (sc.transform.position - Pos).sqrMagnitude < R * R);
        return seats;
    }

    public void ExtractNewSeats(GameObject ExtractNewSeatsFrom)
    {
        SeatController[] seats = ExtractNewSeatsFrom.GetComponentsInChildren<SeatController>();
        SeatController[] newScs = new SeatController[scs.Length + seats.Length];
        for (int i = 0; i < scs.Length; i++)
        {
            newScs[i] = scs[i];
        }
        for (int i = 0; i < seats.Length; i++)
        {
            newScs[i + scs.Length] = seats[i];
        }
        scs = newScs;
    }
}
