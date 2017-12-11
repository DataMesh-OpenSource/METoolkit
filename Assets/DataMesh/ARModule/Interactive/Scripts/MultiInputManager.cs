using UnityEngine;
using System;
using HoloLensXboxController;
using DataMesh.AR.Anchor;

#if UNITY_METRO && !UNITY_EDITOR
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Input;
#else
using DataMesh.AR.FakeUWP;
#endif

namespace DataMesh.AR.Interactive
{

    /// <summary>
    /// 注视点、手势操作，以及注视点标志
    /// </summary>
    public class MultiInputManager : DataMesh.AR.MEHoloModuleSingleton<MultiInputManager>
    {
        public enum InputType
        {
            KeybordAndMouse,
            Touch,
            GazeAndGesture
        }


        /// <summary>
        /// 交互的过滤层信息，制定了可以与哪些层物体进行交互
        /// </summary>
        public LayerMask layerMask;

        /// <summary>
        /// 是否用鼠标模拟视线。如果不模拟，则改为点击位置触发Tap
        /// </summary>
        public bool simulateGaze = true;

        /// <summary>
        /// 键盘模拟Manipulation和Navigation时，按下辅助键（Shift和Alt）
        /// </summary>
        public bool needAssistKey = true;

#region 可用的事件回调接口

        /// <summary>
        /// 点击的回调。
        /// 对应电脑为鼠标点击，对应触屏为单指点击，对应hololens为airTap的回调函数。
        /// 注意：这里不返回点击位置，如需要获取点击到的对象的信息，请参考FocusedObject及相关属性
        /// 传输参数为：
        /// int: 点击次数
        /// </summary>
        public System.Action<int> cbTap;

        /// <summary>
        /// 空间拖拽开始时的回调。
        /// 对应电脑为按下并拖拽，对应触屏为单指按下并拖拽，对应hololens为手指按下并移动
        /// 这种拖拽以空间绝对坐标为准，操作方法与导航拖拽（Navigation）一致，只是传递参数不同，请注意与Navigation移动的区别
        /// 传递参数为：
        /// Vector3：距离初始点的偏移位置（这时可能都是0）
        /// </summary>
        public System.Action<Vector3> cbManipulationStart;

        /// <summary>
        /// 空间拖拽中不断触发的回调。
        /// 传递参数为：
        /// Vector3：距离初始点的偏移位置
        /// </summary>
        public System.Action<Vector3> cbManipulationUpdate;

        /// <summary>
        /// 空间拖拽结束时的回调。
        /// 传递参数为：
        /// Vector3：距离初始点的偏移位置
        /// </summary>
        public System.Action<Vector3> cbManipulationEnd;

        /// <summary>
        /// 导航拖拽开始时的回调。
        /// 对应电脑为按下并拖拽，对应触屏为单指按下并拖拽，对应hololens为手指按下并移动
        /// 这种拖拽以导航（Navigation）为目的，操作方法与Manupulation一致，只是传递参数不同，请注意与Manipulation移动的区别
        /// 传输参数为：
        /// Vector3：基于开始时的基准点，向三个方向的相对导航量，值在0~1之间
        /// </summary>
        public System.Action<Vector3> cbNavigationStart;

        /// <summary>
        /// 导航拖拽中不断触发的回调
        /// 传输参数为：
        /// Vector3：基于开始时的基准点，向三个方向的相对导航量，值在0~1之间
        /// </summary>
        public System.Action<Vector3> cbNavigationUpdate;

        /// <summary>
        /// 导航拖拽结束时触发的回调
        /// 传输参数为：
        /// Vector3：基于开始时的基准点，向三个方向的相对导航量，值在0~1之间
        /// </summary>
        public System.Action<Vector3> cbNavigationEnd;

