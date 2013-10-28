using UnityEngine;
using System.Collections;

public class DUIElement : MonoBehaviour 
{
	// Member Fields

    // Member Properties
	public Vector2 m_dimensions 
	{ 
		get;
		set;
	}
	
	public Vector2 m_viewPos
    {
        get
        {
            DUIView parentView = transform.parent.GetComponent<DUIView>();

            Vector2 viewPos = transform.localPosition;
            Vector2 dimensions = parentView.m_dimensions + m_dimensions;

            viewPos += (dimensions * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            DUIView parentView = transform.parent.GetComponent<DUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.m_dimensions - m_dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f);

            transform.localPosition = localPos;
        }
    }
	
	// Member Methods
}
