using System.Collections;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared.Utilities;
using UnityEngine;

namespace HurricaneVR.Framework.Weapons
{
    [RequireComponent(typeof(HVRShotgunMagazine))]
    public class HVRShotGun : HVRRayCastGun
    {
        [Header("Shotgun Settings")]
        public HVRShotGunType ShotGunType = HVRShotGunType.Pump;

        public int NumberOfPellets = 5;
        public float ShotRadius = 0.05f;


        [Header("Shotgun Components")]
        public HVRPump Pump;
        public Animator Animator;
        public ParticleSystem CasingSystem;
        public HVRBolt Bolt;
        public GameObject ChamberedRound;
        public GameObject ChamberedCasing;

        [Header("Round Ejection")]
        [Tooltip("Shotgun shell round prefab")]
        public GameObject ShellPrefab;
        [Tooltip("Eject position and forward direction")]
        public Transform EjectDirection;
        [Tooltip("Velocity of the ejected round")]
        public float EjectVelocity;
        [Tooltip("Angular velocity of the ejected round")]
        public Vector3 EjectAngularVelocity;

        private bool _casingChambered;

        protected override void Start()
        {
            base.Start();

            if (!Animator)
            {
                Animator = GetComponent<Animator>();
            }

            Ammo = GetComponent<HVRShotgunMagazine>();

            if (Pump)
            {
                Pump.ChamberRound.AddListener(OnChamberRound);
                Pump.FullRelease.AddListener(OnPumpReleased);
                Pump.EjectReached.AddListener(OnPumpPulledBack);
            }
        }

        private void OnChamberRound()
        {
            if (IsBulletChambered)
                return;
            GunSounds?.PlaySlideForward();
            ChamberRound();
            if (IsBulletChambered)
            {
                EnableChamberedRound();
            }
        }

        protected override void FireBullets(Vector3 direction)
        {
            for (int i = 0; i < NumberOfPellets; i++)
            {
                var xy = Random.insideUnitCircle * ShotRadius;
                var newDirection = direction + transform.TransformDirection(xy);
                FireBullet(newDirection);
            }
        }

        public override void ReleaseAmmo()
        {

        }

        private void OnPumpPulledBack()
        {
            GunSounds?.PlaySlideBack();
            EjectBullet();
            if (_casingChambered)
            {
                EjectCasing();
                _casingChambered = false;
            }
        }

        private void OnPumpReleased()
        {
            GunSounds?.PlaySlideForward();
            ChamberRound();
            if (Bolt)
            {
                Bolt.IsPushedBack = false;
            }

            if (IsBulletChambered)
            {
                EnableChamberedRound();
            }
        }

        protected override void AfterFired()
        {
            base.AfterFired();

            if (ShotGunType == HVRShotGunType.Pump)
            {
                _casingChambered = true;
            }

            if (RequiresChamberedBullet)
            {
                DisableChamberedRound();

                if (OutOfAmmo)
                {
                    if (Animator)
                    {
                        Animator.enabled = false;
                    }

                    if (ShotGunType == HVRShotGunType.SemiAutomatic)
                    {
                        Bolt.PushBack();
                        EjectCasing();
                    }
                }
            }

            if (!OutOfAmmo)
            {
                if (Animator)
                {
                    Animator.enabled = true;
                    Animator.SetTrigger("Fire");
                }

                EnableChamberedCasing();
            }
        }

        //call from animation
        public void DisableFireAnimator()
        {

            Animator.enabled = false;
        }

        protected virtual void EnableChamberedCasing()
        {
            if (ChamberedCasing)
            {
                ChamberedCasing.SetActive(true);
            }
        }

        protected virtual void DisableChamberedCasing()
        {
            if (ChamberedCasing)
            {
                ChamberedCasing.SetActive(false);
            }
        }


        protected virtual void DisableChamberedRound()
        {
            if (ChamberedRound)
            {
                ChamberedRound.SetActive(false);
            }
        }

        //call from animation
        protected virtual void EnableChamberedRound()
        {
            if (ChamberedRound)
            {
                ChamberedRound.SetActive(true);
            }
        }

        public override void EjectBullet()
        {
            if (IsBulletChambered)
            {
                IsBulletChambered = false;
                DisableChamberedRound();
                if (ShellPrefab && EjectDirection)
                {
                    var shell = Instantiate(ShellPrefab, EjectDirection.position, Quaternion.identity);
                    var rb = shell.GetRigidbody();
                    var direction = EjectDirection.forward;
                    direction *= EjectVelocity;
                    rb.velocity = direction;
                    rb.angularVelocity = EjectAngularVelocity;
                    var colliders = shell.GetComponentsInChildren<Collider>();
                    StartCoroutine(colliders.IgnoreCollisionForSeconds(Grabbable.Colliders, 2));
                }
            }
        }

      

        public override void EjectCasing()
        {
            DisableChamberedCasing();

            if (CasingSystem)
            {
                CasingSystem.Emit(1);
            }
        }
    }

    public enum HVRShotGunType
    {
        Pump,
        SemiAutomatic
    }
}