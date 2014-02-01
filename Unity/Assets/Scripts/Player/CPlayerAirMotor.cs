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


	public enum EInputState : ushort
	{
		FlyForward	= 1 << 0,
		FlyBackward	= 1 << 1,
		FlyUp		= 1 << 2,
		Down		= 1 << 3,
		StrafeLeft	= 1 << 4,
		StrafeRight	= 1 << 5,
		RollLeft	= 1 << 6,
		RollRight	= 1 << 7,
		Turbo		= 1 << 8,
	}


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_vRotation = _cRegistrar.CreateNetworkVar<Vector3>(OnNetworkVarSync, new Vector3());
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
					cAirMotor.m_usMovementStates = (ushort)_cStream.ReadUShort();

					if (cPlayerActor != CGamePlayers.SelfActor)
					{
						cAirMotor.transform.eulerAngles = new Vector3(_cStream.ReadFloat(),
						                                              _cStream.ReadFloat(),
						                                              _cStream.ReadFloat());
					}
					else
					{
						_cStream.ReadFloat();_cStream.ReadFloat();_cStream.ReadFloat();
					}
					break;

				default:
					Debug.LogError(string.Format("Unknown network action ({0})", (int)eNetworkAction));
					break;
				}
			}
		}
	}


	void Start()
	{
		if (gameObject == CGamePlayers.SelfActor)
		{
			gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone += OnEventEnterShip;
			gameObject.GetComponent<CActorGravity>().EventExitedGravityZone += OnEventLeaveShip;
		}
	}


	void OnDestroy()
	{
		gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone -= OnEventEnterShip;
		gameObject.GetComponent<CActorGravity>().EventExitedGravityZone -= OnEventLeaveShip;
	}


	void Update()
	{
		if(m_IsActive)
		{
			if (CGamePlayers.SelfActor == gameObject)
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

				transform.rotation = Quaternion.Lerp(m_RealignFromRotation, m_RealignToRotation, m_RealignTimer/m_RealignTime);

				m_vRotation.Set(transform.eulerAngles);
			}
		}
	}


	public void FixedUpdate()
	{
        if (CNetwork.IsServer)
        {
			if(m_IsActive)
			{
				UpdateMovement();

                m_vRotation.Set(transform.eulerAngles);
            }
        }
	}


	void UpdateInput()
	{
		m_usMovementStates  = 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveForward)	? (ushort)EInputState.FlyForward		: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveBackwards)	? (ushort)EInputState.FlyBackward	: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveLeft)		? (ushort)EInputState.StrafeLeft	: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveRight)		? (ushort)EInputState.StrafeRight	: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyUp)			? (ushort)EInputState.FlyUp			: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyDown)        ? (ushort)EInputState.Down        : (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyRollLeft)    ? (ushort)EInputState.RollLeft    : (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyRollRight)   ? (ushort)EInputState.RollRight   : (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.Sprint)  		? (ushort)EInputState.Turbo     	: (ushort)0;

		transform.Rotate(new Vector3(0.0f, CUserInput.MouseMovementX, 0.0f));
		transform.Rotate(new Vector3(CUserInput.MouseMovementY, 0.0f, 0.0f));


        s_cSerializeStream.Write((byte)ENetworkAction.UpdateStates);
        s_cSerializeStream.Write((ushort)m_usMovementStates);
        s_cSerializeStream.Write(transform.eulerAngles.x);
        s_cSerializeStream.Write(transform.eulerAngles.y);
        s_cSerializeStream.Write(transform.eulerAngles.z);
	}


	void UpdateMovement()
	{
		// Direction movement
		Vector3 deltaMovementAcceleration = new Vector3();
		deltaMovementAcceleration.z += ((m_usMovementStates & (uint)EInputState.FlyForward)  	 > 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.z -= ((m_usMovementStates & (uint)EInputState.FlyBackward) 	 > 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.x -= ((m_usMovementStates & (uint)EInputState.StrafeLeft)  > 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.x += ((m_usMovementStates & (uint)EInputState.StrafeRight) > 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.y += ((m_usMovementStates & (uint)EInputState.FlyUp) 		 > 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.y -= ((m_usMovementStates & (uint)EInputState.Down) 		 > 0) ? 1.0f : 0.0f;

		// Apply direction movement acceleration
		deltaMovementAcceleration  = deltaMovementAcceleration.normalized;
		deltaMovementAcceleration *= ((m_usMovementStates & (uint)EInputState.Turbo) > 0) ? m_fTurboAcceleration : m_fMovementAcceleration;

		// Rotation
		Vector3 deltaRotationAcceleration = new Vector3();
		deltaRotationAcceleration.z -= ((m_usMovementStates & (uint)EInputState.RollLeft)  > 0) ? m_fRollAcceleration : 0;
		deltaRotationAcceleration.z += ((m_usMovementStates & (uint)EInputState.RollRight) > 0) ? m_fRollAcceleration : 0;

		if (!rigidbody.isKinematic)
		{
			rigidbody.AddRelativeForce(deltaMovementAcceleration, ForceMode.Acceleration);
			rigidbody.AddRelativeTorque(deltaRotationAcceleration, ForceMode.Acceleration);
		}
	}


	void OnEventEnterShip()
	{
		if(!m_IsActive)
			return;

		m_IsActive = false;

		if(CNetwork.IsServer)
		{
			// Constrain the rotation axis again
			rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
			rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
			rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ; 

			// Reset drag values
			rigidbody.drag = 0.1f;
			rigidbody.angularDrag = 0.1f;

			// Start realigning to the ship
			m_RealignWithShip = true;
			m_RealignFromRotation = transform.rotation;
			m_RealignToRotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
			m_RealignTimer = 0.0f;
			m_RealignTime = Mathf.Clamp(Quaternion.Angle(m_RealignFromRotation, m_RealignToRotation) / 180.0f, 0.5f, 2.0f);
		}
	}


	void OnEventLeaveShip()
	{
		if(m_IsActive)
			return;

		m_IsActive = true;

		if(CNetwork.IsServer)
		{
			// Release the rotation axis constraints
			rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationX; 
			rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationY; 
			rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ; 

			// Set drag values high
			rigidbody.drag = 0.5f;
			rigidbody.angularDrag = 1.0f;

			// Stop any realignment
			m_RealignWithShip = false;
		}
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_vRotation)
        {
            if (gameObject != CGamePlayers.SelfActor)
            {
                transform.eulerAngles = m_vRotation.Get();
            }
        }
    }


// Member Fields


    CNetworkVar<Vector3> m_vRotation = null;


	bool m_IsActive = false;

	
	float m_fTurboAcceleration = 50.0f;
	float m_fMovementAcceleration = 10.0f;
	float m_fRollAcceleration = 2.0f;


	float m_fMouseMovementX = 0;
	float m_fMouseMovementY = 0;
	ushort m_usMovementStates = 0;


	bool m_RealignWithShip = false;
	float m_RealignTimer = 0.0f;
	float m_RealignTime = 0.5f;
	Quaternion m_RealignFromRotation = Quaternion.identity;
	Quaternion m_RealignToRotation = Quaternion.identity;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
