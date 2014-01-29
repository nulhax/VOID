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
public class CDUIButtonSelectModule : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CModuleInterface.EType m_ModuleType = CModuleInterface.EType.INVALID;
	public CDUIModuleCreationRoot m_ModuleCreationRoot = null;

	// Member Properties
	
	
	// Member Methods
	public void OnSelect(bool _IsSelected)
	{
		if(CNetwork.IsServer && _IsSelected)
		{
			// Inform the stage to change model
			m_ModuleCreationRoot.ChangeModuleType(m_ModuleType);
		}
	}
}
