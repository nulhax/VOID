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

	private int m_NumWorkingCircuitryComponents = 0;
	private int m_NumCircuitryComponents = 0;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PowerStorage = gameObject.GetComponent<CPowerStorageBehaviour>();
		
		// Register for when the circuitry breaks/fixes
		foreach(GameObject comp in gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.CircuitryComp))
		{
			CComponentInterface ci = comp.GetComponent<CComponentInterface>();
			ci.EventComponentBreak += HandleCircuitryBreaking;
			ci.EventComponentFix += HandleCircuitryFixing;

			++m_NumCircuitryComponents;
			++m_NumWorkingCircuitryComponents;
		}
		
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
	
	private void HandleCircuitryBreaking(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			Debug.Log("Broke");
			--m_NumWorkingCircuitryComponents;

			// Change the battery capacity
			m_PowerStorage.BatteryCapacity = m_PowerBatteryCapacity * ((float)m_NumWorkingCircuitryComponents / (float)m_NumCircuitryComponents);

			// Deactive the charge availablity
			if(m_NumWorkingCircuitryComponents == 0)
			{
				m_PowerStorage.DeactivateBatteryChargeAvailability();
			}
		}
	}
	
	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			++m_NumWorkingCircuitryComponents;

			// Change the battery capacity
			m_PowerStorage.BatteryCapacity = m_PowerBatteryCapacity * ((float)m_NumWorkingCircuitryComponents / (float)m_NumCircuitryComponents);

			// Activate the charge availablity
			m_PowerStorage.ActivateBatteryChargeAvailability();
		}
	}
}
