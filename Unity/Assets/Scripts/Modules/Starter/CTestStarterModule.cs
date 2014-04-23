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

[RequireComponent(typeof(CPowerGeneratorInterface))]
[RequireComponent(typeof(CPowerBatteryInterface))]
[RequireComponent(typeof(CNaniteStorage))]
[RequireComponent(typeof(CAtmosphereGeneratorInterface))]
public class CTestStarterModule: MonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events
	
	
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
	

	void InitialiseAtmosphere()
	{
		m_AtmosphereGenerator = gameObject.GetComponent<CAtmosphereGeneratorInterface>();
		
//		// Get the DUI of the power generator
//		m_DUIAtmosphereGeneration = m_AtmosConsole.DUIRoot.GetComponent<CDUIAtmosphereGeneratorRoot>();
//		m_DUIAtmosphereGeneration.RegisterAtmosphereGenerator(gameObject);
	}


	void InitialisePowerGenerator()
	{
		m_PowerGenerator = gameObject.GetComponent<CPowerGeneratorInterface>();
		
//		// Get the DUI of the power generator
//		m_DUIPowerGeneration = m_PowerGenConsole.DUIRoot.GetComponent<CDUIPowerGeneratorRoot>();
//		m_DUIPowerGeneration.RegisterPowerGenerator(gameObject);
	}


	void InitialisePowerCapacitor()
	{
		m_PowerStorage = gameObject.GetComponent<CPowerBatteryInterface>();
		
//		// Get the DUI of the power generator
//		m_DUIPowerCapacitor = m_PowerCapConsole.DUIRoot.GetComponent<CDUIPowerCapacitorRoot>();
//		m_DUIPowerCapacitor.RegisterPowerCapacitor(gameObject);
	}


	void InitialiseNantieStorage()
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


// Member Fields


    public float m_MaxPowerGenerationRate = 10.0f;
    public float m_MaxPowerBatteryCapacity = 500.0f;
    public float m_MaxNaniteCapacity = 500.0f;
    public float m_MaxAtmosphereGenerationRate = 30.0f;

    public CDUIConsole m_PowerGenConsole = null;
    public CDUIConsole m_PowerCapConsole = null;
    public CDUIConsole m_NaniteConsole = null;
    public CDUIConsole m_AtmosConsole = null;

    CPowerGeneratorInterface m_PowerGenerator = null;
    CDUIPowerGeneratorRoot m_DUIPowerGeneration = null;

    CPowerBatteryInterface m_PowerStorage = null;
    CDUIPowerCapacitorRoot m_DUIPowerCapacitor = null;

    CAtmosphereGeneratorInterface m_AtmosphereGenerator = null;
    CDUIAtmosphereGeneratorRoot m_DUIAtmosphereGeneration = null;

    CNaniteStorage m_NaniteStorage = null;
    CDUINaniteCapsuleRoot m_DUINaniteCapsule = null;

    int m_AmbientHumSoundIndex = -1;


}
