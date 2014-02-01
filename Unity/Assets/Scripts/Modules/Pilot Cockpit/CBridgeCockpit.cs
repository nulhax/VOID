//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CBridgeCockpit.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */

[RequireComponent(typeof(CCockpit))]
public class CBridgeCockpit : CNetworkMonoBehaviour 
{
    // Member Types
	public enum EInteractionEvent : byte
	{
		INVALID,
		
		Nothing,
		PlayerPiloting,
		
		MAX
	}
	
    // Member Delegates & Events
	
	// Member Fields
	private CShipPilotState m_CockpitPilotState = new CShipPilotState();
	
	private EInteractionEvent m_CurrentPlayerInteractionEvent = EInteractionEvent.Nothing;
	
	private GameObject m_AttachedPlayerActor = null;
	private CNetworkVar<CNetworkViewId> m_AttachedPlayerActorViewId = null;
	
	static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eYawLeftKey = KeyCode.A;
    static KeyCode m_eYawRightKey = KeyCode.D;
	static KeyCode m_eStrafeLeftKey = KeyCode.Q;
    static KeyCode m_eStrafeRightKey = KeyCode.E;
	static KeyCode m_eExitKey = KeyCode.F;
	
    // Member Properties
	public GameObject AttachedPlayerActor
	{
		get { return(m_AttachedPlayerActor); }
	}
	
	public CShipPilotState CockpitPilotState
	{
		get { return(m_CockpitPilotState); }
	}

    // Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_AttachedPlayerActorViewId = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
    }
	
	public void OnNetworkVarSync(INetworkVar _NetworkVar)
    {
        if (_NetworkVar == m_AttachedPlayerActorViewId)
        {
			// Attach the pilot if it isnt attached
			if(m_AttachedPlayerActorViewId.Get() != null)
			{
				AttachPlayer(m_AttachedPlayerActorViewId.Get());
			}
			// Detach the pilot if it needs to detach
			else if(m_AttachedPlayerActorViewId.Get() == null)
			{
				DetachPlayer();
			}
		}
	}
	
	public static void SerializeCockpitInteractions(CNetworkStream _cStream)
    {
		GameObject pilotingCockpit = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>().PilotingCockpit;
		
		if(pilotingCockpit == null)
			return;
		
		CBridgeCockpit cockpit = pilotingCockpit.GetComponent<CBridgeCockpit>();
		if(cockpit.m_AttachedPlayerActor != null && CGamePlayers.SelfActor != cockpit.m_AttachedPlayerActor)
			return;
			
		switch(cockpit.m_CurrentPlayerInteractionEvent)
		{
		case EInteractionEvent.PlayerPiloting:
			_cStream.Write((byte)EInteractionEvent.PlayerPiloting);
			_cStream.Write(cockpit.CockpitPilotState.CurrentState);
			_cStream.Write(cockpit.CockpitPilotState.CurrentRotationState.x);
			_cStream.Write(cockpit.CockpitPilotState.CurrentRotationState.y);
			cockpit.CockpitPilotState.ResetStates();
			break;
		}
    }

	public static void UnserializeCockpitInteractions(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {	
		EInteractionEvent interactionEvent = (EInteractionEvent)_cStream.ReadByte();
		CBridgeCockpit bridgeCockpit = CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>().PilotingCockpit.GetComponent<CBridgeCockpit>();
		
		switch(interactionEvent)
		{
		case EInteractionEvent.PlayerPiloting:
			uint motorState = _cStream.ReadUInt();
			float rotationX = _cStream.ReadFloat();
			float rotationY = _cStream.ReadFloat();
			bridgeCockpit.CockpitPilotState.SetCurrentState(motorState, new Vector2(rotationX, rotationY));
			break;
		}
    }

	
    void OnPlayerEnter(ulong _ulPlayerId)
    {
        if(m_AttachedPlayerActor == null && CNetwork.IsServer)
		{
			m_AttachedPlayerActorViewId.Set(CGamePlayers.FindPlayerActor(_ulPlayerId).GetComponent<CNetworkView>().ViewId);
		}
    }

	
    void OnPlayerLeave(ulong _ulPlayerId)
    {
		if(m_AttachedPlayerActor != null && CNetwork.IsServer)
		{
            m_AttachedPlayerActorViewId.Set(null);
		}
    }
	
	public void Start()
	{
        gameObject.GetComponent<CCockpit>().EventPlayerEnter += new CCockpit.HandlePlayerEnter(OnPlayerEnter);
        gameObject.GetComponent<CCockpit>().EventPlayerLeave += new CCockpit.HandlePlayerLeave(OnPlayerLeave);
	}
	
	public void Update()
	{
		// Update the pilot states
		if(m_AttachedPlayerActor != null)
		{	
			// Get the players input
			if(CGamePlayers.SelfActor == m_AttachedPlayerActor)
			{
				UpdatePlayerInput();
			}
			
			m_AttachedPlayerActor.transform.position = transform.position;
			m_AttachedPlayerActor.transform.rotation = transform.rotation;
			m_AttachedPlayerActor.GetComponent<CPlayerHead>().ActorHead.transform.rotation = CGameShips.Ship.transform.rotation;
			
			// Make sure the actor is still alive
			if(CNetwork.IsServer)
			{
				if(!CGamePlayers.SelfActor.GetComponent<CPlayerHealth>().Alive)
				{
					m_AttachedPlayerActorViewId.Set(null);
				}
			}
		}
	}
	
	private void UpdatePlayerInput()
	{
		m_CockpitPilotState.ResetStates();
		
		// Move forwards
        if (Input.GetKey(m_eMoveForwardKey))
        {
			m_CockpitPilotState.MovingForward = true;
        }

        // Move backwards
        if (Input.GetKey(m_eMoveBackwardsKey))
        {
			m_CockpitPilotState.MovingBackward = true;
        }

        // Move left
        if ( Input.GetKey(m_eYawLeftKey))
        {
            m_CockpitPilotState.MovingLeft = true;
        }

        // Move right
        if (Input.GetKey(m_eYawRightKey))
        {
            m_CockpitPilotState.MovingRight = true;
        }
		
		// Roll left
        if ( Input.GetKey(m_eStrafeLeftKey))
        {
            m_CockpitPilotState.RollLeft = true;
        }

        // Roll right
        if (Input.GetKey(m_eStrafeRightKey))
        {
            m_CockpitPilotState.RollRight = true;
        }
		
		Vector2 rotationState = m_CockpitPilotState.Rotation;
		
		// Rotate around Y
		if (Input.GetAxis("Mouse X") != 0.0f)
        {
            rotationState.x += Input.GetAxis("Mouse X");
        }
		
		// Rotate around X
		if (Input.GetAxis("Mouse Y") != 0.0f)
        {
            rotationState.y += Input.GetAxis("Mouse Y");
        }
		
		m_CockpitPilotState.Rotation = rotationState;
	}

    private void AttachPlayer(CNetworkViewId _PlayerActorNetworkViewId)
    {
		m_CurrentPlayerInteractionEvent = EInteractionEvent.PlayerPiloting;

        m_AttachedPlayerActor = CNetwork.Factory.FindObject(_PlayerActorNetworkViewId);

		// Register this cockpit as the piloting cockpit of the ship
		CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>().PilotingCockpit = gameObject;
    }

    private void DetachPlayer()
    {
		m_CurrentPlayerInteractionEvent = EInteractionEvent.Nothing;

		m_AttachedPlayerActor = null;

		m_CockpitPilotState.ResetStates();
    }
}

