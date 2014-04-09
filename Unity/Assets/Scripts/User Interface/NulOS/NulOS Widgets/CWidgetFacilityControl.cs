﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIFacilityControlPower.cs
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

[RequireComponent(typeof(CNOSWidget))]
public class CWidgetFacilityControl : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	
	public UILabel m_PowerConsumers = null;
	public UILabel m_PowerConsumption = null;
	
	private GameObject m_CachedFacility = null;
	private CFacilityPower m_CachedFacilityPower = null;
	
	private bool m_Registered = false;

	private static int m_ToggleGroupCount = 0;

	// Member Properties
	
	
	// Member Methods
	
	public void RegisterFacility(GameObject _Facility)
	{
		m_CachedFacility = _Facility;
		m_CachedFacilityPower = _Facility.GetComponent<CFacilityPower>();

		m_Registered = true;
	}

	private void Awake()
	{
		m_ToggleGroupCount += 1;
		foreach(UIToggle tog in GetComponent<CNOSWidget>().m_MainWidget.GetComponentsInChildren<UIToggle>())
		{
			tog.group = m_ToggleGroupCount;
		}
	}
	
	private void Update()
	{
		if(m_Registered)
			UpdatePowerLabels();
	}
	
	public void OnGravityToggle()
	{
		if(CNetwork.IsServer && m_Registered)
		{
			// Set the gravity state for the facility
			//m_CachedFacility.GetComponent<CFacilityGravity>().SetGravityEnabled(UIToggle.current.value);
		}
	}
	
	private void UpdatePowerLabels()
	{
		// Get the current charge, intial capacity and current capacity
		float consumptionRate = m_CachedFacilityPower.PowerConsumptionRate;
		int numConsumers = 9000;
		int numActiveConsumers = 7777;

        /*
		foreach(GameObject consumer in m_CachedFacilityPower.PowerConsumers)
		{
			CModulePowerConsumption mpc = consumer.GetComponent<CModulePowerConsumption>();
			if(mpc.IsConsumingPower)
				++numActiveConsumers;
		}
         * */
		
		// Update the labels
		m_PowerConsumption.text = consumptionRate.ToString();
		m_PowerConsumers.text = numActiveConsumers.ToString() + " / " + (numConsumers - numActiveConsumers).ToString();
	}
}
