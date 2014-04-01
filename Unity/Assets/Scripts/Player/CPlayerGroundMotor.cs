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


    [ABitSize(4)]
	public enum ENetworkAction : byte
	{
        INVALID,

        MAX
	}


	public enum EState
	{
        INVALID         = -1,

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
		m_bStates    = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, 0);
	}


	[ALocalOnly]
	public void DisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Add(_cFreezeRequester.GetType());
        gameObject.GetComponent<CThirdPersonAnimController>().DisableAnimation();
	}


	[ALocalOnly]
	public void ReenableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Remove(_cFreezeRequester.GetType());
        gameObject.GetComponent<CThirdPersonAnimController>().EnableAnimation();
	}


    [ALocalOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
	}


    [AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);

		if (cPlayerActor != null)
		{
            while (_cStream.HasUnreadData)
            {
                // Retrieve player actor motor  
                CPlayerGroundMotor cPlayerActorMotor = cPlayerActor.GetComponent<CPlayerGroundMotor>();

                ENetworkAction eNetworkAction = (ENetworkAction)_cStream.Read<byte>();

                switch (eNetworkAction)
                {
                    default:
                        Debug.LogError(string.Format("Unknown network action ({0})", (byte)eNetworkAction));
                        break;
                }
            }
		}
	}

	
	void Start()
	{
		m_CachedCapsuleCollider = GetComponent<CapsuleCollider>();

        if (CNetwork.IsServer)
        {
            // Subscribe to client axis events
            CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisChange);
            CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);

            // Subscribe to client input events
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Forward,       OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Backwards,     OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_StrafeLeft,    OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_StrafeRight,   OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveGround_Crouch,  OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveGround_Jump,    OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Turbo,         OnEventClientInputChange);
        }

		if (!CNetwork.IsServer)
		{
			rigidbody.isKinematic = true;
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}
	}


    void OnDestory()
    {
        if (CNetwork.IsServer)
        {
            // Unsubscribe to client axis events
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisChange);
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);

            // Unsubscribe from client input events
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Forward,     OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Backwards,   OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_StrafeLeft,  OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_StrafeRight, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveGround_Crouch,      OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveGround_Jump,        OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Turbo,             OnEventClientInputChange);
        }
    }
	
	
	void Update()
	{
		CPlayerAirMotor cAirMotor = GetComponent<CPlayerAirMotor>();

		if(!cAirMotor.IsActive)
		{
			UpdateGrounded();

            if (CNetwork.IsServer)
            {
                m_fRotationY.Set(transform.eulerAngles.y);
            }
		}
	}
	

	void FixedUpdate()
	{
        if (CNetwork.IsServer)
        {
			CPlayerAirMotor cAirMotor = GetComponent<CPlayerAirMotor>();

			if (!cAirMotor.IsActive)
		    {
                UpdateRotation();
                UpdateMovement();
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


    [AServerOnly]
	void UpdateRotation()
	{
		if (!InputDisabled)
		{
            if (m_fDeltaMouseX != 0.0f)
            {
                transform.Rotate(0.0f, m_fDeltaMouseX, 0.0f);

                m_fDeltaMouseX = 0.0f;
            }
		}
	}


    [AServerOnly]
	void UpdateMovement()
	{
        if (!InputDisabled)
        {
            // Direction movement
            Vector3 vMovementVelocity = new Vector3();
            vMovementVelocity += ((m_uiMovementStates & (uint)EState.MoveForward)  > 0) ? transform.forward : Vector3.zero;
            vMovementVelocity -= ((m_uiMovementStates & (uint)EState.MoveBackward) > 0) ? transform.forward : Vector3.zero;
            vMovementVelocity -= ((m_uiMovementStates & (uint)EState.MoveLeft)     > 0) ? transform.right   : Vector3.zero;
            vMovementVelocity += ((m_uiMovementStates & (uint)EState.MoveRight)    > 0) ? transform.right   : Vector3.zero;

            // Apply direction movement speed
            vMovementVelocity = vMovementVelocity.normalized;
            vMovementVelocity *= ((m_uiMovementStates & (uint)EState.Sprint) > 0) ? k_fSprintSpeed : k_fMoveSpeed;

            // Jump 
            if ((m_uiMovementStates & (uint)EState.Jump) > 0 && IsGrounded)
            {
                vMovementVelocity.y = k_fJumpSpeed;
            }

            // Apply movement velocity
            if (!rigidbody.isKinematic)
            {
                rigidbody.velocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
                rigidbody.AddForce(vMovementVelocity, ForceMode.VelocityChange);
            }
        }
	}


    [AServerOnly]
    void OnEventClientAxisChange(CUserInput.EAxis _eAxis, ulong _ulPlayerId, float _fValue)
    {
        // Check player is the owner of this actor
        if (_ulPlayerId == GetComponent<CPlayerInterface>().PlayerId)
        {
            switch (_eAxis)
            {
                case CUserInput.EAxis.MouseX:
                    m_fDeltaMouseX += _fValue;
                    break;

                case CUserInput.EAxis.MouseY:
                    m_fDeltaMouseY += _fValue;
                    break;

                default:
                    Debug.LogError("Unknown mouse axis: " + _eAxis);
                    break;
            }
        }
    }


    [AServerOnly]
    void OnEventClientInputChange(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        // Check player is the owner of this actor
        if (_ulPlayerId == GetComponent<CPlayerInterface>().PlayerId)
        {
            EState eTargetState = EState.INVALID;

            // Match the input towards a movement state
            switch (_eInput)
            {
                case CUserInput.EInput.Move_Forward:
                    eTargetState = EState.MoveForward;
                    break;

                case CUserInput.EInput.Move_Backwards:
                    eTargetState = EState.MoveBackward;
                    break;

                case CUserInput.EInput.Move_StrafeLeft:
                    eTargetState = EState.MoveLeft;
                    break;

                case CUserInput.EInput.Move_StrafeRight:
                    eTargetState = EState.MoveRight;
                    break;

                case CUserInput.EInput.MoveGround_Crouch:
                    eTargetState = EState.Crouch;
                    break;

                case CUserInput.EInput.MoveGround_Jump:
                    eTargetState = EState.Jump;
                    break;

                case CUserInput.EInput.Move_Turbo:
                    eTargetState = EState.Sprint;
                    break;

                default:
                    Debug.LogError(string.Format("Unknown client input cange. Input({0})", _eInput));
                    break;
            }

            if (eTargetState != EState.INVALID)
            {
                // Update state
                if (_bDown)
                {
                    m_uiMovementStates |= (uint)eTargetState;
                }
                else
                {
                    m_uiMovementStates &= ~(uint)eTargetState;
                }
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
                transform.eulerAngles = new Vector3(0.0f, m_fRotationY.Get(),  0.0f);
            }
        }
	}


// Member Fields


    const float k_fJumpSpeed   = 2.0f;
    const float k_fMoveSpeed   = 6.5f;
    const float k_fSprintSpeed = 8.0f;


	List<Type> m_cInputDisableQueue = new List<Type>();


    CNetworkVar<float> m_fRotationY = null;
	CNetworkVar<byte>  m_bStates = null;


    float m_fDeltaMouseX = 0.0f;
    float m_fDeltaMouseY = 0.0f;


	uint m_uiMovementStates = 0;
	

	bool m_bGrounded = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();




    CapsuleCollider m_CachedCapsuleCollider = null;


};
