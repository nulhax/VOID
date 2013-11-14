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
	public enum EShipMovementState : uint
	{
		MoveForward 	= 1 << 0,
		MoveBackward 	= 1 << 1,
		MoveLeft 		= 1 << 2,
		MoveRight 		= 1 << 3,
		RollLeft 		= 1 << 4,
		RollRight 		= 1 << 5,
	}
	
	public class CShipPilotState
	{
		uint m_CurrentMovementState = 0;
		Vector2 m_CurrentRotationState = Vector2.zero;
		float m_LastUpdateTimeStamp = 0.0f;
			
		public uint CurrentState { get { return(m_CurrentMovementState); } }
		public Vector2 CurrentRotationState { get { return(m_CurrentRotationState); } }
		public float TimeStamp { get { return(m_LastUpdateTimeStamp); } }
			
		public void SetCurrentState(uint _NewMoveState, Vector2 _NewRotState, float _TimeStamp)
		{
			if(CNetwork.IsServer)
			{
				if(m_LastUpdateTimeStamp < _TimeStamp)
				{
					m_CurrentMovementState = _NewMoveState;
					m_CurrentRotationState = _NewRotState;
				}
			}
			else
			{
				Logger.Write("CBridgeCockpit CShipPilotState: Only server can direcly set the pilot state!");
			}
		}
		
		public Vector2 Rotation
		{
			set { m_CurrentRotationState = value; }
			get { return(m_CurrentRotationState); }
		}
		
		public bool MovingForward
		{
			set { SetState(value, CBridgeCockpit.EShipMovementState.MoveForward); }
			get { return(GetState(CBridgeCockpit.EShipMovementState.MoveForward)); }
		}
		
		public bool MovingBackward
		{
			set { SetState(value, CBridgeCockpit.EShipMovementState.MoveBackward); }
			get { return(GetState(CBridgeCockpit.EShipMovementState.MoveBackward)); }
		}
		
		public bool MovingLeft
		{
			set { SetState(value, CBridgeCockpit.EShipMovementState.MoveLeft); }
			get { return(GetState(CBridgeCockpit.EShipMovementState.MoveLeft)); }
		}
		
		public bool MovingRight
		{
			set { SetState(value, CBridgeCockpit.EShipMovementState.MoveRight); }
			get { return(GetState(CBridgeCockpit.EShipMovementState.MoveRight)); }
		}
		
		public bool RollLeft
		{
			set { SetState(value, CBridgeCockpit.EShipMovementState.RollLeft); }
			get { return(GetState(CBridgeCockpit.EShipMovementState.RollLeft)); }
		}
		
		public bool RollRight
		{
			set { SetState(value, CBridgeCockpit.EShipMovementState.RollRight); }
			get { return(GetState(CBridgeCockpit.EShipMovementState.RollRight)); }
		}
		
		private void SetState(bool _Value, EShipMovementState _State)
		{
			if(_Value)
			{
				m_CurrentMovementState |= (uint)_State;
			}
			else
			{
				m_CurrentMovementState &= ~(uint)_State;
			}
			
			m_LastUpdateTimeStamp = Time.time;
		}
		
		private bool GetState(EShipMovementState _State)
		{
			return((m_CurrentMovementState & (uint)_State) != 0);
		}
		
		public void ResetStates()
		{
			m_CurrentRotationState = Vector2.zero;
			m_CurrentMovementState = 0;
			m_LastUpdateTimeStamp = Time.time;
		}
	}

    // Member Delegates & Events

	
	// Member Fields
	public CShipPilotState m_PilotState = new CShipPilotState();
	
	GameObject m_AttachedPlayerActor = null;
	
	static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;
	static KeyCode m_eRollLeftKey = KeyCode.Q;
    static KeyCode m_eRollRightKey = KeyCode.E;
	
    // Member Properties


    // Member Methods
	public override void InstanceNetworkVars()
    {
		
    }
	
	public static void SerializePlayerState(CNetworkStream _cStream)
    {
//		if(CGame.PlayerActorViewId != 0)
//		{
//			CPlayerHeadMotor actorHeadMotor = CGame.PlayerActor.GetComponent<CPlayerHeadMotor>();
//			
//			_cStream.Write(actorHeadMotor.m_HeadMotorState.CurrentRotationState.x);
//			_cStream.Write(actorHeadMotor.m_HeadMotorState.CurrentRotationState.y);
//			_cStream.Write(actorHeadMotor.m_HeadMotorState.TimeStamp);
//			
//			actorHeadMotor.m_HeadMotorState.ResetStates();
//		}	
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
//		float rotationX = _cStream.ReadFloat();
//		float rotationY = _cStream.ReadFloat();
//		float timeStamp = _cStream.ReadFloat();
//		
//		CPlayerHeadMotor actorHeadMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerHeadMotor>();
//		
//		actorHeadMotor.m_HeadMotorState.SetCurrentRotation(new Vector2(rotationX, rotationY), timeStamp);
    }
	
	public void Start()
	{
		CInteractableObject IO = GetComponent<CInteractableObject>();
		
		IO.UseAction1 += HandlerPlayerActorAction1;
	}
	
	public void Update()
	{
		if(CGame.PlayerActor == m_AttachedPlayerActor)
		{
			UpdatePlayerInput();
			
			m_AttachedPlayerActor.transform.position = transform.position;
			m_AttachedPlayerActor.transform.eulerAngles = transform.eulerAngles;
		}
	}
	
	public void FixedUpdate()
	{
		ProcessMovement();
	}
	
	private void UpdatePlayerInput()
	{
		m_PilotState.ResetStates();
		
		// Move forwards
        if (Input.GetKey(m_eMoveForwardKey))
        {
			m_PilotState.MovingForward = true;
        }

        // Move backwards
        if (Input.GetKey(m_eMoveBackwardsKey))
        {
			m_PilotState.MovingBackward = true;
        }

        // Move left
        if ( Input.GetKey(m_eMoveLeftKey))
        {
            m_PilotState.MovingLeft = true;
        }

        // Move right
        if (Input.GetKey(m_eMoveRightKey))
        {
            m_PilotState.MovingRight = true;
        }
		
		// Roll left
        if ( Input.GetKey(m_eRollLeftKey))
        {
            m_PilotState.RollLeft = true;
        }

        // Roll right
        if (Input.GetKey(m_eRollRightKey))
        {
            m_PilotState.RollRight = true;
        }
		
		Vector2 rotationState = m_PilotState.Rotation;
		
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
		
		m_PilotState.Rotation = rotationState;
	}
	
	private void HandlerPlayerActorAction1(ushort _PlayerActorNetworkViewId, ushort _InteractableObjectNetworkViewId, RaycastHit _RayHit)
	{
		// Get the player actor
		GameObject playerActor = CNetwork.Factory.FindObject(_PlayerActorNetworkViewId);
		
		// Attach this player to the cockpit
		AttachPlayer(playerActor);
	}
	
	private void AttachPlayer(GameObject _PlayerActor)
	{
		m_AttachedPlayerActor = _PlayerActor;
			
		m_AttachedPlayerActor.GetComponent<CPlayerHeadMotor>().enabled = false;
		m_AttachedPlayerActor.GetComponent<CPlayerBodyMotor>().enabled = false;
		m_AttachedPlayerActor.collider.enabled = false;
	}
	
	public void DetachPlayer()
	{
		m_AttachedPlayerActor.GetComponent<CPlayerHeadMotor>().enabled = true;
		m_AttachedPlayerActor.GetComponent<CPlayerBodyMotor>().enabled = true;
	}
	
	protected void ProcessMovement()
    {
		Vector3 vDirForward = transform.parent.parent.forward;
		Vector3 vDirLeft = transform.parent.parent.right * -1.0f;
		Vector3 movementForce = Vector3.zero;
		
		// Moving 
        if (m_PilotState.MovingForward &&
            !m_PilotState.MovingBackward)
        {
            movementForce.z += 100.0f;
        }
        else if (m_PilotState.MovingBackward &&
            	 !m_PilotState.MovingForward)
        {
            movementForce.z -= 100.0f;
        }
		
		// Strafing
        if (m_PilotState.RollLeft &&
        	!m_PilotState.RollRight)
        {
            movementForce.x -= 100.0f;
        }
        else if (m_PilotState.RollRight &&
        		 !m_PilotState.RollLeft)
        {
            movementForce.x += 100.0f;
        }
        
		
		// Apply the movement
		transform.parent.parent.rigidbody.AddRelativeForce(movementForce, ForceMode.Acceleration);
		
		
		Vector3 angularForce = Vector3.zero;
		
		
		// Yaw Rotation
        if (m_PilotState.MovingLeft &&
        	!m_PilotState.MovingRight)
        {
            angularForce.y -= 1.0f;
        }
        else if (m_PilotState.MovingRight &&
        		 !m_PilotState.MovingLeft)
        {
            angularForce.y += 1.0f;
        }
		
		// Roll rotation
		if(m_PilotState.CurrentRotationState.x != 0.0f)
		{
			angularForce.z -= m_PilotState.CurrentRotationState.x;
		}
		
		// Pitch rotation
		if(m_PilotState.CurrentRotationState.y != 0.0f)
		{
			angularForce.x += m_PilotState.CurrentRotationState.y;
		}
		
		
		// Apply the rotation
		transform.parent.parent.rigidbody.AddRelativeTorque(angularForce, ForceMode.Acceleration);
	}
}

