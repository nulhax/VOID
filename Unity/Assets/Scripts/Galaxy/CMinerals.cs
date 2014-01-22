//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CMinerals.cs
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


public class CMinerals : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public float Quantity
	{ 
		set { m_fQuantity.Set(value); }
		
		get { return m_fQuantity.Get(); } 
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_fQuantity = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


	public float DecrementQuanity(float _fQuantity)
	{
		if (_fQuantity <= m_fQuantity.Get())
		{
			m_fQuantity.Set(m_fQuantity.Get() - _fQuantity);
		}
		else
		{
			_fQuantity = m_fQuantity.Get();
			m_fQuantity.Set(0.0f);
		}
		
		return (_fQuantity);
	}


	void Start()
	{
		if(CNetwork.IsServer)
		{
			//ResourceQuanity = CGalaxy.instance.CalculateAsteroidResourceAmount(CGalaxy.instance.RelativePointToAbsoluteCell(transform.position));
		}
	}


	void Update()
	{
		// Empty
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_fQuantity)
		{
			gameObject.renderer.material.SetColor("_Color", new Color(1.0f - Quantity, 1.0f - Quantity, 1.0f));
		}
	}


// Member Fields


	CNetworkVar<float> m_fQuantity = null;


};
