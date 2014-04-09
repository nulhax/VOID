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


public class CPlayerHead : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	public float HeadEulerX
	{
		get { return (m_fHeadEulerX.Get()); }
	}

    public float HeadEulerY
    {
        get { return (m_fHeadEulerY.Get()); }
    }

	public GameObject Head
	{
		get 
        { 
            return (GetComponent<CPlayerInterface>().Model.transform.FindChild("Head").gameObject); 
        }
	}

	public bool InputDisabled
	{
		get { return (m_cInputDisableQueue.Count > 0); }
	}


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_fHeadEulerX = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, 0.1f);
        m_fHeadEulerY = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, 0.1f);
    }


	[ALocalOnly]
	public void DisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Add(_cFreezeRequester.GetType());
	}


	[ALocalOnly]
	public void EnableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Remove(_cFreezeRequester.GetType());
	}


    [ALocalOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
        // Empty
	}


    [AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
        // Empty
	}


    void Start()
    {
        if (CGamePlayers.SelfActor == gameObject)
        {
            // Setup the game cameras
            CGameCameras.SetupCameras();

            // Setup the HUD
            CGameHUD.SetupHUD();

            // Set the ship view perspective of the camera to the actors head
            TransferPlayerPerspectiveToShipSpace();

            // Register event handler for entering/exiting ship
            gameObject.GetComponent<CActorBoardable>().EventBoard     += TransferPlayerPerspectiveToShipSpace;
            gameObject.GetComponent<CActorBoardable>().EventDisembark += TransferPlayerPerspectiveToGalaxySpace;

            // Add audoio listener to head
            Head.AddComponent<AudioListener>();

            gameObject.GetComponent<CPlayerGroundMotor>().EventInputStatesChange += NotifyMovementStateChange;
            m_initialOffset = GetComponent<CPlayerInterface>().Model.transform.FindChild("Head").gameObject.transform.localPosition;
        }

        if (CNetwork.IsServer)
        {
            // Register for mouse movement input
            CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisChange);
            CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);
        }
        else if (gameObject == CGamePlayers.SelfActor)
        {
            // Register for mouse movement input
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventAxisChange);
            CUserInput.SubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventAxisChange);
        }
    }


    void OnDestroy()
    {
        // Unregister
        if (CGamePlayers.SelfActor == gameObject)
        {
            gameObject.GetComponent<CActorBoardable>().EventBoard     -= TransferPlayerPerspectiveToShipSpace;
            gameObject.GetComponent<CActorBoardable>().EventDisembark -= TransferPlayerPerspectiveToGalaxySpace;
        }

        if (CNetwork.IsServer)
        {
            // Register for mouse movement input
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseX, OnEventClientAxisChange);
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);
        }
        else if (gameObject == CGamePlayers.SelfActor)
        {
            // Register for mouse movement input
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseX, OnEventAxisChange);
            CUserInput.UnsubscribeAxisChange(CUserInput.EAxis.MouseY, OnEventAxisChange);
        }
    }

    void NotifyMovementStateChange(ushort _usPreviousStates, ushort _usNewSates)
    {
        m_MovementState = _usNewSates;         
    }   

    void Update()
    {
        // Empty

        if (gameObject != CGamePlayers.SelfActor)
            GetComponent<CPlayerInterface>().Model.GetComponent<CPlayerSkeleton>().m_playerNeck.transform.localEulerAngles = new Vector3(0.0f,
                                                                                                                                         90.0f + Head.transform.localEulerAngles.y,
                                                                                                                                        -67.31491f + Head.transform.localEulerAngles.x);        
    }


    void FixedUpdate()
    {
        UpdateRotation();

        if (CNetwork.IsServer)
        {
            m_fHeadEulerX.Value = Head.transform.localEulerAngles.x;
            m_fHeadEulerY.Value = Head.transform.localEulerAngles.y;
        }

        if (CGamePlayers.SelfActor == gameObject)
        {
            HeadBob();
        }
    }

    void HeadBob()
    {
        //Determine current states
        bool bRunForward;
        bool bWalkBack;
        bool bSprint;
        bool bJump;
        bool bCrouch;
        bool bStrafeLeft;
        bool bStrafeRight;          
        
        bRunForward =  ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Forward)     > 0) ? true : false;   
        bWalkBack    = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Backward)    > 0) ? true : false;   
        bJump        = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Jump)        > 0) ? true : false;   
        bCrouch      = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Crouch)      > 0) ? true : false;   
        bStrafeLeft  = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.StrafeLeft)  > 0) ? true : false;   
        bStrafeRight = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.StrafeRight) > 0) ? true : false;
        bSprint      = ((m_MovementState & (uint)CPlayerGroundMotor.EInputState.Turbo)       > 0) ? true : false;   

        //Determine head bob amount
        if((bRunForward || bWalkBack || bStrafeLeft || bStrafeRight) && !bJump)
        {
            m_fHeadBobAmount = m_kfHeadBobRunAmount;
            m_fHeadBobSpeed = m_kfHeadBobRunSpeed;
            Head.transform.localPosition = m_initialOffset;
        }
        if((bSprint && bRunForward) && !bJump)
        {
            m_fHeadBobAmount = m_kfHeadBobSprintAmount;
            m_fHeadBobSpeed = m_kfHeadBobSprintSpeed;
            Head.transform.localPosition = m_initialOffset;
        }

        //Only apply head bob if character is moving
        if ((bRunForward || bWalkBack || bStrafeLeft || bStrafeRight) && !bJump)
        {
            Vector3 headPosition = Head.transform.localPosition;
            headPosition.y += Mathf.Cos(Time.fixedTime * m_fHeadBobSpeed) * m_fHeadBobAmount;        
            Head.transform.localPosition = headPosition;

            Vector3 headRotation = Head.transform.localRotation.eulerAngles;
            headRotation.z += Mathf.Cos(Time.fixedTime * m_fHeadBobSpeed) * (m_fHeadBobAmount * 2);   
            Head.transform.localRotation = Quaternion.Euler(headRotation);
        } 
        else
        {
            //Reset head bob
            Head.transform.localPosition = m_initialOffset;
            Vector3 headRotation = Head.transform.localRotation.eulerAngles;
            //Reset roll
            headRotation.z = 0;
            Head.transform.localRotation = Quaternion.Euler(headRotation);
        }
    }


    void UpdateRotation()
    {
        if (InputDisabled)
            return;

        // Run on server or locally
        if (CNetwork.IsServer ||
            gameObject == CGamePlayers.SelfActor)
        {
            if (m_fMouseDeltaX == 0.0f &&
                m_fMouseDeltaY == 0.0f)
                return;

            Vector3 vLocalRotation = Head.transform.localRotation.eulerAngles;

            if (vLocalRotation.x > 180.0f)
            {
                vLocalRotation.x -= 360.0f;
            }

            if (vLocalRotation.y > 180.0f)
            {
                vLocalRotation.y -= 360.0f;
            }

            vLocalRotation.x = Mathf.Clamp(vLocalRotation.x + m_fMouseDeltaY, k_fRotationXMin, k_fRotationXMax);
            vLocalRotation.y = Mathf.Clamp(vLocalRotation.y + m_fMouseDeltaX, -k_fRotationYLimit, k_fRotationYLimit);

            Head.transform.localEulerAngles = vLocalRotation;

            float fOverRotationY = vLocalRotation.y + m_fMouseDeltaX;

            if (fOverRotationY < -k_fRotationYLimit)
            {
                GetComponent<CPlayerGroundMotor>().CorrectHeadOverRotate(fOverRotationY + k_fRotationYLimit);
            }
            else if (fOverRotationY > k_fRotationYLimit)
            {
                GetComponent<CPlayerGroundMotor>().CorrectHeadOverRotate(fOverRotationY - k_fRotationYLimit);
            }

            m_fMouseDeltaX = 0.0f;
            m_fMouseDeltaY = 0.0f;
        }

        // Lerp to remote rotation
        else
        {
            Head.transform.localRotation = Quaternion.RotateTowards(Head.transform.localRotation, 
                                                                    Quaternion.Euler(m_fHeadEulerX.Value, m_fHeadEulerY.Value, 0.0f), 
                                                                    360.0f * Time.fixedDeltaTime);
        }
    }


    [ALocalOnly]
	void TransferPlayerPerspectiveToShipSpace()
	{
		CGameCameras.SetObserverSpace(true);

		// Remove the galaxy observer component
		Destroy(gameObject.GetComponent<GalaxyObserver>());
	}
	

    [ALocalOnly]
	void TransferPlayerPerspectiveToGalaxySpace()
	{
		CGameCameras.SetObserverSpace(false);

		// Add the galaxy observer component
		gameObject.AddComponent<GalaxyObserver>();
	}


    [ALocalOnly]
    void OnEventAxisChange(CUserInput.EAxis _eAxis, float _fValue)
    {
        OnEventClientAxisChange(_eAxis, CNetwork.PlayerId, _fValue);
    }


    [AServerOnly]
    void OnEventClientAxisChange(CUserInput.EAxis _eAxis, ulong _ulPlayerId, float _fValue)
    {
        if (GetComponent<CPlayerInterface>().PlayerId != _ulPlayerId)
            return;

        switch (_eAxis)
        {
            case CUserInput.EAxis.MouseX:
                m_fMouseDeltaX += _fValue;
                break;

            case CUserInput.EAxis.MouseY:
                m_fMouseDeltaY += _fValue;
                break;

            default:
                Debug.LogError("Unknown client axis: " + _eAxis);
                break;
        }
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }


// Member Fields

    //Headbob amounts   
    public float m_kfHeadBobRunAmount = 0.07f;
    public float m_kfHeadBobSprintAmount = 0.09f;

    //Headbob speeds
    public const float m_kfHeadBobRunSpeed = 8;
    public const float m_kfHeadBobSprintSpeed = 10;

    Vector3 m_initialOffset;
 
    public float m_fHeadBobAmount = 0;
    public float m_fHeadBobSpeed = 0;

    const float k_fRotationXMin   = -70; // Up
    const float k_fRotationXMax   =  54; // Down
    const float k_fRotationYLimit =  68; // Degrees


	List<Type> m_cInputDisableQueue = new List<Type>();
	
	
	CNetworkVar<float> m_fHeadEulerX = null;
    CNetworkVar<float> m_fHeadEulerY = null;


    float m_fMouseDeltaX = 0.0f;
    float m_fMouseDeltaY = 0.0f;
	float m_fLocalXRotation = 0.0f;

    ushort m_MovementState;

};
