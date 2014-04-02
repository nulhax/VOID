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


    [ABitSize(4)]
	public enum ENetworkAction : byte
	{
        INVALID,

        MAX
	}


	public enum EMoveState
	{
        INVALID     = -1,

		Forward	    = 1 << 0,
		Backward	= 1 << 1,
		Up		    = 1 << 2,
		Down		= 1 << 3,
		StrafeLeft	= 1 << 4,
		StrafeRight	= 1 << 5,
		RollLeft	= 1 << 6,
		RollRight	= 1 << 7,
		Turbo		= 1 << 8,
	}


// Member Delegates & Events


// Member Properties


	public bool IsActive
	{
		get { return(m_bActive.Get()); }
	}


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "RemoteAlign");

		m_bActive = _cRegistrar.CreateNetworkVar(OnNetworkVarSync, false);
    }


	[ALocalOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
        if (s_cSerializeStream.ByteSize > 0)
        {
            _cStream.Write(s_cSerializeStream);
            s_cSerializeStream.Clear();
        }
	}
	
	
	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);

		if (cPlayerActor != null)
		{
			CPlayerAirMotor cAirMotor = cPlayerActor.GetComponent<CPlayerAirMotor>();

			while (_cStream.HasUnreadData)
			{
				ENetworkAction eNetworkAction = (ENetworkAction)_cStream.Read<byte>();

				switch (eNetworkAction)
				{
				default:
					Debug.LogError(string.Format("Unknown network action ({0})", (int)eNetworkAction));
					break;
				}
			}
		}
	}


	void Start()
	{
		// Register the entering/exiting gravity zones
		gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone += OnPlayerEnterGravityZone;
		gameObject.GetComponent<CActorGravity>().EventExitedGravityZone += OnPlayerLeaveGravityZone;

        if (CNetwork.IsServer)
        {
            // Register input changes from the mouse
            CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisChange);
            CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);

            // Subscribe to client input events
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Forward,       OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Backwards,     OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_StrafeLeft,    OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_StrafeRight,   OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_Down,       OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_Up,         OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_RollLeft,   OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.MoveFly_RollRight,  OnEventClientInputChange);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Move_Turbo,         OnEventClientInputChange);
        }
	}


	void OnDestroy()
	{
		// Unregister the entering/exiting gravity zones
		gameObject.GetComponent<CActorGravity>().EventEnteredGravityZone -= OnPlayerEnterGravityZone;
		gameObject.GetComponent<CActorGravity>().EventExitedGravityZone -= OnPlayerLeaveGravityZone;

        if (CNetwork.IsServer)
        {
		    // Unegister input changes from the mouse
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisChange);
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);

            // Unsubscribe to client input events
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Forward, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Backwards, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_StrafeLeft, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_StrafeRight, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_Down, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_Up, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_RollLeft, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.MoveFly_RollRight, OnEventClientInputChange);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Move_Turbo, OnEventClientInputChange);
        }
	}


	void Update()
	{
		// Update the interpolations
		UpdateInterpolations();
	}


    void FixedUpdate()
    {
        if (CNetwork.IsServer)
        {
            if (m_bActive.Get() &&
                !m_RealignBodyWithShip)
            {
                UpdateMovement();
            }
        }
    }


	[AServerOnly]
	void UpdateInterpolations()
	{
		if(m_RealignBodyWithShip)
		{
			if(CNetwork.IsServer)
			{
				m_RealignBodyTimer += Time.deltaTime;
				if(m_RealignBodyTimer > m_RealignBodyTime)
				{
					m_RealignBodyTimer = m_RealignBodyTime;
					m_RealignBodyWithShip = false;
				}

				// Set the lerped rotation
				transform.rotation = Quaternion.Slerp(m_RealignFromRotation, m_RealignToRotation, m_RealignBodyTimer/m_RealignBodyTime);
					
				if(!m_RealignBodyWithShip)
				{
					// Player is now in gravity zone, set inactive
					m_bActive.Set(false);
					
					// Stop syncing the rotations
					GetComponent<CActorNetworkSyncronized>().SyncTransform();
					GetComponent<CActorNetworkSyncronized>().m_SyncRotation = false;
					
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


    [AServerOnly]
	void UpdateMovement()
	{
		// Movement
		Vector3 deltaMovementAcceleration = new Vector3();
		deltaMovementAcceleration.z += ((m_uiMovementStates & (uint)EMoveState.Forward)  	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.z -= ((m_uiMovementStates & (uint)EMoveState.Backward) 	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.x -= ((m_uiMovementStates & (uint)EMoveState.StrafeLeft)  	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.x += ((m_uiMovementStates & (uint)EMoveState.StrafeRight) 	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.y += ((m_uiMovementStates & (uint)EMoveState.Up) 		 	> 0) ? 1.0f : 0.0f;
		deltaMovementAcceleration.y -= ((m_uiMovementStates & (uint)EMoveState.Down) 		 	> 0) ? 1.0f : 0.0f;

		// Apply movement acceleration
		deltaMovementAcceleration  = deltaMovementAcceleration.normalized;
		deltaMovementAcceleration *= ((m_uiMovementStates & (uint)EMoveState.Turbo) > 0) ? k_fTurboAcceleration : k_fMovementAcceleration;

		// Rotation
		Vector3 deltaRotationAcceleration = new Vector3();
		deltaRotationAcceleration.x = m_fMouseDeltaY;
        deltaRotationAcceleration.y = m_fMouseDeltaX;
		deltaRotationAcceleration.z -= ((m_uiMovementStates & (uint)EMoveState.RollLeft)  	> 0) ? 1.0f : 0.0f;
		deltaRotationAcceleration.z += ((m_uiMovementStates & (uint)EMoveState.RollRight) 	> 0) ? 1.0f : 0.0f;

		// Apply rotation acceleration
		deltaRotationAcceleration.x *= k_fRotationAccelerationX;
		deltaRotationAcceleration.y *= k_fRotationAccelerationY;
		deltaRotationAcceleration.z *= k_fRotationAccelerationZ;

		rigidbody.AddRelativeForce(deltaMovementAcceleration, ForceMode.Acceleration);
		rigidbody.AddRelativeTorque(deltaRotationAcceleration, ForceMode.Acceleration);


        m_fMouseDeltaX = 0.0f;
        m_fMouseDeltaY = 0.0f;
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


    [AServerOnly]
    void OnEventClientInputChange(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        // Check player is the owner of this actor
        if (_ulPlayerId == GetComponent<CPlayerInterface>().PlayerId)
        {
            EMoveState eTargetState = EMoveState.INVALID;

            // Match the input towards a movement state
            switch (_eInput)
            {
                case CUserInput.EInput.Move_Forward:
                    eTargetState = EMoveState.Forward;
                    break;

                case CUserInput.EInput.Move_Backwards:
                    eTargetState = EMoveState.Backward;
                    break;

                case CUserInput.EInput.Move_StrafeLeft:
                    eTargetState = EMoveState.StrafeLeft;
                    break;

                case CUserInput.EInput.Move_StrafeRight:
                    eTargetState = EMoveState.StrafeRight;
                    break;

                case CUserInput.EInput.MoveFly_Down:
                    eTargetState = EMoveState.Down;
                    break;

                case CUserInput.EInput.MoveFly_Up:
                    eTargetState = EMoveState.Up;
                    break;

                case CUserInput.EInput.MoveFly_RollLeft:
                    eTargetState = EMoveState.RollLeft;
                    break;

                case CUserInput.EInput.MoveFly_RollRight:
                    eTargetState = EMoveState.RollRight;
                    break;

                case CUserInput.EInput.Move_Turbo:
                    eTargetState = EMoveState.Turbo;
                    break;

                default:
                    Debug.LogError(string.Format("Unknown client input cange. Input({0})", _eInput));
                    break;
            }

            if (eTargetState != EMoveState.INVALID)
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


	[AServerOnly]
	void OnPlayerEnterGravityZone()
	{
		// Start realigning body to the ship
		m_RealignBodyWithShip = true;

		// Set the values to use for realigment
		m_RealignFromRotation = transform.rotation;
		m_RealignToRotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
		m_RealignBodyTimer = 0.0f;
		m_RealignBodyTime = Mathf.Clamp(Quaternion.Angle(m_RealignFromRotation, m_RealignToRotation) / 180.0f, 0.5f, 2.0f);
	}


	[AServerOnly]
	void OnPlayerLeaveGravityZone()
	{
		// Player is now in no-gravity zone, set active
		m_bActive.Set(true);

		// Stop any realignment with ship
		m_RealignBodyWithShip = false;

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


    void OnNetworkVarSync(INetworkVar _SyncedVar)
    {
        if (_SyncedVar == m_bActive)
        {
            if (m_bActive.Get())
            {
                if (CGamePlayers.SelfActor == gameObject)
                {
                    // Placeholder: Disable movement input
                    GetComponent<CPlayerHead>().DisableInput(this);
                    GetComponent<CPlayerGroundMotor>().DisableInput(this);
                }

                // Debug fix: make the head body realign instantly
                Quaternion headRot = GetComponent<CPlayerHead>().Head.transform.rotation;
                transform.rotation = headRot;
                GetComponent<CPlayerHead>().Head.transform.rotation = headRot;
            }
            else
            {
                if (CGamePlayers.SelfActor == gameObject)
                {
                    // Placeholder: Enable movement input
                    GetComponent<CPlayerHead>().EnableInput(this);
                    GetComponent<CPlayerGroundMotor>().ReenableInput(this);
                }

                // Stop any head realignment with body
                m_RealignBodyWithHead = false;
            }
        }
    }


// Member Fields


    const float k_fMovementAcceleration  = 10.0f;
    const float k_fRotationAccelerationX = 0.5f;
    const float k_fRotationAccelerationY = 0.5f;
    const float k_fRotationAccelerationZ = 1.0f;
    const float k_fTurboAcceleration     = 50.0f;


	CNetworkVar<bool> m_bActive = null;


    float m_fMouseDeltaX = 0.0f;
    float m_fMouseDeltaY = 0.0f;
	

	uint m_uiMovementStates = 0;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();




    Quaternion m_RealignFromRotation = Quaternion.identity;
    Quaternion m_RealignToRotation = Quaternion.identity;

    float m_RealignBodyTimer = 0.0f;
    float m_RealignBodyTime = 0.5f;
    float m_RealignHeadTimer = 0.0f;
    float m_RealignHeadTime = 0.5f;

    bool m_RealignBodyWithShip = false;
    bool m_RealignBodyWithHead = false;


};
