using System.Collections;
using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    public class HVRDestroyTimer : MonoBehaviour
    {
        public void StartTimer(float timeout)
        {
            StartCoroutine(Cleanup(timeout));
        }

        private IEnumerator Cleanup(float timeout)
        {
            yield return new WaitForSeconds(timeout);
            Destroy(this.gameObject);
        }
    }
}