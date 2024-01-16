using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;

public class SocketTest : MonoBehaviour
{
    public void Socketed(HVRSocket socket, HVRGrabbable grabbable)
    {
        UnityEngine.Debug.Log($"{gameObject.name} was socketed: {socket.gameObject.name} || Time: {Time.time}");
    }

    public void Unsocketed(HVRSocket socket, HVRGrabbable grabbable)
    {
        UnityEngine.Debug.Log($"{gameObject.name} was unsocketed: {socket.gameObject.name} || Time: {Time.time}");
    }
}
