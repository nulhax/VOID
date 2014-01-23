//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTestPowerCapacitor.cs
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


[RequireComponent(typeof(CPowerStorageBehaviour))]
public class CTestPowerCapacitor: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_PowerBatteryCapacity = 1000.0f;

	private float m_PrevPowerBatteryCapacity = 0.0f;
	private CPowerStorageBehaviour m_PowerStorage = null;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PowerStorage = gameObject.GetComponent<CPowerStorageBehaviour>();

		// Register for when the fusebox breaks/fixes
		CFuseBoxBehaviour fbc = gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.FuseBox)[0].GetComponent<CFuseBoxBehaviour>();
		fbc.EventBroken += HandleFuseBoxBreaking;
		fbc.EventFixed += HandleFuseBoxFixing;
		
		// Debug: Set the charge to half its total capacity
		if(CNetwork.IsServer)
		{
			m_PowerStorage.BatteryCapacity = m_PowerBatteryCapacity;
			m_PowerStorage.BatteryCharge = m_PowerStorage.BatteryCapacity / 2;
			
			m_PrevPowerBatteryCapacity = m_PowerBatteryCapacity;
		}
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{	
			if(m_PrevPowerBatteryCapacity != m_PowerBatteryCapacity)
			{
				m_PowerStorage.BatteryCapacity = m_PowerBatteryCapacity;
				
				m_PrevPowerBatteryCapacity = m_PowerBatteryCapacity;
			}
		}
	}
	
	private void HandleFuseBoxBreaking(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			m_PowerStorage.DeactivateBatteryChargeAvailability();
		}
	}
	
	private void HandleFuseBoxFixing(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			m_PowerStorage.ActivateBatteryChargeAvailability();
		}
	}
}
