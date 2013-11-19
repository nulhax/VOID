//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNaniteReplicator.cs
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

public class CNaniteReplicator : CNetworkMonoBehaviour 
{
	
// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions

	public override void InstanceNetworkVars()
	{

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
		// Get the size of the asteroid input into the replicator	
		// Add the unit size to the amount of nanites in the replicator
	
	}
	
	
	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
		
	}
	// Member Fields
	float m_fAsteroidSize;
	float m_fTotalNanites;
}
