//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityLighting.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityLighting : MonoBehaviour
{
	// Member Types
	public enum ELightingState
	{
		Normal,
		NoPower,
	}

	
	// Member Delegates & Events
	
	
	// Member Fields
	public GameObject m_NormalLights = null;
	public GameObject m_NoPowerLights = null;

	private ELightingState m_LightingState = ELightingState.Normal;

	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		// Register when facility power events
		GetComponent<CFacilityPower>().EventFacilityPowerDeactivated += SetLightingNoPower;
		GetComponent<CFacilityPower>().EventFacilityPowerActivated += SetLightingNormal;

		UpdateLightingState();
	}

	private void UpdateLightingState()
	{
		switch (m_LightingState) 
		{
		case ELightingState.NoPower:
                if (m_NormalLights != null)
			m_NormalLights.SetActive(false);
                if (m_NoPowerLights != null)
			m_NoPowerLights.SetActive(true);
			break;

		case ELightingState.Normal:
            if (m_NormalLights != null)
			m_NormalLights.SetActive(true);
            if (m_NoPowerLights != null)
			m_NoPowerLights.SetActive(false);
			break;

		default:
			break;
		}
	}

	private void SetLightingNoPower(GameObject _Facility)
	{
		m_LightingState = ELightingState.NoPower;

		UpdateLightingState();
	}
	
	private void SetLightingNormal(GameObject _Facility)
	{
		m_LightingState = ELightingState.Normal;

		UpdateLightingState();
	}
}
