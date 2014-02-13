//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCockpit.cs
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
public class CCockpit : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		EnterCockpit,
		LeaveCockpit
	}


// Member Delegates & Events


	[AServerOnly]
	public delegate void HandlePlayerEnter(ulong _ulPlayerId);
	public event HandlePlayerEnter EventPlayerEnter;


	[AServerOnly]
	public delegate void HandlePlayerLeave(ulong _ulPlayerId);
	public event HandlePlayerLeave EventPlayerLeave;


// Member Properties


	public ulong MountedPlayerId
	{
		get { return (m_cMountedPlayerId.Get()); }
	}


	public GameObject MountedPlayerActor
	{
		get
		{
			if (!IsMounted)
			{
				return (null);
			}
			else
			{
				return (CNetwork.Factory.FindObject(CGamePlayers.GetPlayerActorViewId(MountedPlayerId)));
			}
		}
	}


	public bool IsMounted
	{
		get { return (MountedPlayerId != 0); }
	}


	public GameObject SeatObject
	{
		get { return (m_cSeat); }
	}

	
// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_cMountedPlayerId = _cRegistrar.CreateNetworkVar<ulong>(OnNetworkVarSync, 0);
	}


	public void OnNetworkVarSync(INetworkVar _cSynedVar)
	{
		if (_cSynedVar == m_cMountedPlayerId)
		{
			if (m_cMountedPlayerId.Get() == CNetwork.PlayerId)
			{
				CGamePlayers.SelfActor.GetComponent<CPlayerGroundMotor>().DisableInput(this);
				CGamePlayers.SelfActor.GetComponent<CPlayerHead>().DisableInput(this);

				// Remember the entering head rotations, set the rotation of the head to that of the seat
				m_EnterHeadXRot = CGamePlayers.SelfActor.GetComponent<CPlayerHead>().HeadEulerX;
				CGamePlayers.SelfActor.GetComponent<CPlayerHead>().ActorHead.transform.LookAt(m_LookAt.position);
				CGamePlayers.SelfActor.GetComponent<CPlayerHead>().SetHeadRotations(CGamePlayers.SelfActor.GetComponent<CPlayerHead>().ActorHead.transform.localEulerAngles.x);
			}

			// Unlock player movement locally
			if (m_cMountedPlayerId.GetPrevious() == CNetwork.PlayerId)
			{
				// Ensure self actor exists
				// - Players can leave the game while in the cockpits and their actors wont exist at this stage
				if (CGamePlayers.SelfActor != null)
				{
					CGamePlayers.SelfActor.GetComponent<CPlayerGroundMotor>().ReenableInput(this);
					CGamePlayers.SelfActor.GetComponent<CPlayerHead>().ReenableInput(this);

					CGamePlayers.SelfActor.GetComponent<CPlayerHead>().SetHeadRotations(m_EnterHeadXRot);
				}
			}
		}
	}


	public void Start()
	{
		// Sign up for event
		gameObject.GetComponent<CActorInteractable>().EventUse += OnEventInteractionUse;
		CNetwork.Server.EventPlayerDisconnect += OnPlayerDisconnect;


        CUserInput.SubscribeInputChange(CUserInput.EInput.Use, OnInputUse);
	}


	public void OnDestroy()
	{
        gameObject.GetComponent<CActorInteractable>().EventUse -= OnEventInteractionUse;
	}


	public void Update()
	{
        // If processed on the server
        if (CNetwork.IsServer)
        {
            // Local broken component variable
            bool bBrokenComponentFound = false;

            // Iterate through the list of components
            foreach (CComponentInterface Component in m_Components)
            {
                // If a broken component is found
                if (!Component.IsFunctional)
                {
                    // Update the broken component variable
                    bBrokenComponentFound = true;

                    // Break out of the loop to prevent obsolete processing
                    break;
                }
            }

            // If a component is broken
            if (bBrokenComponentFound)
            {
                // If a player is in the cockpit
                if (IsMounted)
                {
                    // Eject the player from the cockpit
                    HandleLeaveCockpit(MountedPlayerId);
                }
            }
        }
	}


	[AClientOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
    {
		_cStream.Write(s_cSerializeStream);

		s_cSerializeStream.Clear();
    }


	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		while (_cStream.HasUnreadData)
		{
			CNetworkViewId cCockpitObjectViewId = _cStream.ReadNetworkViewId();
			ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();

			GameObject cCockpitObject = CNetwork.Factory.FindObject(cCockpitObjectViewId);

			CCockpit cCockpit = cCockpitObject.GetComponent<CCockpit>();

			switch (eAction)
			{
				case ENetworkAction.EnterCockpit:
					cCockpit.HandleEnterCockpit(_cNetworkPlayer.PlayerId);
					break;

				case ENetworkAction.LeaveCockpit:
					cCockpit.HandleLeaveCockpit(_cNetworkPlayer.PlayerId);
					break;

				default:
					Debug.LogError(string.Format("Unknown network action ({0})"));
					break;
			}
		}
	}


	[AClientOnly]
    void OnInputUse(CUserInput.EInput _eInput, bool _bDown)
	{
		if (_bDown &&
		    MountedPlayerId == CNetwork.PlayerId)
		{
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write((byte)ENetworkAction.LeaveCockpit);
		}
	}

	
	[AClientOnly]
    void OnEventInteractionUse(RaycastHit _tRayHit, CNetworkViewId _cPlayerActorViewId, bool _bDown)	
	{
		// Check there is no one in the cockpit locally
		if (_bDown &&
            !IsMounted)
		{
			// Write in enter cockpit action
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write((byte)ENetworkAction.EnterCockpit);
		}
	}


	[AServerOnly]
	void OnPlayerDisconnect(CNetworkPlayer _cNetworkPlayer)
	{

		if (MountedPlayerId == _cNetworkPlayer.PlayerId)
		{
			//Debug.LogError("Player left cockpit" + gameObject.name);

			HandleLeaveCockpit(_cNetworkPlayer.PlayerId);
		}
	}


	[AServerOnly]
	void HandleEnterCockpit(ulong _ulPlayerId)
	{
		GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_ulPlayerId);

		if ( cPlayerActor != null &&!IsMounted)
		{
			CNetworkViewId cPlayerActorViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

			// Save position on player when entering
			m_vEnterPosition = cPlayerActor.transform.position;
			m_EnterRotation = cPlayerActor.transform.rotation;

			// Parent the player to the cockpit seat
			cPlayerActor.GetComponent<CNetworkView>().SetParent(m_cSeat.GetComponent<CNetworkView>().ViewId);

			// Teleport player in cockpit
			cPlayerActor.GetComponent<CNetworkView>().SetPosition(m_cSeat.transform.position);
			cPlayerActor.GetComponent<CNetworkView>().SetRotation(m_cSeat.transform.rotation);

			// Set the player kinematic
			cPlayerActor.rigidbody.isKinematic = true;

			m_cMountedPlayerId.Set(_ulPlayerId);

			// Notify observers
			if (EventPlayerEnter != null) EventPlayerEnter(m_cMountedPlayerId.Get());
		}
	}


	[AServerOnly]
	void HandleLeaveCockpit(ulong _ulPlayerId)
	{
		// Allow player to leave cockpit
		if (MountedPlayerId == _ulPlayerId)
		{
			// Teleport player back to entered position
			GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_ulPlayerId);

			if (cPlayerActor != null)
			{
				// UnParent the player to the cockpit seat
				cPlayerActor.GetComponent<CNetworkView>().SetParent(null);

				// Move player back to positions when entered
				cPlayerActor.GetComponent<CNetworkView>().SetPosition(m_vEnterPosition);
				cPlayerActor.GetComponent<CNetworkView>().SetRotation(m_EnterRotation);

				// Turn of kinematic
				cPlayerActor.rigidbody.isKinematic = false;
			}

			m_cMountedPlayerId.Set(0);
			
			// Notify observers
			if (EventPlayerLeave != null) EventPlayerLeave(m_cMountedPlayerId.GetPrevious());

			//Debug.Log(string.Format("Player ({0}) left cockpit", _ulPlayerId));
		}
	}


// Member Fields


	public GameObject m_cSeat = null;
	public Transform m_LookAt = null;


	CNetworkVar<ulong> m_cMountedPlayerId = null;

	float m_EnterHeadXRot = 0.0f;

	Vector3 m_vEnterPosition = Vector3.zero;
	Quaternion m_EnterRotation = Quaternion.identity;


    public CComponentInterface[] m_Components;

	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
