using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LuggageController : MonoBehaviour
{
    public float[] x, y, z;

    Transform[,,] briefcases;

    // Start is called before the first frame update
    void Start()
    {
        briefcases = new Transform[x.Length, y.Length, z.Length];
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        TransferLuggage(other.gameObject);
    }

    public void TransferLuggage(GameObject From)
    {
        PlayerController pc;
        HelperController hc = null;
        if (From.CompareTag("Player")
            && (null != (pc = From.GetComponent<PlayerController>())
                || null != (hc = From.GetComponent<HelperController>())))
        {
            Transform[] luggage = (null != hc) ? hc.GetLuggage() : pc.GetLuggage();
            int luggageIndex = 0;
            for (int yi = 0; yi < briefcases.GetLength(1) && luggageIndex < luggage.Length; yi++)
            {
                for (int zi = 0; zi < briefcases.GetLength(2) && luggageIndex < luggage.Length; zi++)
                {
                    for (int xi = 0; xi < briefcases.GetLength(0) && luggageIndex < luggage.Length; xi++)
                    {
                        if (null != luggage[luggageIndex]
                            && luggage[luggageIndex].CompareTag("luggage")
                            && null == briefcases[xi, yi, zi])
                        {
                            briefcases[xi, yi, zi] = luggage[luggageIndex++];
                            briefcases[xi, yi, zi].SetParent(transform);
                            briefcases[xi, yi, zi].position = new Vector3(x[xi], y[yi], z[zi]);
                            briefcases[xi, yi, zi].rotation = Quaternion.identity;
                        }
                    }
                }
            }
        }
    }

    public bool HaveBriefcase(Transform Briefcase)
    {
        for (int xi = 0; xi < briefcases.GetLength(0); xi++)
        {
            for (int yi = 0; yi < briefcases.GetLength(1); yi++)
            {
                for (int zi = 0; zi < briefcases.GetLength(2); zi++)
                {
                    if (Briefcase == briefcases[xi, yi, zi])
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void RemoveBriefcase(Transform Briefcase)
    {
        for (int xi = 0; xi < briefcases.GetLength(0); xi++)
        {
            for (int yi = 0; yi < briefcases.GetLength(1); yi++)
            {
                for (int zi = 0; zi < briefcases.GetLength(2); zi++)
                {
                    if (Briefcase == briefcases[xi, yi, zi])
                    {
                        briefcases[xi, yi, zi] = null;
                    }
                }
            }
        }
    }
}
