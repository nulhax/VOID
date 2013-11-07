using UnityEngine;
using System.Collections;

public class CDUIElement : MonoBehaviour 
{
	// Member Types
	public enum EElementType
	{
		INVALID = -1,
		
		Button,
		Field,
		
		MAX
	}
	
	// Member Fields
	protected Vector2 m_Dimensions = Vector2.zero;
	
	protected uint m_ElementID = uint.MaxValue;
	protected uint m_ParentViewID = uint.MaxValue;
	
	protected EElementType m_ElementType = EElementType.INVALID;
	
    // Member Properties
	public EElementType ElementType
	{
		protected set
		{
			if(m_ElementType == EElementType.INVALID)
			{
				m_ElementType = value;
			}
			else
			{
				Debug.Log("ElementType set cannot be set twice!");
			}
		}
		get
		{
			return(m_ElementType);
		}
	}
	
	public Vector2 Dimensions 
	{ 
		private set
		{
			m_Dimensions = value;
		}
		get 
		{ 
			return(m_Dimensions); 
		} 
	}
	
    public uint ElementID
	{
		set
		{
			if(m_ElementID == uint.MaxValue)
			{
				m_ElementID = value;
			}
			else
			{
				Debug.LogError("You cannot set the element ID twice!");
			}
		}
		get{ return(m_ElementID); }
	}
	
	public uint ParentViewID
	{
		set
		{
			if(m_ParentViewID == uint.MaxValue)
			{
				m_ParentViewID = value;
			}
			else
			{
				Debug.LogError("You cannot set the parent subview ID twice!");
			}
		}
		get{ return(m_ParentViewID); }
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
