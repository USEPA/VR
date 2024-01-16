using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MeshHelper : MonoBehaviour
    {
        public static void SetMeshColor(MeshRenderer _mesh, Color _color)
        {
            _mesh.material.color = _color;
        }

        public static void SetMeshMaterial(MeshRenderer _mesh, Material _material)
        {
            _mesh.material = _material;
        }
        public static void SetMeshEmissionColor(MeshRenderer _mesh, Color _color)
        {
            _mesh.material.SetColor("_EmissionColor", _color);
        }

        public static void SetMeshEmissionColor(MeshRenderer _mesh, Color _color, float _intensity)
        {
            _mesh.material.SetColor("_EmissionColor", _color * _intensity);
        }

        public static float GetMeshBoundsArea(MeshRenderer _mesh)
        {
            return MathHelper.CalculateRectangularPrismArea(_mesh.bounds.size);
        }
    }
}

