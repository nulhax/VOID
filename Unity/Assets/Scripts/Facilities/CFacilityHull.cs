﻿
//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityHull.cs
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


public class CFacilityHull : CNetworkMonoBehaviour
{

// Member Types


    public enum EEventType
    {
        Breached,
        BreachFixed
    }


// Member Delegates & Events
	

	public delegate void NotifyEvent(EEventType _eEventType);

	public event NotifyEvent EventBreached;
    public event NotifyEvent EventBreachFixed;


// Member Properties
	

	public bool IsBreached
	{
        get { return (m_bBreached.Get()); }
	}


// Member Methods

	public void AddBreach(GameObject breach)
	{
		m_Breaches.Add(breach);

		if (CNetwork.IsServer && m_Breaches.Count == 1)	// If this is the first breach...
			m_bBreached.Set(true);
	}

	public void RemoveBreach(GameObject breach)
	{
		m_Breaches.Remove(breach);

		if (CNetwork.IsServer && m_Breaches.Count <= 0)
			m_bBreached.Set(false);
	}

    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_bBreached = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
    }


	void OnDestroy()
	{
        // Empty
	}


	void Update()
	{
        // Empty

        // Debug
        //if (CNetwork.IsServer && Input.GetKeyDown(KeyCode.P))
		//	m_bBreached.Set(!m_bBreached.Get());
	}
	

	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_bBreached)
        {
            if (m_bBreached.Get())
            {
                if (EventBreached != null) EventBreached(EEventType.Breached);
            }
            else
            {
                if (EventBreachFixed != null) EventBreachFixed(EEventType.BreachFixed);
            }
        }
    }


// Member Fields


    CNetworkVar<bool> m_bBreached = null;
	System.Collections.Generic.List<GameObject> m_Breaches = new List<GameObject>();

};
