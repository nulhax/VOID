//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNOSWidgetMain.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


[RequireComponent(typeof(CDUIElement))]
public class CNOSWidgetMain : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private CNOSWidget m_Widget = null;
	private CNOSPanelRoot m_PanelRoot = null;


	// Member Properties


	// Member Methods
	private void Start()
	{
		m_Widget = CUtility.FindInParents<CNOSWidget>(gameObject);
		m_PanelRoot = CUtility.FindInParents<CNOSPanelRoot>(gameObject);
	}

	private void OnClick()
	{
		// Right click to close the widget
		if(UICamera.currentTouchID == -2)
		{
			m_Widget.HideMainWidget();
		}
		else if(UICamera.currentTouchID == -3)
		{
			m_Widget.ToggleMainWidgetMaximise();
		}
	}

	private void OnPress()
	{
		// Focus this widget
		m_PanelRoot.FocusWidget(m_Widget);
	}
}
