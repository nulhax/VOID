//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTestPowerGenerator.cs
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


[RequireComponent(typeof(CPowerGenerationBehaviour))]
public class CTestPowerGenerator: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_MaxPowerGenerationRate = 15.0f;
	public GameObject m_DUIConsole = null;

	private CPowerGenerationBehaviour m_PowerGenerator = null;
	private CDUIPowerGeneratorRoot m_DUIPowerGeneration = null;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PowerGenerator = gameObject.GetComponent<CPowerGenerationBehaviour>();

		// Register for when the calibrator breaks/fixes
		CComponentInterface ci = gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.CalibratorComp)[0].GetComponent<CComponentInterface>();
		ci.EventHealthChange += HandleCalibrationHealthChange;

		// Register for when the circuitry breaks/fixes
		ci = gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.CircuitryComp)[0].GetComponent<CComponentInterface>();
		ci.EventComponentBreak += HandleCircuitryBreaking;
		ci.EventComponentFix += HandleCircuitryFixing;

		// Register for when the generation rate changes
		m_PowerGenerator.EventGenerationRateChanged += HandleGenerationRateChange;

		// Get the DUI of the power generator
		m_DUIPowerGeneration = m_DUIConsole.GetComponent<CDUIConsole>().DUI.GetComponent<CDUIPowerGeneratorRoot>();

		// Set the generation rate
		m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate;
	}

	private void HandleCalibrationHealthChange(CComponentInterface _Component, CActorHealth _ComponentHealth)
	{
		m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate * (_ComponentHealth.health / _ComponentHealth.health_initial);
	}

	private void HandleCircuitryBreaking(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.DeactivatePowerGeneration();
		}
	}

	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.ActivatePowerGeneration();
		}
	}

	private void HandleGenerationRateChange(GameObject _PowerGen)
	{
		// Update the UI generation rate
		if(m_DUIPowerGeneration != null)
		{
			m_DUIPowerGeneration.SetPowerGenerationRate(m_PowerGenerator.PowerGenerationRate, m_MaxPowerGenerationRate);
		}
	}
}
