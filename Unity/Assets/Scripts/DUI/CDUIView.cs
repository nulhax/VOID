//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public enum EQuality
{
	INVALID = -1,
	
    VeryHigh,
    High,
    Medium,
    Low,
    VeryLow,
	
	MAX
}

public class CDUIView : MonoBehaviour
{
    // Member Fields
	protected Vector2 m_Dimensions = Vector2.zero;
	protected uint m_ViewID = uint.MaxValue;
	
	protected Dictionary<uint, GameObject> m_Elements = new Dictionary<uint, GameObject>();
	
	private uint m_ElementIdCount = 0;
	
    // Member Properties
    public uint ViewID
	{
		set
		{
			if(m_ViewID == uint.MaxValue)
			{
				m_ViewID = value;
			}
			else
			{
				Debug.LogError("You cannot set a subview ID twice!");
			}
		}
		get{ return(m_ViewID); }
	}
	
	public Vector2 Dimensions 
	{
		get
		{
			return(m_Dimensions);
		}
	}

    // Member Methods
	public CDUIButton AddButton(string _text)
    {
        // Create the game object
        GameObject buttonGo = new GameObject("Button_" + _text);

        // Set the default values
        buttonGo.layer = gameObject.layer;
        buttonGo.transform.parent = transform;
        buttonGo.transform.localRotation = Quaternion.identity;
        buttonGo.transform.localPosition = Vector3.zero;

        // Add the DUIbutton
        CDUIButton duiButton = buttonGo.AddComponent<CDUIButton>();
		duiButton.ElementID = ++m_ElementIdCount;
		duiButton.ParentViewID = m_ViewID;
		
        // Initialise the button
        duiButton.Initialise(_text);
		
		m_Elements.Add(duiButton.ElementID, buttonGo);

        return (duiButton);
    }
	
	public CDUIField AddField(string _text)
    {
        // Create the game object
        GameObject fieldGo = new GameObject("Field_" + _text);

        // Set the default values
        fieldGo.layer = gameObject.layer;
        fieldGo.transform.parent = transform;
        fieldGo.transform.localRotation = Quaternion.identity;
        fieldGo.transform.localPosition = Vector3.zero;

        // Add the DUIfield
        CDUIField duiField = fieldGo.AddComponent<CDUIField>();
		duiField.ElementID = ++m_ElementIdCount;
		duiField.ParentViewID = m_ViewID;

        // Initialise the field
        duiField.Initialise(_text, Color.white);
		
		m_Elements.Add(duiField.ElementID, fieldGo);

        return (duiField);
    }
	
	public CDUISprite AddSprite()
    {
        // Create the game object
        GameObject spriteGo = new GameObject("Sprite");

        // Set the default values
        spriteGo.layer = gameObject.layer;
        spriteGo.transform.parent = transform;
        spriteGo.transform.localRotation = Quaternion.identity;
        spriteGo.transform.localPosition = Vector3.zero;

        // Add the DUIsprite
        CDUISprite duiSprite = spriteGo.AddComponent<CDUISprite>();
		duiSprite.ElementID = ++m_ElementIdCount;
		duiSprite.ParentViewID = m_ViewID;

        // Initialise the sprite
        duiSprite.Initialise();
		
		m_Elements.Add(duiSprite.ElementID, spriteGo);

        return (duiSprite);
    }
	
	public CDUIElement GetDUIElement(uint _ElementId)
	{
		CDUIElement duiElement = null;
		
		if(m_Elements.ContainsKey(_ElementId))
		{
			duiElement = m_Elements[_ElementId].GetComponent<CDUIElement>();
		}
		else
		{
			Debug.LogError("GetDUIElement, id sent in doesn't exsist!");
		}
		
		return(duiElement);
	}
	
	public void ClearDUIElements()
	{
		foreach(GameObject element in m_Elements.Values)
		{
			Destroy(element);
		}
	}
	
    protected void DebugDrawRect(Rect _rect, Color _color, float _offset)
    {
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;
        Rect rect = _rect;

        rect.x += _offset; rect.y += _offset; rect.width -= _offset * 2; rect.height -= _offset * 2;

        start = transform.rotation * new Vector3(rect.x * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.y * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;
        end = transform.rotation * new Vector3(rect.xMax * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.y * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, _color);

        start = transform.rotation * new Vector3(rect.x * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.y * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;
        end = transform.rotation * new Vector3(rect.x * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.yMax * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, _color);

        start = transform.rotation * new Vector3(rect.xMax * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.yMax * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;
        end = transform.rotation * new Vector3(rect.x * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.yMax * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, _color);

        start = transform.rotation * new Vector3(rect.xMax * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.yMax * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;
        end = transform.rotation * new Vector3(rect.xMax * m_Dimensions.x - (m_Dimensions.x * 0.5f), rect.y * m_Dimensions.y - (m_Dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end, _color);
    }
}
	
/***************************** OLD STUFF THAT COULD BE USEFUL LATER *******************************************/	
//    // Helper Methods
//    protected static XmlDocument LoadXML(TextAsset _uiXmlDoc)
//    {
//        // Load the XML reader and document for parsing information
//        XmlTextReader xReader = new XmlTextReader(new StringReader(_uiXmlDoc.text));
//        XmlDocument Xml = new XmlDocument();
//        Xml.Load(xReader);
//
//        return (Xml);
//    }
//
//    public static Vector3 StringToVector3(string _string)
//    {
//        float x = 0.0f;
//        float y = 0.0f;
//        float z = 0.0f;
//
//        var temp = _string.Substring(0, _string.Length).Split(',');
//        if (temp.Length > 0)
//            x = float.Parse(temp[0]);
//        if (temp.Length > 1)
//            y = float.Parse(temp[1]);
//        if (temp.Length > 2)
//            z = float.Parse(temp[2]);
//        Vector3 rValue = new Vector3(x, y, z);
//
//        return rValue;
//    }
//
//    public static Vector2 StringToVector2(string _string)
//    {
//        float x = 0.0f;
//        float y = 0.0f;
//
//        var temp = _string.Substring(0, _string.Length).Split(',');
//        if (temp.Length > 0)
//            x = float.Parse(temp[0]);
//        if (temp.Length > 1)
//            y = float.Parse(temp[1]);
//        Vector2 rValue = new Vector2(x, y);
//
//        return rValue;
//    }
//
//    public static Rect StringToRect(string _string)
//    {
//        float x = 0.0f;
//        float y = 0.0f;
//        float width = 0.0f;
//        float height = 0.0f;
//
//        var temp = _string.Substring(0, _string.Length).Split(',');
//        if (temp.Length > 0)
//            x = float.Parse(temp[0]);
//        if (temp.Length > 1)
//            y = float.Parse(temp[1]);
//        if (temp.Length > 2)
//            width = float.Parse(temp[2]);
//        if (temp.Length > 3)
//            height = float.Parse(temp[3]);
//        Rect rValue = new Rect(x, y, width, height);
//
//        return rValue;
//    }