//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CBridgeLifeSupportSystem.cs
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


[RequireComponent(typeof(CAtmosphereGeneratorBehaviour))]
public class CTestAtmosphereGenerator: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_MaxAtmosphereGenerationRate = 60.0f;
	public CDUIConsole m_DUIConsole = null;
	
	public CComponentInterface m_CircuitryComponent = null;
	public CComponentInterface m_LiquidComponent = null;
	
	private CAtmosphereGeneratorBehaviour m_AtmosphereGenerator = null;
	private CDUIAtmosphereGeneratorRoot m_DUIAtmosphereGeneration = null;


	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_AtmosphereGenerator = gameObject.GetComponent<CAtmosphereGeneratorBehaviour>();

		// Register for when the calibrator breaks/fixes
		m_LiquidComponent.EventHealthChange += HandleFluidHealthChange;
		
		// Register for when the circuitry breaks/fixes
		m_CircuitryComponent.EventComponentBreak += HandleCircuitryBreaking;
		m_CircuitryComponent.EventComponentFix += HandleCircuitryFixing;

		// Register for when the generation rate changes
		m_AtmosphereGenerator.EventGenerationRateChanged += HandleGenerationRateChange;
		
		// Get the DUI of the power generator
		m_DUIAtmosphereGeneration = m_DUIConsole.DUI.GetComponent<CDUIAtmosphereGeneratorRoot>();

		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_AtmosphereGenerator.AtmosphereGenerationRate = m_MaxAtmosphereGenerationRate;
		}
	}

	private void HandleFluidHealthChange(CComponentInterface _Component, CActorHealth _ComponentHealth)
	{
		m_AtmosphereGenerator.AtmosphereGenerationRate = m_MaxAtmosphereGenerationRate * (_ComponentHealth.health / _ComponentHealth.health_initial);
	}
	
	private void HandleCircuitryBreaking(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.DeactivateGeneration();
		}
		
		// Update the UI generation active
		m_DUIAtmosphereGeneration.SetAtmosphereGenerationActive(false);
	}
	
	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.ActivateGeneration();
		}
		
		// Update the UI generation active
		m_DUIAtmosphereGeneration.SetAtmosphereGenerationActive(true);
	}
	
	private void HandleGenerationRateChange(CAtmosphereGeneratorBehaviour _AtmosphereGen)
	{
		// Update the UI generation rate
		m_DUIAtmosphereGeneration.SetAtmosphereGenerationRate(m_AtmosphereGenerator.AtmosphereGenerationRate, m_MaxAtmosphereGenerationRate);
	}
}
