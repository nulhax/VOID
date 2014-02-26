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

	private int m_AmbientHumSoundIndex = -1;


	// Member Properties


	// Member Methods

	void Awake()
	{
		CAudioCue audioCue = GetComponent<CAudioCue>();
		if (audioCue == null)
			audioCue = gameObject.AddComponent<CAudioCue>();
		m_AmbientHumSoundIndex = audioCue.AddSound("Audio/AtmosphereGeneratorAmbientHum", 0.0f, 0.0f, true);
	}

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

		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_AtmosphereGenerator.AtmosphereGenerationRate = m_MaxAtmosphereGenerationRate;
		}

		// Begin playing the sound.
		// Todo: Once individual sounds can be disabled, this must be moved to where the engine turns on and off.
		GetComponent<CAudioCue>().Play(transform, 1.0f, true, m_AmbientHumSoundIndex);
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
		}
	}
	
	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.ActivateGeneration();
		}
	}
}
