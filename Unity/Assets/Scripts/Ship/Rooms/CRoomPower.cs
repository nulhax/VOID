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


public class CRoomPower : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	float PowerConsumption { get { return (m_fPowerConsumption.Get()); } }


// Member Functions


	public override void InstanceNetworkVars()
	{
		m_fPowerConsumption = new CNetworkVar<float>(OnNetworkVarSync);
	}


	public void Start()
	{
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}


	void OnNetworkVarSync(INetworkVar _cVarInstance)
	{
	}


// Member Fields


	CNetworkVar<float> m_fPowerConsumption;


};
