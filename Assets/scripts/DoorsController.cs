using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsController : MonoBehaviour
{
    //[SerializeField] PathManager pathMan;
    [SerializeField] [Range(0.000f, 1000)] float playerDist = 1;
    [SerializeField] float doorXPos = 12.56f;
    [SerializeField] [Range(0.000f, 5)] float animDist = 1.1f;
    [SerializeField] [Range(0.001f, 1)] float animTime = 0.25f;
    [SerializeField] Transform player;
    [SerializeField] Transform door1, door2;

    public bool Functional => functional;
    bool functional = false;

    private IEnumerator updateEnum = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        (updateEnum = updateEnum ?? update().GetEnumerator())?.MoveNext();

    }

    IEnumerable<int> update()
    {
        while (!functional)
        { 
            yield return 0;
        }
        while (true)
        {
            while (GameManager.TrainState.Moving == GameManager.Instance.State
                    && Mathf.Abs(player.position.x - transform.position.x - doorXPos) > playerDist
                    && Mathf.Abs(player.position.x - transform.position.x + doorXPos) > playerDist)
            {
                door1.localPosition -= door1.localPosition.z * Vector3.forward;
                door2.localPosition -= door2.localPosition.z * Vector3.forward;
                yield return 0;
            }
            for (float t = 0; t < 1; t += Mathf.Min(1 - t, Time.deltaTime / animTime))
            {
                door1.localPosition += (-t * animDist - door1.localPosition.z) * Vector3.forward;
                door2.localPosition += ( t * animDist - door2.localPosition.z) * Vector3.forward;
                yield return 0;
            }
            while (GameManager.TrainState.Station == GameManager.Instance.State
                    || Mathf.Abs(player.position.x - transform.position.x - doorXPos) <= playerDist
                    || Mathf.Abs(player.position.x - transform.position.x + doorXPos) <= playerDist)
            {
                door1.localPosition += (-animDist - door1.localPosition.z) * Vector3.forward;
                door2.localPosition += (animDist - door2.localPosition.z) * Vector3.forward;
                yield return 0;
            }
            for (float t = 1; t > 0; t -= Mathf.Min(t, Time.deltaTime / animTime))
            {
                door1.localPosition += (-t * animDist - door1.localPosition.z) * Vector3.forward;
                door2.localPosition += ( t * animDist - door2.localPosition.z) * Vector3.forward;
                yield return 0;
            }
            yield return 0;
        }
    }

    public void Enable()
    {
        functional = true;
        GameManager.Instance.StartStates();
    }
}
