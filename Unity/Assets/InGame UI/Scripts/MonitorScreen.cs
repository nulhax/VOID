using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonitorScreen : MonoBehaviour
{
    public enum Quality
    {
        Excelent,
        Good,
        Average,
        Bad,
    }

    // Member Variables 
    public TextAsset m_UIXML;
    public Quality m_Quality            = Quality.Good;
    public float m_Width                = 1.0f;
    public float m_Height               = 0.5f;

    public Camera m_RenderCamera        { get; set; }
    public RenderTexture m_RenderTex    { get; set; }
    public GameObject m_UI              { get; set; }

    // Member Methods
    void Start()
    {
        SetupRenderTex();

        SetupScreenCamera();

        SetupUI();
    }

    void Update()
    {
        m_RenderTex.DiscardContents(true, true);
        RenderTexture.active = m_RenderTex;

        m_RenderCamera.Render();

        RenderTexture.active = null;
    }

    void SetupRenderTex()
    {
        float ppm = 0.0f;
        switch (m_Quality)
        {
            case Quality.Excelent:
                ppm = 1024;
                break;
            case Quality.Good:
                ppm = 512;
                break;
            case Quality.Average:
                ppm = 256;
                break;
            case Quality.Bad:
                ppm = 128;
                break;
            default:
                break;
        }

        int rtWidth = (int)(m_Width * ppm);
        int rtHeight = (int)(m_Height * ppm);

        // Create a new render texture
        m_RenderTex = new RenderTexture(rtWidth, rtHeight, 16);
        m_RenderTex.name = name + " RT";
        m_RenderTex.Create();

        // Set it onto the material of the screen
        renderer.sharedMaterial.SetTexture("_MainTex", m_RenderTex);
    }

    void SetupScreenCamera()
    {
        // Get the render camera and set its target as the render texture
        m_RenderCamera = GetComponentInChildren<Camera>();
        m_RenderCamera.targetTexture = m_RenderTex;

        // Reset the camera ortho size
        m_RenderCamera.orthographicSize = m_Height * 0.5f;
    }

    void SetupUI()
    {
        // Create the UI game object
        m_UI = new GameObject();
        m_UI.name = transform.name + "_UI";
        m_UI.transform.parent = transform;
        m_UI.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);
        m_UI.transform.localRotation = Quaternion.identity;
        m_UI.layer = LayerMask.NameToLayer("UI");
        
        // Add the UI script to the game object.
        m_UI.AddComponent<DiegeticUI>();
    }

    public void CheckButtonCollision(RaycastHit _rh)
    {
        MonitorScreen sUI = GetComponent<MonitorScreen>();
        Vector3 offset = new Vector3(_rh.textureCoord.x * sUI.m_Width - sUI.m_Width * 0.5f,
                                     _rh.textureCoord.y * sUI.m_Height - sUI.m_Height * 0.5f,
                                     0.0f);

        offset = transform.rotation * offset;
        Vector3 rayOrigin = m_UI.transform.position + offset + transform.forward * -1.0f;

        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;
        float rayLength = 2.0f;

        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("UI")))
        {
            ButtonUI bUI = hit.transform.parent.gameObject.GetComponent<ButtonUI>();
            if (bUI)
            {
                Debug.Log("Button Hit: " + hit.transform.parent.name);
                bUI.ButtonPressed();
            }
            else
            {
                Debug.Log("Button Hit: " + hit.transform.name);
            }

            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.green, 0.5f);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayLength, Color.red, 0.5f);
        }
    }
}
