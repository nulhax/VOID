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


/* Implementation */


public class CDUIField : CDUIElement 
{
	// Member Fields
	private GameObject m_TextField = null;

    // Member Properties
    public string Text
    {
        get
        {
            return m_TextField.GetComponent<TextMesh>().text;
        }
        set
        {
            m_TextField.GetComponent<TextMesh>().text = value;
			RecalculateDimensions();
        }
    }

    // Member Methods
	public void Awake()
	{
		ElementType = CDUIElement.EElementType.Field;
	}
	
    public void Initialise(string _text, Color _textColor, int _fontSize = 96, float _characterSize = 0.00625f)
    {
        InitialiseText(_text,_textColor,_fontSize,_characterSize);
    }

    private void InitialiseText(string _text, Color _textColor, int _fontSize, float _characterSize)
    {
        // Create the text field object
        m_TextField = new GameObject(name + "_Text");
        m_TextField.transform.parent = transform;
        m_TextField.transform.localPosition = Vector3.zero;
        m_TextField.transform.localRotation = Quaternion.identity;
        m_TextField.layer = gameObject.layer;

        // Add the mesh renderer
        MeshRenderer mr = m_TextField.AddComponent<MeshRenderer>();
        mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));

        // Add the text mesh
        TextMesh textMesh = m_TextField.AddComponent<TextMesh>();
        textMesh.fontSize = _fontSize;
		textMesh.characterSize = _characterSize;
        textMesh.color = _textColor;
        textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.offsetZ = -0.01f;
		textMesh.fontStyle = FontStyle.Italic;
		Text = _text;
    }
	
	private void RecalculateDimensions()
	{
		m_Dimensions = m_TextField.GetComponent<MeshRenderer>().bounds.size;
	}
}
