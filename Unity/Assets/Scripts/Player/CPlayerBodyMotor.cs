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
	bool m_bGrounded = false;
	
	private Vector3 m_GravityForce = Vector3.zero;
	private Vector3 m_TangVelocity = Vector3.zero;
	private Vector3 m_PrevTangVelocity = Vector3.zero;
	
	private Vector3 m_CurrentPos = Vector3.zero;
	private Vector3 m_CurrentMovementVelocity = Vector3.zero;
	private Vector3 m_CurrentCompoundGravity = Vector3.zero;
	

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
	public void Start()
	{
		if(!CNetwork.IsServer)
		{
			gameObject.rigidbody.isKinematic = true;
		}
	}
	
	static float bvla = 0.0f;
    public void Update()
    {	
		if(CGame.PlayerActor == gameObject && !FreezeMovmentInput)
		{
			UpdatePlayerInput();
		}
		
		if(CNetwork.IsServer)
		{
			// Placeholder: Make gravity relative to the ship
			m_GravityForce = CGame.Ship.transform.up * -m_Gravity;
			
			bvla += Time.deltaTime;
			if(bvla > 2.0f)
				m_CurrentPos = Quaternion.Inverse(CGame.Ship.rigidbody.rotation) * (rigidbody.worldCenterOfMass - CGame.Ship.rigidbody.worldCenterOfMass); bvla = 0.0f;
		}
    }
	
	public void FixedUpdate()
	{	
		if(CNetwork.IsServer)
		{
			// Compensate for ship movement
			ProcessShipCompensation();
			
			// Process the movement of the player
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
			
			_cStream.Write(actorMotor.m_MotorState.CurrentState);
			_cStream.Write(actorMotor.m_MotorState.TimeStamp);
			
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
	
    private void UpdatePlayerInput()
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
	
	private void ProcessMovement()
    {
		float moveSpeed = m_MovementSpeed;
		Vector3 moveVelocity = Vector3.zero;

		// Sprinting
		if(m_MotorState.Sprinting)
		{
			moveSpeed = m_SprintSpeed;
		}
		
		// Moving 
        if(m_MotorState.MovingForward != m_MotorState.MovingBackward)
		{
			moveVelocity.z = m_MotorState.MovingForward ? 1.0f : -1.0f;
		}
		
		// Strafing
		if(m_MotorState.MovingLeft != m_MotorState.MovingRight)
		{
			moveVelocity.x = m_MotorState.MovingLeft ? -1.0f : 1.0f;
		}
		
		// Normaize the move velocuity vector and multiply by the speed
		moveVelocity = moveVelocity.normalized * moveSpeed;
		
		Ray ray = new Ray(rigidbody.position, -transform.up);
		if(Physics.Raycast(ray, collider.bounds.extents.y))
		{
			m_bGrounded = true;
		}
		else
		{
			m_bGrounded = false;
		}
		
		if(!m_bGrounded)
		{	
			// Add the gravity gain to the velocity.
			rigidbody.AddForce(m_GravityForce, ForceMode.Acceleration);
		}
		else
		{
			m_CurrentMovementVelocity = moveVelocity;
		}
		
		
		
		// Get the relative velocity
		//Vector3 relativeVelocity = Quaternion.Inverse(transform.rotation) * rigidbody.velocity;
		
		// Set the new velocity, conserve the Y velocity for gravity
		//rigidbody.AddRelativeForce(new Vector3(newVelocity.x, relativeVelocity.y, newVelocity.z) - relativeVelocity, ForceMode.VelocityChange);
	}
	
	static float timer = 0;
	private void ProcessShipCompensation()
    {
		Rigidbody shipRigidBody = CGame.Ship.rigidbody;
		CShipMotor shipMotor = shipRigidBody.GetComponent<CShipMotor>();
		
		// Get the current tangential velocity
		m_TangVelocity = shipRigidBody.GetRelativePointVelocity(m_CurrentPos);	
		
		// Calculate the centripedal acceleration of the actor based on the last two tangential velocities
		Vector3 centripedalAccel = (m_TangVelocity - m_PrevTangVelocity) / Time.fixedDeltaTime;
		
		// Save the tangential velocity
		m_PrevTangVelocity = m_TangVelocity;
		
		// Add the compensation centripedal acceleration amount to the actor
		rigidbody.AddForce(centripedalAccel, ForceMode.Acceleration);

		// Set the current velocity as the tangential velocity
		rigidbody.velocity = m_TangVelocity + m_CurrentMovementVelocity;
		
//		Debug.DrawLine(rigidbody.worldCenterOfMass, rigidbody.worldCenterOfMass + centripedalAccel.normalized * 5.0f, Color.cyan);
//		Debug.DrawLine(rigidbody.worldCenterOfMass, rigidbody.worldCenterOfMass + m_TangVelocity.normalized * 5.0f, Color.green);
//		Debug.DrawLine(rigidbody.worldCenterOfMass, rigidbody.worldCenterOfMass + m_PrevTangVelocity.normalized * 5.0f, Color.yellow);
//		Debug.DrawLine(rigidbody.worldCenterOfMass, shipRigidBody.worldCenterOfMass, Color.blue);
		
		
		timer+=Time.fixedDeltaTime;
		if(timer > 0.2f)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.transform.position = rigidbody.position;
			DestroyImmediate(go.collider);
			timer = 0.0f;
		}
		
		
		
//		// Get the angular velocity ship
//		Vector3 angularVelocityCompensation = shipRigidBody.angularVelocity;
//		
//		// Convert it to relative angular velocity of the actor
//		angularVelocityCompensation = Quaternion.Inverse(transform.rotation) * angularVelocityCompensation;
//		
//		// Add the compensation angular velocity amount to the actor
//		rigidbody.AddRelativeTorque(angularVelocityCompensation, ForceMode.VelocityChange);
	}
};
