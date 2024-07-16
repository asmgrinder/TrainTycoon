using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public bool IsActive => gameObject.activeSelf;
    public static MenuController Instance => instance;
    static MenuController instance;

    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(bool On)
    {
        gameObject.SetActive(On);
    }

    public void LoadLocation(int Location)
    {
        if (Location < SceneManager.sceneCountInBuildSettings
            && SceneManager.GetActiveScene().buildIndex != Location)
        {
            SceneManager.LoadScene(Location);
        }
        gameObject.SetActive(false);
    }
}
