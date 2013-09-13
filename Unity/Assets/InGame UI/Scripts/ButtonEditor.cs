using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ButtonEditor : MonoBehaviour
{
    // Member Variables
    public string m_Text;
    public Font m_Font;
    public float m_CharacterSize = 0.1f;
    public Color m_Color;

    public float m_ButtonWidth;
    public float m_ButtonHeight;

    private TextMesh m_TextMesh = null;

    // Member Methods
    void OnEnable()
    {
        m_TextMesh = GetComponentInChildren<TextMesh>();

        m_Text = m_TextMesh.text;
        m_Font = m_TextMesh.font;
        m_CharacterSize = m_TextMesh.characterSize;
        m_Color = m_TextMesh.color;
    }

    void Update()
    {
        m_TextMesh.text = m_Text;
        m_TextMesh.characterSize =  m_CharacterSize;
        m_TextMesh.color = m_Color;

        if(m_TextMesh.font != m_Font)
        {
            m_TextMesh.font = m_Font;
        }
    }
}
