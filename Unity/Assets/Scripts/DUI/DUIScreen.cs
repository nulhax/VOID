using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class DUIScreen : MonoBehaviour
{
    // Member Variables 
    public TextAsset m_DUIXML;

    private XmlDocument m_xmlDoc;
    private DUI m_DUI;

    // Member Methods
    private void Start()
    {
        // Load the XML reader and document for parsing information
        XmlTextReader xReader = new XmlTextReader(new StringReader(m_DUIXML.text));
        m_xmlDoc = new XmlDocument();
        m_xmlDoc.Load(xReader);

        // Setup the DUI object
        SetupDUI();
    }

    private void Update()
    {
        // Check for resetting the UI
        if (Input.GetKeyUp(KeyCode.F1))
        {
            // Release the render texture
            m_DUI.ReleaseRenderTex();

            // Destroy the game object
            Destroy(m_DUI.gameObject);

            // Call start to reset
            SetupDUI();
        }
    }

    private void SetupDUI()
    {
        // Create the DUI game object
        GameObject duiGo = new GameObject();
        duiGo.name = transform.parent.name + "_DUI";
        duiGo.layer = LayerMask.NameToLayer("DUI");
        duiGo.transform.position = transform.position + new Vector3(0.0f, 0.0f, -2.0f);
        duiGo.transform.localRotation = Quaternion.identity;

        // Add the DUI component and initialise
        m_DUI = duiGo.AddComponent<DUI>();
        m_DUI.Initialise(m_xmlDoc, renderer.sharedMaterial);
    }

    public void CheckDUICollisions(RaycastHit _rh)
    {
        m_DUI.CheckButtonCollisions(_rh);
    }
}
