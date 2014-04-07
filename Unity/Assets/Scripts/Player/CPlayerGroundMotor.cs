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


    public enum EMotorType
    {
        INVALID,

        Normal,
        AirThusters,
        MagneticBoots,

        MAX
    }


	public enum EInputState
	{
        INVALID     = -1,

		Forward		= 1 << 0,
		Backward	= 1 << 1,
		StrafeLeft	= 1 << 2,
		StrafeRight = 1 << 3,
		Jump		= 1 << 4,
		Turbo		= 1 << 5,
		Crouch		= 1 << 6,
        FlyUp       = 1 << 7,
        FlyDown     = 1 << 8,
        RollRight   = 1 << 9,
        RollLeft    = 1 << 10,

        MAX
	}


// Member Delegates & Events


    public delegate void InputStatesChangeHandler(ushort _usPreviousStates, ushort _usNewSates);
	public event InputStatesChangeHandler EventInputStatesChange;


// Member Properties


	public bool IsInputDisabled
	{
		get { return (m_cInputDisableQueue.Count > 0); }
	}


	public bool IsGrounded
	{
		get { return (m_bGrounded); }
	}


	public ushort InputStates
	{
		get { return (m_usInputStates.Get()); }
	}


    public ushort PreviousInputStates
	{
		get { return (m_usInputStates.GetPrevious()); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
        m_fPositionX = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fPositionSendInterval);
        m_fPositionY = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fPositionSendInterval);
        m_fPositionZ = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fPositionSendInterval);
        m_fRotationX = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fRotationSendInterval);
        m_fRotationY = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fRotationSendInterval);
        m_fRotationZ = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fRotationSendInterval);
        m_usInputStates = _cRegistrar.CreateReliableNetworkVar<ushort>(OnNetworkVarSync);
	}


	[ALocalOnly]
	public void DisableInput(object _cRequester)
	{
		m_cInputDisableQueue.Add(_cRequester.GetType());
        gameObject.GetComponent<CThirdPersonAnimController>().DisableAnimation();
	}


	[ALocalOnly]
	public void EnableInput(object _cRequester)
	{
		m_cInputDisableQueue.Remove(_cRequester.GetType());
        gameObject.GetComponent<CThirdPersonAnimController>().EnableAnimation();
	}


    [ALocalAndServerOnly]
    public void CorrectHeadOverRotate(float _fY)
    {
        transform.Rotate(new Vector3(0.0f, _fY, 0.0f));
        m_bRealignBody = true;
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
		m_cCachedCapsuleCollider = GetComponent<CapsuleCollider>();

        // Register the entering/exiting gravity zones
        gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone += OnEventPlayerEnterGravityZone;
        gameObject.GetComponent<CActorGravity>().EventExitedGravityZone += OnEventPlayerLeaveGravityZone;

        gameObject.GetComponent<CActorLocator>().EventEnterShip += OnEventEnterShip;
        gameObject.GetComponent<CActorLocator>().EventLeaveShip += OnEventLeaveShip;

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
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Turbo,         OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Crouch,        OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveGround_Jump,    OnEventClientInputChange);

            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_Down,       OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_Up,         OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_RollLeft,   OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_RollRight,  OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Turbo,         OnEventClientInputChange);
        }
        else if (gameObject == CGamePlayers.SelfActor)
        {
            // Subscribe to axis events
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventAxisChange);
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventAxisChange);

            // Subscribe to input events
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Forward,       OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Backwards,     OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_StrafeLeft,    OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_StrafeRight,   OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Turbo,         OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Crouch,        OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveGround_Jump,    OnEventInputChange);

            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_Down,       OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_Up,         OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_RollLeft,   OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_RollRight,  OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Turbo,         OnEventInputChange);
        }

		if (!CNetwork.IsServer &&
            gameObject != CGamePlayers.SelfActor)
		{
			rigidbody.isKinematic = true;
		}
	}


    void OnDestory()
    {
        // Unregister the entering/exiting gravity zones
        gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone -= OnEventPlayerEnterGravityZone;
        gameObject.GetComponent<CActorGravity>().EventExitedGravityZone -= OnEventPlayerLeaveGravityZone;

        gameObject.GetComponent<CActorLocator>().EventEnterShip -= OnEventEnterShip;
        gameObject.GetComponent<CActorLocator>().EventLeaveShip -= OnEventLeaveShip;

        if (CNetwork.IsServer)
        {
            // Unsubscribe to client axis events
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisChange);
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);

            // Unsubscribe from client input events
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Forward,      OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Backwards,    OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_StrafeLeft,   OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_StrafeRight,  OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Crouch, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveGround_Jump,   OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Turbo,        OnEventClientInputChange);

            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_Down,      OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_Up,        OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_RollLeft,  OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_RollRight, OnEventClientInputChange);
        }
        else if (gameObject == CGamePlayers.SelfActor)
        {
            // Subscribe to axis events
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventAxisChange);
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventAxisChange);
                       
            // Subscribe to input events
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Forward,       OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Backwards,     OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_StrafeLeft,    OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_StrafeRight,   OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Turbo,         OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Crouch,        OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveGround_Jump,    OnEventInputChange);
                     
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_Down,       OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_Up,         OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_RollLeft,   OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_RollRight,  OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Turbo,         OnEventInputChange);
        }
    }
	
	
	void Update()
	{
        // Empty
	}
	

	void FixedUpdate()
	{
        UpdateGrounded();

        if (m_bRealignBody)
        {
            RealignBody();
        }

        switch (m_eMotorType)
        {
            case EMotorType.Normal:
                UpdateNormalMovement();
                break;

            case EMotorType.AirThusters:
                //UpdateAirMovement();
                break;

            case EMotorType.MagneticBoots:
                //UpdateMagneticMovement();
                break;

            default:
                Debug.LogError(string.Format("Unknown motor type: " + m_eMotorType));
                break;
        }

        if (CNetwork.IsServer)
        {
            m_fPositionX.Value = transform.position.x;
            m_fPositionY.Value = transform.position.y;
            m_fPositionZ.Value = transform.position.z;

            m_fRotationX.Value = transform.eulerAngles.x;
            m_fRotationY.Value = transform.eulerAngles.y;
            m_fRotationZ.Value = transform.eulerAngles.z;
        }
	}


	void UpdateGrounded()
	{
        if (!CNetwork.IsServer &&
            gameObject != CGamePlayers.SelfActor)
        {
            return;
        }

		Vector3 p1 = transform.position + m_cCachedCapsuleCollider.center + (Vector3.up * ((m_cCachedCapsuleCollider.height * 0.5f) + 0.5f));
		Vector3 p2 = p1 - (Vector3.up * m_cCachedCapsuleCollider.height);
		RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, m_cCachedCapsuleCollider.radius * 0.5f, -transform.up, 0.35f);

		m_bGrounded = false;

		foreach(RaycastHit hit in hits) 
		{
			if(!hit.collider.isTrigger && hit.collider != m_cCachedCapsuleCollider) 
			{
				m_bGrounded = true;
				break;
			}
		}
	}


	void RealignBody()
	{
        if (IsInputDisabled)
            return;

        /*
        // Run on server or locally
        if (CNetwork.IsServer ||
            gameObject == CGamePlayers.SelfActor)
        {
         */
            Quaternion qBodyRotation = rigidbody.transform.rotation;
            Quaternion qHeadRotation = GetComponent<CPlayerHead>().Head.transform.rotation;
            Quaternion qHeadRotationRelative = Quaternion.Euler(0.0f, GetComponent<CPlayerHead>().Head.transform.eulerAngles.y, 0.0f);  // Relative to body (Y Only)

            float fAngle = Quaternion.Angle(qBodyRotation, qHeadRotationRelative);

            rigidbody.transform.rotation = Quaternion.RotateTowards(qBodyRotation, qHeadRotationRelative, 180.0f * Time.fixedDeltaTime);
            GetComponent<CPlayerHead>().Head.transform.rotation = qHeadRotation;

            if (fAngle == 0.0f)
            {
                m_bRealignBody = false;
            }
        /*}
   
       // Lerp to remote rotation
       else
       {
           transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                                         Quaternion.Euler(m_fRotationX.Value, m_fRotationY.Value, m_fRotationZ.Value),
                                                         360.0f * Time.fixedDeltaTime);
       }*/
	}


	void UpdateNormalMovement()
	{
        if (IsInputDisabled)
            return;

        // Run on server or locally
        if (CNetwork.IsServer ||
            gameObject == CGamePlayers.SelfActor)
        {
            Vector3 vHeadRotation = new Vector3(0.0f, GetComponent<CPlayerHead>().Head.transform.localEulerAngles.y, 0.0f);

            // Direction movement
            Vector3 vMovementVelocity = new Vector3();
            vMovementVelocity += ((m_uiInputStates & (uint)EInputState.Forward)     > 0) ? transform.forward : Vector3.zero;
            vMovementVelocity -= ((m_uiInputStates & (uint)EInputState.Backward)    > 0) ? transform.forward : Vector3.zero;
            vMovementVelocity -= ((m_uiInputStates & (uint)EInputState.StrafeLeft)  > 0) ? transform.right   : Vector3.zero;
            vMovementVelocity += ((m_uiInputStates & (uint)EInputState.StrafeRight) > 0) ? transform.right   : Vector3.zero;

            // Apply direction movement speed
            vMovementVelocity = vMovementVelocity.normalized;
            vMovementVelocity *= ((m_uiInputStates & (uint)EInputState.Turbo) > 0) ? k_fSprintSpeed : k_fMoveSpeed;

            // Jump 
            if ((m_uiInputStates & (uint)EInputState.Jump) > 0 && IsGrounded)
            {
                vMovementVelocity.y = k_fJumpSpeed;
            }

            if (!rigidbody.isKinematic)
            {
                // Apply movement velocity
                rigidbody.velocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
                rigidbody.AddForce(Quaternion.Euler(vHeadRotation) * vMovementVelocity, ForceMode.VelocityChange);

                if (vMovementVelocity != Vector3.zero)
                {
                    m_bRealignBody = true;
                }
            }
        }

        // Lerp to remote rotation
        if (!CNetwork.IsServer)
        {
            Vector3 vTargetPosition = new Vector3(m_fPositionX.Value, m_fPositionY.Value, m_fPositionZ.Value);

            if (gameObject != CGamePlayers.SelfActor ||
                Math.Abs((vTargetPosition - transform.position).magnitude) > k_fMoveSpeed * k_fPositionSendInterval * 3)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                                                         new Vector3(m_fPositionX.Value, m_fPositionY.Value, m_fPositionZ.Value),
                                                         k_fMoveSpeed * Time.fixedDeltaTime);
            }
        }
	}


    [AServerOnly]
    void UpdateAirMovement()
    {
        if (IsInputDisabled)
            return;

        // Movement
        Vector3 vDeltaAcceleration = new Vector3();
        vDeltaAcceleration.z += ((m_uiInputStates & (uint)EInputState.Forward)     > 0) ? 1.0f : 0.0f;
        vDeltaAcceleration.z -= ((m_uiInputStates & (uint)EInputState.Backward)    > 0) ? 1.0f : 0.0f;
        vDeltaAcceleration.x -= ((m_uiInputStates & (uint)EInputState.StrafeLeft)  > 0) ? 1.0f : 0.0f;
        vDeltaAcceleration.x += ((m_uiInputStates & (uint)EInputState.StrafeRight) > 0) ? 1.0f : 0.0f;
        vDeltaAcceleration.y += ((m_uiInputStates & (uint)EInputState.FlyUp)       > 0) ? 1.0f : 0.0f;
        vDeltaAcceleration.y -= ((m_uiInputStates & (uint)EInputState.FlyDown)     > 0) ? 1.0f : 0.0f;

        // Apply movement acceleration
        vDeltaAcceleration = vDeltaAcceleration.normalized;
        vDeltaAcceleration *= ((m_uiInputStates & (uint)EInputState.Turbo) > 0) ? k_fTurboAcceleration : k_fMovementAcceleration;
        
        // Rotation
        Vector3 vDeltaRotationAcceleration = new Vector3();
        vDeltaRotationAcceleration.x = m_fMouseDeltaY;
        vDeltaRotationAcceleration.y = m_fMouseDeltaX;
        vDeltaRotationAcceleration.z -= ((m_uiInputStates & (uint)EInputState.RollLeft)  > 0) ? 1.0f : 0.0f;
        vDeltaRotationAcceleration.z += ((m_uiInputStates & (uint)EInputState.RollRight) > 0) ? 1.0f : 0.0f;
        
        // Apply rotation acceleration
        vDeltaRotationAcceleration.x *= k_fRotationAccelerationX;
        vDeltaRotationAcceleration.y *= k_fRotationAccelerationY;
        vDeltaRotationAcceleration.z *= k_fRotationAccelerationZ;
        Debug.LogError(vDeltaRotationAcceleration);
        rigidbody.AddRelativeForce(vDeltaAcceleration, ForceMode.Acceleration);
        rigidbody.AddRelativeTorque(vDeltaRotationAcceleration, ForceMode.Acceleration);

        // Clear mouse deltas
        m_fMouseDeltaX = 0.0f;
        m_fMouseDeltaY = 0.0f;
    }


    [AServerOnly]
    void UpdateMagneticMovement()
    {
        if (IsInputDisabled)
            return;
    }


    void ChangeMotorType(EMotorType _eNewMotorType)
    {
        switch (_eNewMotorType)
        {
            case EMotorType.Normal:
                // Enable the rotation axis constraints
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
                break;

            case EMotorType.AirThusters:
                // Release the rotation axis constraints
                rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationX;
                rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationY;
                rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
                break;

            case EMotorType.MagneticBoots:
                // Enable the rotation axis constraints
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
                break;

            default:
                Debug.LogError(string.Format("Unknown motor type: " + m_eMotorType));
                break;
        }

        m_eMotorType = _eNewMotorType;
    }


    [ALocalOnly]
    void OnEventAxisChange(CUserInput.EAxis _eAxis, float _fValue)
    {
        OnEventClientAxisChange(_eAxis, CNetwork.PlayerId, _fValue);
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
                    m_fMouseDeltaX += _fValue;
                    break;

                case CUserInput.EAxis.MouseY:
                    m_fMouseDeltaY += _fValue;
                    break;

                default:
                    Debug.LogError("Unknown mouse axis: " + _eAxis);
                    break;
            }
        }
    }


    [ALocalOnly]
    void OnEventInputChange(CUserInput.EInput _eInput, bool _bDown)
    {
        OnEventClientInputChange(_eInput, CNetwork.PlayerId, _bDown);
    }


    [AServerOnly]
    void OnEventClientInputChange(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        // Check player is the owner of this actor
        if (_ulPlayerId == GetComponent<CPlayerInterface>().PlayerId)
        {
            EInputState eTargetState = EInputState.INVALID;

            // Match the input towards a movement state
            switch (_eInput)
            {
                case CUserInput.EInput.Move_Forward:
                    eTargetState = EInputState.Forward;
                    break;

                case CUserInput.EInput.Move_Backwards:
                    eTargetState = EInputState.Backward;
                    break;

                case CUserInput.EInput.Move_StrafeLeft:
                    eTargetState = EInputState.StrafeLeft;
                    break;

                case CUserInput.EInput.Move_StrafeRight:
                    eTargetState = EInputState.StrafeRight;
                    break;

                case CUserInput.EInput.Move_Crouch:
                    eTargetState = EInputState.Crouch;
                    break;

                case CUserInput.EInput.MoveGround_Jump:
                    eTargetState = EInputState.Jump;
                    break;

                case CUserInput.EInput.Move_Turbo:
                    eTargetState = EInputState.Turbo;
                    break;

                case CUserInput.EInput.MoveFly_Down:
                    eTargetState = EInputState.FlyDown;
                    break;

                case CUserInput.EInput.MoveFly_Up:
                    eTargetState = EInputState.FlyUp;
                    break;

                case CUserInput.EInput.MoveFly_RollLeft:
                    eTargetState = EInputState.RollLeft;
                    break;

                case CUserInput.EInput.MoveFly_RollRight:
                    eTargetState = EInputState.RollRight;
                    break;

                default:
                    Debug.LogError(string.Format("Unknown client input cange. Input({0})", _eInput));
                    break;
            }

            if (eTargetState != EInputState.INVALID)
            {
                // Update state
                if (_bDown)
                {
                    m_uiInputStates |= (uint)eTargetState;
                }
                else
                {
                    m_uiInputStates &= ~(uint)eTargetState;
                }

				m_usInputStates.Value = (ushort)m_uiInputStates;

                if (gameObject == CGamePlayers.SelfActor)
                {
                    // Notify event observers
                    if (EventInputStatesChange != null) EventInputStatesChange(PreviousInputStates, InputStates);
                }
            }
        }
    }


    [AServerOnly]
    void OnEventPlayerEnterGravityZone()
    {
        return;

        // Start realigning body to the ship
        m_RealignBodyWithShip = true;

        // Set the values to use for realigment
        m_RealignFromRotation = transform.rotation;
        m_RealignToRotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
        m_RealignBodyTimer = 0.0f;
        m_RealignBodyTime = Mathf.Clamp(Quaternion.Angle(m_RealignFromRotation, m_RealignToRotation) / 180.0f, 0.5f, 2.0f);
    }


    [AServerOnly]
    void OnEventPlayerLeaveGravityZone()
    {
        return;

        // Stop any realignment with ship
        m_RealignBodyWithShip = false;

        // Set drag values high
        rigidbody.drag = 0.5f;
        rigidbody.angularDrag = 1.0f;

        // Start syncing the rotations
        GetComponent<CActorNetworkSyncronized>().m_SyncRotation = true;
    }


    void OnEventEnterShip(GameObject _cActor)
    {
        if (_cActor != gameObject)
            Debug.LogError("Unknown actor in event enter ship.");

        ChangeMotorType(EMotorType.Normal);
    }


    void OnEventLeaveShip(GameObject _cActor)
    {
        if (_cActor != gameObject)
            Debug.LogError("Unknown actor in event leave ship.");

        ChangeMotorType(EMotorType.AirThusters);
    }


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
        if (!CNetwork.IsServer &&
            gameObject != CGamePlayers.SelfActor)
        {
            if (_cSyncedVar == m_usInputStates)
            {
                // Notify event observers
                if (EventInputStatesChange != null) EventInputStatesChange(PreviousInputStates, InputStates);
            }
            else if (_cSyncedVar == m_fPositionX ||
                     _cSyncedVar == m_fPositionY ||
                     _cSyncedVar == m_fPositionZ)
            {
                m_bRealignBody = true;
            }
        }
	}


