//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomPower.cs
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


public class CFacilityPower : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


    public float PowerConsumption
    {
        set { m_fPowerConsumption.Set(value); }
        get { return (m_fPowerConsumption.Get()); }
    }


// Member Functions


	public override void InstanceNetworkVars()
	{
		m_fPowerConsumption = new CNetworkVar<float>(OnNetworkVarSync);
	}


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
        // Empty
	}


	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
        // Empty
	}


// Member Fields


	CNetworkVar<float> m_fPowerConsumption;
};
