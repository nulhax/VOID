//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerLocator.cs
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


public class CPlayerLocator : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void NotifyEnterShip();
    public event NotifyEnterShip EventEnterShip;


    public delegate void NotifyLeaveShip();
    public event NotifyLeaveShip EventLeaveShip;


    public delegate void NotifyEnterFacility(GameObject _cFacility);
    public event NotifyEnterFacility EventEnterFacility;


    public delegate void NotifyLeaveFacility(GameObject _cFacility);
    public event NotifyLeaveFacility EventLeaveFacility;


// Member Properties


    public CNetworkViewId ContainingFacilityViewId
    {
        get
        {
            return (m_cFacilityViewId.Get());
        }
    }


    public GameObject ContainingFacility
    {
        get
        {
			if (m_cFacilityViewId.Get() == null)
			{
				return (null);
			}

            return (CNetwork.Factory.FindObject(m_cFacilityViewId.Get()));
        }
    }


// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_cFacilityViewId = _cRegistrar.CreateReliableNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
	}


    [AServerOnly]
    public void SetContainingFacility(GameObject _cFacility)
    {
		if (_cFacility != null)
		{
			m_cFacilityViewId.Set(_cFacility.GetComponent<CNetworkView>().ViewId);
		}
		else
		{
			m_cFacilityViewId.Set(null);
		}

		//Debug.LogError("Contain facility:"+ _cFacility);
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
		// Empty
	}


    void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    {
        if (_cSyncedNetworkVar == m_cFacilityViewId)
        {
            // Not in any facility anymore
            if (m_cFacilityViewId.Get() == null)
            {
				// Notify leave facility observers
				if (EventEnterFacility != null) EventLeaveFacility(CNetwork.Factory.FindObject(m_cFacilityViewId.GetPrevious()));

                // Was in a facility before
                if (m_cFacilityViewId.GetPrevious() != null)
                {
					// Notify leave ship observers
					if (EventLeaveShip != null) EventLeaveShip();
                }
            }
            else
            {
                // Notify enter facility observers
                if (EventEnterFacility != null) EventEnterFacility(CNetwork.Factory.FindObject(m_cFacilityViewId.Get()));

				// Was not in a facility before
				if (m_cFacilityViewId.GetPrevious() == null)
				{
					// Notify enter ship observers
					if (EventEnterShip != null) EventEnterShip();
				}
            }
        }
    }


// Member Fields


    CNetworkVar<CNetworkViewId> m_cFacilityViewId = null;


};
