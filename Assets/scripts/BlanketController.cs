using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlanketController : MonoBehaviour
{
    [SerializeField] GameObject blanketPrefab;
    [SerializeField] Vector3 initialBlanketPosition;
    [SerializeField] float nextBlanketDelay = 0.1f;

    public bool CanTakeBlanket => 0 == nextBlanketTimer;

    float nextBlanketTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        nextBlanketTimer -= Mathf.Min(nextBlanketTimer, Time.deltaTime);
    }

    private void OnTriggerStay(Collider other)
    {
        if (0 == nextBlanketTimer
            && other.CompareTag("Player")
            && other.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            CreateAndTransferBlanket(pc as PlayerBase);
        }
    }

    public void CreateAndTransferBlanket(PlayerBase Player)
    {
        if (Player.CanTakeBlanket)
        {
            GameObject blanketInstance = Instantiate(blanketPrefab, initialBlanketPosition, Quaternion.identity);
            Player.TakeBlanket(blanketInstance.transform);
            nextBlanketTimer = nextBlanketDelay;
        }
    }
}
