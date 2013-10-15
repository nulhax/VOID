using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;

public class DUIConsole : MonoBehaviour 
{
    // Member Variables 
    public TextAsset m_consoleXML;
    public GameObject m_screenObject;

    private Material m_screenMat;
    private DUIMainView m_DUI;

    private void Start()
    {
        // Save the material of the screen
        m_screenMat = m_screenObject.renderer.sharedMaterial;

        // Set up the Diegetic User interface Object
        SetupDUI();
    }

    private void Update()
    {
        // Check for resetting the UI
        if (Input.GetKeyUp(KeyCode.F1))
        {
            // Release the render texture
            ((RenderTexture)m_screenMat.GetTexture("_MainTex")).Release();

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
        duiGo.name = name + "_DUI";
        duiGo.layer = LayerMask.NameToLayer("DUI");
        duiGo.transform.position = transform.position + new Vector3(0.0f, 0.0f, -2.0f);
        duiGo.transform.localRotation = Quaternion.identity;

        // Add the DUI component
        m_DUI = duiGo.AddComponent<DUIMainView>();

        // Initialise the DUI Component
        m_DUI.Initialise(m_consoleXML, m_screenMat);
    }

    public void CheckDUICollisions(RaycastHit _rh)
    {
        m_DUI.CheckButtonCollisions(_rh);
    }
}
