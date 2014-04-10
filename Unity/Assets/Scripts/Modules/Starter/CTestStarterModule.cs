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

[RequireComponent(typeof(CPowerGenerationBehaviour))]
[RequireComponent(typeof(CPowerStorageBehaviour))]
[RequireComponent(typeof(CNaniteStorage))]
[RequireComponent(typeof(CAtmosphereGeneratorBehaviour))]
public class CTestStarterModule: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_MaxPowerGenerationRate = 10.0f;
	public float m_MaxPowerBatteryCapacity = 500.0f;
	public float m_MaxNaniteCapacity = 500.0f;
	public float m_MaxAtmosphereGenerationRate = 30.0f;

	public CDUIConsole m_PowerGenConsole = null;
	public CDUIConsole m_PowerCapConsole = null;
	public CDUIConsole m_NaniteConsole = null;
	public CDUIConsole m_AtmosConsole = null;

	private CPowerGenerationBehaviour m_PowerGenerator = null;
	private CDUIPowerGeneratorRoot m_DUIPowerGeneration = null;
	
	private CPowerStorageBehaviour m_PowerStorage = null;
	private CDUIPowerCapacitorRoot m_DUIPowerCapacitor = null;
	
	private CAtmosphereGeneratorBehaviour m_AtmosphereGenerator = null;
	private CDUIAtmosphereGeneratorRoot m_DUIAtmosphereGeneration = null;

	private CNaniteStorage m_NaniteStorage = null;
	private CDUINaniteCapsuleRoot m_DUINaniteCapsule = null;

	private int m_AmbientHumSoundIndex = -1;
	
	
	// Member Properties
	
	
	// Member Methods
	
	void Awake()
	{
		CAudioCue audioCue = GetComponent<CAudioCue>();
		if (audioCue == null)
		{
			audioCue = gameObject.AddComponent<CAudioCue>();
		}
		//m_AmbientHumSoundIndex = audioCue.AddSound("Audio/AtmosphereGeneratorAmbientHum", 0.0f, 0.0f, true);
	}
	
	public void Start()
	{
		// Initialise the systems
		InitialiseAtmosphere();
		InitialisePowerGenerator();
		InitialisePowerCapacitor();
		InitialiseNantieStorage();
		
		// Begin playing the sound.
		// Todo: Once individual sounds can be disabled, this must be moved to where the engine turns on and off.
		GetComponent<CAudioCue>().Play(transform, 0.25f, true, 0);
	}
	
	private void InitialiseAtmosphere()
	{
		m_AtmosphereGenerator = gameObject.GetComponent<CAtmosphereGeneratorBehaviour>();
		
//		// Get the DUI of the power generator
//		m_DUIAtmosphereGeneration = m_AtmosConsole.DUIRoot.GetComponent<CDUIAtmosphereGeneratorRoot>();
//		m_DUIAtmosphereGeneration.RegisterAtmosphereGenerator(gameObject);
		
		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_AtmosphereGenerator.AtmosphereGenerationRate = m_MaxAtmosphereGenerationRate;
		}
	}

	private void InitialisePowerGenerator()
	{
		m_PowerGenerator = gameObject.GetComponent<CPowerGenerationBehaviour>();
		
//		// Get the DUI of the power generator
//		m_DUIPowerGeneration = m_PowerGenConsole.DUIRoot.GetComponent<CDUIPowerGeneratorRoot>();
//		m_DUIPowerGeneration.RegisterPowerGenerator(gameObject);
		
		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate;
			m_PowerGenerator.PowerGenerationRatePotential = m_MaxPowerGenerationRate;
		}
	}

	private void InitialisePowerCapacitor()
	{
		m_PowerStorage = gameObject.GetComponent<CPowerStorageBehaviour>();
		
//		// Get the DUI of the power generator
//		m_DUIPowerCapacitor = m_PowerCapConsole.DUIRoot.GetComponent<CDUIPowerCapacitorRoot>();
//		m_DUIPowerCapacitor.RegisterPowerCapacitor(gameObject);
		
		// Debug: Set the charge to half its total capacity
		if(CNetwork.IsServer)
		{
			m_PowerStorage.BatteryCapacity = m_MaxPowerBatteryCapacity;
			m_PowerStorage.BatteryCharge = m_PowerStorage.BatteryCapacity / 2;
		}
	}

	private void InitialiseNantieStorage()
	{
		m_NaniteStorage = gameObject.GetComponent<CNaniteStorage>();

//		// Get the DUI of the power generator
//		m_DUINaniteCapsule = m_NaniteConsole.DUIRoot.GetComponent<CDUINaniteCapsuleRoot>();
//		m_DUINaniteCapsule.RegisterNaniteCapsule(gameObject);
		
		if(CNetwork.IsServer)
		{
			m_NaniteStorage.SetCapacity(m_MaxNaniteCapacity);
		}
	}
}
