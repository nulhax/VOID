using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Reflection;

public class DiegeticUI : MonoBehaviour
{
    private TextAsset m_UIXML;
    private float m_MainViewWidth;
    private float m_MainViewHeight;

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

	void Start () 
    {
        // Get the initial variables from the screen
        MonitorScreen sUI = transform.parent.GetComponent<MonitorScreen>();
        m_UIXML = sUI.m_UIXML;
        m_MainViewWidth = sUI.m_Width;
        m_MainViewHeight = sUI.m_Height;

        // Load the XML reader and document for parsing information
        XmlDocument xDoc = new XmlDocument();
        XmlTextReader xReader = new XmlTextReader(new StringReader(m_UIXML.text));
        xDoc.Load(xReader);
        XmlNode xUI = xDoc.SelectSingleNode("ui");

        // Setup main view
        SetupMainView(xUI.SelectSingleNode("mainview"));
	}

	void SetupMainView(XmlNode _xMainView)
    {
        foreach (XmlNode button in _xMainView.SelectNodes("button"))
        {
            CreateButton(button, gameObject);
        }
    }

    void CreateButton(XmlNode _xButton, GameObject _partentWindow)
    {
        // Create the button game object
        string asset = "Assets/InGame UI/Buttons/" + _xButton.Attributes["type"].Value + ".prefab";
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
                EventInfo ei = typeof(ButtonUI).GetEvent("m_" + eventName);
                ei.AddEventHandler(buttonGo.GetComponent<ButtonUI>(), System.Delegate.CreateDelegate(typeof(System.Action), component, mi));
            }
        }
    }
}
