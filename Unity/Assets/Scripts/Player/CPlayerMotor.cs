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
public class CPlayerMotor : CNetworkMonoBehaviour
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


    const float k_fPositionSendInterval  = 0.066f;
    const float k_fRotationSendInterval  = 0.033f;
    const float k_fAlignBodySpeedNormal  = 270.0f;
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


    public delegate void StateChangeHandler(EState _ePrevious, EState _eNew);
    public event StateChangeHandler EventStateChange;


// Member Properties


    public Quaternion ClientPosition
    {
        get { return (Quaternion.Euler(m_fRemotePositionX.Value, m_fRemotePositionY.Value, m_fRemotePositionZ.Value)); }
    }


    public Quaternion ClientRotation
    {
        get { return (Quaternion.Euler(m_fRemoteRotationX.Value, m_fRemoteRotationY.Value, m_fRemoteRotationZ.Value)); }
    }


    public EState State
    {
        get { return (m_eState); }
    }


	public ushort InputStates
	{
		get { return (m_usRemoteInputStates.Get()); }
	}


    public ushort PreviousInputStates
	{
		get { return (m_usRemoteInputStates.GetPrevious()); }
	}


    public bool IsInputDisabled
    {
        get { return (m_cInputDisableQueue.Count > 0); }
    }


    public bool IsGrounded
    {
        get { return (m_bGrounded); }
    }


// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        m_fRemotePositionX = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fPositionSendInterval);
        m_fRemotePositionY = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fPositionSendInterval);
        m_fRemotePositionZ = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fPositionSendInterval);
        m_fRemoteRotationX = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fRotationSendInterval);
        m_fRemoteRotationY = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fRotationSendInterval);
        m_fRemoteRotationZ = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, k_fRotationSendInterval);

        m_eRemoteState = _cRegistrar.CreateReliableNetworkVar<EState>(OnNetworkVarSync);
        m_usRemoteInputStates = _cRegistrar.CreateReliableNetworkVar<ushort>(OnNetworkVarSync);
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

        if (m_cInputDisableQueue.Count == 0)
        {
            gameObject.GetComponent<CThirdPersonAnimController>().EnableAnimation();
        }
	}


    [AOwnerAndServerOnly]
    public bool IsInputStateActive(EInputState _eState)
    {
        return ((m_usInputStates & (ushort)_eState) > 0);
    }


    [ALocalOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
        GameObject cSelfActor = CGamePlayers.SelfActor;

        if ( cSelfActor != null &&
            !cSelfActor.GetComponent<CPlayerMotor>().IsInputDisabled)
        {
            _cStream.Write(ENetworkAction.SyncRotation);
            _cStream.Write(cSelfActor.transform.position.x);
            _cStream.Write(cSelfActor.transform.position.y);
            _cStream.Write(cSelfActor.transform.position.z);
            _cStream.Write(cSelfActor.transform.eulerAngles.x);
            _cStream.Write(cSelfActor.transform.eulerAngles.y);
            _cStream.Write(cSelfActor.transform.eulerAngles.z);
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
                CPlayerMotor cPlayerActorMotor = cPlayerActor.GetComponent<CPlayerMotor>();

                ENetworkAction eNetworkAction = _cStream.Read<ENetworkAction>();

                switch (eNetworkAction)
                {
                    case ENetworkAction.SyncRotation:
                        if (!cPlayerActorMotor.IsInputDisabled)
                        {
                            cPlayerActorMotor.m_fRemotePositionX.Value = _cStream.Read<float>();
                            cPlayerActorMotor.m_fRemotePositionY.Value = _cStream.Read<float>();
                            cPlayerActorMotor.m_fRemotePositionZ.Value = _cStream.Read<float>();
                            cPlayerActorMotor.m_fRemoteRotationX.Value = _cStream.Read<float>();
                            cPlayerActorMotor.m_fRemoteRotationY.Value = _cStream.Read<float>();
                            cPlayerActorMotor.m_fRemoteRotationZ.Value = _cStream.Read<float>();
                        }
                        else
                        {
                            _cStream.IgnoreBytes(6 * sizeof(float));
                        }
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
        if (CNetwork.IsServer)
        {
            m_fRemotePositionX.Value = transform.position.x;
            m_fRemotePositionY.Value = transform.position.y;
            m_fRemotePositionZ.Value = transform.position.z;
        }

		m_cCapsuleCollider = GetComponent<CapsuleCollider>();

        // Register the entering/exiting gravity zones
        gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone += OnEventPlayerEnterGravityZone;
        gameObject.GetComponent<CActorGravity>().EventExitedGravityZone += OnEventPlayerLeaveGravityZone;

        gameObject.GetComponent<CActorLocator>().EventEnterShip += OnEventEnterShip;
        gameObject.GetComponent<CActorLocator>().EventLeaveShip += OnEventLeaveShip;

        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
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


    void OnDestroy()
    {
        // Unregister the entering/exiting gravity zones
        gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone -= OnEventPlayerEnterGravityZone;
        gameObject.GetComponent<CActorGravity>().EventExitedGravityZone -= OnEventPlayerLeaveGravityZone;

        gameObject.GetComponent<CActorLocator>().EventEnterShip -= OnEventEnterShip;
        gameObject.GetComponent<CActorLocator>().EventLeaveShip -= OnEventLeaveShip;

        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
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
        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe &&
            Input.GetKeyDown(KeyCode.O) &&
            CNetwork.IsServer)
        {
            if (GetComponent<CActorLocator>().CurrentFacility != null)
            {
                if (GetComponent<CActorLocator>().CurrentFacility.GetComponent<CFacilityGravity>().IsGravityEnabled)
                {
                    GetComponent<CActorLocator>().CurrentFacility.GetComponent<CFacilityGravity>().SetGravityEnabled(false);
                }
                else
                {
                    GetComponent<CActorLocator>().CurrentFacility.GetComponent<CFacilityGravity>().SetGravityEnabled(true);
                } 
            }
        }
	}
	

	void FixedUpdate()
	{
        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            UpdateGrounded();
            UpdateBodyHeadRealigning();

            switch (m_eState)
            {
                case EState.AligningBodyToShipInternal:
                    RealignBodyWithShipInternal();
                    break;

                case EState.AligningBodyToShipExternal:
                    RealignBodyWithShipExternal();
                    break;

                case EState.WalkingWithinShip:
                    UpdateGroundMovement();
                    break;

                case EState.AirThustersInShip:
                case EState.AirThustersInSpace:
                    UpdateThustersMovement();
                    break;

                case EState.WalkingShipExterior:
                    // UpdateMagneticMovement();
                    break;

                default:
                    Debug.LogError(string.Format("Unknown motor type: " + m_eState));
                    break;
            }
        }

        if (State == EState.AligningBodyToShipInternal &&
            (CNetwork.IsServer ||
             gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe))
        {
            // Change state when angle has reached 0
            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f)) == 0.0f)
            {
                ChangeState(EState.WalkingWithinShip);
            }
        }

        if (gameObject != CGamePlayers.SelfActor /*||
            (State != EState.AligningBodyToShipInternal &&
             State != EState.AligningBodyToShipExternal)*/)
        {
            UpdatePositionRotationLerping();
        }

        // Clear mouse deltas
        m_fMouseDeltaX = 0.0f;
        m_fMouseDeltaY = 0.0f;
	}


	void UpdateGrounded()
	{
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


    [ALocalOnly]
    void RealignBodyWithShipInternal()
    {
        // Align rigid body
        rigidbody.transform.rotation = Quaternion.RotateTowards(rigidbody.transform.rotation,
                                                                Quaternion.Euler(0.0f, rigidbody.transform.eulerAngles.y, 0.0f), 
                                                                k_fAlignBodySpeedNormal * Time.fixedDeltaTime);
    }


    [ALocalOnly]
    void RealignBodyWithShipExternal()
    {
        if (Quaternion.Angle(transform.rotation, m_qTargetAlignRotation) == 0.0f)
        {
            ChangeState(EState.WalkingShipExterior);
        }
    }


    [ALocalOnly]
	void UpdateBodyHeadRealigning()
	{
        if (!m_bRealignBodyWithHead)
            return;

        Quaternion qBodyRotation = rigidbody.transform.rotation;
        Quaternion qHeadRotation = GetComponent<CPlayerHead>().Head.transform.rotation;
        Quaternion qHeadRotationRelative = new Quaternion();
        float fAlignSpeed = 0.0f;

        switch (m_eState)
        {
            case EState.AligningBodyToShipExternal:
            case EState.AligningBodyToShipInternal:
                break;

            case EState.WalkingWithinShip:
                fAlignSpeed = k_fAlignBodySpeedNormal;
                qHeadRotationRelative = Quaternion.Euler(0.0f, GetComponent<CPlayerHead>().Head.transform.eulerAngles.y, 0.0f); // Relative to body (Y Only)
                break;

            case EState.AirThustersInShip:
            case EState.AirThustersInSpace:
                fAlignSpeed = k_fAlignBodySpeedThusers;
                qHeadRotationRelative = Quaternion.Euler(GetComponent<CPlayerHead>().Head.transform.eulerAngles.x,
                                                         GetComponent<CPlayerHead>().Head.transform.eulerAngles.y,
                                                         GetComponent<CPlayerHead>().Head.transform.eulerAngles.z);
                break;
        }

        rigidbody.transform.rotation = Quaternion.RotateTowards(qBodyRotation, qHeadRotationRelative, fAlignSpeed * Time.fixedDeltaTime);
        GetComponent<CPlayerHead>().Head.transform.rotation = qHeadRotation;

        float fAngle = Quaternion.Angle(qBodyRotation, qHeadRotationRelative);

        if (fAngle == 0.0f)
        {
            //Debug.LogError("Ended");
            m_bRealignBodyWithHead = false;
        }
	}


    [ALocalOnly]
	void UpdateGroundMovement()
	{
        if (IsInputDisabled)
            return;

        Quaternion vHeadRotation = Quaternion.Euler(0.0f, GetComponent<CPlayerHead>().Head.transform.localEulerAngles.y, 0.0f);

        // Direction movement
        Vector3 vMovementVelocity = new Vector3();
        vMovementVelocity += IsInputStateActive(EInputState.Forward)     ? transform.forward : Vector3.zero;
        vMovementVelocity -= IsInputStateActive(EInputState.Backward)    ? transform.forward : Vector3.zero;
        vMovementVelocity -= IsInputStateActive(EInputState.StrafeLeft)  ? transform.right   : Vector3.zero;
        vMovementVelocity += IsInputStateActive(EInputState.StrafeRight) ? transform.right   : Vector3.zero;

        // Apply direction movement speed
        vMovementVelocity = vMovementVelocity.normalized;
        vMovementVelocity *= ((m_usInputStates & (uint)EInputState.Run) > 0) ? k_fSprintSpeed : k_fMoveSpeed;

        // Jump 
        if ((m_usInputStates & (uint)EInputState.Jump) > 0 && IsGrounded)
        {
            vMovementVelocity.y = k_fJumpSpeed;
        }

        if (!rigidbody.isKinematic)
        {
            // Apply movement velocity
            rigidbody.velocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
            rigidbody.velocity += vHeadRotation * vMovementVelocity;

            if (vMovementVelocity != Vector3.zero)
            {
                m_bRealignBodyWithHead = true;
            }
        }
	}


    [ALocalOnly]
    void UpdateThustersMovement()
    {
        if (IsInputDisabled)
            return;
        
        CPlayerHead cPlayerHead = GetComponent<CPlayerHead>();
        Quaternion cHeadRotation = cPlayerHead.HeadRotation;

        Vector3 vVelocity            = Quaternion.Inverse(cHeadRotation) * rigidbody.velocity;
        Vector3 vAngularVelocity     = Quaternion.Inverse(cHeadRotation) * rigidbody.angularVelocity;
        Vector3 vAcceleration        = Vector3.zero;
        Vector3 vAngularAcceleration = Vector3.zero;

        // Apply roll
        ComputeDirectionalSpeed(EInputState.RollLeft, EInputState.RollRight, -k_fThusterMaxSpeedRoll, -k_fThusterAccelerationRoll, vAngularVelocity.z, ref vAngularAcceleration.z);
        ComputeDirectionalSpeed(EInputState.RollRight, EInputState.RollLeft,  k_fThusterMaxSpeedRoll,  k_fThusterAccelerationRoll, vAngularVelocity.z, ref vAngularAcceleration.z);

        // Apply pitch & yaw
        ComputeDirectionalSpeed(EInputState.INVALID, EInputState.INVALID, (m_fMouseDeltaX > 0) ? k_fThusterMaxSpeedRoll : -k_fThusterMaxSpeedRoll, m_fMouseDeltaX, vAngularVelocity.y, ref vAngularAcceleration.y);
        ComputeDirectionalSpeed(EInputState.INVALID, EInputState.INVALID, (m_fMouseDeltaY > 0) ? k_fThusterMaxSpeedRoll : -k_fThusterMaxSpeedRoll, m_fMouseDeltaY, vAngularVelocity.x, ref vAngularAcceleration.x);
            
        // Apply directional movement
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
            // Apply drag
            rigidbody.drag = 2.0f;
            rigidbody.angularDrag = 2.0f;

            // Stop
            if (vVelocity.magnitude < 0.2f)
            {
                vVelocity = Vector3.zero;
            }
        }

        // Set velocity
        rigidbody.velocity = rigidbody.transform.rotation * vVelocity;

        // Apply froce and torque
        rigidbody.AddForce(cHeadRotation * vAcceleration, ForceMode.Acceleration);
        rigidbody.AddTorque(cHeadRotation * vAngularAcceleration, ForceMode.Acceleration);
    }


    [AOwnerAndServerOnly]
    void UpdateMagneticMovement()
    {
        if (IsInputDisabled)
            return;
    }


    void UpdatePositionRotationLerping()
    {
        if (IsInputDisabled)
            return;

        // Generate remote position
        Vector3 vRemotePosition = new Vector3(m_fRemotePositionX.Value, m_fRemotePositionY.Value, m_fRemotePositionZ.Value);
        Vector3 vRemoteEuler = new Vector3(m_fRemoteRotationX.Value, m_fRemoteRotationY.Value, m_fRemoteRotationZ.Value);
        float fLerpVelocity = -100.0f;

        switch (m_eState)
        {
            case EState.AirThustersInShip:
            case EState.AirThustersInSpace:
                fLerpVelocity = k_fThusterMaxSpeedForward;
                break;

            case EState.WalkingShipExterior:
            case EState.WalkingWithinShip:
                fLerpVelocity = k_fMoveSpeed;
                break;

            default:
                fLerpVelocity = k_fMoveSpeed;
                break;
        }

        float fRemoteDistance = Math.Abs((vRemotePosition - transform.position).magnitude);

        if (gameObject != CGamePlayers.SelfActor)
        {
            if (fRemoteDistance > k_fMoveSpeed * k_fPositionSendInterval * 3.0f)
            {
                // Lerp to position
                rigidbody.transform.position = vRemotePosition;

            }
            else if (gameObject != CGamePlayers.SelfActor)
            {
                // Lerp to position
                rigidbody.transform.position = Vector3.MoveTowards(rigidbody.transform.position,
                                                                   vRemotePosition,
                                                                   fLerpVelocity * Time.fixedDeltaTime);
            }

            if (Quaternion.Angle(rigidbody.transform.rotation, Quaternion.Euler(vRemoteEuler)) > k_fAlignBodySpeedNormal * Time.fixedDeltaTime * 3.0f)
            {
                rigidbody.transform.rotation = Quaternion.Euler(vRemoteEuler);
            }
            else 
            {
                // Lerp to rotation 
                rigidbody.transform.rotation = Quaternion.RotateTowards(rigidbody.transform.rotation, Quaternion.Euler(vRemoteEuler), k_fAlignBodySpeedNormal * Time.fixedDeltaTime);
            }
        }
    }


    void ChangeState(EState _eNewState)
    {
        // Run functionality for chaning state
        switch (_eNewState)
        {
            case EState.AligningBodyToShipExternal:
            case EState.AligningBodyToShipInternal:
                {
                    // Enable the rotation axis constraints
                    rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
                    rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
                    rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
                }
                break;

            case EState.WalkingWithinShip:
                {

                }
                break;

            case EState.AirThustersInShip:
            case EState.AirThustersInSpace:
                {
                    m_bRealignBodyWithHead = true;

                    // Release the rotation axis constraints
                    rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationX;
                    rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationY;
                    rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
                }
                break;

            case EState.WalkingShipExterior:
                {
                    m_bRealignBodyWithHead = true;

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

        //Debug.LogError(_eNewState);

        EState ePreviousState = m_eState;
        m_eState = _eNewState;

        // Set remote state
        if (CNetwork.IsServer)
        {
            m_eRemoteState.Set(m_eState);
        }

        // Notify observers
        if (EventStateChange != null) EventStateChange(ePreviousState, m_eState);
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


    [AOwnerAndServerOnly]
    void OnEventAxisChange(CUserInput.EAxis _eAxis, float _fValue)
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


    [AOwnerAndServerOnly]
    void OnEventInputChange(CUserInput.EInput _eInput, bool _bDown)
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
            ushort usPreviousInputStates = m_usInputStates;

            // Update state
            if (_bDown)
            {
                m_usInputStates |= (ushort)eTargetState;
            }
            else
            {
                m_usInputStates &= (ushort)~eTargetState;
            }

            // Set remote input states
            if (CNetwork.IsServer)
            {
                m_usRemoteInputStates.Value = (ushort)m_usInputStates;
            }

            // Notify observers
            if (EventInputStatesChange != null) EventInputStatesChange(usPreviousInputStates, m_usInputStates);
        }
    }


    [AOwnerAndServerOnly]
    void OnEventPlayerEnterGravityZone()
    {
        // Check this is the server or I own this actor
        if (CNetwork.IsServer ||
            gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            // Check we are using air thusters within the ship
            if (m_eState == EState.AirThustersInShip)
            {
                ChangeState(EState.AligningBodyToShipInternal);
            }
        }
    }


    [AOwnerAndServerOnly]
    void OnEventPlayerLeaveGravityZone()
    {
        // Check this is the server or I own this actor
        if (CNetwork.IsServer ||
            gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            // Check if this actor is not in the ship
            if (!gameObject.GetComponent<CActorLocator>().IsInShip)
            {
                ChangeState(EState.AirThustersInSpace);
            }

            // Actor is in the ship with no gravity
            else
            {
                ChangeState(EState.AirThustersInShip);
            }
        }
    }


    [AOwnerAndServerOnly]
    void OnEventEnterShip(GameObject _cActor)
    {
        // Check this is the server or I own this actor
        if (CNetwork.IsServer ||
            gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            // Check facility has gravity
            if (GetComponent<CActorLocator>().CurrentFacility.GetComponent<CFacilityGravity>().IsGravityEnabled)
            {
                ChangeState(EState.AligningBodyToShipInternal);
            }

            // Use air thusters within ship
            else
            {
                ChangeState(EState.AirThustersInShip);
            }
        }
    }


    [AOwnerAndServerOnly]
    void OnEventLeaveShip(GameObject _cActor)
    {
        // Check this is the server or I own this actor
        if (CNetwork.IsServer ||
            gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            // Check user was using air thusters within the ship (OnEventPlayerLeaveGravityZone() wont be called in this state)
            if (m_eState == EState.AirThustersInShip)
            {
                ChangeState(EState.AirThustersInSpace);
            }
        }
    }
    
    
    [AOwnerAndServerOnly]
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
            if (_cSyncedVar == m_usRemoteInputStates)
            {
                // Notify event observers
                if (EventInputStatesChange != null) EventInputStatesChange(PreviousInputStates, InputStates);
            }
            else if (_cSyncedVar == m_fRemotePositionX ||
                     _cSyncedVar == m_fRemotePositionY ||
                     _cSyncedVar == m_fRemotePositionZ)
            {
                m_bRealignBodyWithHead = true;
            }
            else if (_cSyncedVar == m_eRemoteState)
            {
                ChangeState(m_eRemoteState.Get());
            }
        }
	}


// Member Fields


    CNetworkVar<float> m_fRemotePositionX = null;
    CNetworkVar<float> m_fRemotePositionY = null;
    CNetworkVar<float> m_fRemotePositionZ = null;
    CNetworkVar<float> m_fRemoteRotationX = null;
    CNetworkVar<float> m_fRemoteRotationY = null;
    CNetworkVar<float> m_fRemoteRotationZ = null;
	CNetworkVar<ushort> m_usRemoteInputStates = null;
    CNetworkVar<EState> m_eRemoteState = null;


    Quaternion m_qTargetAlignRotation = new Quaternion();
    CapsuleCollider m_cCapsuleCollider = null;
    List<Type> m_cInputDisableQueue = new List<Type>();


    EState m_eState = EState.WalkingWithinShip;


    float m_fMouseDeltaX = 0.0f;
    float m_fMouseDeltaY = 0.0f;


	ushort m_usInputStates = 0;

    public bool m_bRealignBodyWithHead = false;
	bool m_bGrounded = false;
    bool m_bInSpace  = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
