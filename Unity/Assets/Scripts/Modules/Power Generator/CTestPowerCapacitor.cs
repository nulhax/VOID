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
using System.Linq;


[RequireComponent(typeof(CPowerStorageBehaviour))]
public class CTestPowerCapacitor: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_MaxPowerBatteryCapacity = 1000.0f;
	public CDUIConsole m_DUIConsole = null;

	public CComponentInterface m_Circuitry1 = null;
	public CComponentInterface m_Circuitry2 = null;

	private CPowerStorageBehaviour m_PowerStorage = null;
	private CDUIPowerCapacitorRoot m_DUIPowerCapacitor = null;


	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PowerStorage = gameObject.GetComponent<CPowerStorageBehaviour>();

		// Register charge/capacity state chages
		m_PowerStorage.EventBatteryCapacityChanged += HandleCapacitorStateChange;
		m_PowerStorage.EventBatteryChargeChanged += HandleCapacitorStateChange;

		// Register for when the circuitry breaks/fixes
		m_Circuitry1.EventComponentBreak += HandleCircuitryBreaking;
		m_Circuitry1.EventComponentFix += HandleCircuitryFixing;
		m_Circuitry2.EventComponentBreak += HandleCircuitryBreaking;
		m_Circuitry2.EventComponentFix += HandleCircuitryFixing;

		// Register for when the circuitry takes damage
		m_PowerStorage.EventBatteryCapacityChanged += HandleCapacitorStateChange;
		m_PowerStorage.EventBatteryChargeChanged += HandleCapacitorStateChange;

		// Get the DUI of the power generator
		m_DUIPowerCapacitor = m_DUIConsole.DUI.GetComponent<CDUIPowerCapacitorRoot>();
		m_DUIPowerCapacitor.InitialCapacity = m_MaxPowerBatteryCapacity;

		// Debug: Set the charge to half its total capacity
		if(CNetwork.IsServer)
		{
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity;
			m_PowerStorage.BatteryCharge = m_PowerStorage.BatteryCapacity / 2;
		}
	}

	private void HandleCapacitorStateChange(CPowerStorageBehaviour _Capacitor)
	{
		m_DUIPowerCapacitor.UpdateCapacitorVariables(_Capacitor.BatteryCharge, _Capacitor.BatteryCapacity);
	}
	
	private void HandleCircuitryBreaking(CComponentInterface _Component)
	{
		int index = m_Circuitry1 == _Component ? 0 : 1;
		if(CNetwork.IsServer)
		{
			// Get the number of working circuitry components
			int numWorkingCircuitryComps = 0;
			if(m_Circuitry1.IsFunctional) numWorkingCircuitryComps++;
			if(m_Circuitry2.IsFunctional) numWorkingCircuitryComps++;

			// Set the new battery capacity
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity * ((float)numWorkingCircuitryComps / 2.0f);

			// Deactive the charge availablity
			if(numWorkingCircuitryComps == 0)
			{
				m_PowerStorage.DeactivateBatteryChargeAvailability();
			}
		}

		// Update the UI
		m_DUIPowerCapacitor.SetCircuitryStateChange(index, false);
	}
	
	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		int index = m_Circuitry1 == _Component ? 0 : 1;
		if(CNetwork.IsServer)
		{
			// Get the number of working circuitry components
			int numWorkingCircuitryComps = 0;
			if(m_Circuitry1.IsFunctional) numWorkingCircuitryComps++;
			if(m_Circuitry2.IsFunctional) numWorkingCircuitryComps++;

			// Set the new battery capacity
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity * ((float)numWorkingCircuitryComps / 2.0f);

			// Activate the charge availablity
			m_PowerStorage.ActivateBatteryChargeAvailability();
		}

		// Update the UI
		m_DUIPowerCapacitor.SetCircuitryStateChange(index, true);
	}
}
