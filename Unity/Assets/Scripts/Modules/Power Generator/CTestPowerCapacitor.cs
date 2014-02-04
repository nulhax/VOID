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
	public GameObject m_DUIConsole = null;
	public List<CComponentInterface> m_CircuitryComponents = new List<CComponentInterface>();
	
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
		m_CircuitryComponents[0].EventComponentBreak += HandleCircuitryBreaking;
		m_CircuitryComponents[0].EventComponentFix += HandleCircuitryFixing;
		m_CircuitryComponents[1].EventComponentBreak += HandleCircuitryBreaking;
		m_CircuitryComponents[1].EventComponentFix += HandleCircuitryFixing;

		// Register for when the circuitry takes damage
		m_PowerStorage.EventBatteryCapacityChanged += HandleCapacitorStateChange;
		m_PowerStorage.EventBatteryChargeChanged += HandleCapacitorStateChange;

		// Get the DUI of the power generator
		m_DUIPowerCapacitor = m_DUIConsole.GetComponent<CDUIConsole>().DUI.GetComponent<CDUIPowerCapacitorRoot>();
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
		if(CNetwork.IsServer)
		{
			// Update the UI
			int index = m_CircuitryComponents.FindIndex((item) => item == _Component);
			m_DUIPowerCapacitor.SetCircuitryStateChange(index, false);

			// Get the number of working circuitry components
			int numWorkingCircuitryComps = m_CircuitryComponents.Sum((ci) => {
				return(ci.IsFunctional ? 1 : 0);
			});

			// Set the new battery capacity
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity * ((float)numWorkingCircuitryComps / 2.0f);

			// Deactive the charge availablity
			if(numWorkingCircuitryComps == 0)
			{
				m_PowerStorage.DeactivateBatteryChargeAvailability();
			}
		}
	}
	
	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			// Update the UI
			int index = m_CircuitryComponents.FindIndex((item) => item == _Component);
			m_DUIPowerCapacitor.SetCircuitryStateChange(index, false);

			// Get the number of working circuitry components
			int numWorkingCircuitryComps = m_CircuitryComponents.Sum((ci) => {
				return(ci.IsFunctional ? 1 : 0);
			});

			// Set the new battery capacity
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity * ((float)numWorkingCircuitryComps / 2.0f);

			// Activate the charge availablity
			m_PowerStorage.ActivateBatteryChargeAvailability();
		}
	}
}
