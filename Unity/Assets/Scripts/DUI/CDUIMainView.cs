using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

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
	
	private Rect m_TitleRect = new Rect();
    private Rect m_NavAreaRect = new Rect();
    private Rect m_SubViewAreaRect = new Rect();
	
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
			m_TitleRect.Set(0.0f, 0.8f, 1.0f, 0.2f); 
			m_NavAreaRect.Set(0.0f, 0.0f, 0.2f, 0.8f); 
			m_SubViewAreaRect.Set(0.3f, 0.0f, 0.6f, 0.8f); 
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
		// Place the subview in the middle
        float x = m_SubViewAreaRect.center.x * m_Dimensions.x - (m_Dimensions.x * 0.5f);
        float y = m_SubViewAreaRect.center.y * m_Dimensions.y - (m_Dimensions.y * 0.5f);
        _duiSubView.transform.localPosition = new Vector3(x, y);
		
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
		
		// Set this as the active subview
		SetActiveSubView(_duiSubView.gameObject);
    }
	
	public void SetActiveSubView(GameObject _SubView)
	{
		if(m_ActiveSubView != null)
			m_ActiveSubView.SetActive(false);
		
		m_ActiveSubView = _SubView;
		m_ActiveSubView.SetActive(true);
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
                float x = m_NavAreaRect.center.x * m_Dimensions.x - (m_Dimensions.x * 0.5f);
                float y = ((m_NavAreaRect.yMax - m_NavAreaRect.y) * (count + 0.5f) / numSubViews) * m_Dimensions.y - (m_Dimensions.y * 0.5f);

                navButton.transform.localPosition = new Vector3(x, y);
            }
            else
            {
                float x = (m_NavAreaRect.xMax - m_NavAreaRect.x) * (count + 0.5f) / numSubViews * m_Dimensions.x - (m_Dimensions.x * 0.5f);
                float y = m_NavAreaRect.center.y * m_Dimensions.y - (m_Dimensions.y * 0.5f);

                navButton.transform.localPosition = new Vector3(x, y);
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
