using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletController : MonoBehaviour
{
    public int UsesBeforeCleaning = 1;
    public float CleaningTime = 1f;

    [SerializeField] MeshRenderer requestCircle;
    [SerializeField] Material requestMaterial;

    public bool Available => usedCount < UsesBeforeCleaning;
    public bool NeedCleaning
    {
        get
        {
            return cleanTimer > 0;
        }
        set
        {
            doCleaning = !value;
        }
    }
    
    int usedCount = 0;

    bool doCleaning = false;

    float cleanTimer = 0;

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
        while (true)
        {
            requestCircle.gameObject.SetActive(false);
            requestCircle.material = null;
            while (Available)
            {
                yield return 0;
            }
            requestCircle.gameObject.SetActive(true);
            requestCircle.material = requestMaterial;
            cleanTimer = CleaningTime;
            while (cleanTimer > 0)
            {
                doCleaning = false;
                yield return 0;
                cleanTimer -= doCleaning ? Mathf.Min(cleanTimer, Time.deltaTime) : 0;
            }
            usedCount = 0;
            yield return 0;
        }
    }

    public void IncUsed()
    {
        usedCount++;
    }

    //public void ResetUsed()
    //{
    //    usedCount = 0;
    //}

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doCleaning = true;
        }
    }
}
