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


public class CCockpit : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		EnterCockpit,
		LeaveCockpit
	}


// Member Delegates & Events


	public delegate void HandlePlayerEnter(ushort _usEnteringPlayerActorViewId);
	public event HandlePlayerEnter EventPlayerEnter;


	public delegate void HandlePlayerLeave(ushort _usLeavingPlayerActorViewId);
	public event HandlePlayerLeave EventPlayerLeave;


// Member Properties


	public ushort ContainedPlayerActorViewId
	{
		get { return (m_cContainedPlayerActorViewId.Get()); }
	}


	public GameObject ContainedPlayerActor
	{
		get
		{
			if (!IsMounted)
			{
				return (null);
			}
			else
			{
				return (CNetwork.Factory.FindObject(ContainedPlayerActorViewId));
			}
		}
	}


	public bool IsMounted
	{
		get { return (ContainedPlayerActorViewId != 0); }
	}


	public GameObject SeatObject
	{
		get { return (m_cSeat); }
	}

	
// Member Methods


	public override void InstanceNetworkVars()
	{
		m_cContainedPlayerActorViewId = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
	}


	public void OnNetworkVarSync(INetworkVar _cSynedNetworkVar)
	{
		if (_cSynedNetworkVar == m_cContainedPlayerActorViewId)
		{
			if (m_cContainedPlayerActorViewId.Get() == 0)
			{
				// Notify observers
				if (EventPlayerLeave != null) EventPlayerLeave(m_usLastContainedPlayerActorViewId);
			}
			else
			{
				// Lock player movement locally
				if (m_cContainedPlayerActorViewId.Get() == CGame.PlayerActorViewId)
				{
					CGame.PlayerActor.GetComponent<CPlayerMotor>().DisableInput(this);
					CGame.PlayerActor.GetComponent<CPlayerHead>().DisableInput(this);

					// Move player head into rotation
					CGame.PlayerActor.GetComponent<CPlayerHead>().transform.rotation = m_cSeat.transform.rotation;
				}

				// Notify observers
				if (EventPlayerEnter != null) EventPlayerEnter(m_cContainedPlayerActorViewId.Get());
			}

			// Unlock player movement locally
			if (m_usLastContainedPlayerActorViewId == CGame.PlayerActorViewId)
			{
				CGame.PlayerActor.GetComponent<CPlayerMotor>().UndisableInput(this);
				CGame.PlayerActor.GetComponent<CPlayerHead>().UndisableInput(this);
			}

			m_usLastContainedPlayerActorViewId = m_cContainedPlayerActorViewId.Get();
		}
	}


	public void Start()
	{
		// Make sure the game object has a intractable object component
		if (gameObject.GetComponent<CInteractableObject>() == null)
		{
			gameObject.AddComponent<CInteractableObject>();
		}

		// Sign up for event
		gameObject.GetComponent<CInteractableObject>().EventUse += new CInteractableObject.NotifyInteraction(OnUseInteraction);


		CGame.UserInput.EventUse += new CUserInput.NotifyKeyChange(OnInputUseChange);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		if (CGame.PlayerActorViewId != 0 &&
			ContainedPlayerActorViewId == CGame.PlayerActorViewId)
		{
			CGame.PlayerActor.GetComponent<CPlayerHead>().transform.rotation = m_cSeat.transform.rotation;
			CGame.PlayerActor.GetComponent<CPlayerHead>().ActorHead.transform.rotation = Quaternion.identity;
		}
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
		if (ContainedPlayerActorViewId == 0)
		{
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write((byte)ENetworkAction.EnterCockpit);
		}
	}


	[AServerMethod]
	void HandleEnterCockpit(ulong _ulPlayerActorViewId)
	{
		GameObject cPlayerActor = CGame.FindPlayerActor(_ulPlayerActorViewId);
		ushort cPlayerActorViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

		if (ContainedPlayerActorViewId == 0)
		{
			// Allow player to enter cockpit
			if (cPlayerActor != null)
			{
				m_cContainedPlayerActorViewId.Set(cPlayerActorViewId);

				// Disable player movement on server
				cPlayerActor.GetComponent<CPlayerMotor>().DisableInput(this);

				// Save position on player when entering
				m_vEnterPosition = cPlayerActor.transform.position;

				// Teleport player in cockpit
				cPlayerActor.transform.position = gameObject.transform.position;

				// Rotate player in cockpit
				cPlayerActor.transform.rotation = gameObject.transform.rotation;

				// Notify observers
				if (EventPlayerEnter != null) EventPlayerEnter(cPlayerActorViewId);

				Debug.Log(string.Format("Player ({0}) entered cockpit", _ulPlayerActorViewId));
			}
		}
	}


	[AServerMethod]
	void HandleLeaveCockpit(ulong _ulPlayerActorViewId)
	{
		GameObject cPlayerActor = CGame.FindPlayerActor(_ulPlayerActorViewId);
		ushort cPlayerActorViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

		// Allow player to leave cockpit
		if (ContainedPlayerActorViewId == cPlayerActorViewId)
		{
			m_cContainedPlayerActorViewId.Set(0);

			// Teleport player back to entered position
			cPlayerActor.transform.position = m_vEnterPosition;
			m_vEnterPosition = Vector3.zero;

			// Enable player movement on server
			cPlayerActor.GetComponent<CPlayerMotor>().UndisableInput(this);

			Debug.Log(string.Format("Player ({0}) left cockpit", _ulPlayerActorViewId));
		}
	}


	[AClientMethod]
	void OnInputUseChange(bool _bDown)
	{
		if (_bDown &&
			ContainedPlayerActorViewId != 0 &&
			ContainedPlayerActorViewId == CGame.PlayerActorViewId)
		{
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write((byte)ENetworkAction.LeaveCockpit);
		}
	}


// Member Fields


	public GameObject m_cSeat = null;


	CNetworkVar<ushort> m_cContainedPlayerActorViewId = null;


	Vector3 m_vEnterPosition = new Vector3();


	ushort m_usLastContainedPlayerActorViewId = 0;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
