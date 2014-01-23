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


    public override void InstanceNetworkVars()
    {
        m_vRotation = new CNetworkVar<Vector3>(OnNetworkVarSync, new Vector3());
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
			gameObject.GetComponent<CPlayerLocator>().EventEnterShip += OnEventEnterShip;
		}
	}


	void OnDestroy()
	{
		gameObject.GetComponent<CPlayerLocator>().EventEnterShip -= OnEventEnterShip;
	}


	void Update()
	{
		CPlayerLocator cSelfLocator = gameObject.GetComponent<CPlayerLocator>();
		
		if (cSelfLocator.ContainingFacility == null ||
		    cSelfLocator.ContainingFacility.GetComponent<CFacilityGravity>().IsGravityEnabled == false)
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
        if (CNetwork.IsServer)
        {
            CPlayerLocator cSelfLocator = gameObject.GetComponent<CPlayerLocator>();

            if (cSelfLocator.ContainingFacility == null ||
                cSelfLocator.ContainingFacility.GetComponent<CFacilityGravity>().IsGravityEnabled == false)
            {
                UpdateMovement();

                m_vRotation.Set(transform.eulerAngles);
            }
        }
	}


	void UpdateInput()
	{
		m_usMovementStates  = 0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveForward)	? (ushort)EState.FlyForward		: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveBackwards)	? (ushort)EState.FlyBackward	: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveLeft)		? (ushort)EState.StrafeLeft	: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.MoveRight)		? (ushort)EState.StrafeRight	: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyUp)			? (ushort)EState.FlyUp			: (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyDown)        ? (ushort)EState.Down        : (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyRollLeft)    ? (ushort)EState.RollLeft    : (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.FlyRollRight)   ? (ushort)EState.RollRight   : (ushort)0;
		m_usMovementStates |= CUserInput.IsInputDown(CUserInput.EInput.Sprint)  		? (ushort)EState.Turbo     	: (ushort)0;

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
        gameObject.transform.eulerAngles = new Vector3(0.0f, gameObject.transform.eulerAngles.y, 0.0f);
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


	float m_fMovementSpeed = 20.5f;
	float m_fTurboSpeed = 50.0f;
	float m_fRotateSpeed = 50.0f;


	float m_fMouseMovementX = 0;
	float m_fMouseMovementY = 0;
	ushort m_usMovementStates = 0;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