        /// <summary>
        /// 掐捏操作开始时的回调
        /// 目前仅用于触摸屏，对应操作为两指按下后扩大或缩小
        /// 传输参数为：
        /// Vector3：第一个手指的位置
        /// Vector3：第二个手指的位置
        /// </summary>
        public System.Action<Vector3, Vector3> cbPinchStart;

        /// <summary>
        /// 掐捏操作中不断触发的回调
        /// 传输参数为：
        /// Vector3：第一个手指的位置
        /// Vector3：第二个手指的位置
        /// float：当前两指之间的距离
        /// </summary>
        public System.Action<Vector3, Vector3, float> cbPinchUpdate;

        /// <summary>
        /// 掐捏操作结束时的回调
        /// 传输参数为：
        /// Vector3：第一个手指的位置
        /// Vector3：第二个手指的位置
        /// </summary>
        public System.Action<Vector3, Vector3> cbPinchEnd;

        /// <summary>
        /// 旋转操作开始时的回调
        /// 目前仅用于触摸屏，对应操作是两指按下后旋转
        /// 传输参数为：
        /// Vector3：第一个手指的位置
        /// Vector3：第二个手指的位置
        /// </summary>
        public System.Action<Vector3, Vector3> cbRotationStart;

        /// <summary>
        /// 旋转操作中不断触发的回调
        /// 传输参数为：
        /// Vector3：第一个手指的位置
        /// Vector3：第二个手指的位置
        /// float：距离最初位置，旋转过的角度
        /// </summary>
        public System.Action<Vector3, Vector3, float> cbRotationUpdate;

        /// <summary>
        /// 旋转操作结束时触发的回调
        /// 传输参数为：
        /// Vector3：第一个手指的位置
        /// Vector3：第二个手指的位置
        /// </summary>
        public System.Action<Vector3, Vector3> cbRotationEnd;

#endregion


#region 交互物体相关的信息

        /// <summary>
        /// 当前聚焦的目标物体
        /// 鼠标键盘操作时，会模拟hololens操作，因此该物体并不是点击鼠标位置对应的物体，而是当时摄影机注视的物体
        /// 触屏操作时，
        /// </summary>
        [HideInInspector]
        public GameObject FocusedObject { get; private set; }

        /// <summary>
        /// 头部所在的位置，也就是用户视线的起点
        /// </summary>
        [HideInInspector]
        public Vector3 headPosition;

        /// <summary>
        /// 用户注视的方向
        /// </summary>
        [HideInInspector]
        public Vector3 gazeDirection;

        /// <summary>
        /// 与注视物体的碰撞位置
        /// </summary>
        [HideInInspector]
        public Vector3 hitPoint;

        /// <summary>
        /// 与注视物体的碰撞法线
        /// </summary>
        [HideInInspector]
        public Vector3 hitNormal;

        /// <summary>
        /// 注视物体的碰撞器
        /// </summary>
        [HideInInspector]
        public Collider hitCollider;

#endregion

#region 私有及临时成员

        /// <summary>
        /// 交互类型
        /// </summary>
        [HideInInspector]
        public InputType InteractiveType { get; protected set; }

        [HideInInspector]
        public XBoxControllerInputManager controllerInput;

        private Camera mainCamera;
        private Transform mainCameraTransform;

        //public CursorController cursor;
        //public RecodingController recording;


        private GestureRecognizer manipulationRecognizer;
        private GestureRecognizer navigationRecognizer;

        private GestureRecognizer currentRecognizer;

        private bool canCapture = true;

        private GameObject oldFocusObject = null;

        private float t;

        private Ray ray;
        private bool hasKeyDown;
        private bool hasTouch;
        private bool hasDrag;
        private Vector2 touchStartPos;
        private float touchTime;
        private bool navigationSimStart = false;
        private Vector3 navigationStartPosition;
        private Vector3 navDelta;
        private float navSpeed = 0.2f;
        private bool manipulationSimStart = false;
        private Vector3 manipulationStartPosition;
        private float manipulationSpeed = 2;
        private float currentAnchorSpeed = 2;
        private Vector3 maniDelta;
        private float anchorVelocityFactor = 1;
        private CameraFlyController cameraFly = null;

