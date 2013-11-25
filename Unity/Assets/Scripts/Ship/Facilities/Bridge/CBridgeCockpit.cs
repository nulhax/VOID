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


public class CBridgeCockpit : CNetworkMonoBehaviour 
{
    // Member Types
	public enum EInteractionEvent : byte
	{
		INVALID,
		
		Nothing,
		PlayerEnter,
		PlayerExit,
		PlayerPiloting,
		
		MAX
	}
	
    // Member Delegates & Events
	
	// Member Fields
	private CShipPilotState m_CockpitPilotState = new CShipPilotState();
	
	private EInteractionEvent m_CurrentPlayerInteractionEvent = EInteractionEvent.Nothing;
	
	private GameObject m_AttachedPlayerActor = null;
	private CNetworkVar<ushort> m_AttachedPlayerActorViewId = null;
	
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
	public override void InstanceNetworkVars()
    {
		m_AttachedPlayerActorViewId = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
    }
	
	public void OnNetworkVarSync(INetworkVar _NetworkVar)
    {
        if (_NetworkVar == m_AttachedPlayerActorViewId)
        {
			// Attach the pilot if it isnt attached
			if(m_AttachedPlayerActorViewId.Get() != 0)
			{
				AttachPlayer(m_AttachedPlayerActorViewId.Get());
			}
			// Detach the pilot if it needs to detach
			else if(m_AttachedPlayerActorViewId.Get() == 0)
			{
				DetachPlayer();
			}
		}
	}
	
	public static void SerializeCockpitInteractions(CNetworkStream _cStream)
    {
		GameObject pilotingCockpit = CGame.Ship.GetComponent<CShipGalaxySimulatior>().GalaxyShip.GetComponent<CShipMotor>().PilotingCockpit;
		
		if(pilotingCockpit == null)
			return;
		
		CBridgeCockpit cockpit = pilotingCockpit.GetComponent<CBridgeCockpit>();
		switch(cockpit.m_CurrentPlayerInteractionEvent)
		{
		case EInteractionEvent.PlayerEnter:
			_cStream.Write((byte)EInteractionEvent.PlayerEnter);
			break;
			
		case EInteractionEvent.PlayerExit:
			_cStream.Write((byte)EInteractionEvent.PlayerExit);
			break;
			
		case EInteractionEvent.PlayerPiloting:
			_cStream.Write((byte)EInteractionEvent.PlayerPiloting);
			_cStream.Write(cockpit.CockpitPilotState.CurrentState);
			_cStream.Write(cockpit.CockpitPilotState.CurrentRotationState.x);
			_cStream.Write(cockpit.CockpitPilotState.CurrentRotationState.y);
			_cStream.Write(cockpit.CockpitPilotState.TimeStamp);
			cockpit.CockpitPilotState.ResetStates();
			break;
		}
    }

	public static void UnserializeCockpitInteractions(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {	
		EInteractionEvent interactionEvent = (EInteractionEvent)_cStream.ReadByte();
		CBridgeCockpit bridgeCockpit = CGame.GalaxyShip.GetComponent<CShipMotor>().PilotingCockpit.GetComponent<CBridgeCockpit>();
		
		switch(interactionEvent)
		{
		case EInteractionEvent.PlayerEnter:
			bridgeCockpit.m_AttachedPlayerActorViewId.Set(CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CNetworkView>().ViewId);
			break;
			
		case EInteractionEvent.PlayerExit:
			bridgeCockpit.m_AttachedPlayerActorViewId.Set(0);
			break;
			
		case EInteractionEvent.PlayerPiloting:
			uint motorState = _cStream.ReadUInt();
			float rotationX = _cStream.ReadFloat();
			float rotationY = _cStream.ReadFloat();
			float timeStamp = _cStream.ReadFloat();
			bridgeCockpit.CockpitPilotState.SetCurrentState(motorState, new Vector2(rotationX, rotationY), timeStamp);
			break;
		}
    }
	
	public void Awake()
	{
		gameObject.AddComponent<CInteractableObject>();
		gameObject.AddComponent<CNetworkView>();
	}
	
	public void Start()
	{
		// Register this cockpit as the piloting cockpit of the ship
		CGame.GalaxyShip.GetComponent<CShipMotor>().PilotingCockpit = gameObject;
		
		// Make this object interactable with action 1
		CInteractableObject IO = GetComponent<CInteractableObject>();
		IO.InteractionUse += HandlerPlayerActorUseAction;
	}
	
	public void Update()
	{	
		// Update the pilot states
		if(m_AttachedPlayerActor != null)
		{
			if(CGame.PlayerActor == m_AttachedPlayerActor)
			{
				UpdatePlayerInput();
			}
			
			m_AttachedPlayerActor.transform.position = transform.position;
			m_AttachedPlayerActor.transform.rotation = transform.rotation;
			m_AttachedPlayerActor.GetComponent<CPlayerHeadMotor>().ActorHead.transform.rotation = transform.parent.parent.rotation;
			
			CPlayerMotor bodyMotor = m_AttachedPlayerActor.GetComponent<CPlayerMotor>();
			CPlayerHeadMotor headMotor = m_AttachedPlayerActor.GetComponent<CPlayerHeadMotor>();
			
			bodyMotor.FreezeMovmentInput = true;
			headMotor.FreezeHeadInput = true;
		}
	}
	
	private void UpdatePlayerInput()
	{
		m_CockpitPilotState.ResetStates();
		
		// Exit
        if (Input.GetKey(m_eExitKey))
        {
			m_CurrentPlayerInteractionEvent = EInteractionEvent.PlayerExit;
        }
		
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
	
	private void HandlerPlayerActorUseAction(RaycastHit _RayHit)
	{
		if(m_AttachedPlayerActor == null)
		{
			m_CurrentPlayerInteractionEvent = EInteractionEvent.PlayerEnter;
		}
	}
	
	private void AttachPlayer(ushort _PlayerActorNetworkViewId)
	{
		m_AttachedPlayerActor = CNetwork.Factory.FindObject(_PlayerActorNetworkViewId);	
		
		m_CurrentPlayerInteractionEvent = EInteractionEvent.PlayerPiloting;
	}
	
	private void DetachPlayer()
	{
		m_AttachedPlayerActor.transform.position = transform.position + transform.up * 2.0f;
		
		CPlayerMotor bodyMotor = m_AttachedPlayerActor.GetComponent<CPlayerMotor>();
		CPlayerHeadMotor headMotor = m_AttachedPlayerActor.GetComponent<CPlayerHeadMotor>();
		
		bodyMotor.collider.enabled = true;
		
		bodyMotor.FreezeMovmentInput = false;
		headMotor.FreezeHeadInput = false;
		
		m_AttachedPlayerActor = null;
		
		m_CurrentPlayerInteractionEvent = EInteractionEvent.Nothing;
	}
}

