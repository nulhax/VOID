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


[RequireComponent(typeof(CAtmosphereConditioningBehaviour))]
public class CTestAtmosphereConditioner: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_AtmosphereCapacitySupport = 2000.0f;

	private float m_PrevAtmosphereCapacitySupport = 0.0f;
	private CAtmosphereConditioningBehaviour m_AtmosphereConditioner = null;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_AtmosphereConditioner = gameObject.GetComponent<CAtmosphereConditioningBehaviour>();

		// Register for when the fusebox breaks/fixes
		CFuseBoxBehaviour fbc = gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.FuseBox)[0].GetComponent<CFuseBoxBehaviour>();
		fbc.EventBroken += HandleFuseBoxBreaking;
		fbc.EventFixed += HandleFuseBoxFixing;
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			if(m_AtmosphereCapacitySupport != m_PrevAtmosphereCapacitySupport)
			{
				m_AtmosphereConditioner.AtmosphereCapacitySupport = m_AtmosphereCapacitySupport;

				m_PrevAtmosphereCapacitySupport = m_AtmosphereCapacitySupport;
			}
		}
	}
	
	private void HandleFuseBoxBreaking(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereConditioner.DeactivateConditioning();
		}
	}
	
	private void HandleFuseBoxFixing(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereConditioner.ActivateConditioning();
		}
	}
}
