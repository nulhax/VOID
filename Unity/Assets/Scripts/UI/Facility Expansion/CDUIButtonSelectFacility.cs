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
public class CDUIButtonSelectFacility : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CFacilityInterface.EType m_FacilityType = CFacilityInterface.EType.INVALID;
	public GameObject m_FacilityStageObject = null;
	
	// Member Properties

	
	// Member Methods
	public void OnSelect(bool _IsSelected)
	{
		if(CNetwork.IsServer && _IsSelected)
			m_FacilityStageObject.GetComponent<CDUIStageFacilityObject>().ChangeFacilityType(m_FacilityType);
	}
}
