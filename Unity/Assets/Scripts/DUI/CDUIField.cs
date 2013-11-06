using UnityEngine;
using System.Collections;

public class CDUIField : CDUIElement 
{
	// Member Fields
    private TextMesh m_textMesh;


    // Member Properties
    public string m_text
    {
        get
        {
            return m_textMesh.text;
        }
        set
        {
            m_textMesh.text = value;
        }
    }

    // Member Methods
    public void Initialise(string _text, Color _textColor, int _fontSize = 24, float _characterSize = 0.025f)
    {
        InitialiseText(_text,_textColor,_fontSize,_characterSize);
    }

    private void InitialiseText(string _text, Color _textColor, int _fontSize, float _characterSize)
    {
        // Create the text object
        GameObject text = new GameObject(name + "_text");
        text.transform.parent = transform;
        text.transform.localPosition = Vector3.zero;
        text.transform.localRotation = Quaternion.identity;
        text.layer = gameObject.layer;

        // Add the mesh renderer
        MeshRenderer mr = text.AddComponent<MeshRenderer>();
        mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));

        // Add the text mesh
        m_textMesh = text.AddComponent<TextMesh>();
        m_textMesh.fontSize = _fontSize;
		m_textMesh.characterSize = _characterSize;
        m_textMesh.color = _textColor;
        m_textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
        m_textMesh.anchor = TextAnchor.MiddleCenter;
        m_textMesh.offsetZ = -0.01f;
        m_textMesh.text = _text;
		m_textMesh.fontStyle = FontStyle.Italic;
		
		// Get the dimensions for the text
		m_Dimensions = new Vector2(mr.bounds.size.x, mr.bounds.size.y);
    }
}
