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


public enum ELayoutStyle
{
	INVALID = -1,
	
    Layout_1,
	Layout_2,
	
	MAX
}

public class CDUIMainView : CDUIView
{
    // Member Fields
    private ELayoutStyle m_Layout = ELayoutStyle.INVALID;
	
	public Rect m_TitleRect = new Rect();
    public Rect m_NavAreaRect = new Rect();
    public Rect m_SubViewAreaRect = new Rect();
	
	GameObject m_ActiveSubView = null;
	
	private Dictionary<uint, GameObject> m_NavButtonSubViewPairs = new Dictionary<uint, GameObject>();
	
    // Member Properties
	
    // Member Methods
    private void Update()
    {
		// Draw debug information
        DebugRenderRects();
    }

    public void Initialise(ELayoutStyle _Layout, Vector2 _Dimensions)
    {
        m_Dimensions = _Dimensions;
		m_Layout = _Layout;
		
        // Setup the rects according to layout
		switch (m_Layout) 
		{
		case ELayoutStyle.Layout_1: 
			m_TitleRect.Set(0.275f, 0.85f, 0.6f, 0.15f); 
			m_NavAreaRect.Set(0.035f, 0.35f, 0.235f, 0.5f); 
			m_SubViewAreaRect.Set(0.275f, 0.05f, 0.6f, 0.8f); 
			break;
			
		default:
			Debug.LogError("Invalid Layout for console provided!");	
			break;
		}
    }
	
    public void SetTitle(string _TitleText)
    {
        Vector3 localPos = new Vector3(m_TitleRect.center.x * m_Dimensions.x - (m_Dimensions.x * 0.5f),
                                       m_TitleRect.center.y  * m_Dimensions.y - (m_Dimensions.y * 0.5f));

        // Add a field for the title
        CDUIField duiField = AddField(_TitleText);
		
		// Position it to the center of the title area
		duiField.transform.localPosition = localPos;
    }
	
	public void SetupSubView(CDUISubView _duiSubView)
    {
		// Initialise the DUI Component
        _duiSubView.Initialise(new Vector2(m_SubViewAreaRect.width * m_Dimensions.x, m_SubViewAreaRect.height * m_Dimensions.y));
		
		 // Add the navigation button
		CDUIButton duiNavButton = AddButton(_duiSubView.gameObject.name);
		
        // Register the button for the event
        duiNavButton.PressDown += NavButtonPressed;
		
		// Add to the dictionaries
		m_NavButtonSubViewPairs[duiNavButton.ElementID] = _duiSubView.gameObject;
		
        // Reposition the buttons
        RepositionNavButtons();
		
		// Set this as the active subview by default
		SetActiveSubView(_duiSubView.gameObject);
    }
	
	public void SetActiveSubView(GameObject _SubView)
	{
		// Reposition all of the subviews out of view of the camera
		foreach(GameObject subView in m_NavButtonSubViewPairs.Values)
		{
			float x = m_SubViewAreaRect.center.x * m_Dimensions.x - (m_Dimensions.x * 0.5f);
        	float y = m_SubViewAreaRect.center.y * m_Dimensions.y - (m_Dimensions.y * 0.5f);
			
			subView.transform.localPosition = new Vector3(x, y, -1.5f);
		}
		
		// Move this subview back into view
		if(m_NavButtonSubViewPairs.ContainsValue(_SubView))
		{
			m_ActiveSubView = _SubView;
			
			m_ActiveSubView.transform.localPosition = new Vector3(m_ActiveSubView.transform.localPosition.x, m_ActiveSubView.transform.localPosition.y, 0.0f);
		}
		else
		{
			Debug.Log("SetActiveSubView, subview doesn't belong to this DUI!!");
		}
	}
	
    private void RepositionNavButtons()
    {
        int numSubViews = m_NavButtonSubViewPairs.Count;
        int count = 0;

        foreach (uint navButtonID in m_NavButtonSubViewPairs.Keys)
        {
            GameObject navButton = m_Elements[navButtonID];
           
            // Calculate the position for the nav button to go
            if (m_NavAreaRect.width * Dimensions.x < m_NavAreaRect.height * Dimensions.y)
            {
                float x = m_NavAreaRect.center.x;
                float y = m_NavAreaRect.yMin + m_NavAreaRect.height * (float)(count + 1) / (float)(numSubViews + 1);

                navButton.GetComponent<CDUIElement>().MiddleCenterViewPos = new Vector2(x, y);
            }
            else
            {
                float x = m_NavAreaRect.xMin + m_NavAreaRect.width * (float)(count + 1) / (float)(numSubViews + 1);
                float y = m_NavAreaRect.center.y;

                navButton.GetComponent<CDUIElement>().MiddleCenterViewPos = new Vector2(x, y);
            }

            count += 1;
        }
    }

    private void NavButtonPressed(CDUIButton _sender)
    {
		GameObject subView = m_NavButtonSubViewPairs[_sender.ElementID];
		
		SetActiveSubView(subView);
    }

    // Debug functions
    private void DebugRenderRects()
    {
        // Render self rect
        DebugDrawRect(new Rect(0.0f, 0.0f, 1.0f, 1.0f), Color.white, 0);

        // Title rect
        DebugDrawRect(m_TitleRect, Color.cyan, 0.005f);

        // Nav area rect
        DebugDrawRect(m_NavAreaRect, Color.yellow, 0.005f);

        // Subview area rect
        DebugDrawRect(m_SubViewAreaRect, Color.red, 0.005f);   
    }
}
