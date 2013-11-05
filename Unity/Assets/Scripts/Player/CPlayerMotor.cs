﻿//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CPlayerMotor : CNetworkMonoBehaviour
{

// Member Types
	public enum EPlayerMovementState : uint
	{
		MoveForward 	= 1 << 0,
		MoveBackward 	= 1 << 1,
		MoveLeft 		= 1 << 2,
		MoveRight 		= 1 << 3,
		Jump 			= 1 << 4,
		Sprint 			= 1 << 5,
	}
	
	public class CPlayerMovementState
	{
		uint m_CurrentMovementState = 0;
		float m_LastUpdateTimeStamp = 0.0f;
		
		public float CurrentState { get { return(m_CurrentMovementState); } }
		public float TimeStamp { get { return(m_LastUpdateTimeStamp); } }
		
		public void SetCurrentState(uint _NewState, float _TimeStamp)
		{
			if(CNetwork.IsServer)
			{
				if(m_LastUpdateTimeStamp < _TimeStamp)
				{
					m_CurrentMovementState = _NewState;
				}
			}
			else
			{
				Logger.Write("Player MotorState: Only server can direcly set the motor state!");
			}
		}
		
		public bool MovingForward
		{
			set { SetState(value, CPlayerMotor.EPlayerMovementState.MoveForward); }
			get { return(GetState(CPlayerMotor.EPlayerMovementState.MoveForward)); }
		}
		
		public bool MovingBackward
		{
			set { SetState(value, CPlayerMotor.EPlayerMovementState.MoveBackward); }
			get { return(GetState(CPlayerMotor.EPlayerMovementState.MoveBackward)); }
		}
		
		public bool MovingLeft
		{
			set { SetState(value, CPlayerMotor.EPlayerMovementState.MoveLeft); }
			get { return(GetState(CPlayerMotor.EPlayerMovementState.MoveLeft)); }
		}
		
		public bool MovingRight
		{
			set { SetState(value, CPlayerMotor.EPlayerMovementState.MoveRight); }
			get { return(GetState(CPlayerMotor.EPlayerMovementState.MoveRight)); }
		}
		
		public bool Jumping
		{
			set { SetState(value, CPlayerMotor.EPlayerMovementState.Jump); }
			get { return(GetState(CPlayerMotor.EPlayerMovementState.Jump)); }
		}
		
		public bool Sprinting
		{
			set { SetState(value, CPlayerMotor.EPlayerMovementState.Sprint); }
			get { return(GetState(CPlayerMotor.EPlayerMovementState.Sprint)); }
		}
		
		private void SetState(bool _Value, EPlayerMovementState _State)
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
		
		private bool GetState(EPlayerMovementState _State)
		{
			return((m_CurrentMovementState & (uint)_State) != 0);
		}
		
		public void ResetStates()
		{
			m_CurrentMovementState = 0;
			m_LastUpdateTimeStamp = Time.time;
		}
	}
	
// Member Fields
	public float m_Gravity = 9.81f;
	public float m_MovementSpeed = 4.0f;
	public float m_SprintSpeed = 7.0f;
	public float m_JumpSpeed = 3.0f;

	
	CPlayerMovementState m_MotorState = new CPlayerMovementState();
	
	
	Vector3 m_Velocity = Vector3.zero;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;
	static KeyCode m_eJumpKey = KeyCode.Space;
	static KeyCode m_eSprintKey = KeyCode.LeftShift;
	
	
// Member Properties	

	
// Member Methods
    public void Update()
    {	
		if(CGame.Actor == gameObject)
		{
			ClientUpdatePlayerInput();
		}
		
		if(CNetwork.IsServer)
		{	
			// Process the actor movements
			ProcessMovement();
		}
    }
	
	public override void InstanceNetworkVars()
	{
		
	}
	
    public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.ActorViewId != 0)
		{
			CPlayerMotor actorMotor = CGame.Actor.GetComponent<CPlayerMotor>();
			
			_cStream.Write(actorMotor.m_MotorState.CurrentState);
			_cStream.Write(actorMotor.m_MotorState.TimeStamp);
			
			actorMotor.m_MotorState.ResetStates();
		}	
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		CPlayerMotor actorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerMotor>();
		
		uint motorState = _cStream.ReadUInt();
		float timeStamp = _cStream.ReadFloat();
		
		actorMotor.m_MotorState.SetCurrentState(motorState, timeStamp);
    }
	
    protected void ClientUpdatePlayerInput()
	{	
		// Move forwards
        if (Input.GetKey(m_eMoveForwardKey))
        {
			m_MotorState.MovingForward = true;
        }

        // Move backwards
        if (Input.GetKey(m_eMoveBackwardsKey))
        {
			m_MotorState.MovingBackward = true;
        }

        // Move left
        if ( Input.GetKey(m_eMoveLeftKey))
        {
            m_MotorState.MovingLeft = true;
        }

        // Move right
        if (Input.GetKey(m_eMoveRightKey))
        {
            m_MotorState.MovingRight = true;
        }
		
		// Jump
		if(Input.GetKey(m_eJumpKey))
		{
			m_MotorState.Jumping = true;
		}
		
		// Sprint
		if (Input.GetKey(m_eSprintKey))
		{
			m_MotorState.Sprinting = true;
		}
	}
	
	
	protected void ProcessMovement()
    {
		CharacterController charController = GetComponent<CharacterController>();
		
		// Only if grounded
		if(charController.isGrounded)
		{
			Vector3 vDirForward = transform.TransformDirection(Vector3.forward);
			Vector3 vDirLeft = transform.TransformDirection(Vector3.left);
			float moveSpeed = m_MovementSpeed;
			m_Velocity = new Vector3(0.0f, m_Velocity.y, 0.0f);
			
			// Sprinting
			if(m_MotorState.Sprinting)
			{
				moveSpeed = m_SprintSpeed;
			}
			
			// Moving 
	        if (m_MotorState.MovingForward &&
	            !m_MotorState.MovingBackward)
	        {
	            m_Velocity += vDirForward * moveSpeed;
	        }
	        else if (m_MotorState.MovingBackward &&
	            	 !m_MotorState.MovingForward)
	        {
	            m_Velocity -= vDirForward * moveSpeed;
	        }
			
			// Strafing
	        if (m_MotorState.MovingLeft &&
            	!m_MotorState.MovingRight)
	        {
	            m_Velocity += vDirLeft * moveSpeed;
	        }
	        else if (m_MotorState.MovingRight &&
            		 !m_MotorState.MovingLeft)
	        {
	            m_Velocity -= vDirLeft * moveSpeed;
	        }
			
			// Jumping
			if(m_MotorState.Jumping)
			{
				m_Velocity.y = m_JumpSpeed;
			}
		}
		else
		{
			// Apply the gravity
			m_Velocity.y += -m_Gravity * Time.deltaTime;
		}
		
		// Apply the movement
		charController.Move(m_Velocity * Time.deltaTime);
	}
};