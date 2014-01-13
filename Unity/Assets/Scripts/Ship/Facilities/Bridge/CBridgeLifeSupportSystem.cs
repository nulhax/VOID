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

public class CBridgeLifeSupportSystem: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_AtmosphereGenerationRate = 20.0f;
	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		gameObject.GetComponent<CLifeSupportAtmosphereDistribution>().AtmosphereDistributionRate = m_AtmosphereGenerationRate;
	}
}
