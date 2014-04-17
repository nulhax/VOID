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


[RequireComponent(typeof(CPowerGeneratorInterface))]
public class CPowerGeneratorSmallBehaviour: MonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events
	

// Member Properties
	
	
// Member Methods


	void Awake()
	{
		CAudioCue audioCue = GetComponent<CAudioCue>();

		if (audioCue == null)
			audioCue = gameObject.AddComponent<CAudioCue>();
	}


	void Start()
	{
        m_cModuleInterface = GetComponent<CModuleInterface>();

        if (CNetwork.IsServer)
        {
            // Register for when the calibrator breaks/fixes
            m_CalibrationComponent.GetComponent<CActorHealth>().EventOnSetHealth += OnEventComponentHealthChange;

            // Register for when the circuitry breaks/fixes
            m_CircuitryComponent.EventBreakStateChange += HandleCircuitryBreaking;
            m_CircuitryComponent.EventBreakStateChange += HandleCircuitryFixing;
        }

		// Set the cubemap for the children
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.material.SetTexture("_Cube", transform.parent.GetComponent<CModulePortInterface>().CubeMapSnapshot);
		}

		// Begin playing the sound.
		// Todo: Once individual sounds can be disabled, this must be moved to where the power generator turns on and off.
		GetComponent<CAudioCue>().Play(transform, 0.25f, true, 0);
	}


    [AServerOnly]
    void OnEventComponentHealthChange(CActorHealth _cSender, float _fPreviousHealth, float _fNewHealth)
	{
		if (CNetwork.IsServer)
		{
            m_cModuleInterface.SetFuntionalRatio(_fNewHealth / _cSender.health_initial);
		}
	}


    [AServerOnly]
    void HandleCircuitryBreaking(CComponentInterface _cSender, bool _bBroken)
	{
		//m_PowerGenerator.DeactivatePowerGeneration();
        GetComponent<CAudioCue>().StopAllSound();
	}


    [AServerOnly]
    void HandleCircuitryFixing(CComponentInterface _cSender, bool _bBroken)
	{
		//m_PowerGenerator.ActivatePowerGeneration();
        GetComponent<CAudioCue>().Play(transform, 0.25f, true, 0);
	}


// Member Fields


    public CDUIConsole m_DUIConsole = null;

    public CComponentInterface m_CircuitryComponent = null;
    public CComponentInterface m_CalibrationComponent = null;


    CModuleInterface m_cModuleInterface = null;


}
