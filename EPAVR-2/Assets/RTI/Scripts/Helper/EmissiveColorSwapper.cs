using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class EmissiveColorSwapper : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Color m_inactiveColor;
        [SerializeField] private Color m_activeColor;
        [Header("Default Configuration")]
        [SerializeField] private bool m_startActive = false;
        [SerializeField] private bool m_setBaseColorToBlack = false;
        #endregion
        #region Protected Variables
        protected MeshRenderer m_mesh;
        protected bool m_isActive = false;
        #endregion
        #region Public Properties
        public MeshRenderer Mesh
        {
            get
            {
                if (!m_mesh) m_mesh = GetComponent<MeshRenderer>();
                return m_mesh;
            }
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            if (m_setBaseColorToBlack) MeshHelper.SetMeshColor(Mesh, Color.black);
            SetColor(m_startActive);
        }

        public void SetColor(bool _value)
        {
            m_isActive = _value;
            MeshHelper.SetMeshEmissionColor(Mesh, (_value) ? m_activeColor : m_inactiveColor);
        }
    }
}

