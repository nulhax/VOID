using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;

public enum EQuality
{
    VeryHigh,
    High,
    Medium,
    Low,
    VeryLow
}

public class DUIView : MonoBehaviour
{
    protected XmlNode m_uiXmlNode;
    protected Vector2 m_dimensions;

    // Helper functions
    protected static XmlDocument LoadXML(TextAsset _uiXmlDoc)
    {
        // Load the XML reader and document for parsing information
        XmlTextReader xReader = new XmlTextReader(new StringReader(_uiXmlDoc.text));
        XmlDocument Xml = new XmlDocument();
        Xml.Load(xReader);

        return (Xml);
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

    public static Vector2 StringToVector2(string rString)
    {
        float x = 0.0f;
        float y = 0.0f;

        var temp = rString.Substring(0, rString.Length).Split(',');
        if (temp.Length > 0)
            x = float.Parse(temp[0]);
        if (temp.Length > 1)
            y = float.Parse(temp[1]);
        Vector2 rValue = new Vector2(x, y);

        return rValue;
    }

    public static Rect StringToRect(string rString)
    {
        float x = 0.0f;
        float y = 0.0f;
        float width = 0.0f;
        float height = 0.0f;

        var temp = rString.Substring(0, rString.Length).Split(',');
        if (temp.Length > 0)
            x = float.Parse(temp[0]);
        if (temp.Length > 1)
            y = float.Parse(temp[1]);
        if (temp.Length > 2)
            width = float.Parse(temp[2]);
        if (temp.Length > 3)
            height = float.Parse(temp[3]);
        Rect rValue = new Rect(x, y, width, height);

        return rValue;
    }
}