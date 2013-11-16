//  Auckland
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


public class CPlayerBodyMotor : CNetworkMonoBehaviour
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
		
		public uint CurrentState { get { return(m_CurrentMovementState); } }
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
			set { SetState(value, CPlayerBodyMotor.EPlayerMovementState.MoveForward); }
			get { return(GetState(CPlayerBodyMotor.EPlayerMovementState.MoveForward)); }
		}
		
		public bool MovingBackward
		{
			set { SetState(value, CPlayerBodyMotor.EPlayerMovementState.MoveBackward); }
			get { return(GetState(CPlayerBodyMotor.EPlayerMovementState.MoveBackward)); }
		}
		
		public bool MovingLeft
		{
			set { SetState(value, CPlayerBodyMotor.EPlayerMovementState.MoveLeft); }
			get { return(GetState(CPlayerBodyMotor.EPlayerMovementState.MoveLeft)); }
		}
		
		public bool MovingRight
		{
			set { SetState(value, CPlayerBodyMotor.EPlayerMovementState.MoveRight); }
			get { return(GetState(CPlayerBodyMotor.EPlayerMovementState.MoveRight)); }
		}
		
		public bool Jumping
		{
			set { SetState(value, CPlayerBodyMotor.EPlayerMovementState.Jump); }
			get { return(GetState(CPlayerBodyMotor.EPlayerMovementState.Jump)); }
		}
		
		public bool Sprinting
		{
			set { SetState(value, CPlayerBodyMotor.EPlayerMovementState.Sprint); }
			get { return(GetState(CPlayerBodyMotor.EPlayerMovementState.Sprint)); }
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

	
	public CPlayerMovementState m_MotorState = new CPlayerMovementState();
	
	
	bool m_FreezeMovmentInput = false;
	bool m_UsingGravity = true;
	Vector3 m_GravityForce = Vector3.zero;
	Vector3 m_Velocity = Vector3.zero;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;
	static KeyCode m_eJumpKey = KeyCode.Space;
	static KeyCode m_eSprintKey = KeyCode.LeftShift;
	
	
// Member Properties	
	public bool FreezeMovmentInput
	{
		set { m_FreezeMovmentInput = value; }
		get { return(m_FreezeMovmentInput); }
	}
	
	public bool UsingGravity
	{
		set { m_UsingGravity = value; }
		get { return(m_UsingGravity); }
	}
	
	public Vector3 GravityForce
	{
		set { m_GravityForce = value; }
		get { return(m_GravityForce); }
	}
	
// Member Methods
    public void Update()
    {	
		if(CGame.PlayerActor == gameObject)
		{
			UpdatePlayerInput();
		}
    }
	
	
	public void FixedUpdate()
	{	
		if(CNetwork.IsServer)
		{
			transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, 0.0f);
			
			ProcessMovement();
		}
	}
	
	public override void InstanceNetworkVars()
	{
		
	}
	
    public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.PlayerActorViewId != 0)
		{	
			CPlayerBodyMotor actorMotor = CGame.PlayerActor.GetComponent<CPlayerBodyMotor>();
			
			if(!actorMotor.FreezeMovmentInput)
			{
				_cStream.Write(actorMotor.m_MotorState.CurrentState);
				_cStream.Write(actorMotor.m_MotorState.TimeStamp);
			}
			actorMotor.m_MotorState.ResetStates();
		}	
    }

	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		CPlayerBodyMotor actorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerBodyMotor>();
		
		uint motorState = _cStream.ReadUInt();
		float timeStamp = _cStream.ReadFloat();
		
		actorMotor.m_MotorState.SetCurrentState(motorState, timeStamp);
    }
	
    protected void UpdatePlayerInput()
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
		float moveSpeed = m_MovementSpeed;
		Vector3 newVelocity = Vector3.zero;
		
		// Sprinting
		if(m_MotorState.Sprinting)
		{
			moveSpeed = m_SprintSpeed;
		}
		
		// Moving 
        if(m_MotorState.MovingForward != m_MotorState.MovingBackward)
		{
			newVelocity.z = m_MotorState.MovingForward ? moveSpeed : -moveSpeed;
		}
		
		// Strafing
		if(m_MotorState.MovingLeft != m_MotorState.MovingRight)
		{
			newVelocity.x = m_MotorState.MovingLeft ? -moveSpeed : moveSpeed;
		}
		
		// Placeholder: Make gravity relative to the ship
		m_GravityForce = CGame.Ship.transform.rotation * Vector3.up * -m_Gravity;
		
		// Jumping
		if(m_MotorState.Jumping)
		{
			rigidbody.AddForce(-m_GravityForce * 10.0f, ForceMode.Impulse);
		}
		
		// Gravity
		if(UsingGravity)
		{
			rigidbody.AddForce(m_GravityForce, ForceMode.Acceleration);
		}
		
		// Get the relative velocity and remove the upwards component
		Vector3 relativeVelocity = Quaternion.Inverse(transform.rotation) * rigidbody.velocity;
		relativeVelocity.y = 0.0f;
		
		// Set the new velocity
		rigidbody.AddRelativeForce(newVelocity - relativeVelocity, ForceMode.VelocityChange);
	}
};
