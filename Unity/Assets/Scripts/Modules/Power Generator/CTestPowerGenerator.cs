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
	public CDUIConsole m_DUIConsole = null;

	public CComponentInterface m_CircuitryComponent = null;
	public CComponentInterface m_CalibrationComponent = null;

	private CPowerGenerationBehaviour m_PowerGenerator = null;
	private CDUIPowerGeneratorRoot m_DUIPowerGeneration = null;

	private int m_AmbientHumSoundIndex = -1;

	// Member Properties
	
	
	// Member Methods

	void Awake()
	{
		CAudioCue audioCue = GetComponent<CAudioCue>();
		if (audioCue == null)
			audioCue = gameObject.AddComponent<CAudioCue>();
		//m_AmbientHumSoundIndex = audioCue.AddSound("Audio/PowerGeneratorAmbientHum", 0.0f, 0.0f, true);
	}

	public void Start()
	{
		m_PowerGenerator = gameObject.GetComponent<CPowerGenerationBehaviour>();

		// Register for when the calibrator breaks/fixes
		m_CalibrationComponent.EventHealthChange += HandleCalibrationHealthChange;
		
		// Register for when the circuitry breaks/fixes
		m_CircuitryComponent.EventComponentBreak += HandleCircuitryBreaking;
		m_CircuitryComponent.EventComponentFix += HandleCircuitryFixing;

		// Get the DUI of the power generator
		m_DUIPowerGeneration = m_DUIConsole.DUIRoot.GetComponent<CDUIPowerGeneratorRoot>();
		m_DUIPowerGeneration.RegisterPowerGenerator(gameObject);

		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate;
			m_PowerGenerator.PowerGenerationRatePotential = m_MaxPowerGenerationRate;
		}

		// Set the cubemap for the children
		foreach(Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.material.SetTexture("_Cube", gameObject.GetComponent<CModuleInterface>().CubeMapSnapshot);
		}

		// Begin playing the sound.
		// Todo: Once individual sounds can be disabled, this must be moved to where the power generator turns on and off.
		GetComponent<CAudioCue>().Play(transform, 0.25f, true, 0);
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
            GetComponent<CAudioCue>().StopAllSound();
		}
	}

	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.ActivatePowerGeneration();
            GetComponent<CAudioCue>().Play(transform, 0.25f, true, 0);
		}
	}
}
