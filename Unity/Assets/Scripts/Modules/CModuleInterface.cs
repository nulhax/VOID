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


// Member Properties


	public GameObject OwnerPlayerActorObject
	{
		get { return (CNetwork.Factory.FindObject(m_usOwnerPlayerActorViewId.Get())); }
	}


	public bool IsHeld
	{
		get { return (m_usOwnerPlayerActorViewId.Get() != 0); }
	}


// Member Functions


	public override void InstanceNetworkVars()
	{
		m_usOwnerPlayerActorViewId = new CNetworkVar<ushort>(OnNetworkVarSync);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if (_cSyncedNetworkVar == m_usOwnerPlayerActorViewId)
		{
			if (IsHeld)
			{
				GameObject cOwnerPlayerActor = OwnerPlayerActorObject;

				gameObject.transform.parent = cOwnerPlayerActor.transform;
				gameObject.transform.localPosition = new Vector3(-0.43f, 0.41f, 0.34f);
				gameObject.transform.localRotation = Quaternion.identity;

				// Turn off  dynamic physics
				if(CNetwork.IsServer)
				{
                	rigidbody.isKinematic = true;
				}

				// Disable dynamic actor
				rigidbody.collider.isTrigger = true;
				GetComponent<CDynamicActor>().enabled = false;
			}
			else
			{
				gameObject.transform.parent = null;

				// Turn on  dynamic physics
				if(CNetwork.IsServer)
				{
                	rigidbody.isKinematic = false;
				}
				
				rigidbody.AddForce(transform.forward * 5.0f, ForceMode.VelocityChange);
				rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.VelocityChange);
				
				// Disable dynamic actor
				rigidbody.collider.isTrigger = false;
				GetComponent<CDynamicActor>().enabled = true;
			}
		}
	}


	public void Awake()
	{
		gameObject.AddComponent<Rigidbody>();
		gameObject.AddComponent<CDynamicActor>();
		gameObject.AddComponent<CInteractableObject>();
		gameObject.AddComponent<CNetworkView>();
		
		gameObject.rigidbody.useGravity = false;
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
			m_usOwnerPlayerActorViewId.Set(CGame.FindPlayerActor(_ulPlayerId).GetComponent<CNetworkView>().ViewId);

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
			m_usOwnerPlayerActorViewId.Set(0);

			// Notify observers
			if (EventDropped != null)
			{
				EventDropped();
			}
		}
	}
	



// Member Fields


	public EType m_eType;


	CNetworkVar<ushort> m_usOwnerPlayerActorViewId = null;


	ulong m_ulOwnerPlayerId = 0;

};
