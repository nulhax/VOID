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
public class CModuleInterface : CNetworkMonoBehaviour
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
		get { return (CNetwork.Factory.FindObject(m_usOwnerActorViewId.Get())); }
	}


	public bool IsHeld
	{
		get { return (m_usOwnerActorViewId.Get() != 0); }
	}

// Member Functions


	public override void InstanceNetworkVars()
	{
		m_usOwnerActorViewId = new CNetworkVar<ushort>(OnNetworkVarSync);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if (_cSyncedNetworkVar == m_usOwnerActorViewId)
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
				
				// Stop recieving syncronizations
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
				
				// Recieve syncronizations
				GetComponent<CActorNetworkSyncronized>().m_SyncPosition = true;
				GetComponent<CActorNetworkSyncronized>().m_SyncRotation = true;
			}
		}
	}


	public void Awake()
	{
		// Empty
	}


	public void Start()
	{
		// Empty
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		// Empty
	}


	[AServerMethod]
	public void Pickup(ulong _ulPlayerId)
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		if (!IsHeld)
		{
			// Set owning player
			m_ulOwnerPlayerId = _ulPlayerId;

			// Set owning object view id
			m_usOwnerActorViewId.Set(CGame.FindPlayerActor(_ulPlayerId).GetComponent<CNetworkView>().ViewId);

			// Notify observers
			if (EventPickedUp != null)
			{
				EventPickedUp();
			}
		}
	}


	[AServerMethod]
	public void Drop()
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		// Check currently held
		if (IsHeld)
		{
			// Remove owner player
			m_ulOwnerPlayerId = 0;

			// Set owning object view id
			m_usOwnerActorViewId.Set(0);

			// Notify observers
			if (EventDropped != null)
			{
				EventDropped();
			}
		}
	}

// Member Fields


	public EType m_eType;


	CNetworkVar<ushort> m_usOwnerActorViewId = null;

	ulong m_ulOwnerPlayerId = 0;

};
