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
	
	
// Member Properties
	
	
// Member Methods


	void Start()
	{
        GetComponent<CFacilityPower>().EventFacilityPowerActiveChange += OnEventFacilityPowerStatusChange;

		UpdateLightingState();
	}


    void OnDestroy()
    {
        GetComponent<CFacilityPower>().EventFacilityPowerActiveChange -= OnEventFacilityPowerStatusChange;
    }


	void UpdateLightingState()
	{
		switch (m_LightingState) 
		{
            case ELightingState.NoPower:
                {
                    if (m_NormalLights != null)
                        m_NormalLights.SetActive(false);
                    if (m_NoPowerLights != null)
                        m_NoPowerLights.SetActive(true);
                }
                break;

            case ELightingState.Normal:
                {
                    if (m_NormalLights != null)
                        m_NormalLights.SetActive(true);
                    if (m_NoPowerLights != null)
                        m_NoPowerLights.SetActive(false);
                }
                break;

		default:
			break;
		}
	}


    void OnEventFacilityPowerStatusChange(GameObject _cFacility, bool _bActive)
    {
        if (_bActive)
        {
            m_LightingState = ELightingState.Normal;

            UpdateLightingState();
        }
        else
        {
            m_LightingState = ELightingState.NoPower;

            UpdateLightingState();
        }
    }


// Member Fields


    public GameObject m_NormalLights = null;
    public GameObject m_NoPowerLights = null;

    private ELightingState m_LightingState = ELightingState.Normal;


}
