using UnityEngine;
using System.Collections;

public class CDUIElement : MonoBehaviour 
{
	// Member Fields
	protected Vector2 m_Dimensions = Vector2.zero;
	protected uint m_ElementID = 0;
	
    // Member Properties
	public Vector2 Dimensions { get { return(m_Dimensions); } }
	
    public uint ElementID
	{
		set
		{
			if(m_ElementID == 0)
			{
				m_ElementID = value;
			}
			else
			{
				Debug.LogError("You cannot set a subview ID twice!");
			}
		}
		get{ return(m_ElementID); }
	}
	
	public Vector2 m_ViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = transform.localPosition;
            Vector2 dimensions = parentView.Dimensions + m_Dimensions;

            viewPos += (dimensions * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions - m_Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f);

            transform.localPosition = localPos;
        }
    }
	
	// Member Methods
}
