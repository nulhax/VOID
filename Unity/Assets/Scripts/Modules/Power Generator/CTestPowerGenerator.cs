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
	public float m_PowerGenerationRate = 15.0f;
	
	private float m_PrevPowerGenerationRate = 0.0f;
	private CPowerGenerationBehaviour m_PowerGenerator = null;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PowerGenerator = gameObject.GetComponent<CPowerGenerationBehaviour>();

		// Register for when the fusebox breaks/fixes
		CFuseBoxBehaviour fbc = gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.FuseBox)[0].GetComponent<CFuseBoxBehaviour>();
		fbc.EventBroken += HandleFuseBoxBreaking;
		fbc.EventFixed += HandleFuseBoxFixing;
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			if(m_PrevPowerGenerationRate != m_PowerGenerationRate)
			{
				m_PowerGenerator.PowerGenerationRate = m_PowerGenerationRate;
			
				m_PrevPowerGenerationRate = m_PowerGenerationRate;
			}
		}
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
}
