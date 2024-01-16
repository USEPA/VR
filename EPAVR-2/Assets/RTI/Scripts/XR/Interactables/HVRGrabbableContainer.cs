using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;

public class HVRGrabbableContainer : MonoBehaviour
{
    #region Protected Variables
    public List<Collider> m_colliders;

    protected List<HVRGrabbable> m_grabbableItems;
    protected List<Collider> m_ignoredColliders;
    #endregion

    #region Initialization
    public virtual void Init()
    {
        // Initialize collider list
        if (m_colliders == null) m_colliders = new List<Collider>();
        if (TryGetComponent<Collider>(out Collider collider)) m_colliders.Add(collider);
        var additionalColliders = GetComponentsInChildren<Collider>().ToList();
        foreach(Collider col in additionalColliders)
        {
            if (col.isTrigger || col.GetComponent<HVRGrabbable>() != null)
            {
                continue;
            }
            m_colliders.Add(col);
        }
        // Check if grabbables should be auto-populated
        if (m_grabbableItems == null || m_grabbableItems.Count < 1)
        {
            m_grabbableItems = GetComponentsInChildren<HVRGrabbable>().ToList();
        }
        // Check if there are grabbables
        if (m_grabbableItems != null && m_grabbableItems.Count > 0)
        {
            // Loop through each pre-registered grabbable and ignore its collisions
            foreach (HVRGrabbable grabbable in m_grabbableItems) IgnoreCollision(grabbable);
        }
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        // Initialize ignored collisions
        //Init();
    }

    /// <summary>
    /// Ignore all collisions between the container and a specified grabbable's colliders
    /// </summary>
    /// <param name="grabbable"></param>
    public void IgnoreCollision(HVRGrabbable grabbable)
    {
        for (var i = 0; i < grabbable.Colliders.Count; i++)
        {
            var c = grabbable.Colliders[i];
            if (!c)
                continue;

            for (var j = 0; j < m_colliders.Count; j++)
            {
                var ourCollider = m_colliders[j];
                if (ourCollider.isTrigger)
                    continue;
                if (m_ignoredColliders == null) m_ignoredColliders = new List<Collider>();
                m_ignoredColliders.Add(c);
                Physics.IgnoreCollision(c, ourCollider);
            }
        }
    }
}
