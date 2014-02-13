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
using System;


/* Implementation */


[RequireComponent(typeof(Rigidbody))]
public class CPlayerGroundMotor : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction : byte
	{
		UpdateStates,
	}


	public enum EState : uint
	{
		MoveForward		= 1 << 0,
		MoveBackward	= 1 << 1,
		MoveLeft		= 1 << 2,
		MoveRight		= 1 << 3,
		Jump			= 1 << 4,
		Sprint			= 1 << 5,
		Crouch			= 1 << 6,
	}


// Member Delegates & Events


	public delegate void NotifyStatesChange(byte _bPreviousStates, byte _bNewSates);
	public event NotifyStatesChange EventStatesChange;


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
		get { return (m_cInputDisableQueue.Count > 0); }
	}


	public bool IsGrounded
	{
		get { return (m_bGrounded); }
	}


	public byte States
	{
		get { return (m_bStates.Get()); }
	}

	
	public byte PreviousStates
	{
		get { return (m_bStates.GetPrevious()); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fRotationY = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_fGravity = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, -9.81f);
		m_fMovementSpeed = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 6.5f);
		m_fSprintSpeed = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 8.0f);
		m_fJumpSpeed = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 2.0f);
		m_bStates = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, 0);
	}

	[AClientOnly]
	public void DisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Add(_cFreezeRequester.GetType());
        gameObject.GetComponent<CThirdPersonAnimController>().DisableAnimation();
	}

	[AClientOnly]
	public void ReenableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Remove(_cFreezeRequester.GetType());
        gameObject.GetComponent<CThirdPersonAnimController>().EnableAnimation();
	}


	public static void SerializePlayerState(CNetworkStream _cStream)
	{
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
	}


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);

		if (cPlayerActor != null)
		{
			// Retrieve player actor motor
			CPlayerGroundMotor cPlayerActorMotor = cPlayerActor.GetComponent<CPlayerGroundMotor>();

			ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();
		
			switch (eNetworkAction)
			{
			case ENetworkAction.UpdateStates:
				{
					cPlayerActorMotor.m_uiMovementStates = _cStream.ReadByte();

					if (cPlayerActor != CGamePlayers.SelfActor)
					{
						cPlayerActor.transform.eulerAngles = new Vector3(0.0f, _cStream.ReadFloat(),  0.0f);
					}
					else
					{
						_cStream.ReadFloat();
					}

					cPlayerActorMotor.m_bStates.Set((byte)cPlayerActorMotor.m_uiMovementStates);
				}
				break;

			default:
				Debug.LogError(string.Format("Unknown network action ({0})", (byte)eNetworkAction));
				break;
			}
		}
	}

	
	void Start()
	{
		m_CachedCapsuleCollider = GetComponent<CapsuleCollider>();

		if (!CNetwork.IsServer)
		{
			rigidbody.isKinematic = true;
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}
	}
	
	
	void Update()
	{
		CPlayerAirMotor airMotor = GetComponent<CPlayerAirMotor>();
		if(!airMotor.IsActive)
		{
			// Process grounded check on server and client
			UpdateGrounded();
			
			// Process input only for client owned actors
			if (CGamePlayers.SelfActor == gameObject)
			{
				UpdateRotation();
				UpdateInput();				
			}
		}
	}
	
	void FixedUpdate()
	{
        if (CNetwork.IsServer)
        {
			CPlayerAirMotor airMotor = GetComponent<CPlayerAirMotor>();
			if (!airMotor.IsActive)
		    {
                ProcessMovement();

                m_fRotationY.Set(transform.eulerAngles.y);
		    }
        }
	}


	void UpdateGrounded()
	{
		Vector3 p1 = transform.position + m_CachedCapsuleCollider.center + (Vector3.up * ((m_CachedCapsuleCollider.height * 0.5f) + 0.5f));
		Vector3 p2 = p1 - (Vector3.up * m_CachedCapsuleCollider.height);
		RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, m_CachedCapsuleCollider.radius * 0.5f, -transform.up, 0.35f);

		m_bGrounded = false;
		foreach(RaycastHit hit in hits) 
		{
			if(!hit.collider.isTrigger && hit.collider != m_CachedCapsuleCollider) 
			{
				m_bGrounded = true;
				break;
			}
		}
	}


	void UpdateRotation()
	{
		if (!InputDisabled)
		{
			transform.Rotate(0.0f, CUserInput.MouseMovementX, 0.0f);
		}
	}


	void UpdateInput()
	{
		m_uiMovementStates  = 0;
		m_uiMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveGround_Forward)	? (uint)EState.MoveForward	: (uint)0;
		m_uiMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveGround_Backwards)	? (uint)EState.MoveBackward	: (uint)0;
		m_uiMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveGround_StrafeLeft)		? (uint)EState.MoveLeft		: (uint)0;
		m_uiMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveGround_StrafeRight)		? (uint)EState.MoveRight	: (uint)0;
		m_uiMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveGround_Jump)			? (uint)EState.Jump			: (uint)0;
		m_uiMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.Move_Turbo)			? (uint)EState.Sprint		: (uint)0;
		m_uiMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveGround_Crouch)         ? (uint)EState.Crouch       : (uint)0;


        s_cSerializeStream.Write((byte)ENetworkAction.UpdateStates);
        s_cSerializeStream.Write((byte)m_uiMovementStates);
        s_cSerializeStream.Write(transform.eulerAngles.y);
	}


	void ProcessMovement()
	{
        if (!InputDisabled)
        {
            // Direction movement
            Vector3 vMovementVelocity = new Vector3();
            vMovementVelocity += ((m_uiMovementStates & (uint)EState.MoveForward) > 0) ? transform.forward : Vector3.zero;
            vMovementVelocity -= ((m_uiMovementStates & (uint)EState.MoveBackward) > 0) ? transform.forward : Vector3.zero;
            vMovementVelocity -= ((m_uiMovementStates & (uint)EState.MoveLeft) > 0) ? transform.right : Vector3.zero;
            vMovementVelocity += ((m_uiMovementStates & (uint)EState.MoveRight) > 0) ? transform.right : Vector3.zero;

            // Apply direction movement speed
            vMovementVelocity = vMovementVelocity.normalized;
            vMovementVelocity *= ((m_uiMovementStates & (uint)EState.Sprint) > 0) ? SprintSpeed : MovementSpeed;

            // Jump 
            if ((m_uiMovementStates & (uint)EState.Jump) > 0 && IsGrounded)
            {
                vMovementVelocity.y = JumpSpeed;
            }

            // Apply movement velocity
            if (!rigidbody.isKinematic)
            {
                rigidbody.velocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
                rigidbody.AddForce(vMovementVelocity, ForceMode.VelocityChange);
            }
        }
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_bStates)
		{
			// Notify event observers
			if (EventStatesChange != null) EventStatesChange(PreviousStates, States);
		}
        else if (_cSyncedVar == m_fRotationY)
        {
            if (gameObject != CGamePlayers.SelfActor)
            {
                transform.eulerAngles = new Vector3(0.0f,
                                                    m_fRotationY.Get(),
				                                    0.0f);
            }
        }
	}


// Member Fields


	List<Type> m_cInputDisableQueue = new List<Type>();


    CNetworkVar<float> m_fRotationY = null;
	CNetworkVar<float> m_fGravity = null;
	CNetworkVar<float> m_fMovementSpeed = null;
	CNetworkVar<float> m_fSprintSpeed = null;
	CNetworkVar<float> m_fJumpSpeed = null;
	CNetworkVar<byte>  m_bStates = null;


	uint m_uiMovementStates = 0;
	

	CapsuleCollider m_CachedCapsuleCollider = null;
	bool m_bGrounded = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
