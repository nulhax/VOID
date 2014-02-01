//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerAirMotor.cs
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


[RequireComponent(typeof(CPlayerSuit))]
public class CPlayerAirMotor : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction : byte
	{
		UpdateStates
	}


	public enum EInputState : uint
	{
		FlyForward	= 1 << 0,
		FlyBackward	= 1 << 1,
		FlyUp		= 1 << 2,
		Down		= 1 << 3,
		StrafeLeft	= 1 << 4,
		StrafeRight	= 1 << 5,
		RollLeft	= 1 << 6,
		RollRight	= 1 << 7,
		YawLeft		= 1 << 8,
		YawRight	= 1 << 9,
		PitchUp		= 1 << 10,
		PitchDown	= 1 << 11,
		Turbo		= 1 << 12,
	}


// Member Delegates & Events


// Member Properties
	public bool IsActive
	{
		get { return(m_IsActive.Get()); }
	}

// Member Methods
    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_IsActive = _cRegistrar.CreateNetworkVar(OnNetworkVarSync, false);
    }


	[AClientOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
	}
	
	
	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.FindPlayerActor(_cNetworkPlayer.PlayerId);

		if (cPlayerActor != null)
		{
			CPlayerAirMotor cAirMotor = cPlayerActor.GetComponent<CPlayerAirMotor>();

			while (_cStream.HasUnreadData)
			{
				ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();

				switch (eNetworkAction)
				{
				case ENetworkAction.UpdateStates:
					cAirMotor.m_usMovementStates = _cStream.ReadUInt();
					break;

				default:
					Debug.LogError(string.Format("Unknown network action ({0})", (int)eNetworkAction));
					break;
				}
			}
		}
	}

	void OnNetworkVarSync(INetworkVar _SyncedVar)
	{
		if(_SyncedVar == m_IsActive)
		{
			if(m_IsActive.Get())
			{
				if(CGamePlayers.SelfActor == gameObject)
				{
					// Placeholder: Disable movement input
					GetComponent<CPlayerHead>().DisableInput(this);
					GetComponent<CPlayerGroundMotor>().DisableInput(this);
				}
			}
			else
			{
				if(CGamePlayers.SelfActor == gameObject)
				{
					// Placeholder: Enable movement input
					GetComponent<CPlayerHead>().ReenableInput(this);
					GetComponent<CPlayerGroundMotor>().ReenableInput(this);
				}
			}
		}
	}


	void Start()
	{
		// Register the entering/exiting gravity zones
		gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone += OnPlayerEnterGravityZone;
		gameObject.GetComponent<CActorGravity>().EventExitedGravityZone += OnPlayerLeaveGravityZone;
	}


	void OnDestroy()
	{
		// Unregister the entering/exiting gravity zones
		gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone -= OnPlayerEnterGravityZone;
		gameObject.GetComponent<CActorGravity>().EventExitedGravityZone -= OnPlayerLeaveGravityZone;
	}


	void Update()
	{
		if(m_IsActive.Get())
		{
			if(CGamePlayers.SelfActor == gameObject)
			{
				UpdateInput();
			}
		}

		if(CNetwork.IsServer)
		{
			if(m_RealignWithShip)
			{
				m_RealignTimer += Time.deltaTime;
				if(m_RealignTimer > m_RealignTime)
				{
					m_RealignTimer = m_RealignTime;
					m_RealignWithShip = false;
				}

				// Set the lerped rotation
				rigidbody.rotation = Quaternion.Slerp(m_RealignFromRotation, m_RealignToRotation, m_RealignTimer/m_RealignTime);

				if(!m_RealignWithShip)
				{
					// Player is now in gravity zone, set inactive
					m_IsActive.Set(false);

					// Stop syncing the rotations
					GetComponent<CActorNetworkSyncronized>().m_SyncRotation = false;
					GetComponent<CActorNetworkSyncronized>().SyncTransform();

					// Constrain the rotation axis again
					rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
					rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
					rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ; 
					
					// Reset drag values
					rigidbody.drag = 0.1f;
					rigidbody.angularDrag = 0.1f;
				}
			}
		}
	}


	public void FixedUpdate()
	{
        if (CNetwork.IsServer)
        {
			if(m_IsActive.Get() && !m_RealignWithShip)
			{
				UpdateMovement();
            }
        }
	}


	void UpdateInput()
	{
		m_usMovementStates  = 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveForward)		? (uint)EInputState.FlyForward	: 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveBackwards)	? (uint)EInputState.FlyBackward	: 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveLeft)		? (uint)EInputState.StrafeLeft	: 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveRight)		? (uint)EInputState.StrafeRight	: 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyUp)			? (uint)EInputState.FlyUp		: 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyDown)        	? (uint)EInputState.Down        : 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyRollRight)    ? (uint)EInputState.RollLeft    : 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyRollLeft)   	? (uint)EInputState.RollRight   : 0;
		m_usMovementStates |= CUserInput.MouseMovementDeltaX < 0.0f    					? (uint)EInputState.YawLeft    	: 0;
		m_usMovementStates |= CUserInput.MouseMovementDeltaX > 0.0f    					? (uint)EInputState.YawRight   	: 0;
		m_usMovementStates |= CUserInput.MouseMovementDeltaY < 0.0f    					? (uint)EInputState.PitchUp    	: 0;
		m_usMovementStates |= CUserInput.MouseMovementDeltaY > 0.0f    					? (uint)EInputState.PitchDown   : 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.Sprint)  		? (uint)EInputState.Turbo     	: 0;
	
		s_cSerializeStream.Write((byte)ENetworkAction.UpdateStates);
		s_cSerializeStream.Write((uint)m_usMovementStates);
	}


	void UpdateMovement()
	{
		// Movement
		Vector3 deltaMovementAcceleration = new Vector3();
		deltaMovementAcceleration.z += ((m_usMovementStates & (uint)EInputState.FlyForward)  	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.z -= ((m_usMovementStates & (uint)EInputState.FlyBackward) 	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.x -= ((m_usMovementStates & (uint)EInputState.StrafeLeft)  	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.x += ((m_usMovementStates & (uint)EInputState.StrafeRight) 	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.y += ((m_usMovementStates & (uint)EInputState.FlyUp) 		 	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.y -= ((m_usMovementStates & (uint)EInputState.Down) 		 	> 0) ? 1.0f : 0.0f;

		// Apply movement acceleration
		deltaMovementAcceleration  = deltaMovementAcceleration.normalized;
		deltaMovementAcceleration *= ((m_usMovementStates & (uint)EInputState.Turbo) > 0) ? m_fTurboAcceleration : m_fMovementAcceleration;

		// Rotation
		Vector3 deltaRotationAcceleration = new Vector3();
		deltaRotationAcceleration.x -= ((m_usMovementStates & (uint)EInputState.PitchUp)  	> 0) ? 1.0f : 0.0f;
		deltaRotationAcceleration.x += ((m_usMovementStates & (uint)EInputState.PitchDown) 	> 0) ? 1.0f : 0.0f;
		deltaRotationAcceleration.y -= ((m_usMovementStates & (uint)EInputState.YawLeft)  	> 0) ? 1.0f : 0.0f;
		deltaRotationAcceleration.y += ((m_usMovementStates & (uint)EInputState.YawRight) 	> 0) ? 1.0f : 0.0f;
		deltaRotationAcceleration.z -= ((m_usMovementStates & (uint)EInputState.RollLeft)  	> 0) ? 1.0f : 0.0f;
		deltaRotationAcceleration.z += ((m_usMovementStates & (uint)EInputState.RollRight) 	> 0) ? 1.0f : 0.0f;

		// Apply rotation acceleration
		deltaRotationAcceleration.x *= m_fRotationAccelerationX;
		deltaRotationAcceleration.y *= m_fRotationAccelerationY;
		deltaRotationAcceleration.z *= m_fRotationAccelerationZ;

		rigidbody.AddRelativeForce(deltaMovementAcceleration, ForceMode.Acceleration);
		rigidbody.AddRelativeTorque(deltaRotationAcceleration, ForceMode.Acceleration);
	}


	[AServerOnly]
	void OnPlayerEnterGravityZone()
	{
		// Start realigning to the ship
		m_RealignWithShip = true;

		// Set the values to use for realigment
		m_RealignFromRotation = transform.rotation;
		m_RealignToRotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
		m_RealignTimer = 0.0f;
		m_RealignTime = Mathf.Clamp(Quaternion.Angle(m_RealignFromRotation, m_RealignToRotation) / 180.0f, 0.5f, 2.0f);
	}


	[AServerOnly]
	void OnPlayerLeaveGravityZone()
	{
		// Player is now in no-gravity zone, set active
		m_IsActive.Set(true);

		// Stop any realignment
		m_RealignWithShip = false;

		// Release the rotation axis constraints
		rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationX; 
		rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationY; 
		rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ; 
		
		// Set drag values high
		rigidbody.drag = 0.5f;
		rigidbody.angularDrag = 1.0f;

		// Start syncing the rotations
		GetComponent<CActorNetworkSyncronized>().m_SyncRotation = true;
	}


// Member Fields
	

	private CNetworkVar<bool> m_IsActive = null;

	
	private float m_fTurboAcceleration = 50.0f;
	private float m_fMovementAcceleration = 10.0f;

	private float m_fRotationAccelerationX = 1.0f;
	private float m_fRotationAccelerationY = 2.0f;
	private float m_fRotationAccelerationZ = 1.0f;
	
	private uint m_usMovementStates = 0;


	private bool m_RealignWithShip = false;
	private float m_RealignTimer = 0.0f;
	private float m_RealignTime = 0.5f;
	private Quaternion m_RealignFromRotation = Quaternion.identity;
	private Quaternion m_RealignToRotation = Quaternion.identity;


	private static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
