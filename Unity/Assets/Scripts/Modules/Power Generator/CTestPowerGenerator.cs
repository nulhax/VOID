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

	private CPowerGenerationBehaviour m_PowerGenerator = null;
	private CDUIPowerGeneratorRoot m_DUIPowerGeneration = null;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PowerGenerator = gameObject.GetComponent<CPowerGenerationBehaviour>();

		// Register for when the fusebox breaks/fixes
		//CFuseBoxBehaviour fbc = gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.FuseBox)[0].GetComponent<CFuseBoxBehaviour>();
		//fbc.EventBroken += HandleFuseBoxBreaking;
		//fbc.EventFixed += HandleFuseBoxFixing;

		// Register for when the generation rate changes
		m_PowerGenerator.EventGenerationRateChanged += HandleGenerationRateChange;

		// Get the DUI of the power generator
		m_DUIPowerGeneration = m_DUIConsole.GetComponent<CDUIConsole>().DUI.GetComponent<CDUIPowerGeneratorRoot>();

		// Set the generation rate
		m_PowerGenerator.PowerGenerationRate = m_MaxPowerGenerationRate;
	}

	private void HandleFuseBoxBreaking(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.DeactivatePowerGeneration();
		}
	}

	private void HandleFuseBoxFixing(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			m_PowerGenerator.ActivatePowerGeneration();
		}
	}

	private void HandleGenerationRateChange(GameObject _PowerGen)
	{
		// Update the UI generation rate
		if(m_DUIPowerGeneration != null)
		{
			m_DUIPowerGeneration.SetPowerGenerationRate(m_PowerGenerator.PowerGenerationRate, m_MaxPowerGenerationRate);
		}
	}
}
