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

        SyncRotation,

        MAX
	}


    public enum EState
    {
        INVALID,

        WalkingWithinShip,
        WalkingShipExterior,
        AirThustersInSpace,
        AirThustersInShip,
        AligningBodyToShipInternal,
        AligningBodyToShipExternal,

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
		Run		    = 1 << 5,
		Crouch		= 1 << 6,
        FlyUp       = 1 << 7,
        FlyDown     = 1 << 8,
        RollRight   = 1 << 9,
        RollLeft    = 1 << 10,
        Stabilize   = 1 << 11,

        MAX
	}


// Member Types


    const float k_fPositionSendInterval  = 0.1f;
    const float k_fRotationSendInterval  = 0.1f;
    const float k_fAlignBodySpeedNormal  = 180.0f;
    const float k_fAlignBodySpeedThusers = 60.0f;

    // Ground movement
    const float k_fJumpSpeed    = 2.0f;
    const float k_fMoveSpeed    = 6.5f;
    const float k_fSprintSpeed  = 8.0f;

    // Air movement
    const float k_fThusterAccelerationForward   = 5.0f;
    const float k_fThusterAccelerationBack      = 5.0f;
    const float k_fThusterAccelerationStrafe    = 5.0f;
    const float k_fThusterAccelerationVertical  = 5.0f;
    const float k_fThusterAccelerationBreadking = 5.0f;
    const float k_fThusterAccelerationRoll      = 2.5f;

    const float k_fThusterMaxSpeedForward   = 10.0f;
    const float k_fThusterMaxSpeedBack      = 10.0f;
    const float k_fThusterMaxSpeedStrafe    = 10.0f;
    const float k_fThusterMaxSpeedVertical  = 10.0f;
    const float k_fThusterMaxSpeedRoll      = 2.0f;


