using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class NewZoneController : MonoBehaviour
{
    public GameObject[] ToTurnOff;
    public GameObject[] ToTurnOn;
    [SerializeField] UnityEvent action;
    [SerializeField] TMP_Text text;
    public int CoinsToTake = 0;


    // Start is called before the first frame update
    void Start()
    {
        text.text = CoinsToTake.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenZone()
    {
        foreach (GameObject go in ToTurnOff)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in ToTurnOn)
        {
            go.SetActive(true);
        }

        if (null != action)
        {
            action.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")
            && GameManager.Instance.SpendCoins(CoinsToTake))
        {
            OpenZone();
            GameManager.Instance.MarkZoneOpen(this);
        }
    }
}
