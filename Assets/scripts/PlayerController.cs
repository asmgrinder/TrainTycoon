using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting.Dependencies.NCalc;
//using Unity.VisualScripting.InputSystem;
using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : PlayerBase
{
    //[SerializeField] InputActionReference touch;
    [SerializeField] Transform cameraT;
    [SerializeField] Vector3 cameraOffset = new Vector3(-20, 24, 0);
    [SerializeField] float moveSpeed = 3;
    [SerializeField] float cameraSpring = 0.2f;
    [SerializeField] float rotSpring = 0.4f;
    //[SerializeField] float touchSens = 0.01f;

    //Animator anim;

    Vector2 moveVec = Vector2.zero;

    //Vector3 lastPos = Vector3.zero;

    float initialY = 0;

    Rigidbody rb;

    //readonly int isWalkingHash = Animator.StringToHash("isWalking");

    // Start is called before the first frame update
    void Start()
    {
        initialize();
        initialY = transform.position.y;
        rb = GetComponent<Rigidbody>();
        //anim = GetComponent<Animator>();
        //EnhancedTouchSupport.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        // attract camera to it's position
        if (null != cameraT)
        {
            Vector3 newPos = transform.position + cameraOffset;
            cameraT.position = Vector3.Lerp(cameraT.position, newPos, cameraSpring);
        }

        //float diagSize = (new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight)).magnitude;
        ////UnityEngine.InputSystem.TouchPhase phase = UnityEngine.InputSystem.TouchPhase.None;
        //foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        //{
        //    if (touch.valid)// && 1 == touch.touchId)
        //    {
        //        switch (touch.phase)
        //        {
        //            case UnityEngine.InputSystem.TouchPhase.Moved:
        //                if (touch.delta.sqrMagnitude > 0
        //                    && (touch.delta / diagSize).magnitude >= touchSens)
        //                {
        //                    moveVec = touch.delta;
        //                }
        //                break;
        //            case UnityEngine.InputSystem.TouchPhase.Ended:
        //            case UnityEngine.InputSystem.TouchPhase.Canceled:
        //                moveVec = Vector2.zero;
        //                break;
        //        }
        //        //phase = touch.phase;
        //        //Debug.Log(touch);
        //        break;
        //    }
        //}

        //Debug.Log(moveVec.magnitude);
        if (null != JoystickController.Instance)
        {
            moveVec = JoystickController.Instance.Direction;
            Vector3 dir = new Vector3(moveVec.y, 0, -moveVec.x).normalized;
            Vector3 newDir = (dir.sqrMagnitude > 0)
                                ? Vector3.Lerp(transform.forward, dir, rotSpring).normalized
                                : dir;
            transform.position += moveSpeed * Time.deltaTime * newDir;
            transform.LookAt(transform.position + newDir);
            transform.position += (initialY - transform.position.y) * Vector3.up;
        }

        processAnimation();
    }

    private void FixedUpdate()
    {
        if (0 == moveVec.sqrMagnitude)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    //public void InputPlayer(InputAction.CallbackContext _context)
    //{
    //    moveVec = _context.ReadValue<Vector2>();
    //}
    //void fingerDown(Finger finger)
    //{
    //    Debug.Log(finger.index + ", " + finger.screenPosition);
    //}

    private void OnTriggerStay(Collider other)
    {
        processTicketsAndLuggage(other.GetComponent<SeatController>());
        processBlanket(other.GetComponent<SeatController>());
    }
}

////UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
//foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
//{
//    if (touch.began)
//    {
//        Debug.Log("touch began" + touch.screenPosition + ", " + touch.phase);
//    }
//    if (touch.ended)
//    {
//        Debug.Log("touch ended" + touch.screenPosition + ", " + touch.phase);
//    }
//}
//moveVec = touch.action.ReadValue<Touch>();
//Debug.Log(moveVec);
//        int touchCount = Input.touchCount;
//        Touch touch = Input.GetTouch(0);
//#if UNITY_EDITOR || UNITY_STANDALONE_WIN
//        if ()
//#endif
//switch (touch.phase)
//{
//    case TouchPhase.Began:
//        break;
//    case TouchPhase.Moved:
//        break;
//    case TouchPhase.Ended:
//        break;
//}
