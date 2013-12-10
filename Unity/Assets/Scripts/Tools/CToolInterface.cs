//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CToolInterface.cs
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

//This Script contains all the tool info, such as:
//		an enum for what type of tool the tool is
//		an ammo counter?
//		slots taken up
//		
//This Script can:
//		Get the type of tool
//		Shoot the Tool
//		Reload the Tool


public class CToolInterface : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction : byte
	{
		PickUp,
	}


// Member Delegates & Events


	public delegate void NotifyPrimaryActivate(GameObject _cGameObject);
	public event NotifyPrimaryActivate EventPrimaryActivate;

	public delegate void NotifyPrimaryDeactivate();
	public event NotifyPrimaryDeactivate EventPrimaryDeactivate;

	public delegate void NotifySecondaryActivate(GameObject _cGameObject);
	public event NotifySecondaryActivate EventSecondaryActivate;

	public delegate void NotifySecondaryDeactivate();
	public event NotifySecondaryDeactivate EventSecondaryDeactivate;

	public delegate void NotifyReload();
	public event NotifyReload EventReload;

	public delegate void NotifyPickedUp();
	public event NotifyPickedUp EventPickedUp;

	public delegate void NotifyDropped();
	public event NotifyDropped EventDropped;

	public delegate void NotifyUse(GameObject _cGameObject);
	public event NotifyUse EventUse;
	
	
// Member Properties


    public GameObject OwnerPlayerActorObject
    {
		get { if (!IsHeld) return null; return (CNetwork.Factory.FindObject(m_usOwnerPlayerActorViewId.Get())); }
    }


    public bool IsHeld
    {
		get { return (m_usOwnerPlayerActorViewId.Get() != 0); }
    }


// Member Functions


    public override void InstanceNetworkVars()
    {
        m_usOwnerPlayerActorViewId = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
    }


    public void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_usOwnerPlayerActorViewId)
        {
			if (IsHeld)
            {
                GameObject cOwnerPlayerActor = OwnerPlayerActorObject;

                gameObject.transform.parent = cOwnerPlayerActor.transform;
                gameObject.transform.localPosition = new Vector3(0.5f, 0.36f, 0.5f);
                gameObject.transform.localRotation = Quaternion.identity;

                // Turn off dynamic physics
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

                // Enable dynamic actor
				rigidbody.collider.isTrigger = false;
                GetComponent<CDynamicActor>().enabled = true;
            }
        }
    }


	public void Awake()
    {
		gameObject.AddComponent<Rigidbody>();
		gameObject.AddComponent<CInteractableObject>();
		gameObject.AddComponent<CDynamicActor>();
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
		if (IsHeld)
        {
            gameObject.transform.localRotation = Quaternion.Euler(OwnerPlayerActorObject.GetComponent<CPlayerHead>().HeadEulerX, gameObject.transform.localRotation.eulerAngles.y, gameObject.transform.localRotation.eulerAngles.z);
        }
	}


	[AServerMethod]
	public void SetPrimaryActive(bool _bActive, GameObject _cInteractableObject)
	{
		// Check not already active
		if (_bActive &&
			!m_bPrimaryActive)
		{
			// Set active
			m_bPrimaryActive = true;

			// Notify observers
			if (EventPrimaryActivate != null)
			{
				EventPrimaryActivate(_cInteractableObject);
			}
		}

		// Check currently active
		else if (!_bActive &&
				 m_bPrimaryActive)
		{
			// Set deactive
			m_bPrimaryActive = false;

			// Notify observers
			if (EventPrimaryDeactivate != null)
			{
				EventPrimaryDeactivate();
			}
		}
	}


	[AServerMethod]
	public void SetSecondaryActive(bool _bActive, GameObject _cInteractableObject)
	{
		// Check not already active
		if (_bActive &&
			!m_bPrimaryActive)
		{
			// Set active
			m_bSecondaryActive = true;

			// Notify observers
			if (EventSecondaryActivate != null)
			{
				EventSecondaryActivate(_cInteractableObject);
			}
		}

		// Check currently active
		else if (!_bActive &&
				 m_bPrimaryActive)
		{
			// Set deactive
			m_bSecondaryActive = false;

			// Notify observers
			if (EventSecondaryDeactivate != null)
			{
				EventSecondaryDeactivate();
			}
		}
	}


	[AServerMethod]
	public void PickUp(ulong _ulPlayerId)
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


	[AServerMethod]
	public void Reload()
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		// Check currently held
		if (IsHeld)
		{
			// Notify observers
			if (EventReload != null)
			{
				EventReload();
			}
		}
	}


	[AServerMethod]
	public void Use(GameObject _cInteractableObject)
	{
		// Check currently held
		if (IsHeld)
		{
			// Notify observers
			if (EventUse != null)
			{
				EventUse(_cInteractableObject);
			}
		}
	}


// Member Fields


    CNetworkVar<ushort> m_usOwnerPlayerActorViewId = null;


    bool m_bPrimaryActive = false;
    bool m_bSecondaryActive = false;
    ulong m_ulOwnerPlayerId = 0;
};
