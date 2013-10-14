using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Reflection;

public enum EScreenQuality
{
    Excelent,
    Good,
    Average,
    Bad,
}

public class DUI : MonoBehaviour
{
    private EScreenQuality m_DUIQuality;
    private XmlNode m_xmlDUI;

    private DUIView m_mainView;
    private Camera m_renderCamera;
    private RenderTexture m_renderTex;

    public void Initialise(XmlDocument _xDoc, Material _sharedScreenMat)
    {
        m_xmlDUI = _xDoc.SelectSingleNode("dui");

        // Get the quality
        m_DUIQuality = (EScreenQuality)System.Enum.Parse(typeof(EScreenQuality), m_xmlDUI.Attributes["quality"].Value);

        // Create the main view
        m_mainView = CreateView(m_xmlDUI.SelectSingleNode("mainview"));

        // Setup the render texture and camera
        SetupRenderTex(_sharedScreenMat);
        SetupRenderCamera();
    }
    private void Update()
    {
        // Update the render texture and camera
        m_renderTex.DiscardContents(true, true);
        RenderTexture.active = m_renderTex;

        m_renderCamera.Render();

        RenderTexture.active = null;
    }

    private void SetupRenderTex(Material _sharedScreenMat)
    {
        // Figure out the pixels per meter for the screen based on quality setting
        float ppm = 0.0f;
        switch (m_DUIQuality)
        {
            case EScreenQuality.Excelent:
                ppm = 1024;
                break;
            case EScreenQuality.Good:
                ppm = 512;
                break;
            case EScreenQuality.Average:
                ppm = 256;
                break;
            case EScreenQuality.Bad:
                ppm = 128;
                break;
            default:
                break;
        }

        int width = (int)(m_mainView.m_width * ppm);
        int height = (int)(m_mainView.m_height * ppm);

        // Create a new render texture
        m_renderTex = new RenderTexture(width, height, 16);
        m_renderTex.name = name + " RT";
        m_renderTex.Create();

        // Set it onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_renderTex);
    }
    private void SetupRenderCamera()
    {
        // Create the camera game object
        GameObject go = new GameObject();
        go.name = transform.name + "_RenderCamera";
        go.transform.parent = transform;
        go.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
        go.transform.localRotation = Quaternion.identity;
        go.layer = LayerMask.NameToLayer("DUI");

        // Get the render camera and set its target as the render texture
        m_renderCamera = go.AddComponent<Camera>();
        m_renderCamera.cullingMask = 1 << LayerMask.NameToLayer("DUI");
        m_renderCamera.orthographic = true;
        m_renderCamera.backgroundColor = Color.black;
        m_renderCamera.nearClipPlane = 0.0f;
        m_renderCamera.farClipPlane = 2.0f;
        m_renderCamera.targetTexture = m_renderTex;
        m_renderCamera.orthographicSize = m_mainView.m_height * 0.5f;
    }

    private DUIView CreateView(XmlNode _xView)
    {
        // Create the view game object
        GameObject viewGo = new GameObject();
        viewGo.name = transform.name + "_MainView";
        viewGo.layer = LayerMask.NameToLayer("DUI");
        viewGo.transform.parent = transform;
        viewGo.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        viewGo.transform.localRotation = Quaternion.identity;

        // Add the DUIView component and initialise
        DUIView duiView = viewGo.AddComponent<DUIView>();
        duiView.Initialise(_xView);
        
        return (duiView);
    }

    public void OpenView()
    {

    }

    public void CheckButtonCollisions(RaycastHit _rh)
    {
        Vector3 offset = new Vector3(_rh.textureCoord.x * m_mainView.m_width - m_mainView.m_width * 0.5f,
                                     _rh.textureCoord.y * m_mainView.m_height - m_mainView.m_height * 0.5f,
                                             0.0f);

        offset = transform.rotation * offset;
        Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;

        Ray ray = new Ray(rayOrigin, transform.forward);
        RaycastHit hit;
        float rayLength = 2.0f;

        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("DUI")))
        {
            DUIButton bUI = hit.transform.parent.gameObject.GetComponent<DUIButton>();
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
    public void ReleaseRenderTex()
    {
        m_renderTex.Release();
    }
}
