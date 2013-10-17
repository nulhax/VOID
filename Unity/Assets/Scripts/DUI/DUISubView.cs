using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class DUISubView : DUIView
{
    public DUIButton m_navButton { get; set; }

    public void Initialise(TextAsset _uiXmlDoc, Vector2 _subViewDimensions)
    {
        // Load the XML file for the UI and save the base node for the ui
        m_uiXmlNode = LoadXML(_uiXmlDoc).SelectSingleNode("subview");

        // Get the window details
        XmlNode windowNode = m_uiXmlNode.SelectSingleNode("window");
        Vector2 windowRatio = StringToVector2(windowNode.Attributes["xyratio"].Value);
        Vector2 subViewRatio = _subViewDimensions;

        // Calc the normalized width/height of the view
        windowRatio /= windowRatio.x > windowRatio.y ? windowRatio.x : windowRatio.y;
        subViewRatio /= subViewRatio.x > subViewRatio.y ? subViewRatio.x : subViewRatio.y;

        if (windowRatio.x > windowRatio.y)
            m_dimensions = windowRatio * (_subViewDimensions.x > _subViewDimensions.y ? _subViewDimensions.x : _subViewDimensions.y);
        else
            m_dimensions = windowRatio * (_subViewDimensions.x > _subViewDimensions.y ? _subViewDimensions.y : _subViewDimensions.x);

        // Create the navigation button
        CreateNavButton();
    }

    private void CreateNavButton()
    {
        XmlNode navButtonNode = m_uiXmlNode.SelectSingleNode("navbutton");

        // Get the data for the nav button
        string prefabPath = "Assets/Resources/Prefabs/DUI/Buttons/" + navButtonNode.Attributes["prefab"].Value + ".prefab";
        string text = navButtonNode.Attributes["text"].Value;

        // Create the game object
        GameObject buttonGo = (GameObject)Instantiate(Resources.LoadAssetAtPath(prefabPath, typeof(GameObject)));

        // Set the default values
        buttonGo.layer = gameObject.layer;
        buttonGo.transform.parent = transform.parent;
        buttonGo.transform.localRotation = Quaternion.identity;

        // Add the DUIbutton
        m_navButton = buttonGo.AddComponent<DUIButton>();

        // Initialise the button
        m_navButton.Initialise();
        m_navButton.m_Text = text;
    }

    private void Update()
    {
        DebugRenderRects();
    }

    // Debug Functions
    private void DebugRenderRects()
    {
        // Test for rendering title, nav and content areas
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;

        start = new Vector3(-(m_dimensions.x * 0.5f), -(m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3((m_dimensions.x * 0.5f), -(m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, Color.green);

        start = new Vector3(-(m_dimensions.x * 0.5f), -(m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(-(m_dimensions.x * 0.5f), (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, Color.green);

        start = new Vector3((m_dimensions.x * 0.5f), (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3((m_dimensions.x * 0.5f), -(m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, Color.green);

        start = new Vector3((m_dimensions.x * 0.5f), (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(-(m_dimensions.x * 0.5f), (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, Color.green);
    }
}
