using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger2Controller : MovingCharController
{
    public Material[] Materials;
    public int SeatIndex = -1;
    [SerializeField] MeshRenderer requestCircle;
    [SerializeField] [Range(0, 4)] float DelayBeforeBedRequest = 2;
    //[SerializeField] [Range(0, 4)] float SleepTime = 10;
    [SerializeField] float bedRequestTime = 0.25f;

    [SerializeField] Vector3 layOffset;
    [SerializeField] Quaternion layRot;

    public bool CanRemoveBlanket => 0 == blanketTimer && blanketRemove;
    public bool BlanketRequest
    {
        get { return blanketTimer > 0 || blanketRemove; }
        set { blanketDec = !value; }
    }

    float blanketTimer = 0;
    bool blanketDec = false;
    bool blanketRemove = false;

    private IEnumerator updateEnum = null;

    const int wayIn = 0;
    const int wayOut = 1;
    const int wayCount = 2;

    protected readonly int idleHash = Animator.StringToHash("Armature_idle");
    protected readonly int walkHash = Animator.StringToHash("Armature_walk");

    private void Awake()
    {
        initialize();
    }

    // Update is called once per frame
    void Update()
    {
        (updateEnum = updateEnum ?? update().GetEnumerator())?.MoveNext();
        //processAnimation();
    }

    IEnumerable<int> update()
    {
        PathManager2 pathMan = PathMan as PathManager2;
        if (null != pathMan
            && null != gameObject
            && null != (way = pathMan.GetPassengerWay(SeatIndex))
            && way.Length >= wayCount
            && Array.FindIndex(way, ow => 0 == ow.Length) < 0)
        {
            anim.Play(walkHash);
            foreach (NextPathPoint nextPoint in nextPathPoint(wayIn))
            {
                transform.position = nextPoint.point;
                transform.LookAt(transform.position + (nextPoint.lastStretch ? -1 : 1) * nextPoint.lookDir);
                yield return 0;
            }
            anim.Play(idleHash);
            // delay
            float t = Time.time; while (Time.time - t < DelayBeforeBedRequest) { yield return 0; }
            // blanket request
            requestCircle.gameObject.SetActive(true);
            requestCircle.material = Materials[0];
            // await request to be processed
            blanketDec = false;
            blanketTimer = bedRequestTime;
            blanketRemove = true;
            while (blanketTimer > 0)
            {
                blanketTimer -= blanketDec ? Mathf.Min(blanketTimer, Time.deltaTime) : 0;
                yield return 0;
            }
            requestCircle.gameObject.SetActive(false);
            requestCircle.material = null;

            GameManager.Instance.BlanketTaken();

            // go to bed
            Quaternion prevRot = transform.rotation;
            transform.rotation = layRot;
            transform.position += layOffset;

            // show blanket
            SeatController sc = pathMan.GetSeat(SeatIndex);
            foreach (Transform child in sc.transform)
            {
                child.gameObject.SetActive(true);
            }

            // await station
            GameManager.TrainState prevState = GameManager.Instance.State;
            while (!(GameManager.TrainState.Moving == prevState
                    && GameManager.TrainState.Station == GameManager.Instance.State))
            {
                prevState = GameManager.Instance.State;  // should be before yield
                yield return 0;
            }

            // hide blanket
            foreach (Transform child in sc.transform)
            {
                child.gameObject.SetActive(false);
            }

            // mark place
            pathMan.MarkSeatBusy(SeatIndex, null);

            transform.rotation = prevRot;
            transform.position -= layOffset;
            anim.Play(walkHash);
            foreach (NextPathPoint nextPoint in nextPathPoint(wayOut))
            {
                transform.position = nextPoint.point;
                transform.LookAt(transform.position + nextPoint.lookDir);
                yield return 0;
            }
            anim.Play(idleHash);
        }
    }

    public void ResetBlanketRemove()
    {
        blanketRemove = false;
    }
}
