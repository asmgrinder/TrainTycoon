using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCharController : PlayerBase
{
    [SerializeField] float walkSpeed = 8;

    protected Vector3[][] way;

    protected struct NextPathPoint
    {
        public NextPathPoint(Vector3 Point, Vector3 LookDir, bool FirstStretch, bool LastStretch)
        {
            point = Point;
            lookDir = LookDir;
            firstStretch = FirstStretch;
            lastStretch = LastStretch;
        }
        public Vector3 point;
        public Vector3 lookDir;
        public bool firstStretch;
        public bool lastStretch;
    }

    protected IEnumerable<NextPathPoint> nextPathPoint(int Way)
    {
        if (Way < way.Length)
        {
            int subIndex = 0;
            while (subIndex < way[Way].Length)
            {
                Vector3 newPos = transform.position;
                Vector3 newLook = Vector3.zero;
                bool firstStretch = false;
                bool lastStretch = false;
                float dist = walkSpeed * Time.deltaTime;
                do
                {
                    Vector3 leftPart = way[Way][subIndex] - newPos;

                    // move char
                    if (dist <= leftPart.magnitude)
                    {   // if dist is less then distance to next route point
                        newPos += dist * leftPart.normalized;
                        dist = 0;
                    }
                    else
                    {   // otherwise move to that point and iterate again
                        dist -= leftPart.magnitude;
                        newPos = way[Way][subIndex];
                        subIndex++;
                    }

                    // set look direction
                    firstStretch = 0 == subIndex;
                    lastStretch = subIndex + 1 >= way[Way].Length;
                    //float sign = (wayIn == Way && subIndex + 1 >= way[wayIn].Length) ? -1 : 1;
                    newLook = leftPart.normalized;
                }
                while (dist > 0 && subIndex < way[Way].Length);
                yield return new NextPathPoint(newPos, newLook, firstStretch, lastStretch);
            }
        }
    }
}
