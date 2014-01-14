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


/* Implementation */


public class CPlayerLocator : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void NotifyEnterShip();
    public event NotifyEnterShip EnterShip;


    public delegate void NotifyLeaveShip();
    public event NotifyLeaveShip EventLeaveShip;


    public delegate void NotifyEnterFacility(GameObject _cFacility);
    public event NotifyEnterFacility EventEnterFacility;


    public delegate void NotifyLeaveFacility(GameObject _cFacility);
    public event NotifyLeaveFacility EventLeaveFacility;


// Member Properties


    public GameObject Facility
    {
        get
        {
            if (m_cFacilityViewId.Get() == 9) return null;
            
            return (CNetwork.Factory.FindObject(m_cFacilityViewId.Get()));
        }
    }


// Member Functions


	public override void InstanceNetworkVars()
	{
        m_cFacilityViewId = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
	}


    [AServerOnly]
    public void SetContainingFacility(GameObject _cFacility)
    {
        m_cFacilityViewId.Set(_cFacility.GetComponent<CNetworkView>().ViewId);
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
            if (m_cFacilityViewId.Get() == 0)
            {
                // Was in a facility before
                if (m_cFacilityViewId.GetPrevious() != 0)
                {
                    // Notify leave facility observers
                    if (EventEnterFacility != null) EventLeaveFacility(CNetwork.Factory.FindObject(m_cFacilityViewId.GetPrevious()));
                }
            }
            else
            {
                // Notify enter facility observers
                if (EventEnterFacility != null) EventEnterFacility(CNetwork.Factory.FindObject(m_cFacilityViewId.Get()));
            }
        }
    }


// Member Fields


    CNetworkVar<ushort> m_cFacilityViewId = null;


};