// Member Delegates & Events


    public delegate void InputStatesChangeHandler(ushort _usPreviousStates, ushort _usNewSates);
	public event InputStatesChangeHandler EventInputStatesChange;


    public delegate void MotorTypeChangeHandler(EState _ePrevious, EState _eNew);
    public event MotorTypeChangeHandler EventMotorTypeChange;



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


    public EState State
    {
        get { return (m_eState); }
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
    public bool IsInputStateActive(EInputState _eState)
    {
        return ((m_uiLocalInputStates & (ushort)_eState) > 0);
    }


    [ALocalOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
        GameObject cSelfActor = CGamePlayers.SelfActor;

        if ( cSelfActor != null &&
            !CNetwork.IsServer)
        {
            s_cSerializeStream.Write(ENetworkAction.SyncRotation);
            s_cSerializeStream.Write(cSelfActor.transform.eulerAngles.x);
            s_cSerializeStream.Write(cSelfActor.transform.eulerAngles.y);
            s_cSerializeStream.Write(cSelfActor.transform.eulerAngles.z);
        }
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

                ENetworkAction eNetworkAction = _cStream.Read<ENetworkAction>();

                switch (eNetworkAction)
                {
                    case ENetworkAction.SyncRotation:
                        cPlayerActorMotor.transform.eulerAngles = new Vector3(_cStream.Read<float>(),
                                                                              _cStream.Read<float>(),
                                                                              _cStream.Read<float>());
                        break;

                    default:
                        Debug.LogError(string.Format("Unknown network action ({0})", (byte)eNetworkAction));
                        break;
                }
            }
		}
	}

	
	void Start()
	{
		m_cCapsuleCollider = GetComponent<CapsuleCollider>();

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
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Forward,      OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Backwards,    OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_StrafeLeft,   OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_StrafeRight,  OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Run,          OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Crouch,       OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveGround_Jump,   OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Run,          OnEventClientInputChange);

            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_Down,      OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_Up,        OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_RollLeft,  OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_RollRight, OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_Stabilize, OnEventClientInputChange);

            gameObject.GetComponent<CPlayerHead>().EventRotationYOverflow += OnEventHeadRotationYOverflow;
        }
        else if (gameObject == CGamePlayers.SelfActor)
        {
            // Subscribe to axis events
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventAxisChange);
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventAxisChange);

            // Subscribe to input events
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Forward,      OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Backwards,    OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_StrafeLeft,   OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_StrafeRight,  OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Run,          OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Crouch,       OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveGround_Jump,   OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Move_Run,          OnEventInputChange);

            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_Down,      OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_Up,        OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_RollLeft,  OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_RollRight, OnEventInputChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.MoveFly_Stabilize, OnEventInputChange);

            gameObject.GetComponent<CPlayerHead>().EventRotationYOverflow += OnEventHeadRotationYOverflow;
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
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Crouch,       OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveGround_Jump,   OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Run,          OnEventClientInputChange);

            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_Down,      OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_Up,        OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_RollLeft,  OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_RollRight, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_Stabilize, OnEventClientInputChange);

            gameObject.GetComponent<CPlayerHead>().EventRotationYOverflow -= OnEventHeadRotationYOverflow;
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
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Run,           OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Crouch,        OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveGround_Jump,    OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Move_Run,           OnEventInputChange);      

            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_Down,       OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_Up,         OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_RollLeft,   OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_RollRight,  OnEventInputChange);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.MoveFly_Stabilize,  OnEventInputChange);

            gameObject.GetComponent<CPlayerHead>().EventRotationYOverflow -= OnEventHeadRotationYOverflow;
        }
    }
	
	
	void Update()
	{
        // Empty
	}
	

	void FixedUpdate()
	{
        UpdateGrounded();

        if (m_bRealignBodyWithHead)
        {
            RealignBodyWithHead();
        }

        switch (m_eState)
        {
            case EState.AligningBodyToShipInternal:
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f), 180.0f * Time.fixedDeltaTime);

                    if (Quaternion.Angle(transform.rotation, Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f)) == 0.0f)
                    {
                        ChangeState(EState.WalkingWithinShip);
                    }
                }
                break;

            case EState.AligningBodyToShipExternal:
                {
                    if (Quaternion.Angle(transform.rotation, m_qTargetAlignRotation) == 0.0f)
                    {
                        ChangeState(EState.WalkingShipExterior);
                    }
                }
                break;

            case EState.WalkingWithinShip:
                UpdateGroundMovement();
                break;

            case EState.AirThustersInSpace:
                UpdateAirMovement();
                break;

            case EState.WalkingShipExterior:
                //UpdateMagneticMovement();
                break;

            default:
                Debug.LogError(string.Format("Unknown motor type: " + m_eState));
                break;
        }

        UpdatePositionLerping();

        if (CNetwork.IsServer)
        {
            m_fPositionX.Value = transform.position.x;
            m_fPositionY.Value = transform.position.y;
            m_fPositionZ.Value = transform.position.z;

            m_fRotationX.Value = transform.eulerAngles.x;
            m_fRotationY.Value = transform.eulerAngles.y;
            m_fRotationZ.Value = transform.eulerAngles.z;
        }

        m_fMouseDeltaX = 0.0f;
        m_fMouseDeltaY = 0.0f;
	}


	void UpdateGrounded()
	{
        if (!CNetwork.IsServer &&
            gameObject != CGamePlayers.SelfActor)
        {
            return;
        }

		Vector3 p1 = transform.position + m_cCapsuleCollider.center + (Vector3.up * ((m_cCapsuleCollider.height * 0.5f) + 0.5f));
		Vector3 p2 = p1 - (Vector3.up * m_cCapsuleCollider.height);
		RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, m_cCapsuleCollider.radius * 0.5f, -transform.up, 0.35f);

		m_bGrounded = false;

		foreach(RaycastHit hit in hits) 
		{
			if(!hit.collider.isTrigger && hit.collider != m_cCapsuleCollider) 
			{
				m_bGrounded = true;
				break;
			}
		}
	}


	void RealignBodyWithHead()
	{
        if (IsInputDisabled)
            return;

        Quaternion qBodyRotation = rigidbody.transform.rotation;
        Quaternion qHeadRotation = GetComponent<CPlayerHead>().Head.transform.rotation;
        Quaternion qHeadRotationRelative = new Quaternion();
        float fAlignSpeed = 0.0f;

        switch (m_eState)
        {
            case EState.AligningBodyToShipExternal:
                break;

            case EState.AligningBodyToShipInternal:
                break;

            case EState.WalkingWithinShip:
                fAlignSpeed = k_fAlignBodySpeedNormal;
                qHeadRotationRelative = Quaternion.Euler(0.0f, GetComponent<CPlayerHead>().Head.transform.eulerAngles.y, 0.0f); // Relative to body (Y Only)
                break;

            case EState.AirThustersInSpace:
                fAlignSpeed = k_fAlignBodySpeedThusers;
                qHeadRotationRelative = Quaternion.Euler(GetComponent<CPlayerHead>().Head.transform.eulerAngles.x,
                                                         GetComponent<CPlayerHead>().Head.transform.eulerAngles.y,
                                                         GetComponent<CPlayerHead>().Head.transform.eulerAngles.z); // Relative to body (Y Only)
                break;
        }

        rigidbody.transform.rotation = Quaternion.RotateTowards(qBodyRotation, qHeadRotationRelative, fAlignSpeed * Time.fixedDeltaTime);
        GetComponent<CPlayerHead>().Head.transform.rotation = qHeadRotation;

        float fAngle = Quaternion.Angle(qBodyRotation, qHeadRotationRelative);

        if (fAngle == 0.0f &&
            m_eState != EState.AirThustersInSpace)
        {
            m_bRealignBodyWithHead = false;
        }
	}


	void UpdateGroundMovement()
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
            vMovementVelocity += IsInputStateActive(EInputState.Forward)     ? transform.forward : Vector3.zero;
            vMovementVelocity -= IsInputStateActive(EInputState.Backward)    ? transform.forward : Vector3.zero;
            vMovementVelocity -= IsInputStateActive(EInputState.StrafeLeft)  ? transform.right   : Vector3.zero;
            vMovementVelocity += IsInputStateActive(EInputState.StrafeRight) ? transform.right   : Vector3.zero;

            // Apply direction movement speed
            vMovementVelocity = vMovementVelocity.normalized;
            vMovementVelocity *= ((m_uiLocalInputStates & (uint)EInputState.Run) > 0) ? k_fSprintSpeed : k_fMoveSpeed;

            // Jump 
            if ((m_uiLocalInputStates & (uint)EInputState.Jump) > 0 && IsGrounded)
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
                    m_bRealignBodyWithHead = true;
                }
            }
        }
	}


    void UpdateAirMovement()
    {
        if (IsInputDisabled)
            return;
        
        // Run on server or locally
        if (CNetwork.IsServer ||
            gameObject == CGamePlayers.SelfActor)
        {
            GameObject cHead = GetComponent<CPlayerHead>().Head;

            Vector3 vVelocity            = Quaternion.Inverse(transform.rotation) * rigidbody.velocity;
            Vector3 vAngularVelocity     = Quaternion.Inverse(transform.rotation) * rigidbody.angularVelocity;
            Vector3 vAcceleration        = Vector3.zero;
            Vector3 vAngularAcceleration = Vector3.zero;

            ComputeDirectionalSpeed(EInputState.RollLeft, EInputState.RollRight, -k_fThusterMaxSpeedRoll, -k_fThusterAccelerationRoll, vAngularVelocity.z, ref vAngularAcceleration.z);
            ComputeDirectionalSpeed(EInputState.RollRight, EInputState.RollLeft, k_fThusterMaxSpeedRoll, k_fThusterAccelerationRoll, vAngularVelocity.z, ref vAngularAcceleration.z);

            ComputeDirectionalSpeed(EInputState.INVALID, EInputState.INVALID, (m_fMouseDeltaX > 0) ? k_fThusterMaxSpeedRoll : -k_fThusterMaxSpeedRoll, m_fMouseDeltaX, vAngularVelocity.y, ref vAngularAcceleration.y);
            ComputeDirectionalSpeed(EInputState.INVALID, EInputState.INVALID, (m_fMouseDeltaY > 0) ? k_fThusterMaxSpeedRoll : -k_fThusterMaxSpeedRoll, m_fMouseDeltaY, vAngularVelocity.x, ref vAngularAcceleration.x);
            
            // Movement
            if (!IsInputStateActive(EInputState.Stabilize))
            {
                rigidbody.drag = 0.0f;
                rigidbody.angularDrag = 0.0f;

                ComputeDirectionalSpeed(EInputState.Forward,  EInputState.Backward,  k_fThusterMaxSpeedForward,  k_fThusterAccelerationForward, vVelocity.z, ref vAcceleration.z);
                ComputeDirectionalSpeed(EInputState.Backward, EInputState.Forward,  -k_fThusterMaxSpeedBack,    -k_fThusterAccelerationBack,    vVelocity.z, ref vAcceleration.z);
                ComputeDirectionalSpeed(EInputState.StrafeLeft,  EInputState.StrafeRight, -k_fThusterMaxSpeedStrafe, -k_fThusterAccelerationStrafe, vVelocity.x, ref vAcceleration.x);
                ComputeDirectionalSpeed(EInputState.StrafeRight, EInputState.StrafeLeft,   k_fThusterMaxSpeedStrafe,  k_fThusterAccelerationStrafe, vVelocity.x, ref vAcceleration.x);
                ComputeDirectionalSpeed(EInputState.FlyUp, EInputState.FlyDown,  k_fThusterMaxSpeedVertical,  k_fThusterAccelerationVertical, vVelocity.y, ref vAcceleration.y);
                ComputeDirectionalSpeed(EInputState.FlyDown, EInputState.FlyUp, -k_fThusterMaxSpeedVertical, -k_fThusterAccelerationVertical, vVelocity.y, ref vAcceleration.y);
            }
            else
            {
                rigidbody.drag = 2.0f;
                rigidbody.angularDrag = 2.0f;

                if (vVelocity.magnitude < 0.2f)
                {
                    vVelocity = Vector3.zero;
                }
            }

            rigidbody.velocity        = rigidbody.transform.rotation * vVelocity;
            //rigidbody.angularVelocity = GetComponent<CPlayerHead>().Head.transform.rotation * vAngularVelocity;

            rigidbody.AddForce(GetComponent<CPlayerHead>().Head.transform.rotation * vAcceleration, ForceMode.Acceleration);
            rigidbody.AddRelativeTorque(vAngularAcceleration, ForceMode.Acceleration);
        }
    }


    [AServerOnly]
    void UpdateMagneticMovement()
    {
        if (IsInputDisabled)
            return;
    }


    [ALocalOnly]
    void UpdatePositionLerping()
    {
        // Lerp to remote position
        if (CNetwork.IsServer)
            return;
     
        if (gameObject == CGamePlayers.SelfActor)
        {
            Vector3 vTargetPosition = new Vector3(m_fPositionX.Value, m_fPositionY.Value, m_fPositionZ.Value);

            if (Math.Abs((vTargetPosition - transform.position).magnitude) > k_fMoveSpeed * k_fPositionSendInterval)
            {
                return;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position,
                                                    new Vector3(m_fPositionX.Value, m_fPositionY.Value, m_fPositionZ.Value),
                                                    k_fThusterMaxSpeedForward * Time.fixedDeltaTime);
    }


    void ChangeState(EState _eNewState)
    {
        switch (_eNewState)
        {
            case EState.AligningBodyToShipExternal:
                break;

            case EState.AligningBodyToShipInternal:
                // Enable the rotation axis constraints
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
                break;

            case EState.WalkingWithinShip:
                {

                }
                break;

            case EState.AirThustersInSpace:
                {
                    // Release the rotation axis constraints
                    rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationX;
                    rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationY;
                    rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

                    m_bRealignBodyWithHead = true;
                }
                break;

            case EState.WalkingShipExterior:
                {
                    // Enable the rotation axis constraints
                    rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
                    rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
                    rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
                }
                break;

            default:
                Debug.LogError(string.Format("Unknown motor type: " + m_eState));
                break;
        }

        Debug.LogError(_eNewState);

        EState ePreviousMotorType = m_eState;
        m_eState = _eNewState;

        if (EventMotorTypeChange != null) EventMotorTypeChange(ePreviousMotorType, m_eState);
    }


    void ComputeDirectionalSpeed(EInputState _eInput, EInputState _eOppositeInput, float _fInputMaxSpeed, float _fInputAcceleration,
                                 float _rfInputAxisSpeed, ref float _rfInputAxisAcceleration)
    {
        // Check the input is active and the oposite is not
        // Check the speed max speed for the input's axis has not been reached
        if (_eInput == EInputState.INVALID ||
           ( IsInputStateActive(_eInput) &&
            !IsInputStateActive(_eOppositeInput)))
        {
            if (_fInputMaxSpeed >= 0.0f &&
                _rfInputAxisSpeed > _fInputMaxSpeed)
            {
                return;
            }
            if (_fInputMaxSpeed < 0.0f &&
                _rfInputAxisSpeed < _fInputMaxSpeed)
            {
                return;
            }

            // Append the acceleration to the input's axis
            _rfInputAxisAcceleration += _fInputAcceleration;
        }
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

                case CUserInput.EInput.Move_Run:
                    eTargetState = EInputState.Run;
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

                case CUserInput.EInput.MoveFly_Stabilize:
                    eTargetState = EInputState.Stabilize;
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
                    m_uiLocalInputStates |= (uint)eTargetState;
                }
                else
                {
                    m_uiLocalInputStates &= ~(uint)eTargetState;
                }

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
        // Start realigning body to the ship

    }


    [AServerOnly]
    void OnEventPlayerLeaveGravityZone()
    {
        // Empty
    }


    void OnEventEnterShip(GameObject _cActor)
    {
        ChangeState(EState.AligningBodyToShipInternal);
    }


    void OnEventLeaveShip(GameObject _cActor)
    {
        ChangeState(EState.AirThustersInSpace);
    }
    
    
    [ALocalAndServerOnly]
    void OnEventHeadRotationYOverflow(float _fY)
    {
        if (m_eState == EState.WalkingWithinShip)
        {
            transform.Rotate(new Vector3(0.0f, _fY, 0.0f));
            m_bRealignBodyWithHead = true;
        }
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
                m_bRealignBodyWithHead = true;
            }
        }
	}


// Member Fields


    CNetworkVar<float> m_fPositionX = null;
    CNetworkVar<float> m_fPositionY = null;
    CNetworkVar<float> m_fPositionZ = null;
    CNetworkVar<float> m_fRotationX = null;
    CNetworkVar<float> m_fRotationY = null;
    CNetworkVar<float> m_fRotationZ = null;
	CNetworkVar<ushort> m_usInputStates = null;


    Quaternion m_qTargetAlignRotation = new Quaternion();
    CapsuleCollider m_cCapsuleCollider = null;
    List<Type> m_cInputDisableQueue = new List<Type>();


    EState m_eState = EState.WalkingWithinShip;


    float m_fMouseDeltaX = 0.0f;
    float m_fMouseDeltaY = 0.0f;


	uint m_uiLocalInputStates = 0;


    bool m_bRealignBodyWithHead = false;
	bool m_bGrounded = false;
    bool m_bInSpace  = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
