using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class TestPlayerController : PlayerController
    {
        #region Inspector Assigned Variables
        [Header("Other Important References")]
        [SerializeField] VRUserToolSelection m_toolSelection;
        [SerializeField] TestHand m_testHand;
        [SerializeField] Image m_crosshair;
        [SerializeField] GraphicRaycaster m_graphicRaycaster;
        [SerializeField] Transform m_leftHandDefaultTarget;
        [SerializeField] Transform m_rightHandDefaultTarget;
        public GameObject hitObject;
        [Header("Player Configuration")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        [Space(10)]
        public bool cursorLocked = true;
        #endregion
        #region Private Variables
        public Vector2 m_moveVector;
        public Vector2 m_lookVector;
        public Vector2 m_mousePosition;

        private float m_speed;
        private float m_rotationVelocity;
        private float m_verticalVelocity;
        private float m_cameraTargetPitch;

        private List<RaycastResult> m_graphicRaycastResults;
        private List<RaycastResult> m_previousGraphicRaycastResults;

        private const float m_threshold = 0.01f;
        #endregion
        #region Public Properties
        public Vector3 MousePosition { get => m_mousePosition; }
        #endregion

        #region Initialization
        public override void Init()
        {
            // Call base functionality
            base.Init();
            // Hook up events
            if (VRUserManager.Instance)
            {
                VRUserManager.Instance.OnStateChange += i => SetCursorState(i == PlayerState.InGame);
                SetCursorState(true);
            }

            m_graphicRaycastResults = new List<RaycastResult>();

            m_avatar.LeftHand.Controller.enableInputTracking = false;
            FollowTransform leftHandFollowSolver = m_avatar.LeftHand.gameObject.AddComponent<FollowTransform>();
            leftHandFollowSolver.FollowTarget = m_leftHandDefaultTarget;

            m_avatar.RightHand.Controller.enableInputTracking = false;
            FollowTransform rightHandFollowSolver = m_avatar.RightHand.gameObject.AddComponent<FollowTransform>();
            rightHandFollowSolver.FollowTarget = m_rightHandDefaultTarget;

        }
        #endregion

        // Update is called once per frame
        void Update()
        {
            HandleMovement();
            if (VRUserManager.Instance.CurrentState == PlayerState.Focused)
            {
                Ray ray = Camera.main.ScreenPointToRay(m_mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 3.0f, LayerMask.GetMask("DebugGrabbable")))
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.green);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.magenta);
                }
            }
            /*
            else if (VRUserManager.Instance.CurrentState == PlayerState.InGame)
            {
                //Set up the new Pointer Event
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                //Set the Pointer Event Position to that of the mouse position
                pointerEventData.position = Mouse.current.position.ReadValue();

                List<RaycastResult> prevRaycastResults = new List<RaycastResult>();
                List<GameObject> prevFlaggedObjects = new List<GameObject>();
                if (m_graphicRaycastResults.Count > 1)
                {
                    foreach(RaycastResult result in m_graphicRaycastResults)
                    {
                        if (result.gameObject.TryGetComponent<TutorialVideoPlayer>(out TutorialVideoPlayer videoPlayer))
                        {
                            prevFlaggedObjects.Add(result.gameObject);
                        }
                        else if (result.gameObject.TryGetComponent<Button>(out Button button))
                        {
                            prevFlaggedObjects.Add(button.gameObject);
                            //UnityEngine.Debug.Log($"Previously found button: {button.gameObject.name} || Time: {Time.time}");
                        }
                        //prevRaycastResults.Add(result);
                    }
                }

                m_graphicRaycastResults = new List<RaycastResult>();
                // Gather new results
                EventSystem.current.RaycastAll(pointerEventData, m_graphicRaycastResults);


                if (m_graphicRaycastResults.Count > 0)
                {
                    // Check if there is a TutorialVideoPlayer
                    foreach(RaycastResult newResult in m_graphicRaycastResults)
                    {
                        if (newResult.gameObject.TryGetComponent<TutorialVideoPlayer>(out TutorialVideoPlayer videoPlayer))
                        {
                            // Check if this is the first time this video player has been seen
                            if (prevFlaggedObjects.Count == 0 || !prevFlaggedObjects.Contains(newResult.gameObject))
                            {
                                videoPlayer.OnPointerEnter(pointerEventData);
                                UnityEngine.Debug.Log($"Pointer entered {newResult.gameObject.name} || Time: {Time.time}");
                                prevFlaggedObjects.Remove(newResult.gameObject);
                            }
                        }
                        else if (newResult.gameObject.TryGetComponent<Button>(out Button button))
                        {
                            if (prevFlaggedObjects.Count == 0 || !prevFlaggedObjects.Contains(newResult.gameObject)) 
                            {
                                UnityEngine.Debug.Log($"Pointer entered {newResult.gameObject.name} || Time: {Time.time}");
                                prevFlaggedObjects.Remove(newResult.gameObject);
                            }
                        }
                        
                    }
                }

                if(prevFlaggedObjects.Count > 0)
                {
                    foreach(GameObject flaggedObject in prevFlaggedObjects)
                    {
                        if (flaggedObject.TryGetComponent<TutorialVideoPlayer>(out TutorialVideoPlayer videoPlayer))
                        {
                            videoPlayer.OnPointerExit(pointerEventData);
                            UnityEngine.Debug.Log($"Pointer exited {flaggedObject.name} || Time: {Time.time}");
                        }
                        else if (flaggedObject.TryGetComponent<Button>(out Button button))
                        {
                            UnityEngine.Debug.Log($"Pointer exited {flaggedObject.name} || Time: {Time.time}");
                        }
                    }
                }
            }
            */
        }

        private void LateUpdate()
        {
            HandleCameraRotation();
        }

        #region Movement-Related Functionality
        void HandleMovement()
        {
            // First, get the target speed
            float targetSpeed = MoveSpeed;
            // Zero out speed if there is no movement input
            if (m_moveVector == Vector2.zero) targetSpeed = 0.0f;

            // Get the player's current horizontal speed
            float currentHorizontalSpeed = new Vector3(m_characterController.velocity.x, 0.0f, m_characterController.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = 1.0f; // m_moveVector.magnitude

            // Accelerate/decelerate to target speed
            if (currentHorizontalSpeed < (targetSpeed - speedOffset) || currentHorizontalSpeed > (targetSpeed + speedOffset))
            {
                // Lerp the speed to give a more organic feel
                m_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                m_speed = Mathf.Round(m_speed * 1000f) / 1000f;
            }
            else
            {
                m_speed = targetSpeed;
            }

            // Normalize input direction
            Vector3 inputDirection = new Vector3(m_moveVector.x, 0.0f, m_moveVector.y).normalized;
            if (m_moveVector != Vector2.zero) inputDirection = transform.right * m_moveVector.x + transform.forward * m_moveVector.y;
            // Move the player
            m_characterController.Move(inputDirection.normalized * (m_speed * Time.deltaTime) + new Vector3(0.0f, m_verticalVelocity, 0.0f) * Time.deltaTime);
        }
        #endregion

        #region Camera-Related Functionality
        void HandleCameraRotation()
        {
            if (!VRUserManager.Instance || VRUserManager.Instance.CurrentState != PlayerState.InGame) return;
            // Make sure there is enough input to justify rotation
            if (m_lookVector.sqrMagnitude >= m_threshold)
            {
                m_cameraTargetPitch += m_lookVector.y * RotationSpeed * Time.deltaTime;
                m_rotationVelocity = m_lookVector.x * RotationSpeed * Time.deltaTime;

                // Clamp pitch rotation
                m_cameraTargetPitch = MathHelper.ClampAngle(m_cameraTargetPitch, BottomClamp, TopClamp);

                // Update the camera rotation
                m_xrRig.Camera.gameObject.transform.localRotation = Quaternion.Euler(m_cameraTargetPitch, 0.0f, 0.0f);
                // Handle horizontal rotation
                transform.Rotate(Vector3.up * m_rotationVelocity);
            }
        }
        #endregion

        #region Input-Related Functionality
        public void OnMove(InputValue value)
        {
            m_moveVector = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            m_lookVector = value.Get<Vector2>();
        }

        public void OnMousePosition(InputValue value)
        {
            m_mousePosition = value.Get<Vector2>();
        }


        public void OnToggleInventory()
        {
            UnityEngine.Debug.Log($"OnToggleInventory pressed || Time: {Time.time}");

            if (!VRUserManager.Instance || VRUserManager.Instance.CurrentState == PlayerState.Focused) return;

            m_toolSelection.ToggleMenu();
        }

        public void OnToggleCursorLock()
        {
            if (!VRUserManager.Instance || VRUserManager.Instance.CurrentState == PlayerState.Menu) return;

            if (VRUserManager.Instance.CurrentState == PlayerState.InGame)
            {
                VRUserManager.Instance.SetState(PlayerState.Focused);
            }
            else
            {
                if (m_testHand && m_testHand.CurrentItem != null) m_testHand.ReleaseItem();
                VRUserManager.Instance.SetState(VRUserManager.Instance.PreviousState);
            }
                
        }

        public void OnInteract()
        {
            if (!VRUserManager.Instance || (VRUserManager.Instance.CurrentState == PlayerState.Menu)) return;

            //Set up the new Pointer Event
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            //Set the Pointer Event Position to that of the mouse position
            pointerEventData.position = Mouse.current.position.ReadValue();

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            EventSystem.current.RaycastAll(pointerEventData, results);
            if (results.Count > 0)
            {
                foreach(RaycastResult hit in results)
                {
                    if (hit.gameObject.TryGetComponent<Button>(out Button button))
                    {
                        button.onClick?.Invoke();
                        UnityEngine.Debug.Log($"Interaction hit {hit.gameObject.name} || Time: {Time.time}");
                    }

                }
            }
        }

        public void OnRightClick()
        {
            if (!VRUserManager.Instance || VRUserManager.Instance.CurrentState != PlayerState.Focused) return;

            if (m_testHand.CurrentItem == null)
            {
                // Try to get a draggable object
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(m_mousePosition);

                if (Physics.Raycast(ray, out hit, 3.0f, LayerMask.GetMask("DebugGrabbable")))
                {
                    hitObject = hit.collider.gameObject;
                    if (hit.collider.gameObject.TryGetComponent<TestDraggableItem>(out TestDraggableItem item))
                    {
                        m_testHand.AttemptGrabItem(item);
                    }
                }
            }
            else
            {
                m_testHand.ReleaseItem();
            }
        }
        private void OnApplicationFocus(bool hasFocus)
        {
            if (VRUserManager.Instance)
                SetCursorState((VRUserManager.Instance.CurrentState == PlayerState.InGame));
            else
                SetCursorState(cursorLocked);
        }
        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
            if (m_crosshair != null) m_crosshair.gameObject.SetActive(newState);
        }
        #endregion

        public void ForceToggleToolInView()
        {
            if (!VRUserManager.Instance || VRUserManager.Instance.CurrentTool == null) return;

            VRUserManager.Instance.CurrentTool.ForceToggleInView();
        }

        private void OnDestroy()
        {
            UnityEngine.Debug.Log($"{gameObject.name} should be removing player state event || Time: {Time.time}");
            VRUserManager.Instance.OnStateChange -= i => SetCursorState(i == PlayerState.InGame);
        }
    }
}

