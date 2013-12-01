//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   COxygenProduction.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//



// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class COxygenProduction : CNetworkMonoBehaviour 
{

// Globals

	
// Member Types


// Member Delegates & Events


// Member Properties
public bool bIsOxygen
{
	get { return (m_bIsOxygenated.Get()); }
	set { m_bIsOxygenated.Set(value); }
}
	
	public float fOxygens
	{
		get { return (m_fOxygens.Get()); }
		set { m_fOxygens.Set(value); }
	}
	
// Member Functions
	
	public override void InstanceNetworkVars()
	{
		m_bIsOxygenated = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_fOxygens = new CNetworkVar<float>(OnNetworkVarSync, 100.0f);
		m_fCO2s = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Get oxygen amount from plants
		
		// Get Room collider
		
		// ray cast to ball
		
		// if ball is increase
			// Increase oxygen
		
		// if Decrease then decrease from oxygen
			// But increase CO2
		
		// if cO2 is > 60% then start causing player damage
			// Incrememnt as the cO2 level increases
	}
	
	public void OnDestroy()
	{
		
	}
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{

	}
	
	float GetOxygenAmount()
	{
		return(m_fOxygens.Get());
	}
// Member Fields
	CNetworkVar<bool> m_bIsOxygenated;
	CNetworkVar<float> m_fOxygens;
	CNetworkVar<float> m_fCO2s;

}
