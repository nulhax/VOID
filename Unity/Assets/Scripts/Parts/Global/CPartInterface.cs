//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModule.cs
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

[RequireComponent(typeof(CActorInteractable))]
[RequireComponent(typeof(CActorBoardable))]
[RequireComponent(typeof(CActorGravity))]
public class CPartInterface : CNetworkMonoBehaviour
{

// Member Types


	public enum EType
	{
		PowerCell,
		PlasmaCell,
		FuelCell,
		BlackMatterCell,
		BioCell,
		ReplicatorCell
	}


// Member Delegates & Events


	public delegate void NotifyPickedUp();
	public event NotifyPickedUp EventPickedUp;

	public delegate void NotifyDropped();
	public event NotifyDropped EventDropped;

	public delegate void NotifySwapped();
	public event NotifySwapped EventSwapped;


// Member Properties


	public GameObject OwnerPlayerActor
	{
		get { return (CNetwork.Factory.FindObject(m_cOwnerActorViewId.Get())); }
	}


	public bool IsHeld
	{
		get { return (m_cOwnerActorViewId.Get() != null); }
	}

// Member Functions


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_cOwnerActorViewId = _cRegistrar.CreateReliableNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if (_cSyncedNetworkVar == m_cOwnerActorViewId)
		{
			if (IsHeld)
			{
				GameObject cOwnerPlayerActor = OwnerPlayerActor;
				
				gameObject.transform.parent = cOwnerPlayerActor.transform;
				gameObject.transform.localPosition = new Vector3(-0.5f, 0.36f, 0.5f);
				gameObject.transform.localRotation = Quaternion.identity;
				
				// Turn off dynamic physics
				if(CNetwork.IsServer)
				{
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
				}
				
				// Stop receiving synchronizations
				GetComponent<CActorNetworkSyncronized>().m_SyncPosition = false;
				GetComponent<CActorNetworkSyncronized>().m_SyncRotation = false;
			}
			else
			{
				gameObject.transform.parent = null;
				
				// Turn on dynamic physics
				if(CNetwork.IsServer)
				{
					rigidbody.isKinematic = false;
					rigidbody.detectCollisions = true;
				}
				
				rigidbody.AddForce(transform.forward * 5.0f, ForceMode.VelocityChange);
				rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.VelocityChange);
				
				// Receive synchronizations
				GetComponent<CActorNetworkSyncronized>().m_SyncPosition = true;
				GetComponent<CActorNetworkSyncronized>().m_SyncRotation = true;
			}
		}
	}


    [AServerOnly]
    public void Pickup(ulong _ulPlayerId)
    {
        Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

        if (!IsHeld)
        {
            // Set owning player
            m_ulOwnerPlayerId = _ulPlayerId;

            // Set owning object view id
            m_cOwnerActorViewId.Set(CGamePlayers.GetPlayerActor(_ulPlayerId).GetComponent<CNetworkView>().ViewId);

            // Notify observers
            if (EventPickedUp != null)
            {
                EventPickedUp();
            }
        }
    }


    [AServerOnly]
    public void Drop()
    {
        Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

        // Check currently held
        if (IsHeld)
        {
            // Remove owner player
            m_ulOwnerPlayerId = 0;

            // Set owning object view id
            m_cOwnerActorViewId.Set(null);

            // Notify observers
            if (EventDropped != null)
            {
                EventDropped();
            }
        }
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


// Member Fields


	public EType m_eType;


	CNetworkVar<CNetworkViewId> m_cOwnerActorViewId = null;

	ulong m_ulOwnerPlayerId = 0;

};
