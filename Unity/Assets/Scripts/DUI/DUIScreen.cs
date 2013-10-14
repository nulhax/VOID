using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DUIScreen : MonoBehaviour
{
    // Member Variables 
    public TextAsset m_DUIXML;

    public DUI m_DUI                    { get; set; }

    private GameObject m_DUIGO;

    // Member Methods
    void Start()
    {
        SetupDUI();
    }

    void SetupDUI()
    {
        // Create the DUI game object
        m_DUIGO = new GameObject();
        m_DUIGO.name = transform.parent.name + "_DUI";
        m_DUIGO.transform.position = transform.position + new Vector3(0.0f, 0.0f, -2.0f);
        m_DUIGO.transform.localRotation = Quaternion.identity;
        m_DUIGO.layer = LayerMask.NameToLayer("DUI");

        // Add the UI script to the game object.
        m_DUI = m_DUIGO.AddComponent<DUI>();
    }

    public void CheckUICollisions(RaycastHit _rh)
    {
        m_DUI.CheckButtonCollisions(_rh);
    }
}
