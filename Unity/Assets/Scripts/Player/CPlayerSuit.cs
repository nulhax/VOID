//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerSuit.cs
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


public class CPlayerSuit : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions


	public override void InstanceNetworkVars()
	{
        m_fOxygen = new CNetworkVar<float>(OnNetworkVarSync, 100.0f);
	}


	void Start()
	{
        // Empty
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		
	}


    void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    {
    }


// Member Fields


    const float k_fOxygenDepleteRate = 1.0f;


    CNetworkVar<float> m_fOxygen = null;


};
