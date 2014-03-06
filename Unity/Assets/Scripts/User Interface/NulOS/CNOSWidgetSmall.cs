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
public class CNOSWidgetSmall : UIDragDropItem 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private CNOSWidget m_Widget = null;
	private CNOSPanelRoot m_PanelRoot = null;

	
	// Member Properties
	
	
	// Member Methods
	protected override void Start()
	{
		m_Widget = CUtility.FindInParents<CNOSWidget>(gameObject);
		m_PanelRoot = CUtility.FindInParents<CNOSPanelRoot>(gameObject);

		base.Start();
	}

	protected override void OnDragDropStart()
	{
		// Focus this widget
		m_PanelRoot.FocusWidget(m_Widget);

		base.OnDragDropStart();
	}

	protected override void OnDragDropRelease(GameObject surface)
	{
		// If dropped on a container, then start showing the main widget
		UIDragDropContainer container = surface ? NGUITools.FindInParents<UIDragDropContainer>(surface) : null;
		if(container != null)
		{
			m_Widget.ShowMainWidget(true);
		}

		base.OnDragDropRelease(surface);
	}

	private void OnDoubleClick()
	{
		// Update parent of the widget
		m_Widget.transform.parent = m_PanelRoot.m_MainWidgetContainer.cachedTransform;

		// Notify the widgets that the parent has changed
		NGUITools.MarkParentAsChanged(gameObject);

		// Show the main widget
		m_Widget.ShowMainWidget(false);
	}
}
