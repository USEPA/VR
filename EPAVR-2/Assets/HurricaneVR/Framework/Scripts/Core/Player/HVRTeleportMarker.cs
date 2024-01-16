using UnityEngine;

namespace HurricaneVR.Framework.Core.Player
{
    public class HVRTeleportMarker : HVRTeleportMarkerBase
    {
        public GameObject Arrow;
        public GameObject Ring;
        public GameObject Cylinder;

        public bool UseTeleporterColors = true;
        public Color ValidColor;
        public Color InvalidColor;

        private Material RingMaterial;
        private Material ArrowMaterial;
        private Material CylinderMaterial;



        public Color Color
        {
            get
            {
                if (UseTeleporterColors)
                {
                    return IsTeleportValid ? Teleporter.ValidColor : Teleporter.InvalidColor;
                }

                return IsTeleportValid ? ValidColor : InvalidColor;
            }
        }

        public override void Awake()
        {
            base.Awake();

            RingMaterial = Ring?.GetComponent<MeshRenderer>()?.material;
            ArrowMaterial = Arrow?.GetComponent<MeshRenderer>()?.material;
            CylinderMaterial = Cylinder?.GetComponent<MeshRenderer>()?.material;
        }


        protected override void OnActivated()
        {
            Arrow?.SetActive(true);
            Ring?.SetActive(true);
            Cylinder?.SetActive(true);
        }

        protected override void OnDeactivated()
        {
            Arrow?.SetActive(false);
            Ring?.SetActive(false);
            Cylinder?.SetActive(false);
        }

        public override void OnValidTeleportChanged(bool isTeleportValid)
        {
            base.OnValidTeleportChanged(isTeleportValid);

            UpdateMaterials();
        }

        protected virtual void UpdateMaterials()
        {
            if (RingMaterial)
                RingMaterial.color = Color;
            if (ArrowMaterial)
                ArrowMaterial.color = Color;
            if (CylinderMaterial)
                CylinderMaterial.SetColor("_TintColor", Color);
        }
    }
}