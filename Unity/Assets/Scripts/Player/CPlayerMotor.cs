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


public class CPlayerMotor : CNetworkMonoBehaviour
{

// Member Types
	public enum EInputStates : uint
	{
		MoveForward 	= 1 << 0,
		MoveBackward 	= 1 << 1,
		MoveLeft 		= 1 << 2,
		MoveRight 		= 1 << 3,
		Jump 			= 1 << 4,
		Sprint 			= 1 << 5,
		Action			= 1 << 6
	}
	
// Member Fields
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
	
	
	CNetworkVar<float> m_cHeadRotationX    = null;
    CNetworkVar<float> m_cHeadRotationY    = null;
    CNetworkVar<float> m_cHeadRotationZ    = null;
	CNetworkVar<float> m_cHeadRotationW    = null;
	
	
	GameObject m_ActorHead = null;

	
	uint m_PreviousInputState = 0;
    uint m_CurrentInputState = 0;
	
	
	Vector2 m_CurrentMouseXYState = Vector2.zero;
	
	
	Vector3 m_Velocity = Vector3.zero;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;
	static KeyCode m_eJumpKey = KeyCode.Space;
	static KeyCode m_eSprintKey = KeyCode.LeftShift;
	
	
// Member Properties	
	public GameObject ActorHead { get{ return(m_ActorHead); } }
	
	public uint PreviousInputState { get{ return(m_PreviousInputState); } }
	
	public uint CurrentInputState { get{ return(m_CurrentInputState); } }
	
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
		m_cHeadRotationX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cHeadRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_cHeadRotationZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cHeadRotationW = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }
	
	
    public void OnNetworkVarSync(INetworkVar _rSender)
    {
		if(!CNetwork.IsServer)
		{
			// Head Rotation
	        if (_rSender == m_cHeadRotationX || _rSender == m_cHeadRotationY || _rSender == m_cHeadRotationZ || _rSender == m_cHeadRotationW)
	        {	
	            m_ActorHead.transform.rotation = HeadRotation;
	        }
		}
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
			// Update the players input
			UpdatePlayerInput();
        }
		
		if(CNetwork.IsServer)
		{
			// Process the actor movements and rotations
			ProcessMovement();
			ProcessRotations();
			
			// Syncronize the head rotation
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
	

    public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.ActorViewId != 0 && !CNetwork.IsServer)
		{
			CPlayerMotor cActorMotor = CGame.Actor.GetComponent<CPlayerMotor>();
			
			_cStream.Write(cActorMotor.m_CurrentInputState);
			_cStream.Write(cActorMotor.m_CurrentMouseXYState.x);
			_cStream.Write(cActorMotor.m_CurrentMouseXYState.y);
		}
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
			CPlayerMotor actorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerMotor>();
			
			uint inputState = _cStream.ReadUInt();
			float x = _cStream.ReadFloat();
			float y = _cStream.ReadFloat();
			Vector2 mouseXYState = new Vector2(x, y);
			
			actorMotor.m_PreviousInputState = actorMotor.m_CurrentInputState;
		
        	actorMotor.m_CurrentInputState = inputState;
			actorMotor.m_CurrentMouseXYState = mouseXYState;
        }
    }
	
	
    protected void UpdatePlayerInput()
	{
		// Reset the input states
		m_CurrentInputState = 0;
		m_CurrentMouseXYState = Vector2.zero;
		
		// Move forwards
        if (Input.GetKey(m_eMoveForwardKey))
        {
			m_CurrentInputState |= (uint)EInputStates.MoveForward;
        }

        // Move backwards
        if (Input.GetKey(m_eMoveBackwardsKey))
        {
			m_CurrentInputState |= (uint)EInputStates.MoveBackward;
        }

        // Move left
        if ( Input.GetKey(m_eMoveLeftKey))
        {
            m_CurrentInputState |= (uint)EInputStates.MoveLeft;
        }

        // Move right
        if (Input.GetKey(m_eMoveRightKey))
        {
             m_CurrentInputState |= (uint)EInputStates.MoveRight;
        }
		
		// Jump
		if(Input.GetKey(m_eJumpKey))
		{
			m_CurrentInputState |= (uint)EInputStates.Jump;
		}
		
		// Sprint
		if (Input.GetKey(m_eSprintKey))
		{
			m_CurrentInputState |= (uint)EInputStates.Sprint;
		}
		
		// Action
		if (Input.GetMouseButtonDown(0))
		{
			m_CurrentInputState |= (uint)EInputStates.Action;
		}
		
		// Rotate around Y
		if (Input.GetAxis("Mouse X") != 0.0f)
        {
            m_CurrentMouseXYState.x = Input.GetAxis("Mouse X");
        }
		
		// Rotate around X
		if (Input.GetAxis("Mouse Y") != 0.0f)
        {
            m_CurrentMouseXYState.y = Input.GetAxis("Mouse Y");
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
			if((m_CurrentInputState & (uint)EInputStates.Sprint) != 0)
			{
				moveSpeed = m_SprintSpeed;
			}
			
			// Moving 
	        if ((m_CurrentInputState & (uint)EInputStates.MoveForward) != 0 &&
	            (m_CurrentInputState & ~(uint)EInputStates.MoveBackward) != 0)
	        {
	            m_Velocity += vDirForward * moveSpeed;
	        }
	        else if ((m_CurrentInputState & (uint)EInputStates.MoveBackward) != 0 &&
	            	 (m_CurrentInputState & ~(uint)EInputStates.MoveForward) != 0)
	        {
	            m_Velocity -= vDirForward * moveSpeed;
	        }
			
			// Strafing
	        if ((m_CurrentInputState & (uint)EInputStates.MoveLeft) != 0 &&
	            (m_CurrentInputState & ~(uint)EInputStates.MoveRight) != 0)
	        {
	            m_Velocity += vDirLeft * moveSpeed;
	        }
	        else if ((m_CurrentInputState & (uint)EInputStates.MoveRight) != 0 &&
	            	 (m_CurrentInputState & ~(uint)EInputStates.MoveLeft) != 0)
	        {
	            m_Velocity -= vDirLeft * moveSpeed;
	        }
			
			// Jumping
			if((m_CurrentInputState & (uint)EInputStates.Jump) != 0)
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
		if(m_CurrentMouseXYState.x != 0.0f)
		{
			m_RotationX += m_CurrentMouseXYState.x * m_SensitivityX;
			
			if(m_RotationX > 360.0f)
				m_RotationX -= 360.0f;
			else if(m_RotationX < -360.0f)
				m_RotationX += 360.0f;
				
			m_RotationX = Mathf.Clamp(m_RotationX, m_MinimumX, m_MaximumX);	
		}
		
		// Pitch rotation
		if(m_CurrentMouseXYState.y != 0.0f)
		{
			m_RotationY += m_CurrentMouseXYState.y * m_SensitivityY;
			m_RotationY = Mathf.Clamp(m_RotationY, m_MinimumY, m_MaximumY);
		}
		
		// Apply the pitch to the actor
		transform.eulerAngles = new Vector3(0.0f, m_RotationX, 0.0f);
		
		// Apply the yaw to the camera
		m_ActorHead.transform.eulerAngles = new Vector3(-m_RotationY, m_RotationX, 0.0f);
	}
};
