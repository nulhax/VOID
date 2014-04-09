//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityGravity.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityGravity : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void GravityStatusChangeHandler(GameObject _cFacility, bool _bActive);
    public event GravityStatusChangeHandler EventGravityStatusChange;


// Member Properties


    public bool IsGravityEnabled
    {
        get { return (m_bEnabled.Get()); }
    }
	
	
// Member Methods
	

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bEnabled = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, true);
	}


	[AServerOnly]
	public void SetGravityEnabled(bool _State)
	{
		m_bEnabled.Value = _State;
	}


	void Start()
	{
        if (CNetwork.IsServer)
        {
            GetComponent<CFacilityPower>().EventFacilityPowerActiveChange += OnEventFacilityPowerActiveChange;
        }
	}


    void OnDestroy()
    {
        if (CNetwork.IsServer)
        {
            GetComponent<CFacilityPower>().EventFacilityPowerActiveChange -= OnEventFacilityPowerActiveChange;
        }
    }


    [AServerOnly]
	void OnEventFacilityPowerActiveChange(GameObject _cFacility, bool _bActive)
	{
        m_bEnabled.Value = _bActive;
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_bEnabled)
        {
            Debug.LogError("Synced facility gravity: " + m_bEnabled.Value);
             
            // Notify observers
            if (EventGravityStatusChange != null) EventGravityStatusChange(gameObject, m_bEnabled.Value);
        }
    }


// Member Fields


    CNetworkVar<bool> m_bEnabled = null;


}
