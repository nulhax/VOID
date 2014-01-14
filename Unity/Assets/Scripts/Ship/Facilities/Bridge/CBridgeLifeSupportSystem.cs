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

[RequireComponent(typeof(CLifeSupportSystem))]
public class CBridgeLifeSupportSystem: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_AtmosphereGenerationRate = 15.0f;
	public float m_AtmosphereCapacitySupport = 2000.0f;

	private float m_PrevAtmosphereGenerationRate = 0.0f;
	private float m_PrevAtmosphereCapacitySupport = 0.0f;

	// Member Properties
	
	
	// Member Methods
	public void Update()
	{
		if(m_PrevAtmosphereGenerationRate != m_AtmosphereGenerationRate || 
		   m_AtmosphereCapacitySupport != m_PrevAtmosphereCapacitySupport)
		{
			CLifeSupportSystem lifeSupportSystem = gameObject.GetComponent<CLifeSupportSystem>();

			lifeSupportSystem.AtmosphereDistributionRate = m_AtmosphereGenerationRate;
			lifeSupportSystem.AtmosphereCapacitySupport = m_AtmosphereCapacitySupport;

			m_PrevAtmosphereGenerationRate = m_AtmosphereGenerationRate;
			m_PrevAtmosphereCapacitySupport = m_AtmosphereCapacitySupport;
		}
	}
}