// Member Fields


    const float k_fPositionSendInterval = 0.1f;
    const float k_fRotationSendInterval = 0.1f;


    const float k_fMovementAcceleration = 10.0f;
    const float k_fRotationAccelerationX = 0.5f;
    const float k_fRotationAccelerationY = 0.5f;
    const float k_fRotationAccelerationZ = 1.0f;
    const float k_fTurboAcceleration     = 50.0f;

    const float k_fJumpSpeed   = 2.0f;
    const float k_fMoveSpeed   = 6.5f;
    const float k_fSprintSpeed = 8.0f;


	List<Type> m_cInputDisableQueue = new List<Type>();


    CNetworkVar<float> m_fPositionX = null;
    CNetworkVar<float> m_fPositionY = null;
    CNetworkVar<float> m_fPositionZ = null;
    CNetworkVar<float> m_fRotationX = null;
    CNetworkVar<float> m_fRotationY = null;
    CNetworkVar<float> m_fRotationZ = null;
	CNetworkVar<ushort> m_usInputStates = null;


    EMotorType m_eMotorType = EMotorType.Normal;


    float m_fMouseDeltaX = 0.0f;
    float m_fMouseDeltaY = 0.0f;


	uint m_uiInputStates = 0;


    bool m_bRealignBody = false;
	bool m_bGrounded = false;
    bool m_bOutSide  = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();




    CapsuleCollider m_cCachedCapsuleCollider = null;
    Quaternion m_RealignFromRotation = Quaternion.identity;
    Quaternion m_RealignToRotation = Quaternion.identity;

    float m_RealignBodyTimer = 0.0f;
    float m_RealignBodyTime = 0.5f;
    float m_RealignHeadTimer = 0.0f;
    float m_RealignHeadTime = 0.5f;

    bool m_RealignBodyWithShip = false;
    bool m_RealignBodyWithHead = false;


};
