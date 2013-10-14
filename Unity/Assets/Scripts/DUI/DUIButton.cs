using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

public class DUIButton : MonoBehaviour 
{
    public event EventHandler Press;

    public void Initialise(XmlNode _xButton, float _viewWidth, float _viewHeight)
    {
        // Set the position (optional)
        if (_xButton.Attributes["pos"] != null)
        {
            Vector3 pos = DUIView.StringToVector3(_xButton.Attributes["pos"].Value);
            transform.localPosition = new Vector3(pos.x * _viewWidth - (_viewWidth * 0.5f), pos.y * _viewHeight - (_viewHeight * 0.5f));
        }

        // Set the text (optional)
        if (_xButton.Attributes["text"] != null)
            GetComponentInChildren<TextMesh>().text = _xButton.Attributes["text"].Value;

        // Find the events handle
        foreach (XmlNode xEvent in _xButton.SelectNodes("event"))
        {
            string eventName = string.Empty;

            if (xEvent.Attributes["name"] != null)
                eventName = xEvent.Attributes["name"].Value;
            else
            {
                Debug.LogError(string.Format("DUI: XML Button event attribute [name] not found!"));
                Debug.Break();
            }

            // Find the event
            EventInfo ei = typeof(DUIButton).GetEvent(eventName);
            if (ei == null)
            {
                Debug.LogError(string.Format("DUIButton: Event [{0}] not found within this component!", eventName), gameObject);
                Debug.Break();
            }

            // Find the actions to register to events
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
                switch (targetName)
                {
                    case "{DUI}":
                        targetGo = transform.parent.parent.gameObject;
                        break;

                    default:
                        targetGo = GameObject.Find(targetName);
                        break;
                }
                if (targetGo == null)
                {
                    Debug.LogError(string.Format("DUI: Target [{0}] not found!", targetName));
                    Debug.Break();
                }

                // Find the component
                Component component = targetGo.GetComponent(componentName);
                if (component == null)
                {
                    Debug.LogError(string.Format("DUIButton: Component [{0}] not found within target [{1}]!", componentName, targetName), targetGo);
                    Debug.Break();
                }

                // Find the method
                MethodInfo mi = System.Type.GetType(componentName).GetMethod(actionName);
                if (mi == null)
                {
                    Debug.LogError(string.Format("DUIButton: Action [{0}] not found within component [{1}], within target [{2}]! Perhaps it is not set to public", actionName, componentName, targetName), targetGo);
                    Debug.Break();
                }

                // Register the action on the target
                ei.AddEventHandler(this, System.Delegate.CreateDelegate(typeof(System.Action), component, mi));
            }
        }
    }

    public void ButtonPressed()
    {
        if (Press != null)
        {
            Press(this, null);
        }
    }
}
