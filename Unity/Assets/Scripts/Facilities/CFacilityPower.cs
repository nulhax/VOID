//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityPower.cs
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


	public delegate void PowerActiveChangeHandler(GameObject _cFacility, bool _bActive);
	public event PowerActiveChangeHandler EventFacilityPowerActiveChange;


// Member Properties
	

    public float PowerConsumptionRate
    {
        get { return (m_fConsumptionRate.Get()); }
    }


	public bool IsPowerActive
	{
		get { return (m_bActive.Get()); }
	}


// Member Functions
	

	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		m_fConsumptionRate = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_bActive          = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, true);
	}


    [AServerOnly]
    public void SetPowerActive(bool _bEnabled)
    {
        m_bActive.Set(_bEnabled);
    }


    [AServerOnly]
    public void ChangeConsumptionRate(float _fAmount)
    {
        m_fConsumptionRate.Value += _fAmount;
    }


	void Start()
	{
        // Empty
	}

	
	void Update()
	{
        // Empty
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_bActive)
        {
            if (EventFacilityPowerActiveChange != null) EventFacilityPowerActiveChange(gameObject, m_bActive.Value);
        }
    }


// Member Fields


    CNetworkVar<float> m_fConsumptionRate = null;
    CNetworkVar<bool>  m_bActive          = null;



};
