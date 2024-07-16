using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public class fps : MonoBehaviour
{
    float startTime = 0;
    int framesCount = 0;

    TMP_Text text;

    void OnValidate()
    {
        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        framesCount++;
        if (null != text
            && Time.time - startTime > 0.5f)
        {
            text.text = (framesCount / (Time.time - startTime)).ToString("#.0") + " fps";
            //Debug.Log(text.text);
            startTime = Time.time;
            framesCount = 0;
        }
    }
}
