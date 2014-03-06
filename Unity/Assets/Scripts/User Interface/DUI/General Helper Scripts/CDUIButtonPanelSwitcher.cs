//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIButtonActivator.cs
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
public class CDUIButtonPanelSwitcher : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CDUIPanel m_PanelToSwitchTo = null;

	// Member Properties
	
	
	// Member Methods
	public void OnPress(bool _IsPressed)
	{
		if(CNetwork.IsServer)
		{
			// Find the Root2D script
			CDUIPanelTransitioner r2d = CUtility.FindInParents<CDUIPanelTransitioner>(gameObject);

			// Inform it to switch to this panel
			if(r2d != null)
				r2d.SwitchToPanel(m_PanelToSwitchTo);
			else
				Debug.LogError("CDUIRoot2D was not found in hierarchy");
		}
	}
}
