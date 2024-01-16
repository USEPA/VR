using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class GameObjectHelper
    {
        public static string GetNameIfAvailable(GameObject _object)
        {
            return (_object != null) ? _object.name : "N/A";
        }

        /// <summary>
        /// Gets a random point on a collider using a raycast from a random direction
        /// </summary>
        /// <param name="_col"></param>
        /// <param name="_length"></param>
        /// <returns></returns>
        public static Vector3 GetRandomPointOnMesh(Collider _col, float _length = 100.0f)
        {
            Vector3 direction = Random.onUnitSphere;
            Ray ray = new Ray(_col.transform.position + (direction * _length), -direction);
            RaycastHit hit;
            Vector3 randomPoint = _col.transform.position;
            if (_col.Raycast(ray, out hit, _length))
            {
                randomPoint = hit.point;
            }
            return randomPoint;
        }


        public static Vector3 GetPointOnGround(Vector3 _sourcePosition)
        {
            RaycastHit hit;
            Ray ray = new Ray(_sourcePosition, Vector3.down);

            if (Physics.Raycast(ray, out hit, 10.0f, LayerMask.GetMask("Teleport")))
            {
                // Return the hit point
                return hit.point;
            }
            return _sourcePosition;
        }
    }
}

