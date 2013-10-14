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
    private TextAsset m_DUIXML;

    private float m_MainViewWidth;
    private float m_MainViewHeight;
    private EScreenQuality m_ScreenQuality;

    private Camera m_RenderCamera;
    private RenderTexture m_RenderTex;

    Vector3 StringToVector3(string rString)
    {
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        var temp = rString.Substring(0, rString.Length).Split(',');
        if (temp.Length > 0)
            x = float.Parse(temp[0]);
        if (temp.Length > 1)
            y = float.Parse(temp[1]);
        if (temp.Length > 2)
            z = float.Parse(temp[2]);
        Vector3 rValue = new Vector3(x, y, z);

        return rValue;
    }

    void Start()
    {
        // Get the component responsibile for creating this script
        string screenName = name.Substring(0, name.Length - 4) + "_Screen";
        DUIScreen duiScreen = GameObject.Find(screenName).GetComponent<DUIScreen>();

        // Get the initial variables from the screen
        m_DUIXML = duiScreen.m_DUIXML;

        SetupDUIXML();
        SetupRenderTex(duiScreen.renderer.sharedMaterial);
        SetupDUICamera();
    }

    void Update()
    {
        // Update the render texture
        m_RenderTex.DiscardContents(true, true);
        RenderTexture.active = m_RenderTex;

        m_RenderCamera.Render();

        RenderTexture.active = null;

        // Check for reseting the UI
        CheckResetUI();
    }

    void SetupDUIXML()
    {
        // Load the XML reader and document for parsing information
        XmlDocument xDoc = new XmlDocument();
        XmlTextReader xReader = new XmlTextReader(new StringReader(m_DUIXML.text));
        xDoc.Load(xReader);

        // Initialise the default values
        XmlNode xUI = xDoc.SelectSingleNode("ui");
        m_MainViewWidth = float.Parse(xUI.Attributes["width"].Value);
        m_MainViewHeight = float.Parse(xUI.Attributes["height"].Value);
        m_ScreenQuality = (EScreenQuality)System.Enum.Parse(typeof(EScreenQuality), xUI.Attributes["quality"].Value); 

        // Create the buttons
        foreach (XmlNode button in xUI.SelectSingleNode("mainview").SelectNodes("button"))
        {
            CreateButton(button, gameObject);
        }
    }
    void SetupRenderTex(Material _sharedScreenMat)
    {
        // Figure out the pixels per meter for the screen based on quality setting
        float ppm = 0.0f;
        switch (m_ScreenQuality)
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

        int width = (int)(m_MainViewWidth * ppm);
        int height = (int)(m_MainViewHeight * ppm);

        // Create a new render texture
        m_RenderTex = new RenderTexture(width, height, 16);
        m_RenderTex.name = name + " RT";
        m_RenderTex.Create();

        // Set it onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_RenderTex);
    }
    void SetupDUICamera()
    {
        // Create the camera game object
        GameObject go = new GameObject();
        go.name = transform.name + "_RenderCamera";
        go.transform.parent = transform;
        go.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
        go.transform.localRotation = Quaternion.identity;
        go.layer = LayerMask.NameToLayer("DUI");

        // Get the render camera and set its target as the render texture
        m_RenderCamera = go.AddComponent<Camera>();
        m_RenderCamera.cullingMask = 1 << LayerMask.NameToLayer("DUI");
        m_RenderCamera.orthographic = true;
        m_RenderCamera.backgroundColor = Color.black;
        m_RenderCamera.nearClipPlane = 0.0f;
        m_RenderCamera.farClipPlane = 2.0f;
        m_RenderCamera.targetTexture = m_RenderTex;
        m_RenderCamera.orthographicSize = m_MainViewHeight * 0.5f;
    }

    void CreateButton(XmlNode _xButton, GameObject _partentWindow)
    {
        // Create the button game object
        string asset = "Assets/Resources/Prefabs/DUI/Buttons/" + _xButton.Attributes["type"].Value + ".prefab";
        GameObject buttonGo = (GameObject)Instantiate(Resources.LoadAssetAtPath(asset, typeof(GameObject)));

        // Set the default values
        buttonGo.transform.parent = _partentWindow.transform;
        buttonGo.transform.localPosition = Vector3.zero;
        buttonGo.transform.localRotation = Quaternion.identity;

        // Set the position
        if (_xButton.Attributes["pos"] != null)
        {
            Vector3 pos = StringToVector3(_xButton.Attributes["pos"].Value);
            buttonGo.transform.localPosition = new Vector3(pos.x * m_MainViewWidth - (m_MainViewWidth * 0.5f), pos.y * m_MainViewHeight - (m_MainViewHeight * 0.5f));
        }

        // Set the text
        if (_xButton.Attributes["text"] != null)
            buttonGo.GetComponentInChildren<TextMesh>().text = _xButton.Attributes["text"].Value;

        foreach (XmlNode xEvent in _xButton.SelectNodes("event"))
        {
            string eventName = string.Empty;

            if (xEvent.Attributes["name"] != null)
                eventName = xEvent.Attributes["name"].Value;

            foreach (XmlNode xAction in xEvent.SelectNodes("action"))
            {
                string targetName = string.Empty;
                string componentName = string.Empty;
                string actionName = string.Empty;

                if (xAction.Attributes["target"] != null)
                    targetName = xAction.Attributes["target"].Value;

                if (xAction.Attributes["component"] != null)
                    componentName = xAction.Attributes["component"].Value;

                if (xAction.Attributes["method"] != null)
                    actionName = xAction.Attributes["method"].Value;

                // Find the game object target
                GameObject targetGo = null;
                if (targetName == "::ui")
                    targetGo = gameObject;
                else if (targetName == "::self")
                    targetGo = buttonGo;
                else
                    targetGo = GameObject.Find(targetName);

                // Find the component
                Component component = targetGo.GetComponent(componentName);
                System.Type type = System.Type.GetType(componentName);

                // Find the method
                MethodInfo mi = type.GetMethod(actionName);

                // Find and Register the action on the target
                EventInfo ei = typeof(DUIButton).GetEvent("m_" + eventName);
                ei.AddEventHandler(buttonGo.GetComponent<DUIButton>(), System.Delegate.CreateDelegate(typeof(System.Action), component, mi));
            }
        }
    }

    void CheckResetUI()
    {
        // Check for resetting the UI
        if(Input.GetKeyUp(KeyCode.F1))
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // Release the render texture
            m_RenderTex.Release();

            // Call start to reset
            Start();
        }
    }

    public void CheckButtonCollisions(RaycastHit _rh)
    {
        Vector3 offset = new Vector3(_rh.textureCoord.x * m_MainViewWidth - m_MainViewWidth * 0.5f,
                                     _rh.textureCoord.y * m_MainViewHeight - m_MainViewHeight * 0.5f,
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
}
