using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class L58TeleportationArea : TeleportationArea
    {

        private int _teleportDirection;

        private const float ROTATION_STEP = 45f;
        TeleportRequest m_teleportRequest;

        public void Activate(ActivateEventArgs args, int direction)
        {
            //Debug.Log($"{Time.time} Teleport Activated");
            _teleportDirection = direction;
            OnActivated(args);
            /*
            if (args.interactor is XRRayInteractor rayInteractor)
            {
                rayInteractor.
            }
            */
        }

        public void Activate(ActivateEventArgs args)
        {
            //UnityEngine.Debug.Log($"Activated teleportation: {gameObject.name} | Interactor: {args.interactor.gameObject.name} || Time: {Time.time}");

            /*if (args.interactor is XRRayInteractor rayInteractor && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitInfo))
            {
                m_teleportRequest = new TeleportRequest();
                m_teleportRequest.destinationPosition = hitInfo.point;
                m_teleportRequest.matchOrientation = matchOrientation;

                if (GenerateTeleportRequest(rayInteractor, hitInfo, ref m_teleportRequest))
                {
                    UnityEngine.Debug.Log($"Generated valid teleport request: {gameObject.name} | Position: {m_teleportRequest.destinationPosition.ToString()} || Time: {Time.time}");
                    if (teleportationProvider.QueueTeleportRequest(m_teleportRequest))
                    {
                        UnityEngine.Debug.Log($"Successfully queued teleport request: {m_teleportRequest.destinationPosition.ToString()} || Time: {Time.time}");
                    }
                }
            }*/

            OnActivated(args);
        }

        protected override bool GenerateTeleportRequest(XRBaseInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
        {
            return base.GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest);
            /*
            Debug.Log($"{Time.time} Rotation {_teleportDirection * ROTATION_STEP}");
            teleportRequest.destinationPosition = raycastHit.point;
            teleportRequest.destinationRotation = Quaternion.Euler(0, _teleportDirection * ROTATION_STEP, 0);
            return true;
            */
        }

        public void SetCurrentTeleportTarget(HoverEnterEventArgs args)
        {
            VRUserManager.Instance?.Player?.SetTeleportTarget(this);
            //UnityEngine.Debug.Log($"Set teleport target: {gameObject.name} | Interactor: {args.interactor} || Time: {Time.time}");
        }

        public void ClearCurrentTeleportTarget(HoverExitEventArgs args)
        {
            VRUserManager.Instance?.Player?.ClearTeleportTarget();
            //UnityEngine.Debug.Log($"Cleared teleport target: {gameObject.name} | Interactor: {args.interactor} || Time: {Time.time}");
        }
    }
}

