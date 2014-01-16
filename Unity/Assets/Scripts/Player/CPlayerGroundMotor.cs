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


	public override void InstanceNetworkVars()
	{
		m_fGravity = new CNetworkVar<float>(OnNetworkVarSync, -9.81f);
		m_fMovementSpeed = new CNetworkVar<float>(OnNetworkVarSync, 6.5f);
		m_fSprintSpeed = new CNetworkVar<float>(OnNetworkVarSync, 8.0f);
		m_fJumpSpeed = new CNetworkVar<float>(OnNetworkVarSync, 2.0f);
		m_bStates = new CNetworkVar<byte>(OnNetworkVarSync, 0);
	}


	public void DisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Add(_cFreezeRequester.GetType());

		gameObject.rigidbody.isKinematic = true;
	}


	public void UndisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Remove(_cFreezeRequester.GetType());

		if (!InputDisabled)
		{
			gameObject.rigidbody.isKinematic = false;
		}
	}


	public static void SerializePlayerState(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGame.PlayerActor;

		if (cSelfActor != null)
		{
			// Retrieve my actor motor
			CPlayerGroundMotor cSelfActorMotor = cSelfActor.GetComponent<CPlayerGroundMotor>();

			// Write movement and rotation states
			_cStream.Write((byte)ENetworkAction.UpdateStates);
			_cStream.Write((byte)cSelfActorMotor.m_uiMovementStates);
			_cStream.Write(cSelfActorMotor.transform.eulerAngles.y);
		}
	}


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId);

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

					if (cPlayerActor != CGame.PlayerActor)
					{
						cPlayerActor.transform.eulerAngles = new Vector3(cPlayerActor.transform.eulerAngles.x, 
						                                                 _cStream.ReadFloat(),
						                                                 cPlayerActor.transform.eulerAngles.z);
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
		if (!CNetwork.IsServer)
		{
			rigidbody.isKinematic = true;
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}

		gameObject.GetComponent<Animator>().enabled = false;
	}
	
	
	void Update()
	{
		CPlayerLocator cSelfLocator = gameObject.GetComponent<CPlayerLocator>();
		
		if (cSelfLocator.Facility != null &&
		    cSelfLocator.Facility.GetComponent<CFacilityGravity>().IsGravityEnabled)
		{
			// Process grounded check on server and client
			UpdateGrounded();
			
			// Process input only for client owned actors
			if (CGame.PlayerActor != null &&
			    CGame.PlayerActor == gameObject)
			{
				UpdateRotation();
				UpdateInput();				
			}
		}
	}
	
	void FixedUpdate()
	{
		CPlayerLocator cSelfLocator = gameObject.GetComponent<CPlayerLocator>();
		
		if (cSelfLocator.Facility != null &&
		    cSelfLocator.Facility.GetComponent<CFacilityGravity>().IsGravityEnabled)
		{
			// Needs to be in fixed update!
			
			// Process movement on server and client
			if (CNetwork.IsServer)
			{
				ProcessMovement();				
			}
		}
	}


	void UpdateGrounded()
	{
		m_bGrounded = Physics.Raycast(transform.position + new Vector3(0.0f, 1.0f, 0.0f), -Vector3.up, 1.1f);
	}


	void UpdateRotation()
	{
		if (!InputDisabled)
		{
			transform.Rotate(0.0f, CGame.UserInput.MouseMovementX, 0.0f);
		}
	}


	void UpdateInput()
	{
		m_uiMovementStates  = 0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveForward)	? (uint)EState.MoveForward	: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveBackwards)	? (uint)EState.MoveBackward	: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveLeft)		? (uint)EState.MoveLeft		: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveRight)		? (uint)EState.MoveRight	: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.Jump)			? (uint)EState.Jump			: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.Sprint)			? (uint)EState.Sprint		: (uint)0;
		m_uiMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.Crouch)         ? (uint)EState.Crouch       : (uint)0;	
	}


	void ProcessMovement()
	{
		// Direction movement
		Vector3 vMovementVelocity = new Vector3();
		vMovementVelocity += ((m_uiMovementStates & (uint)EState.MoveForward)  > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_uiMovementStates & (uint)EState.MoveBackward) > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_uiMovementStates & (uint)EState.MoveLeft)     > 0) ? transform.right   : Vector3.zero;
		vMovementVelocity += ((m_uiMovementStates & (uint)EState.MoveRight)    > 0) ? transform.right   : Vector3.zero;

		// Apply direction movement speed
		vMovementVelocity  = vMovementVelocity.normalized;
		vMovementVelocity *= ((m_uiMovementStates & (uint)EState.Sprint) > 0) ? SprintSpeed : MovementSpeed;

		// Jump 
		if ((m_uiMovementStates & (uint)EState.Jump) > 0 &&
		    IsGrounded)
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


	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if (_cSyncedNetworkVar == m_bStates)
		{
			// Notify event observers
			if (EventStatesChange != null) EventStatesChange(PreviousStates, States);
		}
	}


// Member Fields


	List<Type> m_cInputDisableQueue = new List<Type>();


	CNetworkVar<float> m_fGravity = null;
	CNetworkVar<float> m_fMovementSpeed = null;
	CNetworkVar<float> m_fSprintSpeed = null;
	CNetworkVar<float> m_fJumpSpeed = null;
	CNetworkVar<byte>  m_bStates = null;


	float m_fRotationY = 0.0f;


	uint m_uiMovementStates = 0;
	
	
	bool m_bGrounded = false;


};
