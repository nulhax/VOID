using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenUI : MonoBehaviour
{
    // Member Variables
    private RenderTexture m_RenderTex;
    private Camera m_RenderCamera;

    public GameObject m_UI;

    // Member Methods
    void Awake()
    {
        // Create a new render texture
        m_RenderTex = new RenderTexture(256, 256, 16);
        m_RenderTex.name = name + " RT";
        m_RenderTex.Create();

        // Set it onto the material
        renderer.sharedMaterial.SetTexture("_MainTex", m_RenderTex);

        // Get the render camera and set its target as the render texture
        m_RenderCamera = GetComponentInChildren<Camera>();
        m_RenderCamera.targetTexture = m_RenderTex;

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
        m_UI.transform.localPosition = new Vector3(0.0f, 0.0f, -0.25f);
        m_UI.layer = LayerMask.NameToLayer("UI");

        GameObject Button1 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton.prefab", typeof(GameObject)));
        Button1.transform.parent = m_UI.transform;
        Button1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Button1.transform.localPosition = new Vector3(0.0f, sE.m_ScreenHeight * 0.5f - Button1.GetComponent<ButtonEditor>().m_ButtonHeight * 0.5f);
        Button1.GetComponent<ButtonEditor>().m_Text = "Bounce";

        GameObject Button2 = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/InGame UI/Buttons/SimpleButton.prefab", typeof(GameObject)));
        Button2.transform.parent = m_UI.transform;
        Button2.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Button2.transform.localPosition = new Vector3(0.0f, -sE.m_ScreenHeight * 0.5f + Button1.GetComponent<ButtonEditor>().m_ButtonHeight * 0.5f);
        Button2.GetComponent<ButtonEditor>().m_Text = "Stop";
    }

    public void CheckButtonCollision(RaycastHit _rh)
    {
        Ray ray = new Ray(_rh.point + transform.forward * -1.0f, transform.forward);
		RaycastHit hit;

        string rayStuff = "( Origin: ";
        rayStuff += ray.origin.x;
        rayStuff += ", ";
        rayStuff += ray.origin.y;
        rayStuff += ", ";
        rayStuff += ray.origin.z;
        rayStuff += " - Dir: ";
        rayStuff += ray.direction.x;
        rayStuff += ", ";
        rayStuff += ray.direction.y;
        rayStuff += ", ";
        rayStuff += ray.direction.z;
        rayStuff += " )";

        Debug.Log("Ray From Screen:" + rayStuff);
        Debug.DrawRay(ray.origin, ray.direction, Color.red);

        if (Physics.Raycast(ray, out hit, 100.0f, 1 << LayerMask.NameToLayer("UI")))
        {
			Debug.Log(hit.transform.parent.name);

            hit.transform.parent.gameObject.GetComponent<ButtonUI>().ButtonActivated();
        }
    }
}
