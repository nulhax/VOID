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

		// Get the DUI of the power generator
		m_DUIPowerGeneration = m_DUIConsole.DUI.GetComponent<CDUIPowerGeneratorRoot>();
		m_DUIPowerGeneration.RegisterPowerGenerator(gameObject);

        gameObject.GetComponent<CAudioCue>().Play(0.3f, true, 0);

		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate;
			m_PowerGenerator.PowerGenerationRatePotential = m_MaxPowerGenerationRate;
		}

		// Set the cubemap for the children
		foreach(Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.material.SetTexture("_Cube", transform.parent.GetComponent<CModulePortInterface>().CubeMapSnapshot);
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
            gameObject.GetComponent<CAudioCue>().StopAllSound();
		}
	}

	private void HandleCircuitryFixing(CComponentInterface _Component)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.ActivatePowerGeneration();
            gameObject.GetComponent<CAudioCue>().Play(0.3f, true, 0);
		}
	}
}
