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
using System;


/* Implementation */


public class ActorMotor : CNetworkMonoBehaviour
{

// Member Types
	public class ActorInputStates
	{
		public bool m_bMoveForward 	= false;
	    public bool m_bMoveBackward = false;
	    public bool m_bMoveLeft 	= false;
	    public bool m_bMoveRight 	= false;
		public bool m_bJump 		= false;
		public bool m_bSprint 		= false;
		public bool m_bRotateYaw 	= false;
		
		public ActorInputStates() {}
		public ActorInputStates(ActorInputStates _other)
		{
			m_bMoveForward = _other.m_bMoveForward;
			m_bMoveBackward = _other.m_bMoveBackward;
			m_bMoveLeft = _other.m_bMoveLeft;
			m_bMoveRight = _other.m_bMoveRight;
			m_bJump = _other.m_bJump;
			m_bSprint = _other.m_bSprint;
			m_bRotateYaw = _other.m_bRotateYaw;
		}
		
		public override bool Equals(object _other) 
		{ 
			return base.Equals(_other as ActorInputStates); 
		}
		
		public bool Equals(ActorInputStates _other)
		{
			bool bReturn = true;
			
			if( this.m_bMoveForward != _other.m_bMoveForward ||
				this.m_bMoveBackward != _other.m_bMoveBackward ||
				this.m_bMoveLeft != _other.m_bMoveLeft ||
				this.m_bMoveRight != _other.m_bMoveRight ||
				this.m_bJump != _other.m_bJump ||
				this.m_bSprint != _other.m_bSprint ||
				this.m_bRotateYaw != _other.m_bRotateYaw)
				bReturn = false;
			
			return bReturn;
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
	
// Member Fields
    CNetworkVar<float> m_cPositionX    = null;
    CNetworkVar<float> m_cPositionY    = null;
    CNetworkVar<float> m_cPositionZ    = null;
	
	CNetworkVar<float> m_cVelocityX    = null;
    CNetworkVar<float> m_cVelocityY    = null;
    CNetworkVar<float> m_cVelocityZ    = null;
	
    CNetworkVar<float> m_cRotationX    = null;
    CNetworkVar<float> m_cRotationY    = null;
    CNetworkVar<float> m_cRotationZ    = null;
	CNetworkVar<float> m_cRotationW    = null;
	
	
	public float m_Gravity = 9.81f;
	public float m_MovementSpeed = 4.0f;
	public float m_SprintSpeed = 7.0f;
	public float m_JumpSpeed = 3.0f;
	
	public float m_SensitivityX = 0.5f;
	public float m_SensitivityY = 0.5f;

	public float m_MinimumX = -360.0f;
	public float m_MaximumX = 360.0f;

	public float m_MinimumY = -60.0f;
	public float m_MaximumY = 60.0f;

	public float m_RotationX = 0.0f;
	public float m_RotationY = 0.0f;
	
	
	GameObject m_Camera = null;

	
	ActorInputStates m_PreviousActorState;
    ActorInputStates m_CurrentActorState;
	
	
	bool m_Jumped = false;
	
	
	Vector3 m_Velocity = Vector3.zero;
	
	
	static bool s_bStateChanged = false;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;
	static KeyCode m_eJumpKey = KeyCode.Space;
	static KeyCode m_eSprintKey = KeyCode.LeftShift;
	
	
// Member Properties	
	public Vector3 Position
    {
        set 
		{ 
			m_cPositionX.Set(value.x); m_cPositionY.Set(value.y); m_cPositionZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_cPositionX.Get(), m_cPositionY.Get(), m_cPositionZ.Get())); 
		}
    }
				
	public Vector3 Velocity
    {
        set 
		{ 
			m_cVelocityX.Set(value.x); m_cVelocityY.Set(value.y); m_cVelocityZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_cVelocityX.Get(), m_cVelocityY.Get(), m_cVelocityZ.Get())); 
		}
    }
	
	public Quaternion Rotation
    {
        set 
		{ 
			m_cRotationX.Set(value.x); m_cRotationY.Set(value.y); m_cRotationZ.Set(value.z); m_cRotationW.Set(value.w);
		}
        get 
		{ 
			return (new Quaternion(m_cRotationX.Get(), m_cRotationY.Get(), m_cRotationZ.Get(), m_cRotationW.Get())); 
		}
    }

