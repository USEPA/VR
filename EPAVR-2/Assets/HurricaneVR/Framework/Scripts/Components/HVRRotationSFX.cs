using HurricaneVR.Framework.Core.Utils;
using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    public class HVRRotationSFX : MonoBehaviour
    {
        public HVRRotationTracker Tracker;

        public AudioClip[] SFX;
        public float AngleThreshold = 30f;

        [Header("Debug")]
        public float AngleAccumulated;

        protected virtual void Awake()
        {
            if (Tracker)
            {
                Tracker = GetComponent<HVRRotationTracker>();
            }

            if (Tracker)
            {
                Tracker.AngleChanged.AddListener(OnAngleChanged);
            }
        }

        private void OnAngleChanged(float angle, float delta)
        {
            if (SFX != null && SFX.Length > 0)
            {
                AngleAccumulated += Mathf.Abs(delta);
                if (AngleAccumulated > AngleThreshold)
                {
                    var index = Random.Range(0, SFX.Length);
                    var sfx = SFX[index];
                    AngleAccumulated = 0;
                    PlaySFX(sfx);
                }
            }
        }

        public void Update()
        {

        }

        protected virtual void PlaySFX(AudioClip sfx)
        {
            if(SFXPlayer.Instance) SFXPlayer.Instance.PlaySFX(sfx, transform.position);
        }
    }
}