        private float moveForward, moveRight, moveUp;
        private float rotateForward, rotateRight, rotateUp;

        #endregion

        public bool CanCapture()
        {
            return canCapture;
        }

        protected override void Awake()
        {
            base.Awake();

            mainCamera = Camera.main;
            if (mainCamera != null)
                mainCameraTransform = mainCamera.transform;

            GameObject obj = new GameObject();
            obj.transform.SetParent(this.transform);
            controllerInput = obj.AddComponent<XBoxControllerInputManager>();

            t = Time.realtimeSinceStartup;

            //layerMask = ~0;

        }

        [HideInInspector]
        public bool begin = true;
        // Use this for initialization
        protected override void _Init()
        {
#if UNITY_EDITOR || UNITY_STANDALONE

            InteractiveType = InputType.KeybordAndMouse;
            // 只有用鼠标的情况，才需要在主摄像机上挂接飞行脚本 
            if (mainCamera != null)
            {
                cameraFly = mainCamera.GetComponent<CameraFlyController>();
                if (cameraFly == null)
                {
                    cameraFly = mainCamera.gameObject.AddComponent<CameraFlyController>();
                }
            }

#elif WINDOWS_UWP
            if (WorldManager.state == PositionalLocatorState.Unavailable)
            {
                // 如果WorldManager不可用，则认为是非hololens的UWP设备，主要针对surface 
                InteractiveType = InputType.Touch;
            }
            else
            {
                // 如果可用，则认为是hololens
                InteractiveType = InputType.GazeAndGesture;
            }
#else
            // 其他情况，且都认为是移动设备吧！使用Touch操作 
            InteractiveType = InputType.Touch;
#endif

            // Set up a GestureRecognizer to detect Select gestures.
            manipulationRecognizer = new GestureRecognizer();

            manipulationRecognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.ManipulationTranslate);


            manipulationRecognizer.TappedEvent += OnTap;

            manipulationRecognizer.ManipulationStartedEvent += (source, offset, ray) =>
            {
                if (!begin)
                    return;

                if (cbManipulationStart != null)
                    cbManipulationStart(offset);
            };
            manipulationRecognizer.ManipulationUpdatedEvent += (source, offset, ray) =>
            {
                if (!begin)
                    return;

                if (cbManipulationUpdate != null)
                    cbManipulationUpdate(offset);
            };
            manipulationRecognizer.ManipulationCompletedEvent += (source, offset, ray) =>
            {
                if (!begin)
                    return;

                if (cbManipulationEnd != null)
                    cbManipulationEnd(offset);
            };
            manipulationRecognizer.NavigationCanceledEvent += (source, offset, ray) =>
            {
                if (!begin)
                    return;

                if (cbManipulationEnd != null)
                    cbManipulationEnd(offset);
            };

            navigationRecognizer = new GestureRecognizer();

            navigationRecognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.NavigationRailsX | GestureSettings.NavigationRailsY | GestureSettings.NavigationRailsZ);


            navigationRecognizer.TappedEvent += OnTap;

            navigationRecognizer.NavigationStartedEvent += (source, offset, ray) =>
            {
                if (begin)
                    if (cbNavigationStart != null)
                        cbNavigationStart(offset);
            };
            navigationRecognizer.NavigationUpdatedEvent += (source, offset, ray) =>
            {
                if (begin)
                    if (cbNavigationUpdate != null)
                        cbNavigationUpdate(offset);
            };
            navigationRecognizer.NavigationCompletedEvent += (source, offset, ray) =>
            {
                if (begin)
                    if (cbNavigationEnd != null)
                        cbNavigationEnd(offset);
            };
            navigationRecognizer.NavigationCanceledEvent += (source, offset, ray) =>
            {
                if (begin)
                    if (cbNavigationEnd != null)
                        cbNavigationEnd(offset);
            };

