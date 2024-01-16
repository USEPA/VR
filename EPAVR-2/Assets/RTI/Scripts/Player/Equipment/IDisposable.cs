using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public interface IDisposable
    {
        #region Public Properties
        public XRGrabInteractable Interactable { get; }
        public System.Action OnDisposed { get; set; }
        #endregion

        public void Dispose();

    }
}

