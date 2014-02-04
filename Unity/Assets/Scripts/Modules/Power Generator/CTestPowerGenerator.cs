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

	public CComponentInterface m_CircuitryComponent = null;
	public CComponentInterface m_CalibrationComponent = null;

	private CPowerGenerationBehaviour m_PowerGenerator = null;
	private CDUIPowerGeneratorRoot m_DUIPowerGeneration = null;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PowerGenerator = gameObject.GetComponent<CPowerGenerationBehaviour>();

		// Register for when the calibrator breaks/fixes
		m_CalibrationComponent.EventHealthChange += HandleCalibrationHealthChange;
		
		// Register for when the circuitry breaks/fixes
		m_CircuitryComponent.EventComponentBreak += HandleCircuitryBreaking;
		m_CircuitryComponent.EventComponentFix += HandleCircuitryFixing;
		
		// Register for when the generation rate changes
		m_PowerGenerator.EventGenerationRateChanged += HandleGenerationRateChange;

		// Get the DUI of the power generator
		m_DUIPowerGeneration = m_DUIConsole.GetComponent<CDUIConsole>().DUI.GetComponent<CDUIPowerGeneratorRoot>();

		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate;
		}
	}

	private void HandleCalibrationHealthChange(CComponentInterface _Component, CActorHealth _ComponentHealth)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate * (_ComponentHealth.health / _ComponentHealth.health_initial);
		}
	}

	private void HandleCircuitryBreaking(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.DeactivatePowerGeneration();
		}

		// Update the UI generation active
		m_DUIPowerGeneration.SetPowerGenerationActive(false);
	}

	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.ActivatePowerGeneration();
		}

		// Update the UI generation active
		m_DUIPowerGeneration.SetPowerGenerationActive(true);
	}

	private void HandleGenerationRateChange(CPowerGenerationBehaviour _PowerGen)
	{
		// Update the UI generation rate
		m_DUIPowerGeneration.SetPowerGenerationRate(m_PowerGenerator.PowerGenerationRate, m_MaxPowerGenerationRate);
	}
}
