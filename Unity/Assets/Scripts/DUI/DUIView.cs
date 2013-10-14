using UnityEngine;
using System.Collections;
using System.Xml;

public class DUIView : MonoBehaviour
{
    public float m_width    { get; set; }
    public float m_height   { get; set; }

    public void Initialise(XmlNode _xView)
    {
        // Get the view width
        if(_xView.Attributes["width"] != null)
            m_width = float.Parse(_xView.Attributes["width"].Value);
        else
        {
            Debug.LogError(string.Format("DUIView: XML view attribute [width] not found!"));
            Debug.Break();
        }

        // Get the view height
        if (_xView.Attributes["height"] != null)
            m_height = float.Parse(_xView.Attributes["height"].Value);
        else
        {
            Debug.LogError(string.Format("DUIView: XML view attribute [height] not found!"));
            Debug.Break();
        }

        // Create all of the buttons belonging to this view
        foreach (XmlNode button in _xView.SelectNodes("button"))
        {
            CreateButton(button);
        }
    }

    private DUIButton CreateButton(XmlNode _xButton)
    {
        string asset = string.Empty;

        // Create the button game object
        if (_xButton.Attributes["prefab"] != null)
            asset = "Assets/Resources/Prefabs/DUI/Buttons/" + _xButton.Attributes["prefab"].Value + ".prefab";
        else
        {
            Debug.LogError(string.Format("DUIButton: XML button attribute [prefab] not found!"));
            Debug.Break();
        }

        GameObject buttonGo = (GameObject)Instantiate(Resources.LoadAssetAtPath(asset, typeof(GameObject)));

        // Set the default values
        buttonGo.transform.parent = transform;
        buttonGo.transform.localPosition = Vector3.zero;
        buttonGo.transform.localRotation = Quaternion.identity;

        // Add the DUI button component
        DUIButton duiButton = buttonGo.AddComponent<DUIButton>();
        duiButton.Initialise(_xButton, m_width, m_height);

        return (duiButton);
    }

    public static Vector3 StringToVector3(string rString)
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
}
