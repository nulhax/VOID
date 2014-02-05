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


    public bool IsDepleted
    {
        get { return (m_bDepleted); }
    }


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fQuantity = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 300.0f);
	}


	public float DecrementQuanity(float _fQuantity)
	{
        if (!IsDepleted)
        {
            if (_fQuantity <= m_fQuantity.Get())
            {
                m_fQuantity.Set(m_fQuantity.Get() - _fQuantity);
            }
            else
            {
                _fQuantity = m_fQuantity.Get();
                m_fQuantity.Set(0.0f);
                m_bDepleted = true;

                CNetwork.Factory.DestoryObject(gameObject);
            }

            CGameShips.Ship.GetComponent<CShipNaniteSystem>().AddNanites((int)_fQuantity);
        }

        return (m_fQuantity.Get());
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
			//gameObject.GetComponentInChildren<Renderer>.material.SetColor("_Color", new Color(1.0f - Quantity, 1.0f - Quantity, 1.0f));
		}
	}


// Member Fields


	CNetworkVar<float> m_fQuantity = null;


    bool m_bDepleted = false;


};
