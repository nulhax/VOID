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


	public delegate void HandlePlayerEnter(ulong _ulPlayerId);
	public event HandlePlayerEnter EventPlayerEnter;


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
				return (CNetwork.Factory.FindObject(CGame.FindPlayerActorViewId(MountedPlayerId)));
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


	public override void InstanceNetworkVars()
	{
		m_cMountedPlayerId = new CNetworkVar<ulong>(OnNetworkVarSync, 0);
	}


	public void OnNetworkVarSync(INetworkVar _cSynedNetworkVar)
	{
		if (_cSynedNetworkVar == m_cMountedPlayerId)
		{
			if (m_cMountedPlayerId.Get() == 0)
			{
				// Notify observers
				if (EventPlayerLeave != null) EventPlayerLeave(m_cMountedPlayerId.GetPrevious());
			}
			else
			{
				// Lock player movement locally
				if (m_cMountedPlayerId.Get() == CNetwork.PlayerId)
				{
					CGame.PlayerActor.GetComponent<CPlayerMotor>().DisableInput(this);
					CGame.PlayerActor.GetComponent<CPlayerHead>().DisableInput(this);

					// Move player head into rotation
					CGame.PlayerActor.GetComponent<CPlayerHead>().transform.rotation = m_cSeat.transform.rotation;
				}

				// Notify observers
				if (EventPlayerEnter != null) EventPlayerEnter(m_cMountedPlayerId.Get());
			}

			// Unlock player movement locally
			if (m_cMountedPlayerId.GetPrevious() == CNetwork.PlayerId)
			{
				CGame.PlayerActor.GetComponent<CPlayerMotor>().UndisableInput(this);
				CGame.PlayerActor.GetComponent<CPlayerHead>().UndisableInput(this);
			}
		}
	}


	public void Start()
	{
		// Sign up for event
		gameObject.GetComponent<CActorInteractable>().EventUse += new CActorInteractable.NotifyInteraction(OnUseInteraction);


		CGame.UserInput.EventUse += new CUserInput.NotifyKeyChange(OnInputUseChange);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		/*
		// Orient self to cockpit orientation 
		if (CGame.PlayerActorViewId != 0 &&
			ContainedPlayerActorViewId == CGame.PlayerActorViewId)
		{
			CGame.PlayerActor.GetComponent<CPlayerHead>().transform.rotation = m_cSeat.transform.rotation;
			CGame.PlayerActor.GetComponent<CPlayerHead>().ActorHead.transform.rotation = Quaternion.identity;
		}
		*/
	}


	[AClientMethod]
	public static void SerializeOutbound(CNetworkStream _cStream)
    {
		_cStream.Write(s_cSerializeStream);

		s_cSerializeStream.Clear();
    }


	[AServerMethod]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		while (_cStream.HasUnreadData)
		{
			ushort usCockpitObjectViewId = _cStream.ReadUShort();
			ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();

			CCockpit cCockpit = CNetwork.Factory.FindObject(usCockpitObjectViewId).GetComponent<CCockpit>();

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

	
	[AClientMethod]
	void OnUseInteraction(RaycastHit _cRayHit, ushort _usPlayerActorViewId)	
	{
		// Check there is no one in the cockpit locally
		if (MountedPlayerId == 0)
		{
			// Write in enter cockpit action
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write((byte)ENetworkAction.EnterCockpit);
		}
	}


	[AServerMethod]
	void HandleEnterCockpit(ulong _ulPlayerId)
	{
		GameObject cPlayerActor = CGame.FindPlayerActor(_ulPlayerId);
		ushort cPlayerActorViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

		if (MountedPlayerId == 0)
		{
			// Allow player to enter cockpit
			if (cPlayerActor != null)
			{
				m_cMountedPlayerId.Set(_ulPlayerId);

				// Save position on player when entering
				m_vEnterPosition = cPlayerActor.transform.position;

				// Teleport player in cockpit
				cPlayerActor.transform.position = gameObject.transform.position;

				// Rotate player in cockpit
				cPlayerActor.transform.rotation = gameObject.transform.rotation;

				//Debug.Log(string.Format("Player ({0}) entered cockpit", _ulPlayerId));
			}
		}
	}


	[AServerMethod]
	void HandleLeaveCockpit(ulong _ulPlayerId)
	{
		GameObject cPlayerActor = CGame.FindPlayerActor(_ulPlayerId);
		ushort cPlayerActorViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

		// Allow player to leave cockpit
		if (MountedPlayerId == _ulPlayerId)
		{
			m_cMountedPlayerId.Set(0);

			// Teleport player back to entered position
			cPlayerActor.transform.position = m_vEnterPosition;
			m_vEnterPosition = Vector3.zero;

			//Debug.Log(string.Format("Player ({0}) left cockpit", _ulPlayerId));
		}
	}


	[AClientMethod]
	void OnInputUseChange(bool _bDown)
	{
		if (_bDown &&
			MountedPlayerId == CNetwork.PlayerId)
		{
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write((byte)ENetworkAction.LeaveCockpit);
		}
	}


// Member Fields


	public GameObject m_cSeat = null;


	CNetworkVar<ulong> m_cMountedPlayerId = null;


	Vector3 m_vEnterPosition = new Vector3();


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
