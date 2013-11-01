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
	public enum InputStates : uint
	{
		MoveForward 	= 1 << 0,
		MoveBackward 	= 1 << 1,
		MoveLeft 		= 1 << 2,
		MoveRight 		= 1 << 3,
		Jump 			= 1 << 4,
		Sprint 			= 1 << 5,
	}
	
// Member Fields
    CNetworkVar<float> m_cPositionX    = null;
    CNetworkVar<float> m_cPositionY    = null;
    CNetworkVar<float> m_cPositionZ    = null;
	
    CNetworkVar<float> m_cRotationX    = null;
    CNetworkVar<float> m_cRotationY    = null;
    CNetworkVar<float> m_cRotationZ    = null;
	CNetworkVar<float> m_cRotationW    = null;
	
	CNetworkVar<float> m_cHeadRotationX    = null;
    CNetworkVar<float> m_cHeadRotationY    = null;
    CNetworkVar<float> m_cHeadRotationZ    = null;
	CNetworkVar<float> m_cHeadRotationW    = null;
	
	
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
	
	
	GameObject m_ActorHead = null;

	
    uint m_CurrentKeyboardInputState = 0;
	Vector2 m_CurrentMouseInputState = Vector2.zero;
	
	
	Vector3 m_Velocity = Vector3.zero;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;
	static KeyCode m_eJumpKey = KeyCode.Space;
	static KeyCode m_eSprintKey = KeyCode.LeftShift;
	
	
// Member Properties	
	public GameObject ActorHead { get; set; }
	
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
	
	public Quaternion HeadRotation
    {
        set 
		{ 
			m_cHeadRotationX.Set(value.x); m_cHeadRotationY.Set(value.y); m_cHeadRotationZ.Set(value.z); m_cHeadRotationW.Set(value.w);
		}
        get 
		{ 
			return (new Quaternion(m_cHeadRotationX.Get(), m_cHeadRotationY.Get(), m_cHeadRotationZ.Get(), m_cHeadRotationW.Get())); 
		}
    }

