using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PassengerController : MovingCharController
{
    public Material[] Materials;
    public Transform Briefcase;
    [SerializeField] MeshRenderer requestCircle;
    [Range(0, 4)] public float DelayBeforeTickets = 2;
    [Range(0, 4)] public float DelayBeforeLuggageGive = 2;
    [Range(0, 4)] public float DelayAfterLuggageTake = 0.25f;
    public int SeatIndex = -1;

    public bool TicketsRequest
    {
        get { return ticketsTimer > 0; }
        set { ticketsDec = !value; }
    }

    public bool LuggageRequest
    {
        get { return (luggageTimer > 0 || takeLuggage) && luggageMountPoint == Briefcase.parent; }
        set { luggageDec = !value; }
    }

    public bool TakeLuggage => takeLuggage && 0 == luggageTimer && luggageMountPoint == Briefcase.parent;

    [SerializeField] float ticketsTime = 0.5f;
    [SerializeField] float luggageTime = 0.5f;
    [SerializeField] float toiletChance = 0.0001f;
    [SerializeField] float toiletTime = 2;

    bool takeLuggage = false;
    float ticketsTimer = 0;
    bool ticketsDec = false;
    float luggageTimer = 0;
    bool luggageDec = false;

    //Transform briefcaseParent;
    Quaternion initialBriefcaseLocalRot;

    static int toiletVisitors = 0;
    bool toiletChanceTriggered = false;

    private IEnumerator updateEnum = null;

    const int wayIn = 0;
    const int wayLuggage = 1;
    const int wayOut = 2;
    const int wayToToilet = 3;
    const int wayFromToilet = 4;
    const int wayCount = 5;

    protected readonly int idleHash = Animator.StringToHash("Armature_idle");
    protected readonly int walkHash = Animator.StringToHash("Armature_walk");
    protected readonly int sitHash = Animator.StringToHash("Armature_sit");
    protected readonly int sittingHash = Animator.StringToHash("Armature_sitting");
    protected readonly int standupHash = Animator.StringToHash("Armature_standup");

    // Start is called before the first frame update
    void Start()
    {
        initialize();
        luggageMountPoint = Briefcase.parent;
        initialBriefcaseLocalRot = Briefcase.localRotation;
        StartCoroutine(toiletChances());
    }

    // Update is called once per frame
    void Update()
    {
        (updateEnum = updateEnum ?? update().GetEnumerator())?.MoveNext();
        //processAnimation();
    }

    IEnumerable<int> update()
    {
        bool firstStretch;
        bool lastStretch;

        PathManager pathMan = PathMan as PathManager;
        if (null != pathMan
            && null != gameObject
            && null != (way = pathMan.GetPassengerWay(SeatIndex))
            && way.Length >= wayCount
            && Array.FindIndex(way, ow => 0 == ow.Length) < 0)
        {
            //PathMan.MarkSeatBusy(SeatIndex, this);
            // initial delay
            float initialDelay = Random.Range(0, 0.5f);
            float t = Time.time; while (Time.time - t < initialDelay) { yield return 0; }

            // move to seat
            anim.Play(walkHash);
            lastStretch = false;
            foreach (NextPathPoint nextPoint in nextPathPoint(wayIn))
            {
                transform.position = nextPoint.point;
                transform.LookAt(transform.position + (nextPoint.lastStretch ? -1 : 1) * nextPoint.lookDir);
                if (lastStretch != nextPoint.lastStretch)
                {
                    lastStretch = nextPoint.lastStretch;
                    anim.Play(sitHash);
                }
                yield return 0;
            }
            anim.Play(sittingHash);

            // delay before request
            t = Time.time; while (Time.time - t < DelayBeforeTickets) { yield return 0; }
            // ticket request
            requestCircle.material = Materials[0];
            requestCircle.gameObject.SetActive(true);
            // await request to be processed
            ticketsDec = false;
            ticketsTimer = ticketsTime;
            while (TicketsRequest)
            {
                ticketsTimer -= ticketsDec ? Mathf.Min(ticketsTimer, Time.deltaTime) : 0;
                yield return 0;
            }
            // cancel request
            requestCircle.gameObject.SetActive(false);
            requestCircle.material = null;

            GameManager.Instance.TicketSold();

            // another delay
            t = Time.time; while (Time.time - t < DelayBeforeLuggageGive) { yield return 0; }
            // luggage request
            requestCircle.material = Materials[1];
            requestCircle.gameObject.SetActive(true);
            // await request to be processed
            luggageDec = false;
            luggageTimer = luggageTime;
            takeLuggage = true;
            while (LuggageRequest)
            {
                luggageTimer -= luggageDec ? Mathf.Min(luggageTimer, Time.deltaTime) : 0;
                yield return 0;
            }

            // cancel request
            requestCircle.gameObject.SetActive(false);
            requestCircle.material = null;

            GameManager.Instance.LuggageTaken();

            while (!pathMan.Luggage.HaveBriefcase(Briefcase))
            {
                yield return 0;
            }

            toiletChanceTriggered = false;

            // await station
            GameManager.TrainState prevState = GameManager.Instance.State;
            while (!(GameManager.TrainState.Moving == prevState
                    && GameManager.TrainState.Station == GameManager.Instance.State))
            {
                if (pathMan.Toilet.gameObject.activeSelf
                    && toiletChanceTriggered)
                {
                    if (1 == ++toiletVisitors)  // exclusively aquire toilet
                    {
                        requestCircle.gameObject.SetActive(true);
                        requestCircle.material = Materials[2];
                        // go to toilet
                        anim.Play(standupHash);
                        firstStretch = true;
                        lastStretch = false;
                        foreach (NextPathPoint nextPoint in nextPathPoint(wayToToilet))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + (nextPoint.lastStretch ? -1 : 1) * nextPoint.lookDir);
                            if (firstStretch != nextPoint.firstStretch)
                            {
                                firstStretch = nextPoint.firstStretch;
                                anim.Play(walkHash);
                            }
                            if (lastStretch != nextPoint.lastStretch)
                            {
                                lastStretch = nextPoint.lastStretch;
                                anim.Play(sitHash);
                            }

                            yield return 0;
                        }
                        anim.Play(sittingHash);
                        // toilet delay
                        t = Time.time; while (Time.time - t < toiletTime) { yield return 0; }
                        requestCircle.gameObject.SetActive(false);
                        requestCircle.material = null;
                        
                        toiletVisitors--;   // allow other passengers to use toilet
                        
                        pathMan.Toilet.IncUsed();

                        // return from toilet
                        anim.Play(standupHash);
                        firstStretch = true;
                        lastStretch = false;
                        foreach (NextPathPoint nextPoint in nextPathPoint(wayFromToilet))
                        {
                            transform.position = nextPoint.point;
                            transform.LookAt(transform.position + (nextPoint.lastStretch ? -1 : 1) * nextPoint.lookDir);
                            if (firstStretch != nextPoint.firstStretch)
                            {
                                firstStretch = nextPoint.firstStretch;
                                anim.Play(walkHash);
                            }
                            if (lastStretch != nextPoint.lastStretch)
                            {
                                lastStretch = nextPoint.lastStretch;
                                anim.Play(sitHash);
                            }
                            yield return 0;
                        }
                        anim.Play(sittingHash);
                    }
                    else
                    {
                        toiletVisitors--;   // restore counter if aquire failed
                    }
                }
                prevState = GameManager.Instance.State;  // should be before yield
                yield return 0;
            }
            // random delay
            float standUpDelay = Random.Range(0, 0.5f);
            t = Time.time; while (Time.time - t < standUpDelay) { yield return 0; }
            pathMan.MarkSeatBusy(SeatIndex, null);
            //Debug.Log("marked seat free");

            // move to luggage
            anim.Play(standupHash);
            firstStretch = true;
            foreach (NextPathPoint nextPoint in nextPathPoint(wayLuggage))
            {
                transform.position = nextPoint.point;
                transform.LookAt(transform.position + nextPoint.lookDir);
                if (firstStretch != nextPoint.firstStretch)
                {
                    firstStretch = nextPoint.firstStretch;
                    anim.Play(walkHash);
                }
                yield return 0;
            }
            anim.Play(idleHash);
            // another delay
            t = Time.time; while (Time.time - t < DelayAfterLuggageTake) { yield return 0; }

            // get luggage from the shelve
            pathMan.Luggage.RemoveBriefcase(Briefcase);
            Briefcase.SetParent(luggageMountPoint);
            Briefcase.SetLocalPositionAndRotation(Vector3.zero, initialBriefcaseLocalRot);

            // move out of train
            anim.Play(walkHash);
            foreach (NextPathPoint nextPoint in nextPathPoint(wayOut))
            {
                transform.position = nextPoint.point;
                transform.LookAt(transform.position + nextPoint.lookDir);
                yield return 0;
            }
        }
        else
        {
            way = null;
        }
        // destroy when out
        Destroy(gameObject);
        while (true)
        {
            yield return 0;
        }
    }

    IEnumerator toiletChances()
    {
        do
        {
            if (Random.Range(0f, 1f) < toiletChance)
            {
                toiletChanceTriggered = true;
            }
            yield return new WaitForSeconds(0.033333f);
        }
        while (true);
    }
}
