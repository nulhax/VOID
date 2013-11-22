//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPowerGenerator.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Implementation */
public class CPowerGenerator : CNetworkMonoBehaviour 
{
	
// Member Types


// Member Delegates & Events


// Member Properties
	public float PowerOutput 
	{ 
		get { return (m_fGeneratePower.Get()); }
		set { m_fGeneratePower.Set(value); }
	}
	
	public bool PowerEnabled
	{
		get { return (m_bIsPowered.Get()); }
		set { m_bIsPowered.Set(value); }
	}

// Member Functions

	public override void InstanceNetworkVars()
	{
		m_fGeneratePower = new CNetworkVar<float>(OnNetworkVarSync, 1000.0f);
		m_bIsPowered = new CNetworkVar<bool>(OnNetworkVarSync, true);
	}
	
	
// Use this for initialization
	void Start () 
	{
		
	}
	
	public void OnDestroy()
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		
	}
	
	// Member Fields
	CNetworkVar<float> m_fGeneratePower;
	CNetworkVar<bool> m_bIsPowered;
}
