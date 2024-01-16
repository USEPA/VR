using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.HurricaneVR.Framework.Shared.Utilities
{
    public static class CoroutineExtensions
    {
        public static Coroutine ExecuteNextUpdate(this MonoBehaviour behaviour, Action routine)
        {
            return behaviour.StartCoroutine(ExecuteNextUpdate(routine));
        }

        public static Coroutine ExecuteAfterSeconds(this MonoBehaviour behaviour, Action routine, float seconds)
        {
            return behaviour.StartCoroutine(ExecuteAfterSeconds(routine, seconds));
        }

        public static Coroutine ExecuteAfterSecondsUnscaled(this MonoBehaviour behaviour, Action routine, float seconds)
        {
            return behaviour.StartCoroutine(ExecuteAfterSecondsUnscaled(routine, seconds));
        }

        public static Coroutine ExecuteNextFixedUpdate(this MonoBehaviour behaviour, Action routine)
        {
            return behaviour.StartCoroutine(ExecuteNextFixedUpdate(routine));
        }

        public static Coroutine ExecuteAfterFixedUpdates(this MonoBehaviour behaviour, Action routine, int frames)
        {
            return behaviour.StartCoroutine(ExecuteAfterFixedUpdates(routine, frames));
        }

        private static IEnumerator ExecuteAfterSeconds(Action action, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

        private static IEnumerator ExecuteAfterSecondsUnscaled(Action action, float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action();
        }

        private static IEnumerator ExecuteNextUpdate(Action action)
        {
            yield return null;

            action();
        }

        private static IEnumerator ExecuteNextFixedUpdate(Action action)
        {
            yield return new WaitForFixedUpdate();
            action();
        }

        private static IEnumerator ExecuteAfterFixedUpdates(Action action, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            action();
        }

    }
}