// Member Methods
    public override void InstanceNetworkVars()
    {
		m_cPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
        m_cRotationX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_cRotationZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cRotationW = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
		m_cHeadRotationX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cHeadRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_cHeadRotationZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cHeadRotationW = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }


    public void Awake()
	{	
		// Create the actor head object
		m_ActorHead = new GameObject(name + "_Head");
        m_ActorHead.transform.parent = transform;
        m_ActorHead.transform.localPosition = Vector3.zero;
	}


    public void OnDestroy()
    {
    }


    public void Update()
    {
        if (gameObject == CGame.Actor)
        {
			UpdatePlayerInput();
        }
		
		if(CNetwork.IsServer)
		{
			// Process the actor movements and rotations
			ProcessMovement();
			ProcessRotations();
			
			// Syncronize the movement and rotation variables for all clients
			Position = transform.position;
			Rotation = transform.rotation;
			HeadRotation = m_ActorHead.transform.rotation;
		}
    }
	
	
	public void CreatePlayerClientCamera()
    {
		// Disable the current camera
        Camera.main.enabled = false;
		
		// Destory the current head
		Destroy(m_ActorHead);
		
		// Create the camera object for the camera
		m_ActorHead = GameObject.Instantiate(Resources.Load("Prefabs/Player/Actor Camera", typeof(GameObject))) as GameObject;
        m_ActorHead.transform.parent = transform;
        m_ActorHead.transform.localPosition = Vector3.zero;
    }

    public void OnNetworkVarSync(INetworkVar _rSender)
    {
		// Position
        if (_rSender == m_cPositionX || _rSender == m_cPositionY || _rSender == m_cPositionZ)
		{
			transform.position = Position;
		}
		
		// Rotation
        else if (_rSender == m_cRotationX || _rSender == m_cRotationY || _rSender == m_cRotationZ || _rSender == m_cRotationW)
        {	
            transform.rotation = Rotation;
        }
		
		// Head Rotation
        else if (_rSender == m_cHeadRotationX || _rSender == m_cHeadRotationY || _rSender == m_cHeadRotationZ || _rSender == m_cHeadRotationW)
        {	
            m_ActorHead.transform.rotation = HeadRotation;
        }

    }
	

    public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.ActorViewId != 0)
		{
			ActorMotor cActorMotor = CGame.Actor.GetComponent<ActorMotor>();
			
			_cStream.Write(cActorMotor.m_CurrentKeyboardInputState);
			_cStream.Write(cActorMotor.m_CurrentMouseInputState.x);
			_cStream.Write(cActorMotor.m_CurrentMouseInputState.y);
		}
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
			ActorMotor cActorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<ActorMotor>();

            cActorMotor.m_CurrentKeyboardInputState = _cStream.ReadUInt();
			
			float x = _cStream.ReadFloat();
			float y = _cStream.ReadFloat();
			
			cActorMotor.m_CurrentMouseInputState = new Vector2(x, y);
        }
    }
	
	
    protected void UpdatePlayerInput()
	{
		// Reset the input states
		m_CurrentKeyboardInputState = 0;
		m_CurrentMouseInputState = Vector2.zero;
		
		// Move forwards
        if (Input.GetKey(m_eMoveForwardKey))
        {
			m_CurrentKeyboardInputState |= (uint)InputStates.MoveForward;
        }

        // Move backwards
        if (Input.GetKey(m_eMoveBackwardsKey))
        {
			m_CurrentKeyboardInputState |= (uint)InputStates.MoveBackward;
        }

        // Move left
        if ( Input.GetKey(m_eMoveLeftKey))
        {
            m_CurrentKeyboardInputState |= (uint)InputStates.MoveLeft;
        }

        // Move right
        if (Input.GetKey(m_eMoveRightKey))
        {
             m_CurrentKeyboardInputState |= (uint)InputStates.MoveRight;
        }
		
		// Jump
		if(Input.GetKey(m_eJumpKey))
		{
			m_CurrentKeyboardInputState |= (uint)InputStates.Jump;
		}
		
		// Sprint
		if (Input.GetKey(m_eSprintKey))
		{
			m_CurrentKeyboardInputState |= (uint)InputStates.Sprint;
		}
		
		// Rotate around Y
		if (Input.GetAxis("Mouse X") != 0.0f)
        {
            m_CurrentMouseInputState.x = Input.GetAxis("Mouse X");
        }
		
		// Rotate around X
		if (Input.GetAxis("Mouse Y") != 0.0f)
        {
            m_CurrentMouseInputState.y = Input.GetAxis("Mouse Y");
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
			if((m_CurrentKeyboardInputState & (uint)InputStates.Sprint) != 0)
			{
				moveSpeed = m_SprintSpeed;
			}
			
			// Moving 
	        if ((m_CurrentKeyboardInputState & (uint)InputStates.MoveForward) != 0 &&
	            (m_CurrentKeyboardInputState & ~(uint)InputStates.MoveBackward) != 0)
	        {
	            m_Velocity += vDirForward * moveSpeed;
	        }
	        else if ((m_CurrentKeyboardInputState & (uint)InputStates.MoveBackward) != 0 &&
	            	 (m_CurrentKeyboardInputState & ~(uint)InputStates.MoveForward) != 0)
	        {
	            m_Velocity -= vDirForward * moveSpeed;
	        }
			
			// Strafing
	        if ((m_CurrentKeyboardInputState & (uint)InputStates.MoveLeft) != 0 &&
	            (m_CurrentKeyboardInputState & ~(uint)InputStates.MoveRight) != 0)
	        {
	            m_Velocity += vDirLeft * moveSpeed;
	        }
	        else if ((m_CurrentKeyboardInputState & (uint)InputStates.MoveRight) != 0 &&
	            	 (m_CurrentKeyboardInputState & ~(uint)InputStates.MoveLeft) != 0)
	        {
	            m_Velocity -= vDirLeft * moveSpeed;
	        }
			
			// Jumping
			if((m_CurrentKeyboardInputState & (uint)InputStates.Jump) != 0)
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
	
	
	protected void ProcessRotations()
	{
		// Yaw rotation
		if(m_CurrentMouseInputState.x != 0.0f)
		{
			m_RotationX += m_CurrentMouseInputState.x * m_SensitivityX;
			
			if(m_RotationX > 360.0f)
				m_RotationX -= 360.0f;
			else if(m_RotationX < -360.0f)
				m_RotationX += 360.0f;
				
			m_RotationX = Mathf.Clamp(m_RotationX, m_MinimumX, m_MaximumX);	
		}
		
		// Pitch rotation
		if(m_CurrentMouseInputState.y != 0.0f)
		{
			m_RotationY += m_CurrentMouseInputState.y * m_SensitivityY;
			m_RotationY = Mathf.Clamp(m_RotationY, m_MinimumY, m_MaximumY);
		}
		
		// Apply the pitch to the actor
		transform.eulerAngles = new Vector3(0.0f, m_RotationX, 0.0f);
		
		// Apply the yaw to the camera
		m_ActorHead.transform.eulerAngles = new Vector3(-m_RotationY, m_RotationX, 0.0f);
	}
};
