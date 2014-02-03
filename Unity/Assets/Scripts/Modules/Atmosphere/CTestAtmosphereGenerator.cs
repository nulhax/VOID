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


[RequireComponent(typeof(CAtmosphereGeneratorBehaviour))]
public class CTestAtmosphereGenerator: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_AtmosphereGenerationRate = 60.0f;

    public float m_AtmosphereGenerationRateDamaged = 10.0f;

	private float m_PrevAtmosphereGenerationRate = 0.0f;
	private CAtmosphereGeneratorBehaviour m_AtmosphereGenerator = null;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_AtmosphereGenerator = gameObject.GetComponent<CAtmosphereGeneratorBehaviour>();

		// Register for when the fusebox breaks/fixes
		//CComponentInterface ci = gameObject.GetComponent<CModuleInterface>().FindAttachedComponentsByType(CComponentInterface.EType.RatchetComp)[0].GetComponent<CComponentInterface>();
		//ci.EventComponentBreak += HandleFuseBoxBreaking;
		//ci.EventComponentFix += HandleFuseBoxFixing;

        //gameObject.GetComponent<CComponentInterface>().EventComponentFix += HandleFuseBoxFixing;
        //gameObject.GetComponent<CComponentInterface>().EventComponentBreak += HandleFuseBoxBreaking;
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			if(m_PrevAtmosphereGenerationRate != m_AtmosphereGenerationRate)
			{
				m_AtmosphereGenerator.AtmosphereGenerationRate = m_AtmosphereGenerationRate;

				m_PrevAtmosphereGenerationRate = m_AtmosphereGenerationRate;
			}


            if (transform.FindChild("RatchetComponent").GetComponent<CActorHealth>().health <= 0)
            {
                Debug.Log("Atmosphere Generator Broke");
                Debug.Log(transform.FindChild("RatchetComponent").GetComponent<CActorHealth>().health);
                HandleFuseBoxBreaking();
            }

            else if (transform.FindChild("RatchetComponent").GetComponent<CActorHealth>().health >= 1)
            {
                Debug.Log("Atmosphere Generator Fixed");
                HandleFuseBoxFixing();
            }
		}
	}

	private void HandleFuseBoxBreaking()
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.DeactivateGeneration();
		}
	}
	
	private void HandleFuseBoxFixing()
	{
		if(CNetwork.IsServer)
		{
			m_AtmosphereGenerator.ActivateGeneration();
		}
	}
}
