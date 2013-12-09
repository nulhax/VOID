
//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCellSlot.cs
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


public class CCellSlot : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties
	
	
	
// Member Functions
	public override void InstanceNetworkVars()
	{
		m_bIsFunctionalityAllowed = new CNetworkVar<bool>(OnNetworkVarSync, true);
	}

// Member Methods


	public void Start()
	{
		// Empty
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		// Check it's the right cell
		// Check it's not broken
		// Attach cell to replicator in correct orientation 
		// If it's broken, stop the production of resource
	}

	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
		
	}
// Member Fields

	CNetworkVar<bool> m_bIsFunctionalityAllowed;
};
