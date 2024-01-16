using UnityEngine;

namespace HurricaneVR.Framework.Core.Utils
{
    public class ItemResetter : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        public Transform ResetPoint;

        // Update is called once per frame
        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                transform.position = ResetPoint.position;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }
}
