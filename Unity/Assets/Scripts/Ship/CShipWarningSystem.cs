//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipWarningSystem.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	:  
//  Mail    	:  
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShipWarningSystem : CNetworkMonoBehaviour
{
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	
	
	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	private void Start()
	{
		if(CNetwork.IsServer)
		{
			
		}
	}
	
	
	private void Update()
	{
		if(CNetwork.IsServer)
		{
		}
	}
	
	void OnNetworkVarSync(INetworkVar _cSynedVar)
	{
		// Empty
	}
};
