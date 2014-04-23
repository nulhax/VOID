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


[RequireComponent(typeof(CAtmosphereGeneratorInterface))]
public class CAtmosphereGeneratorSmallBehaviour: MonoBehaviour 
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
            // Register for when the calibrator health changes
            m_cLiquidComponent.GetComponent<CActorHealth>().EventOnSetHealth += HandleFluidHealthChange;
        }

		// Begin playing the sound.
		// Todo: Once individual sounds can be disabled, this must be moved to where the engine turns on and off.
		GetComponent<CAudioCue>().Play(transform, 0.25f, true, 0);
	}


    [AServerOnly]
	void HandleFluidHealthChange(CActorHealth _cSender, float _fPreviousHealth, float _fNewHealth)
	{
        m_cModuleInterface.SetFuntionalRatio(_fNewHealth / _cSender.health_initial);

        //gameObject.GetComponentInChildren<Light>().intensity = (m_cModuleInterface.FunctioanlRatio);
	}


// Member Fields


    public CDUIConsole m_cDuiConsole = null;
    public CComponentInterface m_cCircuitryComponent = null;
    public CComponentInterface m_cLiquidComponent = null;


    CModuleInterface m_cModuleInterface = null;


}
