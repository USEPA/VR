using System.Collections;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.ScriptableObjects;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HurricaneVR.Framework.Core.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class HVRJointHand : MonoBehaviour
    {
        [Header("Settings")]
        public HVRJointSettings JointSettings;
        public int SolverIterations = 10;
        public int SolverVelocityIterations = 10;
        public float ReturnSpeed = 5f;
        [FormerlySerializedAs("MaxDistance")]
        public float MaxTargetDistance = .8f;
        public MaxDistanceBehaviour MaxDistanceBehaviour = MaxDistanceBehaviour.HandSweep;

        [Header("Arm Limit")]
        public Transform Anchor;
        public float ArmLength = .75f;

        [Header("Components")]
        [Tooltip("Target transform for position and rotation tracking")]
        public Transform Target;
        public Rigidbody ParentRigidBody;
        public HVRHandStrengthHandler StrengthHandler;
        public HVRTeleporter Teleporter;

        [Header("Events")]
        public UnityEvent MaxDistanceReached = new UnityEvent();
        public UnityEvent ReturnedToController = new UnityEvent();

        [Header("Debug")]
        public bool IsReturningToController;


        public Rigidbody RigidBody { get; private set; }

        public ConfigurableJoint Joint { get; set; }

        public HVRHandGrabber Grabber { get; private set; }


        private Vector3 _previousControllerPosition;
        private Quaternion _previousRotation;

        protected virtual void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            //this joint needs to be created before any offsets are applied to the controller target
            //due to how joints snapshot their initial rotations on creation
            SetupJoint();

            RigidBody.maxAngularVelocity = 150f;
            RigidBody.solverIterations = SolverIterations;
            RigidBody.solverVelocityIterations = SolverVelocityIterations;

            Grabber = GetComponent<HVRHandGrabber>();

            if (!Teleporter && transform.root)
            {
                Teleporter = transform.root.GetComponentInChildren<HVRTeleporter>();
            }

            if (!JointSettings)
                Debug.LogError("JointSettings field is empty, must be populated with HVRJointSettings scriptable object.");

            if (!StrengthHandler)
            {
                if (!TryGetComponent(out StrengthHandler))
                {
                    StrengthHandler = gameObject.AddComponent<HVRHandStrengthHandler>();
                }
            }

            if (ReturnSpeed < .1f)
                ReturnSpeed = 5f;

            StrengthHandler.Joint = Joint;
            StrengthHandler.Initialize(JointSettings);
        }

        protected virtual void Start()
        {
            _previousControllerPosition = Target.position;
            _previousRotation = Target.rotation;
            //fixing the issue where the hand goes to world 0,0,0 at start
            StartCoroutine(StopHandsRoutine());
        }

        protected virtual IEnumerator StopHandsRoutine()
        {
            var count = 0;
            while (count < 100)
            {
                yield return new WaitForFixedUpdate();
                RigidBody.velocity = Vector3.zero;
                RigidBody.angularVelocity = Vector3.zero;
                transform.position = Target.position;
                count++;
            }
        }

        public void Disable()
        {
            RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            RigidBody.isKinematic = true;
        }

        public void Enable()
        {
            RigidBody.isKinematic = false;
            RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        protected virtual void OnEnable()
        {
            Joint.targetRotation = Quaternion.Inverse(Quaternion.Inverse(ParentRigidBody.transform.rotation) * transform.rotation);
        }

        public virtual void SetupJoint()
        {
            //Debug.Log($"{name} joint created.");
            //this joint needs to be created before any offsets are applied to the controller target
            //due to how joints snapshot their initial rotations on creation
            Joint = ParentRigidBody.transform.gameObject.AddComponent<ConfigurableJoint>();
            Joint.autoConfigureConnectedAnchor = false;
            Joint.connectedBody = RigidBody;
            Joint.connectedAnchor = Vector3.zero;
            Joint.anchor = ParentRigidBody.transform.InverseTransformPoint(Target.position);
            Joint.enableCollision = false;
            Joint.enablePreprocessing = false;
            Joint.rotationDriveMode = RotationDriveMode.Slerp;
        }

        protected virtual void FixedUpdate()
        {
            UpdateTargetVelocity();
            UpdateDistanceCheck();

            if (Anchor)
            {
                if (Vector3.Distance(Anchor.position, Target.position) > ArmLength)
                {
                    var localTargetPosition = ParentRigidBody.transform.InverseTransformPoint(Target.position);
                    var localAnchor = ParentRigidBody.transform.InverseTransformPoint(Anchor.position);
                    var dir = localTargetPosition - localAnchor;
                    dir = Vector3.ClampMagnitude(dir, ArmLength);
                    var point = localAnchor + dir;

                    Joint.targetPosition = point;
                }
                else
                {
                    Joint.targetPosition = Vector3.zero;
                }
            }
        }

        private float _timer = 0f;
        protected virtual void UpdateDistanceCheck()
        {
            if (Teleporter.TeleportState == TeleportState.None) _timer += Time.deltaTime;
            else _timer = 0f;
            if (!IsReturningToController && _timer > .5f)
            {
                var reached = false;
                if (Anchor)
                    reached = Vector3.Distance(Anchor.position, transform.position) > ArmLength;
                if (!reached)
                    reached = Vector3.Distance(transform.position, Target.position) > MaxTargetDistance;

                if (reached) OnMaxDistanceReached();
            }
            else if (IsReturningToController)
            {
                transform.position = Vector3.MoveTowards(transform.position, Target.position, ReturnSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, Target.position) < .05f)
                {
                    OnReturned();
                }
            }
        }

        private void OnReturned()
        {
            RigidBody.detectCollisions = true;
            IsReturningToController = false;
            if (Grabber.CollisionHandler)
                Grabber.CollisionHandler.SweepHand(Grabber);
            ReturnedToController.Invoke();
        }

        private void OnMaxDistanceReached()
        {
            if (MaxDistanceBehaviour == MaxDistanceBehaviour.GrabbablePrevents && Grabber.IsGrabbing)
                return;

            if (MaxDistanceBehaviour == MaxDistanceBehaviour.GrabbableDrops && Grabber.GrabbedTarget)
                Grabber.ForceRelease();

            if (MaxDistanceBehaviour == MaxDistanceBehaviour.HandSweep && Grabber.CollisionHandler)
            {
                if (!Grabber.CollisionHandler.SweepHand(Grabber, Grabber.GrabbedTarget, (Grabber.CollisionHandler.ResetTarget.position - Target.position)))
                {
                    transform.position = RigidBody.position = Target.position;
                }
                return;
            }

            IsReturningToController = true;
            MaxDistanceReached.Invoke();
            RigidBody.detectCollisions = false;
        }


        public virtual void UpdateTargetVelocity()
        {
            var worldVelocity = (Target.position - _previousControllerPosition) / Time.fixedDeltaTime;
            _previousControllerPosition = Target.position;
            Joint.targetVelocity = ParentRigidBody.transform.InverseTransformDirection(worldVelocity);

            var angularVelocity = Target.rotation.AngularVelocity(_previousRotation);

            Joint.targetAngularVelocity = Quaternion.Inverse(ParentRigidBody.transform.rotation) * angularVelocity;

            if (Joint.rotationDriveMode == RotationDriveMode.XYAndZ)
            {
                Joint.targetAngularVelocity *= -1;
            }

            _previousRotation = Target.rotation;
        }

        private void OnDrawGizmos()
        {
            if (RigidBody)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(RigidBody.worldCenterOfMass, .017f);
            }
        }
    }

    public enum MaxDistanceBehaviour
    {
        GrabbablePrevents,
        GrabbableDrops,
        HandSweep
    }
}
