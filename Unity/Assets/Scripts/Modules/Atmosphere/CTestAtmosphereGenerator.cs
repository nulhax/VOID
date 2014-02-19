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
		
		// Get the DUI of the power generator
		m_DUIAtmosphereGeneration = m_DUIConsole.DUI.GetComponent<CDUIAtmosphereGeneratorRoot>();
		m_DUIAtmosphereGeneration.RegisterAtmosphereGenerator(gameObject);

        gameObject.GetComponent<CAudioCue>().Play(0.1f, true, 0);

		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_AtmosphereGenerator.AtmosphereGenerationRate = m_MaxAtmosphereGenerationRate;
		}
	}

	private void HandleFluidHealthChange(CComponentInterface _Component, CActorHealth _ComponentHealth)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.AtmosphereGenerationRate = m_MaxAtmosphereGenerationRate * (_ComponentHealth.health / _ComponentHealth.health_initial);
		}
	}
	
	private void HandleCircuitryBreaking(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.DeactivateGeneration();
            gameObject.GetComponent<CAudioCue>().StopAllSound();
		}
	}
	
	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.ActivateGeneration();
            gameObject.GetComponent<CAudioCue>().Play(0.1f, true, 0);
		}
	}
}
