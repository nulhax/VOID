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


public class CDUIElement : MonoBehaviour 
{
	// Member Types
	public enum EElementType
	{
		INVALID = -1,
		
		Button,
		Field,
		Sprite,
		
		MAX
	}
	
	public enum EAnchor
	{
		INVALID = -1,
		
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft,
		LowerCenter,
		LowerRight,
		
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
	
	public Vector2 UpperLeftViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(-m_Dimensions.x * 0.5f, m_Dimensions.y * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(-m_Dimensions.x * 0.5f, m_Dimensions.y * 0.5f);
			
            transform.localPosition = localPos;
        }
    }
	
	public Vector2 UpperCenterViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = transform.localPosition;
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(0.0f, m_Dimensions.y * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(0.0f, m_Dimensions.y * 0.5f);

            transform.localPosition = localPos;
        }
    }

	public Vector2 UpperRightViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(m_Dimensions.x * 0.5f, m_Dimensions.y * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(m_Dimensions.x * 0.5f, m_Dimensions.y * 0.5f);
			
            transform.localPosition = localPos;
        }
    }
	
	public Vector2 MiddleLeftViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(-m_Dimensions.x * 0.5f, 0.0f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();
			
            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(-m_Dimensions.x * 0.5f, 0.0f);

			transform.localPosition = localPos;
        }
    }
	
	public Vector2 MiddleCenterViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = transform.localPosition;
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f);

            transform.localPosition = localPos;
        }
    }

	public Vector2 MiddleRightViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(m_Dimensions.x * 0.5f, 0.0f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(m_Dimensions.x * 0.5f, 0.0f);
			
            transform.localPosition = localPos;
        }
    }
	
	public Vector2 LowerLeftViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(-m_Dimensions.x * 0.5f, -m_Dimensions.y * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(-m_Dimensions.x * 0.5f, -m_Dimensions.y * 0.5f);
			
            transform.localPosition = localPos;
        }
    }
	
	public Vector2 LowerCenterViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = transform.localPosition;
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(0.0f, -m_Dimensions.y * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(0.0f, -m_Dimensions.y * 0.5f);

            transform.localPosition = localPos;
        }
    }

	public Vector2 LowerRightViewPos
    {
        get
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 viewPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            Vector2 dimensions = parentView.Dimensions;

            viewPos += (dimensions * 0.5f) + new Vector2(m_Dimensions.x * 0.5f, -m_Dimensions.y * 0.5f);
            viewPos.x /= dimensions.x;
            viewPos.y /= dimensions.y;

            return (viewPos);
        }

        set 
        {
            CDUIView parentView = transform.parent.GetComponent<CDUIView>();

            Vector2 localPos = value;
            Vector2 dimensions = parentView.Dimensions;

            localPos.x *= dimensions.x;
            localPos.y *= dimensions.y;
            localPos -= (dimensions * 0.5f) + new Vector2(m_Dimensions.x * 0.5f, -m_Dimensions.y * 0.5f);
			
            transform.localPosition = localPos;
        }
    }
	
	public void SetViewPos(EAnchor _ViewPosAnchor, Vector2 _Value)
	{
		
	}
	
	
	// Member Methods
	protected Mesh CreatePlaneMesh(Vector2 _dimensions)
    {
        Mesh mesh = new Mesh();
        mesh.name = name + "_mesh";
        mesh.Clear();

        int numVertices = 4;
        int numTriangles = 6;

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];

        vertices[0] = new Vector3(-(_dimensions.x * 0.5f), -(_dimensions.y * 0.5f));
        vertices[1] = new Vector3((_dimensions.x * 0.5f), -(_dimensions.y * 0.5f));
        vertices[2] = new Vector3(-(_dimensions.x * 0.5f), (_dimensions.y * 0.5f));
        vertices[3] = new Vector3((_dimensions.x * 0.5f), (_dimensions.y * 0.5f));

        uvs[0] = new Vector2(0.0f, 0.0f);
        uvs[1] = new Vector2(1.0f, 0.0f);
        uvs[2] = new Vector2(0.0f, 1.0f);
        uvs[3] = new Vector2(1.0f, 1.0f);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;

        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return (mesh);
    }
}