            currentRecognizer = manipulationRecognizer;

        }

        protected override void _TurnOn()
        {
            StartCapture();
            //currentRecognizer.StartCapturingGestures();
        }
        protected override void _TurnOff()
        {
            StopCapture();
            //currentRecognizer.StopCapturingGestures();
        }

        private void OnTap(InteractionSourceKind source, int tapCount, Ray ray)
        {
            if (!begin)
                return;

            // 需要给目标物体发个指令 
            if (FocusedObject != null)
                FocusedObject.SendMessage("OnTapOnObject", SendMessageOptions.DontRequireReceiver);

            if (cbTap != null)
                cbTap(tapCount);
        }
    
        public void ChangeToManipulationRecognizer()
        {
            if (currentRecognizer != manipulationRecognizer)
            {
                StopCapture();

                currentRecognizer = manipulationRecognizer;

                StartCapture();
            }
        }

        public void ChangeToNavigationRecognizer()
        {
            if (currentRecognizer != navigationRecognizer)
            {
                StopCapture();

                currentRecognizer = navigationRecognizer;

                StartCapture();
            }
        }

        // Update is called once per frame

        void Update()
        {

            if (!canCapture)
                return;


            float dT = Time.realtimeSinceStartup - t;
            t = Time.realtimeSinceStartup;


            // 确定当前注视射线 
            if (InteractiveType == InputType.Touch || (InteractiveType == InputType.KeybordAndMouse && !simulateGaze))
            {
                Vector3 pos;
                if (InteractiveType == InputType.Touch && Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    pos = touch.position;
                }
                else
                {
                    pos = Input.mousePosition;
                }
                headPosition = mainCamera.transform.position;
                Ray r = mainCamera.ScreenPointToRay(pos);
                gazeDirection = r.direction;

                RayCastForObject(ref r);
            }
            else
            {
                headPosition = mainCamera.transform.position;
                gazeDirection = mainCamera.transform.forward;

                RayCastForObject();
            }


            // 触摸屏情况下的输入 
            if (InteractiveType == InputType.Touch)
            {
                if (gazeDirection == Vector3.zero)
                {
                    headPosition = mainCamera.transform.position;
                    gazeDirection = mainCamera.transform.forward;
                }

                if (Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        hasTouch = true;
                        hasDrag = false;
                        touchStartPos = touch.position;
                        touchTime = Time.realtimeSinceStartup;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        if (hasDrag)
                        {
                            if (currentRecognizer == manipulationRecognizer)
                            {
                                Vector3 p1 = mainCamera.ScreenToWorldPoint(new Vector3(touchStartPos.x, touchStartPos.y, 1));  // 假设在距离相机1米的位置上进行操作 
                                Vector3 p2 = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 1));
                                if (cbManipulationEnd != null)
                                    cbManipulationEnd(p2 - p1);
                            }
                            else if (currentRecognizer == navigationRecognizer)
                            {
                                if (cbNavigationUpdate != null)
                                    cbNavigationUpdate(CalNavDeltaInScreen(touch.position, touchStartPos));
                            }
                        }
                        else
                        {
                            if (true/*Time.realtimeSinceStartup - touchTime < 0.3f && Vector3.Distance(touch.position, touchStartPos) < 200*/)
                            {
                                // 判定为一次点击 
                                OnTap(InteractionSourceKind.Other, 1, ray);
                            }
                        }
                        hasDrag = false;
                        hasTouch = false;
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        if (!hasDrag)
                        {
                            if (Time.realtimeSinceStartup - touchTime > 1f || Vector3.Distance(touch.position, touchStartPos) > 80)
                            {
                                // 大于0.2秒，或者移动超过一定像素，则判定为拖拽 
                                hasDrag = true;
                            }
                        }

                        if (hasDrag)
                        {
                            if (currentRecognizer == manipulationRecognizer)
                            {
                                Vector3 p1 = mainCamera.ScreenToWorldPoint(new Vector3(touchStartPos.x, touchStartPos.y, 1));  // 假设在距离相机1米的位置上进行操作 
                                Vector3 p2 = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 1));
                                if (cbManipulationUpdate != null)
                                    cbManipulationUpdate(p2 - p1);
                            }
                            else if (currentRecognizer == navigationRecognizer)
                            {
                                if (cbNavigationUpdate != null)
                                    cbNavigationUpdate(CalNavDeltaInScreen(touch.position,touchStartPos));
                            }
                        }
                    }
                }
            }

            // 即使是Touch模式，也尝试检测键盘鼠标
            if (InteractiveType == InputType.Touch || InteractiveType == InputType.KeybordAndMouse)
            {

                // 模拟Tap
                if (simulateGaze)
                {
                    // 如果模拟注视，则用enter表示tap
                    if (Input.GetKey(KeyCode.Return))
                    {
                        if (!hasKeyDown)
                        {
                            if (CanCapture())
                            {
                                OnTap(InteractionSourceKind.Other, 1, ray);
                                hasKeyDown = true;
                            }
                        }
                    }
                    else
                    {
                        if (hasKeyDown)
                            hasKeyDown = false;
                    }
                }
                else
                {
                    // 如果不模拟注视，则用点击
                    if (Input.GetMouseButtonDown(0))
                    {
                        hasTouch = true;
                        touchStartPos = Input.mousePosition;
                        touchTime = Time.realtimeSinceStartup;
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        if (Time.realtimeSinceStartup - touchTime < 0.2f && Vector3.Distance(Input.mousePosition, touchStartPos) < 40)
                        {
                            // 判定为一次点击 
                            OnTap(InteractionSourceKind.Other, 1, ray);
                        }
                        hasTouch = false;
                    }
                }

                // 按住Ctrl，开始启动Navigateion模拟 
                if (!needAssistKey || (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                {
                    if (!needAssistKey || (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                    {
                        // JKLI-UO或者小键盘来模拟六向manipolation
                        float forward = 0f;
                        bool keyDown = false;
                        if (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.Keypad8) || rotateForward > 0) { forward += 1f; keyDown = true; }
                        if (Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.Keypad2) || rotateForward < 0) { forward -= 1f; keyDown = true; }

                        float right = 0f;
                        if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Keypad6) || rotateRight > 0) { right += 1f; keyDown = true; }
                        if (Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.Keypad4) || rotateRight < 0) { right -= 1f; keyDown = true; }

                        float up = 0f;
                        if (Input.GetKey(KeyCode.O) || Input.GetKey(KeyCode.Keypad9) || rotateUp > 0) { up += 1f; keyDown = true; }
                        if (Input.GetKey(KeyCode.U) || Input.GetKey(KeyCode.Keypad7) || rotateUp < 0) { up -= 1f; keyDown = true; }

                        if (keyDown)
                        {
                            if (!navigationSimStart)
                            {
                                SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.Rotate;
                                navigationSimStart = true;
                                navigationStartPosition = Vector3.zero;
                                navDelta = Vector3.zero;
                                if (cbNavigationStart != null)
                                    cbNavigationStart(navigationStartPosition);

                            }

                            /*
                            navDelta += new Vector3(right, up, forward) * navSpeed * dT;
                            navDelta.x = Mathf.Clamp(navDelta.x, -1, 1);
                            navDelta.y = Mathf.Clamp(navDelta.y, -1, 1);
                            navDelta.z = Mathf.Clamp(navDelta.z, -1, 1);
                            */

                            if (cbNavigationUpdate != null)
                                cbNavigationUpdate(new Vector3(right, up, forward) * currentAnchorSpeed);
                        }
                        else
                        {
                            if (navigationSimStart)
                            {
                                navigationSimStart = false;
                                if (cbNavigationEnd != null)
                                    cbNavigationEnd(navDelta);
                            }
                        }
                    }
                }

                // 按住Shift，开始启动Manipulation模拟 
                if (currentRecognizer == manipulationRecognizer)
                {
                    if (!needAssistKey || (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    {


                        // JKLI-UO或者小键盘来模拟六向manipolation
                        float forward = 0f;
                        bool keyDown = false;
                        if (Input.GetKey(KeyCode.T) || Input.GetKey(KeyCode.Keypad8) || moveForward > 0) { forward += 1f; keyDown = true; }
                        if (Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.Keypad2) || moveForward < 0) { forward -= 1f; keyDown = true; }

                        float right = 0f;
                        if (Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.Keypad6) || moveRight > 0) { right += 1f; keyDown = true; }
                        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.Keypad4) || moveRight < 0) { right -= 1f; keyDown = true; }

                        float up = 0f;
                        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.Keypad9) || moveUp > 0) { up += 1f; keyDown = true; }
                        if (Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.Keypad7) || moveUp < 0) { up -= 1f; keyDown = true; }

                        if (keyDown)
                        {
                            if (!manipulationSimStart)
                            {
                                SceneAnchorController.Instance.spatialAdjustType = SpatialAdjustType.Move;
                                manipulationSimStart = true;
                                manipulationStartPosition = Vector3.zero;
                                maniDelta = Vector3.zero;
                                if (cbManipulationStart != null)
                                    cbManipulationStart(manipulationStartPosition);
                            }
                            maniDelta += mainCameraTransform.TransformDirection(new Vector3(right, up, forward) * currentAnchorSpeed * dT);

                            if (cbManipulationUpdate != null)
                                cbManipulationUpdate(maniDelta);
                        }
                        else
                        {
                            if (manipulationSimStart)
                            {
                                manipulationSimStart = false;
                                if (cbManipulationEnd != null)
                                    cbManipulationEnd(maniDelta);
                            }
                        }

                    }

                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (navigationSimStart)
                    {
                        navigationSimStart = false;
                        if (cbNavigationEnd != null)
                            cbNavigationEnd(CalNavDeltaInScreen(Input.mousePosition, navigationStartPosition));
                    }
                }
            }


            // 如果需要摇杆模拟 
            if (controllerInput.hasContoller)
            {
                // 模拟Tap
                if (controllerInput.GetButtonDown(ControllerButton.A))
                {
                    OnTap(InteractionSourceKind.Controller, 1, ray);
                }

                // 模拟Manipulation
                float leftX = controllerInput.GetAxisLeftThumbstickX();
                float leftY = controllerInput.GetAxisLeftThumbstickY();
                if (leftX != 0 || leftY != 0)
                {
                    float forward = 0f;
                    float right = leftX;
                    float up = 0f;

                    if (controllerInput.GetAxisLeftTrigger() > 0.5f)
                        forward = leftY;
                    else
                        up = leftY;

                    if (!manipulationSimStart)
                    {
                        manipulationSimStart = true;
                        manipulationStartPosition = Vector3.zero;
                        maniDelta = Vector3.zero;
                        if (cbManipulationStart != null)
                            cbManipulationStart(manipulationStartPosition);
                    }
                    maniDelta += mainCameraTransform.TransformDirection(new Vector3(right, up, forward) * manipulationSpeed * dT);

                    if (cbManipulationUpdate != null)
                        cbManipulationUpdate(maniDelta);
                }
                else
                {
                    if (manipulationSimStart)
                    {
                        manipulationSimStart = false;
                        if (cbManipulationEnd != null)
                            cbManipulationEnd(maniDelta);
                    }
                }

                // 模拟Navigation
                leftX = controllerInput.GetAxisRightThumbstickX();
                leftY = controllerInput.GetAxisRightThumbstickY();
                if (leftX != 0 || leftY != 0)
                {
                    float forward = 0f;
                    float right = leftX;
                    float up = 0f;

                    if (controllerInput.GetAxisRightTrigger() > 0.5f)
                        forward = leftY;
                    else
                        up = leftY;

                    if (!navigationSimStart)
                    {

                        navigationSimStart = true;
                        navigationStartPosition = Vector3.zero;
                        if (cbNavigationStart != null)
                            cbNavigationStart(navigationStartPosition);

                    }

                    if (cbNavigationUpdate != null)
                        cbNavigationUpdate(new Vector3(right, up, forward));
                }
                else
                {
                    if (navigationSimStart)
                    {
                        navigationSimStart = false;
                        if (cbNavigationEnd != null)
                            cbNavigationEnd(navDelta);
                    }
                }
            }

        }

        private Vector3 CalNavDeltaInScreen(Vector3 p1, Vector3 p2)
        {
            Vector3 delta = p2 - p1;
            //Debug.Log("delta=" + delta);

            float dx = delta.x / 100f;
            if (dx > 1) dx = 1;
            if (dx < -1) dx = -1;

            float dy = delta.y / 100f;
            if (dy > 1) dy = 1;
            if (dy < -1) dy = -1;

            return new Vector3(dx, dy, 0);
        }

        private void RayCastForObject(ref Ray r)
        {
            ray = r;
            _RayCastForObject();
        }

        private void RayCastForObject()
        {
            ray = new Ray(headPosition, gazeDirection);
            _RayCastForObject();
        }


        private void _RayCastForObject()
        {
            oldFocusObject = FocusedObject;

            bool hasObj = false;
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1000, layerMask))
            {
                
                FocusedObject = hitInfo.collider.gameObject;
                hitPoint = hitInfo.point;
                hitNormal = hitInfo.normal;
                hitCollider = hitInfo.collider;
                //Debug.Log("focus:" + FocusedObject);

                hasObj = true;
            }

            if (!hasObj)
            {
                FocusedObject = null;
                hitPoint = headPosition + gazeDirection * 5;
                hitNormal = gazeDirection;
                hitCollider = null;
            }

            if (oldFocusObject != FocusedObject)
            {
                // 需要发送移入、移出消息 
                if (oldFocusObject != null)
                    oldFocusObject.SendMessage("OnGazeExitObject", SendMessageOptions.DontRequireReceiver);

                if (FocusedObject != null)
                    FocusedObject.SendMessage("OnGazeEnterObject", SendMessageOptions.DontRequireReceiver);

                oldFocusObject = FocusedObject;
            }

        }

        public void StartCapture()
        {
            currentRecognizer.CancelGestures();
            currentRecognizer.StartCapturingGestures();
            canCapture = true;

            Debug.Log("Input Turn On!!!");

            if (cameraFly != null)
            {
                /*
                if (simulateGaze)
                    cameraFly.enabled = true;
                else
                    cameraFly.enabled = false;
                */
                cameraFly.enabled = true;
            }

            if (cameraFly == null)
            {
                Debug.Log("cameraFly is null");
            }
            else
            {
                Debug.Log("Fly = " + cameraFly.enabled);
            }

        }

        public void StopCapture()
        {
            currentRecognizer.CancelGestures();
            currentRecognizer.StopCapturingGestures();
            canCapture = false;

            if (cameraFly != null)
            {
                cameraFly.enabled = false;
            }
        }

        public void SetMoveData(float forward, float right, float up)
        {
            moveForward = forward;
            moveRight = right;
            moveUp = up;
        }

        public void SetRotateData(float forward, float right, float up)
        {
            rotateForward = forward;
            rotateRight = right;
            rotateUp = up;
        }

        public void SetAnchorSpeed(float anchorSpeed)
        {
            currentAnchorSpeed = anchorSpeed;
        }

    }
}