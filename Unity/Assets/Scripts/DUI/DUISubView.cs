using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class DUISubView : DUIView
{
    // Member Fields


    // Member Properties
    public DUIButton m_navButton { get; set; }


    // Member Methods
    private void Update()
    {
        DebugRenderRects();
    }

    public void Initialise(TextAsset _uiXmlDoc, Vector2 _subViewAreaDimensions)
    {
        // Load the XML file for the UI and save the base node for the ui
        m_uiXmlNode = LoadXML(_uiXmlDoc).SelectSingleNode("subview");

        // Get the window details
        XmlNode windowNode = m_uiXmlNode.SelectSingleNode("window");
        
        // Get the normalised ratio of the subview and the subviewarea
        Vector2 subviewRatio = StringToVector2(windowNode.Attributes["xyratio"].Value);
        Vector2 subViewAreaRatio = _subViewAreaDimensions;
        subviewRatio /= subviewRatio.x > subviewRatio.y ? subviewRatio.x : subviewRatio.y;
        subViewAreaRatio /= subViewAreaRatio.x > subViewAreaRatio.y ? subViewAreaRatio.x : subViewAreaRatio.y;

        Vector2 dimensions = Vector2.zero;
        if (subViewAreaRatio.x == subviewRatio.y || subViewAreaRatio.y == subviewRatio.x)
        {
            dimensions.x = subViewAreaRatio.y * subviewRatio.x * _subViewAreaDimensions.x;
            dimensions.y = subViewAreaRatio.x * subviewRatio.y * _subViewAreaDimensions.y;
        }
        else if (subViewAreaRatio.x == subviewRatio.x || subViewAreaRatio.y == subviewRatio.y)
        {
            if (subViewAreaRatio.y > subviewRatio.y)
            {
                dimensions.x = subViewAreaRatio.x / subviewRatio.x * _subViewAreaDimensions.x;
                dimensions.y = subviewRatio.y / subViewAreaRatio.y * _subViewAreaDimensions.y;
            }
            else
            {
                dimensions.x = subViewAreaRatio.y / subviewRatio.y * _subViewAreaDimensions.x;
                dimensions.y = subViewAreaRatio.x / subviewRatio.x * _subViewAreaDimensions.y;
            }
        }

        m_dimensions = dimensions;

        // Create the navigation button
        CreateNavButton();
    }

    public DUIButton AddButton(string _prefabButton)
    {
        string prefabPath = "Assets/Resources/Prefabs/DUI/Buttons/" + _prefabButton + ".prefab";

        // Create the game object
        GameObject buttonGo = (GameObject)Instantiate(Resources.LoadAssetAtPath(prefabPath, typeof(GameObject)));

        // Set the default values
        buttonGo.layer = gameObject.layer;
        buttonGo.transform.parent = transform;
        buttonGo.transform.localPosition = Vector3.zero;
        buttonGo.transform.localRotation = Quaternion.identity;

        // Add the DUIbutton
        DUIButton duiButton = buttonGo.AddComponent<DUIButton>();

        // Initialise the button
        duiButton.Initialise();

        return (duiButton);
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
        m_navButton.m_text = text;
    }

    // Debug Functions
    private void DebugRenderRects()
    {
        // Render self rect
        DebugDrawRect(new Rect(0.0f, 0.0f, 1.0f, 1.0f), Color.green, 0.005f);
    }
}
