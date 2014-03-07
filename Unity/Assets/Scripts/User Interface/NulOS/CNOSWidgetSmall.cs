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

	
	// Member Properties
	
	
	// Member Methods
	protected override void Start()
	{
		m_Widget = CUtility.FindInParents<CNOSWidget>(gameObject);

		base.Start();
	}

	protected override void OnDragDropStart()
	{
		// Focus this widget
		m_Widget.Focus();

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
		// Show the main widget
		m_Widget.ShowMainWidget(false);
	}
}
