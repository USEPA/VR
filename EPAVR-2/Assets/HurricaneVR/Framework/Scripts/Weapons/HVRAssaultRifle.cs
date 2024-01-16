using UnityEngine;

namespace HurricaneVR.Framework.Weapons
{
    public class HVRAssaultRifle : HVRRayCastGun
    {
        [Header("Assault Rifle Components")]
        public HVRChargingHandle ChargingHandle;
        public Animator Animator;
        public ParticleSystem CasingSystem;
        public ParticleSystem BulletEjectSystem;
        public HVRBolt Bolt;
        public GameObject ChamberedRound;
        public GameObject ChamberedCasing;

        protected override void Start()
        {
            base.Start();

            if (!Animator)
            {
                Animator = GetComponent<Animator>();
            }

            ChargingHandle.FullRelease.AddListener(OnChargingHandleReleased);
            ChargingHandle.EjectReached.AddListener(OnChargingHandlePulledBack);
            Bolt.BoltForward.AddListener(OnBoltForward);
        }

        private void OnChargingHandlePulledBack()
        {
            EjectBullet();
        }

        private void OnBoltForward()
        {
            GunSounds?.PlaySlideForward();
            ChamberRound();
            if (IsBulletChambered)
            {
                EnableChamberedRound();
            }
        }

        private void OnChargingHandleReleased()
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

            if (RequiresChamberedBullet)
            {
                DisableChamberedRound();

                if (OutOfAmmo)
                {
                    if (Animator)
                    {
                        Animator.enabled = false;
                    }
                    Bolt.PushBack();
                    EjectCasing();
                }
            }

            if (!OutOfAmmo && Animator)
            {
                Animator.enabled = true;
                Animator.SetTrigger("Fire");
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
                if (BulletEjectSystem)
                {
                    BulletEjectSystem.Emit(1);
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
}