// Member Methods
    public override void InstanceNetworkVars()
    {
		m_cPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
		m_cVelocityX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cVelocityY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cVelocityZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
        m_cRotationX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_cRotationZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cRotationW = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }


    public void Start()
	{
		if (gameObject != CGame.Actor)
		{
			//gameObject.GetComponent<Rigidbody>().isKinematic = true;
		}
		
		m_CurrentActorState = new ActorInputStates();
		m_PreviousActorState = new ActorInputStates();
	}


    public void OnDestroy()
    {
    }


    public void Update()
    {
        if (gameObject == CGame.Actor)
        {
			UpdatePlayerInput();
			ProcessSelfMovement();
			ProcessSelfRotations();
        }
		else
		{
			ProcessOthersMovement();
		}
    }
	
	
	public void CreatePlayerClientCamera()
    {
		// Disable the current camera
        Camera.main.enabled = false;
		
		// Create te camera object for the camera
		m_Camera = GameObject.Instantiate(Resources.Load("Prefabs/Player/Actor Camera", typeof(GameObject))) as GameObject;
        m_Camera.transform.parent = transform;
        m_Camera.transform.localPosition = Vector3.zero;
    }

    public void OnNetworkVarSync(INetworkVar _rSender)
    {
		// Position
        if (_rSender == m_cPositionX || _rSender == m_cPositionY || _rSender == m_cPositionZ)
		{
			if(gameObject.GetComponent<CNetworkView>().ViewId != CGame.ActorViewId)
				transform.position = Position;
		}
		
		// Velocity
        else if (_rSender == m_cVelocityX || _rSender == m_cVelocityY || _rSender == m_cVelocityZ)
        {	
			if(gameObject.GetComponent<CNetworkView>().ViewId != CGame.ActorViewId)
            	m_Velocity = Velocity;
        }
		
		// Rotation
        else if (_rSender == m_cRotationX || _rSender == m_cRotationY || _rSender == m_cRotationZ || _rSender == m_cRotationW)
        {	
			if(gameObject.GetComponent<CNetworkView>().ViewId != CGame.ActorViewId)
            	transform.rotation = Rotation;
        }

    }
	

    public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(s_bStateChanged)
		{	
			CharacterController charController = CGame.Actor.GetComponent<CharacterController>();
			
			_cStream.Write(CGame.Actor.transform.position.x);
			_cStream.Write(CGame.Actor.transform.position.y);
			_cStream.Write(CGame.Actor.transform.position.z);
			
	        _cStream.Write(charController.velocity.x);
			_cStream.Write(charController.velocity.y);
			_cStream.Write(charController.velocity.z);
			
			_cStream.Write(CGame.Actor.transform.rotation.x);
			_cStream.Write(CGame.Actor.transform.rotation.y);
			_cStream.Write(CGame.Actor.transform.rotation.z);
			_cStream.Write(CGame.Actor.transform.rotation.w);
		}
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
			ActorMotor cActorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<ActorMotor>();

            float x = _cStream.ReadFloat();
			float y = _cStream.ReadFloat();
			float z = _cStream.ReadFloat();
			
			cActorMotor.Position = new Vector3(x, y, z);
			
			x = _cStream.ReadFloat();
			y = _cStream.ReadFloat();
			z = _cStream.ReadFloat();
			
			cActorMotor.Velocity = new Vector3(x, y, z);
			
			x = _cStream.ReadFloat();
			y = _cStream.ReadFloat();
			z = _cStream.ReadFloat();
			float w = _cStream.ReadFloat();
			
			cActorMotor.Rotation = new Quaternion(x, y, z, w);
        }
    }
	
	
    protected void UpdatePlayerInput()
	{
		// Move forwards
        if (Input.GetKeyDown(KeyCode.W))
        {
           m_CurrentActorState.m_bMoveForward = true;
        }
        
        // Stop moving forwards
        else if (Input.GetKeyUp(m_eMoveForwardKey))
        {
            m_CurrentActorState.m_bMoveForward = false;
        }

        // Move backwards
        if (Input.GetKeyDown(m_eMoveBackwardsKey))
        {
            m_CurrentActorState.m_bMoveBackward = true;
        }

        // Stop moving backwards
        else if (Input.GetKeyUp(m_eMoveBackwardsKey))
        {
            m_CurrentActorState.m_bMoveBackward = false;
        }

        // Move left
        if ( Input.GetKeyDown(m_eMoveLeftKey))
        {
            m_CurrentActorState.m_bMoveLeft = true;
        }

        // Stop moving left
        else if (Input.GetKeyUp(m_eMoveLeftKey))
        {
            m_CurrentActorState.m_bMoveLeft = false;
        }

        // Move right
        if (Input.GetKeyDown(m_eMoveRightKey))
        {
            m_CurrentActorState.m_bMoveRight = true;
        }

        // Stop moving right
        else if (Input.GetKeyUp(m_eMoveRightKey))
        {
            m_CurrentActorState.m_bMoveRight = false;
        }
		
		// Rotate
		if (Input.GetAxis("Mouse X") != 0.0f)
        {
            m_CurrentActorState.m_bRotateYaw = true;
        }
		else
		{
			 m_CurrentActorState.m_bRotateYaw = false;
		}

		// Jump
		if(Input.GetKeyDown(m_eJumpKey))
		{
			m_CurrentActorState.m_bJump = true;
		}
		else if(Input.GetKeyUp(m_eJumpKey))
		{
			m_CurrentActorState.m_bJump = false;
		}
		
		// Sprint
		if (Input.GetKeyDown(m_eSprintKey))
		{
			m_CurrentActorState.m_bSprint = true;
		}
		else if(Input.GetKeyUp(m_eSprintKey))
		{
			m_CurrentActorState.m_bSprint = false;
		}
		
		// If any of the above are true, the state will change
		if(!m_CurrentActorState.Equals(m_PreviousActorState))
		{
 			s_bStateChanged = true;
		}
		else
		{
			s_bStateChanged = false;
		}
		
		m_PreviousActorState = new ActorInputStates(m_CurrentActorState);
	}
	
	
	protected void ProcessSelfMovement()
    {
		CharacterController charController = GetComponent<CharacterController>();
		
		// Only if grounded
		if(charController.isGrounded)
		{
			Vector3 vDirForward = transform.TransformDirection(Vector3.forward);
			Vector3 vDirLeft = transform.TransformDirection(Vector3.left);
			float moveSpeed = m_CurrentActorState.m_bSprint ? m_SprintSpeed : m_MovementSpeed;
			m_Velocity = new Vector3(0.0f, m_Velocity.y, 0.0f);
			
			// We jumped, we landed, our state has changed
			if(m_Jumped)
			{
				s_bStateChanged = true;
				m_Jumped = false;
			}
			
			// Moving 
	        if (m_CurrentActorState.m_bMoveForward &&
	            !m_CurrentActorState.m_bMoveBackward)
	        {
	            m_Velocity += vDirForward * moveSpeed;
	        }
	        else if (m_CurrentActorState.m_bMoveBackward &&
	                 !m_CurrentActorState.m_bMoveForward)
	        {
	            m_Velocity -= vDirForward * moveSpeed;
	        }
			
			// Strafing
	        if (m_CurrentActorState.m_bMoveLeft &&
	            !m_CurrentActorState.m_bMoveRight)
	        {
	            m_Velocity += vDirLeft * moveSpeed;
	        }
	        else if (m_CurrentActorState.m_bMoveRight &&
	                !m_CurrentActorState.m_bMoveLeft)
	        {
	            m_Velocity -= vDirLeft * moveSpeed;
	        }
			
			// Jumping
			if(m_CurrentActorState.m_bJump)
			{
				m_Velocity.y = m_JumpSpeed;
				m_Jumped = true;
			}
		}
		
		m_Velocity.y += -m_Gravity * Time.deltaTime;
		charController.Move(m_Velocity * Time.deltaTime);
	}
	
	
	protected void ProcessSelfRotations()
	{
		// Yaw rotation
		if(m_CurrentActorState.m_bRotateYaw)
		{
			m_RotationX += Input.GetAxis("Mouse X") * m_SensitivityX;
			
			if(m_RotationX > 360.0f)
				m_RotationX -= 360.0f;
			else if(m_RotationX < -360.0f)
				m_RotationX += 360.0f;
				
			m_RotationX = Mathf.Clamp(m_RotationX, m_MinimumX, m_MaximumX);	
		}
		
		// Pitch rotation
		m_RotationY += Input.GetAxis("Mouse Y") * m_SensitivityY;
		m_RotationY = Mathf.Clamp(m_RotationY, m_MinimumY, m_MaximumY);
		
		// Apply the yaw to the camera
		m_Camera.transform.eulerAngles = new Vector3(-m_RotationY, m_RotationX, 0.0f);
		
		// Apply the pitch to the actor
		transform.eulerAngles = new Vector3(0.0f, m_RotationX, 0.0f);
	}
	
	
	protected void ProcessOthersMovement()
    {
		CharacterController charController = GetComponent<CharacterController>();

		m_Velocity.y += -m_Gravity * Time.deltaTime;
		charController.Move(m_Velocity * Time.deltaTime);
    }
};
