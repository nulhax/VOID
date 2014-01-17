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
public class CPlayerAirMotor : MonoBehaviour
{

// Member Types


	public enum ENetworkAction : byte
	{
		UpdateStates
	}


	public enum EState : ushort
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


	void Start()
	{
		if (gameObject == CGamePlayers.SelfActor)
		{
			gameObject.GetComponent<CPlayerLocator>().EventEnterShip += new CPlayerLocator.NotifyEnterShip(OnEventEnterShip);
		}
	}


	[AClientOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGamePlayers.SelfActor;

		if (cSelfActor != null)
		{
			uint uiMovementStates = cSelfActor.GetComponent<CPlayerAirMotor>().m_usMovementStates;
			float fMouseMovementX = cSelfActor.GetComponent<CPlayerAirMotor>().m_fMouseMovementX;
			float fMouseMovementY = cSelfActor.GetComponent<CPlayerAirMotor>().m_fMouseMovementY;

			_cStream.Write((byte)ENetworkAction.UpdateStates);
			_cStream.Write((ushort)uiMovementStates);
			_cStream.Write(cSelfActor.transform.eulerAngles.x);
			_cStream.Write(cSelfActor.transform.eulerAngles.y);
			_cStream.Write(cSelfActor.transform.eulerAngles.z);
		}
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


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		CPlayerLocator cSelfLocator = gameObject.GetComponent<CPlayerLocator>();
		
		if (cSelfLocator.Facility == null ||
		    cSelfLocator.Facility.GetComponent<CFacilityGravity>().IsGravityEnabled == false)
		{
			if (CGamePlayers.SelfActor != null &&
			    CGamePlayers.SelfActor == gameObject)
			{
				UpdateInput();
			}
		}
	}


	public void FixedUpdate()
	{
		CPlayerLocator cSelfLocator = gameObject.GetComponent<CPlayerLocator>();
		
		if (cSelfLocator.Facility == null ||
		    cSelfLocator.Facility.GetComponent<CFacilityGravity>().IsGravityEnabled == false)
		{
			if (CNetwork.IsServer)
			{
				UpdateMovement();
			}
		}
	}


	void UpdateInput()
	{
		m_usMovementStates  = 0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveForward)	? (ushort)EState.FlyForward		: (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveBackwards)	? (ushort)EState.FlyBackward	: (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveLeft)		? (ushort)EState.StrafeLeft	: (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.MoveRight)		? (ushort)EState.StrafeRight	: (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.FlyUp)			? (ushort)EState.FlyUp			: (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.FlyDown)        ? (ushort)EState.Down        : (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.FlyRollLeft)    ? (ushort)EState.RollLeft    : (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.FlyRollRight)   ? (ushort)EState.RollRight   : (ushort)0;
		m_usMovementStates |= CGame.UserInput.IsInputDown(CUserInput.EInput.Sprint)  		? (ushort)EState.Turbo     	: (ushort)0;

		transform.Rotate(new Vector3(0.0f, CGame.UserInput.MouseMovementX, 0.0f));
		transform.Rotate(new Vector3(CGame.UserInput.MouseMovementY, 0.0f, 0.0f));
	}


	void UpdateMovement()
	{
		// Direction movement
		Vector3 vMovementVelocity = new Vector3();
		vMovementVelocity += ((m_usMovementStates & (uint)EState.FlyForward)  	 > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_usMovementStates & (uint)EState.FlyBackward) 	 > 0) ? transform.forward : Vector3.zero;
		vMovementVelocity -= ((m_usMovementStates & (uint)EState.StrafeLeft)  > 0) ? transform.right   : Vector3.zero;
		vMovementVelocity += ((m_usMovementStates & (uint)EState.StrafeRight) > 0) ? transform.right   : Vector3.zero;
		vMovementVelocity += ((m_usMovementStates & (uint)EState.FlyUp) 		 > 0) ? transform.up   	  : Vector3.zero;
		vMovementVelocity -= ((m_usMovementStates & (uint)EState.Down) 		 > 0) ? transform.up   	  : Vector3.zero;

		// Apply direction movement speed
		vMovementVelocity  = vMovementVelocity.normalized;
		vMovementVelocity *= ((m_usMovementStates & (uint)EState.Turbo) > 0) ? m_fTurboSpeed : m_fMovementSpeed;


		Vector3 vRorationVelocity = new Vector3();
		vRorationVelocity.z -= ((m_usMovementStates & (uint)EState.RollLeft)  > 0) ? m_fRotateSpeed : 0;
		vRorationVelocity.z += ((m_usMovementStates & (uint)EState.RollRight) > 0) ? m_fRotateSpeed : 0;

		
		// Apply movement velocity
		if (!rigidbody.isKinematic)
		{
			//Debug.LogError(vMovementVelocity);
			rigidbody.velocity = vMovementVelocity;
			rigidbody.transform.Rotate(vRorationVelocity * Time.deltaTime);
		}
	}


	void OnEventEnterShip()
	{
		gameObject.transform.rotation = Quaternion.identity;
	}


// Member Fields


	float m_fMovementSpeed = 20.5f;
	float m_fTurboSpeed = 50.0f;
	float m_fRotateSpeed = 50.0f;


	float m_fMouseMovementX = 0;
	float m_fMouseMovementY = 0;
	ushort m_usMovementStates = 0;


};
