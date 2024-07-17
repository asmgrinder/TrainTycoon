using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public int MaxLuggage = 4;
    public PathBase PathMan;
    [SerializeField] protected Transform luggageMountPoint;

    public bool CanTakeBlanket => null != luggageMountPoint && luggageMountPoint.childCount < MaxLuggage;

    protected Animator anim;
    protected bool wasWalking = false;
    protected Vector3 lastPosition = Vector3.zero;
    protected readonly int isIdleHash = Animator.StringToHash("isIdle");
    protected readonly int isWalkingHash = Animator.StringToHash("isWalking");

    //protected readonly int idleHash = Animator.StringToHash("Armature_idle");
    //protected readonly int walkHash = Animator.StringToHash("Armature_walk");

    protected void initialize()
    {
        anim = GetComponent<Animator>();
    }

    protected void processAnimation()
    {
        bool isWalking = (lastPosition - transform.position).sqrMagnitude > 0;
        //if (isWalking != wasWalking)
        //{
        //    anim.Play(isWalking ? walkHash : idleHash, 0);
        //    Debug.Log(isWalking);
        //}
        //wasWalking = isWalking;
        if (anim.GetBool(isWalkingHash) != isWalking)
        {
            anim.SetBool(isWalkingHash, isWalking);
        }
        lastPosition = transform.position;
    }

    protected void processTicketsAndLuggage(SeatController sc)
    {
        PassengerController passenger;
        if (null != sc
            && null != (passenger = sc.Passenger as PassengerController)
            && (passenger.TicketsRequest
                || passenger.LuggageRequest))
        {
            if (passenger.LuggageRequest
                && passenger.TakeLuggage
                && luggageMountPoint.childCount < MaxLuggage
                && passenger.Briefcase.parent != luggageMountPoint)
            {
                passenger.Briefcase.SetParent(luggageMountPoint);
                passenger.Briefcase.localRotation = Quaternion.identity;
                setItemsPos();
            }
            passenger.TicketsRequest = passenger.LuggageRequest = false;
        }
    }

    protected void processBlanket(SeatController sc)
    {
        Transform blanket = null;
        foreach (Transform child in luggageMountPoint)
        {
            if (child.CompareTag("blanket"))
            {
                blanket = child;
                break;
            }
        }
        Passenger2Controller passenger;
        if (null != blanket
            && null != sc
            && null != (passenger = sc.Passenger as Passenger2Controller)
            && passenger.BlanketRequest)
        {
            if (passenger.CanRemoveBlanket)
            {
                Destroy(blanket.gameObject);
                setItemsPos();

                passenger.ResetBlanketRemove();
            }
            passenger.BlanketRequest = false;
        }
    }

    public Transform[] GetLuggage()
    {
        Transform[] luggage = new Transform[luggageMountPoint.childCount];
        int index = 0;
        foreach (Transform child in luggageMountPoint)
        {
            luggage[index++] = child;
        }
        return luggage;
    }

    public void TakeBlanket(Transform Blanket)
    {
        if (luggageMountPoint.childCount < MaxLuggage)
        {
            Blanket.SetParent(luggageMountPoint);
            Blanket.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            setItemsPos();
        }
    }

    protected void setItemsPos()
    {
        Vector3 localPos = Vector3.zero;
        foreach (Transform child in luggageMountPoint)
        {
            child.localPosition = localPos;
            localPos.y += 0.075f;
        }
    }
}
