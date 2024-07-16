using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class JoystickController : MonoBehaviour
{
    [SerializeField] RectTransform slider;

    public Vector2 Direction => direction;
    Vector2 direction = Vector2.zero;

    RectTransform rt;
    RectTransform parentRt;

    public static JoystickController Instance => instance;
    static JoystickController instance;

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
        EnhancedTouchSupport.Enable();
        rt = GetComponent<RectTransform>();
        parentRt = transform.parent.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //bool takeTouch = false;
        //foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        //{
        //    if (touch.valid
        //        && 1 == touch.touchId
        //        && UnityEngine.InputSystem.TouchPhase.None == touch.phase
        //        || (UnityEngine.InputSystem.TouchPhase.None != touch.phase
        //            && touch.startScreenPosition.y < 0.8f * Camera.main.pixelHeight))
        //    {
        //        takeTouch = true;
        //        //Debug.Log(touch + ", " + Camera.main.pixelHeight);
        //    }
        //}

        if (!(MenuController.Instance && MenuController.Instance.IsActive))
        {
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                if (touch.valid)// && 1 == touch.touchId)
                {
                    switch (touch.phase)
                    {
                        case UnityEngine.InputSystem.TouchPhase.Began:
                            if (touch.startScreenPosition.y < 0.8f * Camera.main.pixelHeight)
                            {
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, touch.startScreenPosition, null,    // Camera.main
                                                                                        out Vector2 touchPos);

                                rt.anchoredPosition = touchPos;
                                rt.localScale = Vector3.one;
                                slider.anchoredPosition = Vector2.zero;
                            }
                            break;
                        case UnityEngine.InputSystem.TouchPhase.Moved:
                            if (touch.startScreenPosition.y < 0.8f * Camera.main.pixelHeight)
                            {
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, touch.startScreenPosition, null,    // Camera.main
                                                                                        out Vector2 touchPos1);
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, touch.screenPosition, null,    // Camera.main
                                                                                        out Vector2 touchPos2);
                                direction = (touchPos2 - touchPos1).normalized;
                                slider.anchoredPosition = direction * 0.5f * Mathf.Min(rt.sizeDelta.x, rt.sizeDelta.y);
                            }
                            break;
                        case UnityEngine.InputSystem.TouchPhase.Ended:
                        case UnityEngine.InputSystem.TouchPhase.Canceled:
                            direction = Vector2.zero;
                            rt.localScale = Vector3.zero;
                            slider.anchoredPosition = Vector2.zero;
                            break;
                    }
                    break;
                }
            }
        }
        else
        {
            direction = Vector2.zero;
            rt.localScale = Vector3.zero;
            slider.anchoredPosition = Vector2.zero;
        }
    }

    private void OnEnable()
    {
        TouchSimulation.Enable();
        //UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += fingerDown; 
    }

    private void OnDisable()
    {
        TouchSimulation.Disable();
    }
}
