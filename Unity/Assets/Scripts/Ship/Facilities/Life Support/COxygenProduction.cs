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
const float g_cfMAXROOMAIR = 100.0f;

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
	
	public float fCO2
	{
		get { return (m_fCO2s.Get()); }
		set { m_fCO2s.Set(value); }	
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
		
	}
	
	public void OnDestroy()
	{
		
	}
	
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{

	}
	
	void OnTriggerEnter(Collider _ColliderObject)
	{
		if (CNetwork.IsServer)
        {

		}
	}
	float GetOxygenAmount()
	{
		return(m_fOxygens.Get());
	}
	
	float GetCO2Amount()
	{
		return(m_fCO2s.Get());
	}
	
	// Balance the Co2 with the O2 
	// If one decreases, the other should increase and vice versa
	void BalanceO2vsCO2(float _fOxygenChange, float _fCO2Change)
	{

		
	}
// Member Fields
	CNetworkVar<bool> m_bIsOxygenated;
	CNetworkVar<float> m_fOxygens;
	CNetworkVar<float> m_fCO2s;
}
