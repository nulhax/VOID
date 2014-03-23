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


	// Member Properties


	// Member Methods
	private void Start()
	{
		m_Widget = CUtility.FindInParents<CNOSWidget>(gameObject);
	}

	private void OnClick()
	{
		// Maximise the widget
		if(UICamera.currentTouchID == -3)
		{
			m_Widget.ToggleMainWidgetMaximise();
		}
	}

	private void OnPress()
	{
		// Left click to focus the widget
		if(UICamera.currentTouchID == -1)
		{
			// Focus this widget
			m_Widget.Focus();
		}
		// Right click to close the widget
		if(UICamera.currentTouchID == -2)
		{
			m_Widget.HideMainWidget();
		}
	}

	private void OnScroll(float _Delta)
	{
		// Debug: Disable scroll scaling
		return;

		// Increase local scale of the widget
		Vector3 scale = m_Widget.m_MainWidget.cachedTransform.localScale;
		float uniformScale = scale.x + _Delta;

		// Clamp the scale value
		uniformScale = Mathf.Clamp(uniformScale, 0.5f, 2.0f);

		m_Widget.m_MainWidget.cachedTransform.localScale = Vector3.one * uniformScale;


	}
}
