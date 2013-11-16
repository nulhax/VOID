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
		
		PlayerEnter,
		PlayerLeave,
		PlayerPiloting,
		
		MAX
	}
	
    // Member Delegates & Events
	
	// Member Fields
	GameObject m_AttachedPlayerActor = null;
	CShipPilotState m_CockpitPilotState = new CShipPilotState();
	
	static private CNetworkStream s_CurrentCockpitInteractions = new CNetworkStream();
	
	static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eYawLeftKey = KeyCode.A;
    static KeyCode m_eYawRightKey = KeyCode.D;
	static KeyCode m_eStrafeLeftKey = KeyCode.Q;
    static KeyCode m_eStrafeRightKey = KeyCode.E;
	
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
		
    }
	
	public static void SerializeCockpitInteractions(CNetworkStream _cStream)
    {
		if(CGame.Ship.GetComponent<CShipMotor>().PilotingCockpit)
		{
			CBridgeCockpit cockpit = CGame.Ship.GetComponent<CShipMotor>().PilotingCockpit.GetComponent<CBridgeCockpit>();
			if(cockpit.m_AttachedPlayerActor)
			{	
				_cStream.Write((byte)EInteractionEvent.PlayerPiloting);
				_cStream.Write(cockpit.CockpitPilotState.CurrentState);
				_cStream.Write(cockpit.CockpitPilotState.CurrentRotationState.x);
				_cStream.Write(cockpit.CockpitPilotState.CurrentRotationState.y);
				_cStream.Write(cockpit.CockpitPilotState.TimeStamp);
				
				cockpit.CockpitPilotState.ResetStates();
			}
			
			if(s_CurrentCockpitInteractions.HasUnreadData)
			{
				_cStream.Write(s_CurrentCockpitInteractions);
				s_CurrentCockpitInteractions.Clear();
			}
		}
    }

	public static void UnserializeCockpitInteractions(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {	
		EInteractionEvent interactionEvent = (EInteractionEvent)_cStream.ReadByte();
		CShipMotor shipMotor = CGame.Ship.GetComponent<CShipMotor>();
		
		switch(interactionEvent)
		{
		case EInteractionEvent.PlayerEnter:
			shipMotor.PilotingCockpit.GetComponent<CBridgeCockpit>().InvokeRpcAll("AttachPlayer", CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CNetworkView>().ViewId);
			break;
			
		case EInteractionEvent.PlayerPiloting:
			uint motorState = _cStream.ReadUInt();
			float rotationX = _cStream.ReadFloat();
			float rotationY = _cStream.ReadFloat();
			float timeStamp = _cStream.ReadFloat();
			shipMotor.PilotingCockpit.GetComponent<CBridgeCockpit>().CockpitPilotState.SetCurrentState(motorState, new Vector2(rotationX, rotationY), timeStamp);
			break;
			
		case EInteractionEvent.PlayerLeave:
			break;
		}
    }
	
	public void Start()
	{
		// Register this cockpit as the piloting cockpit of the ship
		CGame.Ship.GetComponent<CShipMotor>().PilotingCockpit = gameObject;
		
		// Make this object interactable with action 1
		CInteractableObject IO = GetComponent<CInteractableObject>();
		
		IO.UseAction1 += HandlerPlayerActorAction1;
	}
	
	public void Update()
	{
		if(m_AttachedPlayerActor != null)
		{
			if(CGame.PlayerActor == m_AttachedPlayerActor)
			{
				UpdatePlayerInput();
			}
			
			m_AttachedPlayerActor.transform.position = transform.position;
			m_AttachedPlayerActor.transform.rotation = transform.rotation;
			m_AttachedPlayerActor.GetComponent<CPlayerHeadMotor>().ActorHead.transform.rotation = transform.parent.parent.rotation;
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
	
	private void HandlerPlayerActorAction1(RaycastHit _RayHit)
	{
		s_CurrentCockpitInteractions.Write((byte)EInteractionEvent.PlayerEnter);
	}
	
	[ANetworkRpc]
	private void AttachPlayer(ushort _PlayerActorNetworkViewId)
	{
		m_AttachedPlayerActor = CNetwork.Factory.FindObject(_PlayerActorNetworkViewId);	
		CPlayerBodyMotor bodyMotor = m_AttachedPlayerActor.GetComponent<CPlayerBodyMotor>();
		CPlayerHeadMotor headMotor = m_AttachedPlayerActor.GetComponent<CPlayerHeadMotor>();
		
		bodyMotor.collider.enabled = false;
		bodyMotor.FreezeMovmentInput = true;
		bodyMotor.m_Gravity = 0.0f;
		
		headMotor.enabled = false;
	}
	
	[ANetworkRpc]
	private void DetachPlayer()
	{
		
	}
}

