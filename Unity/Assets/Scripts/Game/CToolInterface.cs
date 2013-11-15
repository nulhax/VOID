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


	public delegate void ActivatePrimary();
	public delegate void DeactivatePrimary();
	
	public delegate void ActivateSecondary();
	public delegate void DeactivateSecondary();
	
	public delegate void Reload2();

    public delegate void PickedUp();
    public delegate void Dropped();
	
	public event ActivatePrimary EventActivatePrimary;
	public event DeactivatePrimary EventDeactivatePrimary;
	
	public event ActivateSecondary EventActivateSecondary;
	public event DeactivateSecondary EventDeactivateSecondary;

	public event Reload2 EventReload;

    public event PickedUp EventPickedUp;
    public event PickedUp EventDropped;
	
	
// Member Properties


    public GameObject OwnerPlayerActorObject
    {
        get { return (CNetwork.Factory.FindObject(m_usOwnerPlayerActorViewId.Get())); }
    }


    public bool IsHeldByPlayer
    {
        get { return (m_bHeldByPlayer.Get()); }
    }


// Member Functions


    public override void InstanceNetworkVars()
    {
        m_usOwnerPlayerActorViewId = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
        m_bHeldByPlayer = new CNetworkVar<bool>(OnNetworkVarSync, false);
    }


    public void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_bHeldByPlayer)
        {
            if (m_bHeldByPlayer.Get())
            {
                GameObject cOwnerPlayerActor = OwnerPlayerActorObject;

                gameObject.transform.parent = cOwnerPlayerActor.transform;
                gameObject.transform.localPosition = new Vector3(1.0f, 0.0f, 2.0f);
                gameObject.transform.localRotation = Quaternion.identity;

                // Turn off  dynamic physics
                rigidbody.detectCollisions = false;
                rigidbody.isKinematic = true;

                // Disable dynamic actor
                GetComponent<CDynamicActor>().enabled = false;
            }
            else
            {
                gameObject.transform.parent = null;

                // Turn on  dynamic physics
                rigidbody.detectCollisions = true;
                rigidbody.isKinematic = false;
                rigidbody.AddForce(transform.forward * 5.0f, ForceMode.VelocityChange);
                rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.VelocityChange);

                // Disable dynamic actor
                GetComponent<CDynamicActor>().enabled = true;
            }
        }
    }


	public void Awake()
    {
		// Empty
	}


	public void Start()
	{
        CNetwork.Server.EventPlayerDisconnect += new CNetworkServer.NotifyPlayerDisconnect(OnPlayerDisconnect);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
        if (m_bHeldByPlayer.Get())
        {
            gameObject.transform.localRotation = Quaternion.Euler(OwnerPlayerActorObject.GetComponent<CPlayerHeadMotor>().HeadEuler.x, gameObject.transform.localRotation.eulerAngles.y, gameObject.transform.localRotation.eulerAngles.z);
        }
	}


	[AServerMethod]
	public void NotifyPickedUp(ulong _ulPlayerId)
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		if (m_bHeldByPlayer.Get() == false)
		{
			// Set owning player
			m_ulOwnerPlayerId = _ulPlayerId;

            // Set owning object view id
            m_usOwnerPlayerActorViewId.Set(CGame.FindPlayerActor(_ulPlayerId).GetComponent<CNetworkView>().ViewId);

			// Set held
			m_bHeldByPlayer.Set(true);

            // Notify observers
            if (EventPickedUp != null)
            {
                EventPickedUp();
            }
		}
	}


	[AServerMethod]
	public void NotifyDropped()
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		// Check currently held
		if (m_bHeldByPlayer.Get())
		{
			// Remove owner player
            m_ulOwnerPlayerId = 0;

            // Set owning object view id
            m_usOwnerPlayerActorViewId.Set(0);

			// Set un-held
			m_bHeldByPlayer.Set(false);

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
		if (m_bHeldByPlayer.Get())
		{
			// Notify observers
			if (EventReload != null)
			{
				EventReload();
			}
		}
	}


	[AServerMethod]
	public void SetPrimaryActive(bool _bActive)
	{
		// Check not already active
		if (_bActive &&
			!m_bPrimaryActive)
		{
			// Set active
            m_bPrimaryActive = true;

            // Notify observers
            if (EventActivatePrimary != null)
            {
                EventActivatePrimary();
            }
		}

		// Check currently active
		else if (!_bActive &&
				 m_bPrimaryActive)
		{
			// Set deactive
			m_bPrimaryActive = false;

            // Notify observers
            if (EventDeactivatePrimary != null)
            {
                EventDeactivatePrimary();
            }
		}
	}


	[AServerMethod]
	public void SetSecondaryActive(bool _bActive)
	{
		// Check not already active
		if (_bActive &&
			!m_bPrimaryActive)
		{
			// Set active
			m_bSecondaryActive = true;

            // Notify observers
            if (EventActivateSecondary != null)
            {
                EventActivateSecondary();
            }
		}

		// Check currently active
        else if (!_bActive &&
				 m_bPrimaryActive)
		{
			// Set deactive
			m_bSecondaryActive = false;

            // Notify observers
            if (EventDeactivateSecondary != null)
            {
                EventDeactivateSecondary();
            }
		}
	}


    [AServerMethod]
    void OnPlayerDisconnect(CNetworkPlayer _cPlayer)
    {
        if (m_ulOwnerPlayerId == _cPlayer.PlayerId)
        {
            NotifyDropped();
        }
    }

	/*

	public static void SerializeActions(CNetworkStream _cStream)
	{
		// Write in stream
		_cStream.Write(s_cSerializeStream);
	}


	public static void UnserializeActions(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		// Process stream data
		while (_cStream.HasUnreadData)
		{
			// Extract action
			ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();

			// Extract target tool view id
			ushort usToolViewId = _cStream.ReadUShort();

			// Handle action
			switch (eAction)
			{
			case ENetworkAction.PickUp:
				HandlePickupRequest(_cNetworkPlayer, usToolViewId);
				break;
			}
		}
	}
	*/


// Member Fields


    CNetworkVar<ushort> m_usOwnerPlayerActorViewId = null;
    CNetworkVar<bool> m_bHeldByPlayer = null;


    bool m_bPrimaryActive = false;
    bool m_bSecondaryActive = false;
    ulong m_ulOwnerPlayerId = 0;

	
};
