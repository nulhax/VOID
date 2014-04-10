
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CDynamicActor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CActorLocator))]
public class CActorGravity : CNetworkMonoBehaviour 
{
// Member Types


// Member Delegates & Events


	public delegate void NotifyGravityInfulenceChange();

	public event NotifyGravityInfulenceChange EventEnteredGravityZone;
	public event NotifyGravityInfulenceChange EventExitedGravityZone;


// Member Properties


	public bool IsUnderGravityInfluence
	{
		get { return(m_bGravityActive.Get()); }
	}

	
// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bGravityActive = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, true);
	}


    void Start()
    {
        rigidbody.useGravity = true;
		Debug.Log (gameObject.name);

        if (CNetwork.IsServer)
        {
            GetComponent<CActorLocator>().EventFacilityChangeHandler += OnEventActorChangeFacility;
        }
    }


    void OnDestroy()
    {
        if (CNetwork.IsServer)
        {
            GetComponent<CActorLocator>().EventFacilityChangeHandler -= OnEventActorChangeFacility;
        }
    }


	void Update()
	{
        // Empty
	}


    [AServerOnly]
    void OnEventActorChangeFacility(GameObject _cPreviousFacility, GameObject _cNewFacility)
    {
        if (_cPreviousFacility != null)
        {
            _cPreviousFacility.GetComponent<CFacilityGravity>().EventGravityStatusChange -= OnEventFacilityGravityStatusChange;
        }

        if (_cNewFacility != null)
        {
            _cNewFacility.GetComponent<CFacilityGravity>().EventGravityStatusChange += OnEventFacilityGravityStatusChange;

            m_bGravityActive.Value = _cNewFacility.GetComponent<CFacilityGravity>().IsGravityEnabled;
        }
        else
        {
            m_bGravityActive.Value = false;
        }
    }


    [AServerOnly]
    void OnEventFacilityGravityStatusChange(GameObject _cFacility, bool _bActive)
    {
        m_bGravityActive.Value = _bActive;
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_bGravityActive)
        {
            rigidbody.useGravity = m_bGravityActive.Value;

            // Check gravity was disabled
            if (!m_bGravityActive.Value)
            {
                // Give a slight force to the object to get it moving
                if (CNetwork.IsServer && 
                    rigidbody != null)
                {
                    rigidbody.AddForce(Random.onUnitSphere * 0.1f, ForceMode.VelocityChange);
                }

                // Notify observers
                if (EventExitedGravityZone != null) EventExitedGravityZone();
            }
            else
            {
                // Notify observers
                if (EventEnteredGravityZone != null) EventEnteredGravityZone();
            }
        }
    }


// Member Fields


    CNetworkVar<bool> m_bGravityActive = null;


}
