//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerGeneratorSystem.cs
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


public class CPowerGeneratorSystem : CNetworkMonoBehaviour 
{
	
// Member Types


// Member Delegates & Events


// Member Fields
	CNetworkVar<float> m_fPowerGenerationRate;
	CNetworkVar<bool> m_PowerGenerationActive;


// Member Properties
	public float PowerGenerationRate
	{ 
		get { return (m_fPowerGenerationRate.Get()); }

		[AServerOnly]
		set { m_fPowerGenerationRate.Set(value); }
	}
	
	public bool PowerGenerationEnabled
	{
		get { return (m_PowerGenerationActive.Get()); }

		[AServerOnly]
		set { m_PowerGenerationActive.Set(value); }
	}

// Member Functions

	public override void InstanceNetworkVars()
	{
		m_fPowerGenerationRate = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_PowerGenerationActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		if(_cVarInstance == m_PowerGenerationActive)
		{
			if(m_PowerGenerationActive.Get() == true)
			{	
				CGame.Ship.GetComponent<CShipPowerSystem>().RegisterPowerGeneratorSystem(gameObject);
			}
			else
			{
				CGame.Ship.GetComponent<CShipPowerSystem>().RegisterPowerGeneratorSystem(gameObject);
			}
		}
	}
}
