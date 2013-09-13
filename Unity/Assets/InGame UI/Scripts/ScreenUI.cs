using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenUI : MonoBehaviour
{
    // Member Variables
    public RenderTexture m_RenderTex;
    public Camera m_RenderCamera;
    public GameObject m_UI;

    private ScreenEditor m_ScreenEditor;

    // Member Methods
    void Start()
    {
        m_ScreenEditor = GetComponent<ScreenEditor>();

        float ppm = 0.0f;
        switch (m_ScreenEditor.m_Quality)
        {
            case ScreenEditor.Quality.Excelent:
                ppm = 1024;
                break;
            case ScreenEditor.Quality.Good:
                ppm = 512;
                break;
            case ScreenEditor.Quality.Average:
                ppm = 256;
                break;
            case ScreenEditor.Quality.Bad:
                ppm = 128;
                break;
            default:
                break;
        }

        int rtWidth = (int)(m_ScreenEditor.m_Width * ppm);
        int rtHeight = (int)(m_ScreenEditor.m_Height * ppm);

        // Create a new render texture
        m_RenderTex = new RenderTexture(rtWidth, rtHeight, 16);
        m_RenderTex.name = name + " RT";
        m_RenderTex.Create();

        // Get the render camera and set its target as the render texture
        m_RenderCamera = GetComponentInChildren<Camera>();
        m_RenderCamera.targetTexture = m_RenderTex;

        // Set it onto the material
        renderer.sharedMaterial.SetTexture("_MainTex", m_RenderTex);

        // Disable the Screen Editor
        m_ScreenEditor.enabled = false;

        SetupHardcodedUI();
    }

    void Update()
    {
        m_RenderTex.DiscardContents(true, true);
        RenderTexture.active = m_RenderTex;

        m_RenderCamera.Render();

        RenderTexture.active = null;
    }

    void SetupHardcodedUI()
    {
        ScreenEditor sE = GetComponent<ScreenEditor>();

        m_UI = new GameObject();
        m_UI.name = transform.name + "_UI";
        m_UI.transform.parent = transform;
        m_UI.transform.localPosition = Vector3.zero;
        m_UI.transform.localRotation = Quaternion.identity;
        m_UI.layer = LayerMask.NameToLayer("UI");

        GameObject Button1 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton.prefab", typeof(GameObject)));
        Button1.transform.parent = m_UI.transform;
        Button1.transform.localPosition = new Vector3(0.0f, sE.m_Height * 0.5f - Button1.GetComponent<ButtonEditor>().m_ButtonHeight * 0.45f);
        Button1.transform.localRotation = Quaternion.identity;
        Button1.GetComponentInChildren<TextMesh>().text = "Bounce";

        GameObject Button2 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton.prefab", typeof(GameObject)));
        Button2.transform.parent = m_UI.transform;
        Button2.transform.localPosition = new Vector3(0.0f, -sE.m_Height * 0.5f + Button1.GetComponent<ButtonEditor>().m_ButtonHeight * 0.45f);
        Button2.transform.localRotation = Quaternion.identity;
        Button2.GetComponentInChildren<TextMesh>().text = "Stop";

        // Add some color buttons
        GameObject cube = null;

        GameObject Button3 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton1.prefab", typeof(GameObject)));
        Button3.transform.parent = m_UI.transform;
        Button3.transform.localPosition = new Vector3(-sE.m_Width * 0.35f, -sE.m_Height * 0.3f);
        Button3.transform.localRotation = Quaternion.identity;
        cube = Button3.transform.GetChild(0).gameObject;
        cube.renderer.material.color = Color.red;
        cube.rigidbody.AddTorque(new Vector3(1.0f, -1.0f, 0.0f).normalized * 0.1f);

        GameObject Button4 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton1.prefab", typeof(GameObject)));
        Button4.transform.parent = m_UI.transform;
        Button4.transform.localPosition = new Vector3(sE.m_Width * 0.35f, -sE.m_Height * 0.3f);
        Button4.transform.localRotation = Quaternion.identity;
        cube = Button4.transform.GetChild(0).gameObject;
        cube.renderer.material.color = Color.green;
        cube.rigidbody.AddTorque(new Vector3(-1.0f, 0.0f, 1.0f).normalized * 0.1f);

        GameObject Button5 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton1.prefab", typeof(GameObject)));
        Button5.transform.parent = m_UI.transform;
        Button5.transform.localPosition = new Vector3(sE.m_Width * 0.35f, sE.m_Height * 0.3f);
        Button5.transform.localRotation = Quaternion.identity;
        cube = Button5.transform.GetChild(0).gameObject;
        cube.renderer.material.color = Color.blue;
        cube.rigidbody.AddTorque(new Vector3(1.0f, 1.0f, -1.0f).normalized * 0.1f);

        GameObject Button6 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton1.prefab", typeof(GameObject)));
        Button6.transform.parent = m_UI.transform;
        Button6.transform.localPosition = new Vector3(-sE.m_Width * 0.35f, sE.m_Height * 0.3f);
        Button6.transform.localRotation = Quaternion.identity;
        cube = Button6.transform.GetChild(0).gameObject;
        cube.renderer.material.color = Color.yellow;
        cube.rigidbody.AddTorque(new Vector3(-1.0f, -1.0f, 0.0f).normalized * 0.1f);
    }

    public void CheckButtonCollision(RaycastHit _rh)
    {
        Ray ray = new Ray(_rh.point + transform.forward * -1.0f, transform.forward);
		RaycastHit hit;
        float rayLength = 2.0f;

        if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer("UI")))
        {
            ButtonUI bUI = hit.transform.parent.gameObject.GetComponent<ButtonUI>();
            if (bUI)
            {
                Debug.Log("Button Hit: " + hit.transform.parent.name);
                bUI.ButtonActivated();
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
