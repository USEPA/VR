using UnityEngine;

namespace HurricaneVR.Framework.Weapons
{
    public class HVRPistol : HVRRayCastGun
    {
        [Header("Pistol Components")]
        public HVRSlide Slide;
        public Animator Animator;
        public ParticleSystem CasingSystem;
        public ParticleSystem BulletEjectSystem;
        public GameObject ChamberedRound;
        public GameObject ChamberedCasing;

        protected override void Start()
        {
            base.Start();

            if (!Animator)
            {
                Animator = GetComponent<Animator>();
            }

            Slide.FullRelease.AddListener(OnSlideFullRelease);
            Slide.EjectReached.AddListener(OnSlideEjectReached);
        }

        private void OnSlideEjectReached()
        {
            EjectBullet();
        }

        private void OnSlideFullRelease()
        {
            GunSounds?.PlaySlideForward();
            ChamberRound();
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
                    Slide.PushBack();
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

        public override void EjectCasing()
        {
            DisableChamberedCasing();

            if (CasingSystem)
            {
                CasingSystem.Emit(1);
            }
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

                if (OutOfAmmo)
                {
                    Slide.PushBack();
                    //Slide.ForceRelease();
                }
            }
        }
    }
}