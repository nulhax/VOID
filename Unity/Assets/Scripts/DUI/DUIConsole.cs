using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;

public class DUIConsole : MonoBehaviour 
{
    // Member Variables 
    public TextAsset m_consoleXML;
    public GameObject m_screenObject;

    public DUIMainView m_DUIMV        { get; set; }
    private Material m_screenMat;

    public void Initialise()
    {
        // Save the material of the screen
        m_screenMat = m_screenObject.renderer.sharedMaterial;

        // Set up the Diegetic User interface Object
        SetupDUI();
    }

    public void Deinitialise()
    {
        // Release the render texture
        ((RenderTexture)m_screenMat.GetTexture("_MainTex")).Release();

        // Destroy the game object
        Destroy(m_DUIMV.gameObject);
    }

    private void Update()
    {
        // Check the button collisions
        CheckScreenCollision();
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
        m_DUIMV = duiGo.AddComponent<DUIMainView>();

        // Initialise the DUI Component
        m_DUIMV.Initialise(m_consoleXML);

        // Attatch the render texture
        m_DUIMV.AttatchRenderTexture(m_screenMat);
    }

    private void CheckScreenCollision()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 2.0f, 1 << LayerMask.NameToLayer("Screen")))
            {
                if (hit.transform.gameObject == m_screenObject)
                {
                    m_DUIMV.CheckDGUICollisions(hit);
                }
            }
        }        
    }
}
