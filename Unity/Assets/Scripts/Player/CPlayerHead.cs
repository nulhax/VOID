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


    [ABitSize(4)]
    public enum ENetworkAction
    {
        INVALID,

        SyncLocalEuler,

        MAX
    }


// Member Delegates & Events


    public delegate void HeadRotateOverflowHandler(float _fEuler);
    public event HeadRotateOverflowHandler EventRotationXOverflow;
    public event HeadRotateOverflowHandler EventRotationYOverflow;


// Member Properties


	public float RemoteHeadEulerX
	{
		get { return (m_fHeadEulerX.Get()); }
	}

    public float RemoteHeadEulerY
    {
        get { return (m_fHeadEulerY.Get()); }
    }

    public Quaternion RemoteRotation
    {
        get { return (Quaternion.Euler(m_fHeadEulerX.Value, m_fHeadEulerY.Value, 0.0f)); }
    }

	public GameObject Head
	{
		get 
        { 
            return (GetComponent<CPlayerInterface>().Model.transform.FindChild("Head").gameObject); 
        }
	}

    public Quaternion HeadRotation
    {
        get { return (Head.transform.rotation); }
    }

    public Quaternion HeadLocalRotation
    {
        get { return (Head.transform.localRotation); }
    }

	public bool InputDisabled
	{
		get { return (m_cInputDisableQueue.Count > 0); }
	}


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_fHeadEulerX = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, 0.05f);
        m_fHeadEulerY = _cRegistrar.CreateUnreliableNetworkVar<float>(OnNetworkVarSync, 0.05f);
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
        GameObject cSelfActor = CGamePlayers.SelfActor;

        if (cSelfActor != null)
        {
            _cStream.Write(ENetworkAction.SyncLocalEuler);
            _cStream.Write(cSelfActor.GetComponent<CPlayerHead>().Head.transform.localEulerAngles.x);
            _cStream.Write(cSelfActor.GetComponent<CPlayerHead>().Head.transform.localEulerAngles.y);
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
                CPlayerHead cPlayerHead = cPlayerActor.GetComponent<CPlayerHead>();

                // Extract network action
                ENetworkAction eNetworkAction = _cStream.Read<ENetworkAction>();

                switch (eNetworkAction)
                {
                    case ENetworkAction.SyncLocalEuler:
                        cPlayerHead.m_fHeadEulerX.Value = _cStream.Read<float>();
                        cPlayerHead.m_fHeadEulerY.Value = _cStream.Read<float>();
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


    void Update()
    {
        if (gameObject != CGamePlayers.SelfActor)
            GetComponent<CPlayerInterface>().Model.GetComponent<CPlayerSkeleton>().m_playerNeck.transform.localEulerAngles = new Vector3(0.0f,
                                                                                                                                         90.0f + Head.transform.localEulerAngles.y,
                                                                                                                                        -67.31491f + Head.transform.localEulerAngles.x);
    }

    void FixedUpdate()
    {
        if (gameObject == CGamePlayers.SelfActor)
        {
            switch (gameObject.GetComponent<CPlayerMotor>().State)
            {
                case CPlayerMotor.EState.AligningBodyToShipInternal:
                case CPlayerMotor.EState.WalkingWithinShip:
                    UpdateFeelookRotation();
                    break;
            }
        }
        else
        {
            AlignHead();
        }

        // Clean up
        m_fMouseDeltaX = 0.0f;
        m_fMouseDeltaY = 0.0f;
    }


    void UpdateFeelookRotation()
    {
        if (InputDisabled)
            return;

        if (m_fMouseDeltaX == 0.0f &&
            m_fMouseDeltaY == 0.0f)
            return;

        Vector3 vLocalRotation = Head.transform.localRotation.eulerAngles;

        // Make rorations relevant to 180
        if (vLocalRotation.x > 180.0f)
            vLocalRotation.x -= 360.0f;

        if (vLocalRotation.y > 180.0f)
            vLocalRotation.y -= 360.0f;

        // Bound rotations to their limits
        vLocalRotation.x = Mathf.Clamp(vLocalRotation.x + m_fMouseDeltaY, k_fRotationXMin, k_fRotationXMax);
        vLocalRotation.y = Mathf.Clamp(vLocalRotation.y + m_fMouseDeltaX, -k_fRotationYLimit, k_fRotationYLimit);

        Head.transform.localEulerAngles = vLocalRotation;

        float fOverRotationX = vLocalRotation.x + m_fMouseDeltaY;
        float fOverRotationY = vLocalRotation.y + m_fMouseDeltaX;

        // Overflow rotation Y
        if (fOverRotationY < -k_fRotationYLimit)
        {
            if (EventRotationYOverflow != null) EventRotationYOverflow(fOverRotationY + k_fRotationYLimit);
        }
        else if (fOverRotationY > k_fRotationYLimit)
        {
            if (EventRotationYOverflow != null) EventRotationYOverflow(fOverRotationY - k_fRotationYLimit);
        }

        // Overflow rotation X
        if (fOverRotationX < k_fRotationXMin)
        {
            if (EventRotationXOverflow != null) EventRotationXOverflow(fOverRotationX - k_fRotationXMin);
        }
        else if (fOverRotationX > k_fRotationXMax)
        {
            if (EventRotationXOverflow != null) EventRotationXOverflow(fOverRotationX - k_fRotationXMax);
        }
    }


    void AlignHead()
    {
        Head.transform.localRotation = Quaternion.RotateTowards(Head.transform.localRotation,
                                                                Quaternion.Euler(m_fHeadEulerX.Value, m_fHeadEulerY.Value, 0.0f),
                                                                360.0f * Time.fixedDeltaTime);
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


    [AOwnerAndServerOnly]
    void OnEventAxisChange(CUserInput.EAxis _eAxis, float _fValue)
    {
        OnEventClientAxisChange(_eAxis, CNetwork.PlayerId, _fValue);
    }


    [AOwnerAndServerOnly]
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


    [AServerOnly]
    void OnEventMotorStateChange(CPlayerMotor.EState _ePrevious, CPlayerMotor.EState _eNew)
    {
        if (_eNew == CPlayerMotor.EState.AirThustersInSpace)
        {
            //Head.transform.localEulerAngles = Vector3.zero;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }


// Member Fields

    const float k_fRotationXMin   = -70; // Up
    const float k_fRotationXMax   =  54; // Down
    const float k_fRotationYLimit =  68; // Degrees


	List<Type> m_cInputDisableQueue = new List<Type>();
	
	
	CNetworkVar<float> m_fHeadEulerX = null;
    CNetworkVar<float> m_fHeadEulerY = null;


    float m_fMouseDeltaX = 0.0f;
    float m_fMouseDeltaY = 0.0f;
	float m_fLocalXRotation = 0.0f;


};
