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
		m_fHeadEulerX = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
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
	public void SetHeadRotations(float _LocalEulerX)
	{
		m_fLocalXRotation = _LocalEulerX;

		// Apply the rotation
		Head.transform.localEulerAngles = new Vector3(m_fLocalXRotation, 0.0f, 0.0f);
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
        }

        if (CNetwork.IsServer)
        {
            // Register for mouse movement input
            CUserInput.SubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);
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
            CUserInput.UnsubscribeClientAxisChange(CUserInput.EAxis.MouseY, OnEventClientAxisChange);
        }
    }


    void Update()
    {
        // Empty
    }


    void FixedUpdate()
    {
        if (!InputDisabled &&
            m_fDeltaMouseX != 0.0f)
        {
            // Retrieve new rotations
            m_fLocalXRotation += m_fDeltaMouseX;

            // Clamp rotation
            m_fLocalXRotation = Mathf.Clamp(m_fLocalXRotation, m_vCameraMinRotation.x, m_vCameraMaxRotation.x);

            // Apply the pitch to the camera
            SetHeadRotations(m_fLocalXRotation);

            m_fDeltaMouseX = 0.0f;
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


    [AServerOnly]
    void OnEventClientAxisChange(CUserInput.EAxis _eAxis, ulong _ulPlayerId, float _fValue)
    {
        if (GetComponent<CPlayerInterface>().PlayerId == _ulPlayerId)
        {
            switch (_eAxis)
            {
                case CUserInput.EAxis.MouseY:
                    m_fDeltaMouseX += _fValue;
                    break;

                default:
                    Debug.LogError("Unknown client axis: " + _eAxis);
                    break;
            }
        }
	}


    void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    {
        // Head Rotation
        if (CGamePlayers.SelfActor != gameObject &&
            _cSyncedNetworkVar     == m_fHeadEulerX)
        {
            Head.transform.localEulerAngles = new Vector3(m_fHeadEulerX.Get(), 0.0f, 0.0f);
        }
    }


// Member Fields


	List<Type> m_cInputDisableQueue = new List<Type>();
	
	
	CNetworkVar<float> m_fHeadEulerX = null;
	
	
    float m_fDeltaMouseX    = 0.0f;
	float m_fLocalXRotation = 0.0f;
	
	
	Vector2 m_vCameraMinRotation = new Vector2(-50.0f, -360.0f);
	Vector2 m_vCameraMaxRotation = new Vector2(60.0f, 360.0f);
	Vector2 m_vHeadMinRotation = new Vector2(-30, -60);
	Vector2 m_vHeadMaxRotation = new Vector2(30, 70); 


};
