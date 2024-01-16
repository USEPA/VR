using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class SampleVaporArea : SampleArea
    {
        #region Inspector Assigned Variables
        #endregion
        #region Protected Variables
        #endregion
        #region Public Properties
        public override string TypeID => "VaporArea";
        #endregion

        #region Initialization
        public override void Init()
        {
            // Call base functionality
            base.Init();
            // Make sure there is a valid collider
            if (!m_collider) return;
            // Generate the epicenter of the vapor cloud
            m_epicenter = MathHelper.GetRandomPointWithinCollider(m_collider);
            GameObject epicenterMarker = new GameObject();
            epicenterMarker.transform.position = m_epicenter;
            epicenterMarker.transform.rotation = m_collider.transform.rotation;
            epicenterMarker.name = $"{gameObject.name}_Epicenter";
        }
        #endregion

        protected override void Update()
        {
            // Do nothing
        }
    }
}

