using UnityEngine;
using System.Collections;

public class DUIField : DUIElement 
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
    public void Initialise(string _text)
    {
        InitialiseText(_text);
    }

    private void InitialiseText(string _text)
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
        m_textMesh.fontSize = 24;
		m_textMesh.characterSize = 0.025f;
        m_textMesh.color = Color.white;
        m_textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
        m_textMesh.anchor = TextAnchor.MiddleCenter;
        m_textMesh.offsetZ = -0.01f;
        m_textMesh.text = _text;
		m_textMesh.fontStyle = FontStyle.Italic;
		
		// Get the dimensions for the text
		m_dimensions = new Vector2(mr.bounds.size.x, mr.bounds.size.y);
    }
}
