using System;
using System.Collections;
using HurricaneVR.Framework.Components;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Sockets;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HurricaneVR.Framework.Weapons
{
    [RequireComponent(typeof(HVRGrabbable))]
    public class HVRRayCastGun : HVRDamageProvider
    {
        

        [Tooltip("If this grabbable is held, the StabilizedRecoilForce is used when shooting.")]
        public HVRGrabbable StabilizerGrabbable;

        public HVRGrabbable Grabbable { get; private set; }

        [Header("Settings")]
        [Tooltip("Time between shots")]
        public float Cooldown;
        [Tooltip("Physics layers for the ray cast")]
        public LayerMask HitMask;
        public float MuzzleFlashTime = .2f;
        [Tooltip("Flexible bullet range per gun type")]
        public float BarrelRange = 10;
        [Tooltip("Does this gun require ammo inserted to shoot")]
        public bool RequiresAmmo = true;
        [Tooltip("Is chambering required to shoot")]
        public bool RequiresChamberedBullet = true;
        public FireType FireType = FireType.Single;
        [Tooltip("Speed of the bullet trail prefab")]
        public float BulletTrailSpeed = 40f;
        [Tooltip("How fast to kick the magazine out of the gun")]
        public float AmmoEjectVelocity = 1f;
        [Tooltip("How long until we destroy the muzzle smoke object")]
        public float MuzzleSmokeTime = 1.5f;
        [Tooltip("Should the gun automatically chamber the next round after firing")]
        public bool ChambersAfterFiring = true;
        [Tooltip("If true will use damage, force, range, from the ammo")]
        public bool UseAmmoProperties;
        [Tooltip("If not using ammo properties, range of the bullet")]
        public float NoAmmoRange = 40f;

        [Header("Objects")]
        [Tooltip("Optional Direction to eject Ammo - use the z axis")]
        public Transform AmmoEjectDirection; //forward
        [Tooltip("Socket for taking in ammo / magazines")]
        public HVRSocket AmmoSocket;
        [Tooltip("Component that handls gun sfx")]
        public HVRGunSounds GunSounds;
        [Tooltip("Muzzle flash object")]
        public GameObject MuzzleFlashObject;
        [Tooltip("Recoil settings component")]
        public HVRRecoil RecoilComponent;
        [Tooltip("Where the bullet should come from, z forward direction")]
        public Transform BulletOrigin;
        [Tooltip("Muzzle smoke object")]
        public GameObject MuzzleSmoke;

        [FormerlySerializedAs("BulletPrefab")]
        [Header("Prefabs")]
        public GameObject BulletTrailPrefab;


        public UnityEvent Fired = new UnityEvent();
        public GunHitEvent Hit = new GunHitEvent();

        private float _timer;

        public bool IsBulletChambered { get; set; }

        public HVRAmmo Ammo { get; set; }

        public float BulletRange
        {
            get
            {
                if (UseAmmoProperties && Ammo)
                    return Ammo.MaxRange + BarrelRange;
                return BarrelRange + NoAmmoRange;
            }
        }

        public HVRDamageProvider DamageProvider
        {
            get
            {
                if (UseAmmoProperties && Ammo)
                    return Ammo;
                return this;
            }
        }

        public bool OutOfAmmo
        {
            get
            {
                if (RequiresChamberedBullet && IsBulletChambered)
                    return false;

                if (!RequiresAmmo)
                    return false;

                if (!Ammo || Ammo.IsEmpty)
                    return true;

                return false;
            }
        }

        private bool _isFiring;
        private Coroutine _automaticRoutine;

        protected override void Start()
        {
            base.Start();

            Grabbable = GetComponent<HVRGrabbable>();
            Grabbable.Activated.AddListener(OnGrabbableActivated);
            Grabbable.Deactivated.AddListener(OnGrabbableDeactivated);

            Grabbable.Grabbed.AddListener(OnGrabbed);
            Grabbable.Released.AddListener(OnReleased);

            if (StabilizerGrabbable)
            {
                StabilizerGrabbable.Grabbed.AddListener(OnStabilizerGrabbed);
                StabilizerGrabbable.Released.AddListener(OnStabilizerReleased);
            }

            if (AmmoSocket)
            {
                AmmoSocket.Grabbed.AddListener(OnAmmoGrabbed);
            }

            if (!RecoilComponent)
            {
                RecoilComponent = GetComponent<HVRRecoil>();
            }

            if (!GunSounds)
            {
                GunSounds = GetComponent<HVRGunSounds>();
            }
        }

        private void OnReleased(HVRGrabberBase arg0, HVRGrabbable arg1)
        {
            if (RecoilComponent)
            {
                RecoilComponent.HandRigidBody = null;
            }
        }

        private void OnGrabbed(HVRGrabberBase arg0, HVRGrabbable arg1)
        {
            if (arg0 is HVRHandGrabber hand && RecoilComponent)
            {
                RecoilComponent.HandRigidBody = hand.Rigidbody;
            }
        }

        private void OnStabilizerReleased(HVRGrabberBase grabber, HVRGrabbable arg1)
        {
            Grabbable.ForceTwoHandSettings = false;

            if (RecoilComponent)
            {
                RecoilComponent.TwoHanded = false;
            }
        }

        private void OnStabilizerGrabbed(HVRGrabberBase grabber, HVRGrabbable arg1)
        {
            if (Grabbable.PrimaryGrabber && Grabbable.PrimaryGrabber.IsHandGrabber)
            {
                Grabbable.ForceTwoHandSettings = true;

                if (RecoilComponent)
                {
                    RecoilComponent.TwoHanded = true;
                }
            }
        }

        public virtual void ChamberRound()
        {
            if (IsBulletChambered)
                return;

            if (Ammo && Ammo.HasAmmo)
            {
                Ammo.RemoveBullet();
                IsBulletChambered = true;
            }
        }

        protected virtual void OnAmmoGrabbed(HVRGrabberBase grabber, HVRGrabbable grabbable)
        {
            var ammo = grabbable.GetComponent<HVRAmmo>();
            if (!ammo)
            {
                Debug.Log($"{grabbable.name} is missing the ammo component.");
                return;
            }

            Ammo = ammo;

            for (var i = 0; i < Grabbable.Colliders.Count; i++)
            {
                var ourCollider = Grabbable.Colliders[i];
                for (var j = 0; j < grabbable.Colliders.Count; j++)
                {
                    var ammoCollider = grabbable.Colliders[j];
                    Physics.IgnoreCollision(ourCollider, ammoCollider, true);
                }
            }
        }

        private void OnGrabbableActivated(HVRGrabberBase arg0, HVRGrabbable arg1)
        {
            if (!CanFire())
            {
                OnOutofAmmo();
                return;
            }

            if (FireType == FireType.Single)
            {
                if (_timer < Cooldown)
                    return;

                _timer = 0f;

                OnFired();
                AfterFired();
                return;
            }

            if (FireType == FireType.Automatic)
            {
                _isFiring = true;
                if (_automaticRoutine != null)
                    StopCoroutine(_automaticRoutine);
                _automaticRoutine = StartCoroutine(Automatic());
                return;
            }

            if (FireType == FireType.ThreeRoundBurst)
            {
                if (_timer < Cooldown)
                    return;

                _timer = 0f;
            }
        }

        protected virtual void OnGrabbableDeactivated(HVRGrabberBase arg0, HVRGrabbable arg1)
        {
            _isFiring = false;
        }

        private IEnumerator Automatic()
        {
            try
            {
                var elapsed = Cooldown + 1f;
                while (_isFiring && CanFire() && Grabbable.IsBeingHeld)
                {
                    if (elapsed > Cooldown)
                    {
                        elapsed = 0f;
                        OnFired();
                        AfterFired();
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (!CanFire())
                {
                    OnOutofAmmo();
                }
            }
            finally
            {
                _isFiring = false;
            }
        }

        protected virtual void OnOutofAmmo()
        {
            if (GunSounds)
            {
                GunSounds.PlayOutOfAmmo();
                return;
            }
        }

        protected virtual void Update()
        {
            _timer += Time.deltaTime;
            //Debug.DrawLine(BulletOrigin.position, BulletOrigin.position + BulletOrigin.forward * Range);
        }

        public virtual void ReleaseAmmo()
        {
            if (!AmmoSocket || !AmmoSocket.IsGrabbing)
            {
                return;
            }

            var releasedAmmo = Ammo;
            Ammo = null;
            var ammoGrabbable = AmmoSocket.GrabbedTarget;
            AmmoSocket.ForceRelease();



            if (ammoGrabbable)
            {
                var direction = -AmmoSocket.transform.up;
                if (AmmoEjectDirection)
                    direction = AmmoEjectDirection.forward;

                if (ammoGrabbable.Rigidbody)
                    ammoGrabbable.Rigidbody.velocity = direction.normalized * AmmoEjectVelocity;

                var hand = Grabbable.PrimaryGrabber as HVRHandGrabber;
                if (hand)
                {
                    hand.HandPhysics.IgnoreCollision(ammoGrabbable.Colliders, true);
                }

                if (!releasedAmmo.HasAmmo && releasedAmmo.DestroyIfEmpty)
                {
                    releasedAmmo.StartDestroy();
                }
                else
                {
                    StartCoroutine(RenablePhysics(ammoGrabbable, hand));
                }
            }
        }

        private IEnumerator RenablePhysics(HVRGrabbable grabbable, HVRHandGrabber hand)
        {
            yield return new WaitForSeconds(2.50f);

            for (var i = 0; i < Grabbable.Colliders.Count; i++)
            {
                var ourCollider = Grabbable.Colliders[i];
                for (var j = 0; j < grabbable.Colliders.Count; j++)
                {
                    var ammoCollider = grabbable.Colliders[j];
                    Physics.IgnoreCollision(ourCollider, ammoCollider, false);
                }
            }

            if (hand)
            {
                hand.HandPhysics.IgnoreCollision(grabbable.Colliders, false);
            }
        }

        protected virtual void Recoil()
        {
            if (RecoilComponent)
            {
                RecoilComponent.Recoil();
                return;
            }
        }

        protected virtual bool CanFire()
        {
            if (RequiresChamberedBullet)
            {
                return IsBulletChambered;
            }

            return !RequiresAmmo || Ammo && Ammo.HasAmmo;
        }

        protected virtual void OnFired()
        {
            IsBulletChambered = false;

            if (GunSounds)
            {
                GunSounds.PlayGunFire();
            }

            Recoil();

            FireBullets(BulletOrigin.forward);

            if (MuzzleFlashObject)
                StartCoroutine(MuzzleFlash());

            Smoke();

            Fired.Invoke();
        }

        protected virtual void FireBullets(Vector3 direction)
        {
            FireBullet(direction);
        }

        protected virtual void FireBullet(Vector3 direction)
        {
            Vector3 hitLocation;

            if (Physics.Raycast(BulletOrigin.position, direction, out var hit, BulletRange, HitMask, QueryTriggerInteraction.Ignore))
            {
                OnHit(hit, BulletOrigin.forward);
                hitLocation = hit.point;
            }
            else
            {
                hitLocation = BulletOrigin.position + direction * BulletRange;
            }

            if (BulletTrailSpeed > 0f)
            {
                if (BulletTrailPrefab)
                    StartCoroutine(FireBullet(BulletOrigin.position, hitLocation));
            }
        }

        protected virtual void AfterFired()
        {
            IsBulletChambered = false;
            if (RequiresChamberedBullet)
            {
                if (ChambersAfterFiring)
                {
                    ChamberRound();
                }
            }
            else if (RequiresAmmo && Ammo)
            {
                Ammo.RemoveBullet();
            }
        }

        private IEnumerator MuzzleFlash()
        {
            MuzzleFlashObject.SetActive(false);/// ADDED to cancel longer fx like smoke to allow flame fx to fire again.
            MuzzleFlashObject.SetActive(true);
            var elapsed = 0f;



            while (elapsed < MuzzleFlashTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            MuzzleFlashObject.SetActive(false);
        }

        protected virtual void Smoke()
        {
            if (MuzzleSmoke)
            {
                var muzzleSmoke = Instantiate(MuzzleSmoke, MuzzleSmoke.transform.position, MuzzleSmoke.transform.rotation);
                muzzleSmoke.SetActive(true);
                Destroy(muzzleSmoke, MuzzleSmokeTime);
            }
        }

        private IEnumerator FireBullet(Vector3 start, Vector3 destination)
        {
            var direction = destination - start;
            var bullet = Instantiate(BulletTrailPrefab, BulletOrigin.position, Quaternion.identity);

            bullet.transform.rotation = Quaternion.FromToRotation(bullet.transform.forward, direction);

            if (!bullet.activeSelf)
                bullet.SetActive(true);

            var elapsed = 0f;
            var distance = Vector3.Distance(start, destination);
            var velocity = BulletTrailSpeed * Time.deltaTime;
            var time = distance / BulletTrailSpeed;

            while (elapsed < time)
            {
                bullet.transform.position += velocity * direction.normalized;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(bullet);
        }

        protected virtual void OnHit(RaycastHit hit, Vector3 direction)
        {
            var damageHandler = hit.collider.GetComponent<HVRDamageHandlerBase>();
            if (damageHandler)
            {
                damageHandler.HandleDamageProvider(DamageProvider, hit.point, direction);
            }

            Hit.Invoke(damageHandler);
        }

        public virtual void EjectBullet()
        {

        }

        public virtual void EjectCasing()
        {
        }
    }

    public class GunHitEvent : UnityEvent<HVRDamageHandlerBase>
    {

    }

    public enum FireType
    {
        Single,
        ThreeRoundBurst,
        Automatic
    }
}
