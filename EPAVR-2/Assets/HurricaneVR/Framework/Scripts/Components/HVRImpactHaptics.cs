
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HurricaneVR.Framework.Components
{

    [CreateAssetMenu(menuName = "HurricaneVR/Impact Haptics", fileName = "ImpactHaptics")]
    public class HVRImpactHaptics : ScriptableObject
    {
        public float Timeout = .3f;
        public float SqrMagThreshold = .30f;
        public float MaxForce = 600f;
        public AnimationCurve AmpCurve;
        public float Duration = .25f;
        public float Frequency = 150f;

        public void Reset()
        {
            Frequency = 150f;
            Duration = .25f;
            MaxForce = 600f;
            SqrMagThreshold = .30f;
            Timeout = .3f;

            AmpCurve = new AnimationCurve(new Keyframe(0f, .35f, 0f, 0f, 0f, 0f)
            {
                weightedMode = WeightedMode.None
            }, new Keyframe(1f, 1f, 2f, 2f, 0f, 0f)
            {
                weightedMode = WeightedMode.None
            });
        }

#if UNITY_EDITOR
        //unity bugged Reset conext menu on some versions
        [MenuItem("CONTEXT/ImpactHaptics/Reset (Custom)")]
        static void CReset(MenuCommand command)
        {
            (command.context as HVRImpactHaptics)?.Reset();
        }

#endif
    }
}