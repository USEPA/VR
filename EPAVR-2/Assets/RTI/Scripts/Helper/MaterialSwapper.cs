using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    #region Inspector Assigned Variables
    [Header("Important References")]
    [SerializeField] private Material m_inactiveMaterial;
    [SerializeField] private Material m_activeMaterial;
    [Header("Default Configuration")]
    [SerializeField] private bool m_startActive = false;
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
        SetActive(m_startActive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActive(bool _value)
    {
        m_isActive = _value;
        SetMaterial((_value) ? m_activeMaterial : m_inactiveMaterial);
    }

    #region Helper Methods
    public void SetMaterial(Material _material)
    {
        Mesh.material = _material;
    }
    #endregion
}
