//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CButtonSelectFacility.cs
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
public class CDUIButtonSelectTool : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields

	public CDuiDispenserBehaviour m_DispenserRoot = null;
	
	// Member Properties
	
	
	// Member Methods
	public void OnSelect(bool _IsSelected)
	{
		if (CNetwork.IsServer && 
            _IsSelected)
		{
			// Inform the stage to change model
			m_DispenserRoot.OnEventSelectTool(gameObject);
		}
	}
}
