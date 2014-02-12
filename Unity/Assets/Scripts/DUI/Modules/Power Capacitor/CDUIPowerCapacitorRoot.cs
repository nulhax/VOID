//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIPowerGeneratorRoot.cs
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


public class CDUIPowerCapacitorRoot : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIProgressBar m_CapacityBar = null;
	public UILabel m_Charge = null;
	public UILabel m_CapacitorStatus = null;
	public UILabel m_ErrorReport = null;
	public UILabel m_WarningReport = null;
	
	private GameObject m_PowerCapacitor = null;
	private CPowerStorageBehaviour m_CachedPowerStorageBehaviour = null;
	private CTestPowerCapacitor m_CachedPowerCapacitor = null;


	// Member Properties

	
	// Member Methods
	public void RegisterPowerCapacitor(GameObject _PowerCapacitor)
	{
		m_PowerCapacitor = _PowerCapacitor;
		m_CachedPowerStorageBehaviour = m_PowerCapacitor.GetComponent<CPowerStorageBehaviour>();
		m_CachedPowerCapacitor = m_PowerCapacitor.GetComponent<CTestPowerCapacitor>();

		// Register charge/capacity state chages
		m_CachedPowerStorageBehaviour.EventBatteryCapacityChanged += HandleCapacitorStateChange;
		m_CachedPowerStorageBehaviour.EventBatteryChargeChanged += HandleCapacitorStateChange;
		
		// Register for when the circuitry breaks/fixes
		m_CachedPowerCapacitor.m_Circuitry1.EventComponentBreak += HandleCircuitryStateChange;
		m_CachedPowerCapacitor.m_Circuitry1.EventComponentFix += HandleCircuitryStateChange;
		m_CachedPowerCapacitor.m_Circuitry2.EventComponentBreak += HandleCircuitryStateChange;
		m_CachedPowerCapacitor.m_Circuitry2.EventComponentFix += HandleCircuitryStateChange;

		// Update initial values
		UpdateDUI();
	}

	private void HandleCapacitorStateChange(CPowerStorageBehaviour _Capacitor)
	{
		UpdateDUI();
	}

	private void HandleCircuitryStateChange(CComponentInterface _Component)
	{
		UpdateDUI();
	}

	private void UpdateDUI()
	{
		UpdateCapacitorVariables();
		UpdateCircuitryStates();
	}

	private void UpdateCapacitorVariables()
	{
		// Get the current charge, intial capacity and current capacity
		float currentCharge = m_CachedPowerStorageBehaviour.BatteryCharge;
		float currentCapacity = m_CachedPowerStorageBehaviour.BatteryCapacity;
		float initialCapacity = m_CachedPowerCapacitor.m_MaxPowerBatteryCapacity;

		// Update the charge value
		m_CapacityBar.value = currentCharge/currentCapacity;

		// Update the bar colors
		float value = currentCapacity/initialCapacity;
		CDUIUtilites.LerpBarColor(value, m_CapacityBar);
		
		// Update the label
		m_Charge.text = currentCharge.ToString() + " / " + currentCapacity.ToString();
	}

	private void UpdateCircuitryStates()
	{
		int numWorkingComponents = m_CachedPowerCapacitor.NumWorkingCircuitryComponents;

		if(numWorkingComponents == 0)
		{
			m_Charge.color = Color.red;
			m_CapacitorStatus.color = Color.red;
			m_CapacitorStatus.text = "Status: Charge InActive";

			m_WarningReport.enabled = false;
			m_ErrorReport.enabled = true;

			m_ErrorReport.color = Color.red;
			m_ErrorReport.text = "Warning: All circuitry components defective!";
		}
		else if(numWorkingComponents == 1)
		{
			m_Charge.color = Color.yellow;
			m_CapacitorStatus.color = Color.yellow;
			m_CapacitorStatus.text = "Status: Charge NonOptimal";

			m_WarningReport.enabled = true;
			m_ErrorReport.enabled = false;

			m_WarningReport.color = Color.yellow;
			m_WarningReport.text = "Warning: Circuitry component defective!";
		}
		else
		{
			m_Charge.color = Color.cyan;
			m_CapacitorStatus.color = Color.green;
			m_CapacitorStatus.text = "Status: Charge Active";

			m_WarningReport.enabled = false;
			m_ErrorReport.enabled = false;
		}
	}
}
