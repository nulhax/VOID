//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerMotor.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CPlayerMotor : CNetworkMonoBehaviour
{

// Member Types


	public enum EPlayerMovementState : uint
	{
		MoveForward		= 1 << 0,
		MoveBackward	= 1 << 1,
		MoveLeft		= 1 << 2,
		MoveRight		= 1 << 3,
		Jump			= 1 << 4,
		Sprint			= 1 << 5,
	}


// Member Delegates & Events


// Member Properties


	public float MovementSpeed
	{
		set { m_fMovementSpeed.Set(value); }
		get { return (m_fMovementSpeed.Get()); }
	}


	public float SprintSpeed
	{
		set { m_fSprintSpeed.Set(value); }
		get { return (m_fSprintSpeed.Get()); }
	}


	public float JumpSpeed
	{
		set { m_fJumpSpeed.Set(value); }
		get { return (m_fJumpSpeed.Get()); }
	}


	public bool InputDisabled
	{
		set { m_bFreezeMovmentInput.Set(value); }
		get { return (m_bFreezeMovmentInput.Get()); }
	}


	public bool UsingGravity
	{
		set { m_bUsingGravity.Set(value); }
		get { return (m_bUsingGravity.Get()); }
	}


	public bool IsGrounded
	{
		get { return (m_bGrounded); }
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_fRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fGravity = new CNetworkVar<float>(OnNetworkVarSync, -9.81f);
		m_fMovementSpeed = new CNetworkVar<float>(OnNetworkVarSync, 6.5f);
		m_fSprintSpeed = new CNetworkVar<float>(OnNetworkVarSync, 8.0f);
		m_fJumpSpeed = new CNetworkVar<float>(OnNetworkVarSync, 5.0f);
		m_bFreezeMovmentInput = new CNetworkVar<bool>(OnNetworkVarSync, false);
		m_bUsingGravity = new CNetworkVar<bool>(OnNetworkVarSync, true);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if (_cSyncedNetworkVar == m_fRotationY &&
			CGame.PlayerActor != gameObject)
		{
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, m_fRotationY.Get(), transform.eulerAngles.z);
		}
	}


	public void Start()
	{
		// Empty
	}


	public void Update()
	{
		// Process grounded check on server and client
		UpdateGrounded();

		// Process input only for client owned actors
		if (CGame.PlayerActor != null &&
			CGame.PlayerActor == gameObject)
		{
			UpdateInput();
			
		}

		// Process movement on server and client
		//if (CNetwork.IsServer)
			ProcessMovement();
	}


	public static void SerializePlayerState(CNetworkStream _cStream)
	{
		// Retrieve my actor motor
		CPlayerMotor cMyActorMotor = CGame.PlayerActor.GetComponent<CPlayerMotor>();

		// Write movement states
		_cStream.Write(cMyActorMotor.m_uiMovementStates);
		_cStream.Write(cMyActorMotor.transform.eulerAngles.y);
	}


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		// Retrieve player actor motor
		CPlayerMotor cPlayerActorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerMotor>();

		// Read movement states
		uint uiMovementStates = _cStream.ReadUInt();

		// Read rotation y
		float fRotationY = _cStream.ReadFloat();

		// Set movement states
		cPlayerActorMotor.m_uiMovementStates = uiMovementStates;

		// Set rotation y
		cPlayerActorMotor.m_fRotationY.Set(fRotationY);
	}


	void UpdateGrounded()
	{
		m_bGrounded = Physics.Raycast(transform.position, -Vector3.up, collider.bounds.extents.y + 0.1f);
	}


	void UpdateInput()
	{
		m_uiMovementStates  = 0;
		m_uiMovementStates |= Input.GetKey(s_eMoveForwardKey)   ? (uint)EPlayerMovementState.MoveForward  : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eMoveBackwardsKey) ? (uint)EPlayerMovementState.MoveBackward : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eMoveLeftKey)	    ? (uint)EPlayerMovementState.MoveLeft     : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eMoveRightKey)     ? (uint)EPlayerMovementState.MoveRight    : (uint)0;
		m_uiMovementStates |= Input.GetKeyDown(s_eJumpKey)      ? (uint)EPlayerMovementState.Jump         : (uint)0;
		m_uiMovementStates |= Input.GetKey(s_eSprintKey)        ? (uint)EPlayerMovementState.Sprint       : (uint)0;
	}


	void ProcessMovement()
	{
		// Direction movement
		Vector3 vMovementVelocity = new Vector3();
		vMovementVelocity += ((m_uiMovementStates & (uint)EPlayerMovementState.MoveForward)  > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_uiMovementStates & (uint)EPlayerMovementState.MoveBackward) > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_uiMovementStates & (uint)EPlayerMovementState.MoveLeft)     > 0) ? transform.right   : Vector3.zero;
		vMovementVelocity += ((m_uiMovementStates & (uint)EPlayerMovementState.MoveRight)    > 0) ? transform.right   : Vector3.zero;

		// Apply direction movement speed
		vMovementVelocity  = vMovementVelocity.normalized;
		vMovementVelocity *= ((m_uiMovementStates & (uint)EPlayerMovementState.Sprint) > 0) ? SprintSpeed : MovementSpeed;

		// Jump 
		if ((m_uiMovementStates & (uint)EPlayerMovementState.Jump) > 0 &&
			 IsGrounded)
		{
			vMovementVelocity.y = JumpSpeed;
		}

		// Apply movement velocity
		rigidbody.velocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
		rigidbody.AddForce(vMovementVelocity, ForceMode.VelocityChange);

		// Set latest position
		if (CNetwork.IsServer)
		{
			GetComponent<CNetworkInterpolatedObject>().SetCurrentPosition(transform.position);
		}
	}
	

// Member Fields


	CNetworkVar<float> m_fRotationY = null;
	CNetworkVar<float> m_fGravity = null;
	CNetworkVar<float> m_fMovementSpeed = null;
	CNetworkVar<float> m_fSprintSpeed = null;
	CNetworkVar<float> m_fJumpSpeed = null;
	CNetworkVar<bool> m_bFreezeMovmentInput = null;
	CNetworkVar<bool> m_bUsingGravity = null;


	uint m_uiMovementStates = 0;


	bool m_bGrounded = false;
	

    static KeyCode s_eMoveForwardKey = KeyCode.W;
    static KeyCode s_eMoveBackwardsKey = KeyCode.S;
    static KeyCode s_eMoveLeftKey = KeyCode.A;
    static KeyCode s_eMoveRightKey = KeyCode.D;
	static KeyCode s_eJumpKey = KeyCode.Space;
	static KeyCode s_eSprintKey = KeyCode.LeftShift;


};
