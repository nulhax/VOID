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

	public GameObject ActorHead
	{
		get { return (m_cActorHead); }
	}
	
	public GameObject PlayerShipCamera
	{
		get { return (m_cShipCamera); }
		set { m_cShipCamera = value; }
	}
	
	public bool CamerasSwapped
	{
		get { return(PlayerShipCamera.transform.parent != ActorHead.transform); }
	}

	public bool InputDisabled
	{
		get { return (m_cInputDisableQueue.Count > 0); }
	}


// Member Methods


    public override void InstanceNetworkVars()
    {
		m_fHeadEulerX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }
	
	
    public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    {
		// Head Rotation
		if (CGame.PlayerActor != gameObject && 
			_cSyncedNetworkVar == m_fHeadEulerX)
	    {	
	        m_cActorHead.transform.localEulerAngles = new Vector3(m_fHeadEulerX.Get(), 0.0f, 0.0f);
	    }
    }
	

	public void Start()
	{	
		if(CGame.PlayerActor == gameObject)
		{
			// Set the ship view perspective of the camera to the actors head
			TransferPlayerPerspectiveToShipSpace();
			
			// Register event handler for entering/exiting ship
			gameObject.GetComponent<CActorBoardable>().EventBoard += new CActorBoardable.BoardingHandler(TransferPlayerPerspectiveToShipSpace);
			gameObject.GetComponent<CActorBoardable>().EventDisembark += new CActorBoardable.BoardingHandler(TransferPlayerPerspectiveToGalaxySpace);

			// Subscribe to mouse movement input
			CGame.UserInput.EventMouseMoveY += new CUserInput.NotifyMouseInput(OnMouseMoveY);
		}
	}


    public void Update()
    {
		// Empty
    }


	[AServerMethod]
	public void DisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Add(_cFreezeRequester.GetType());
	}


	[AServerMethod]
	public void UndisableInput(object _cFreezeRequester)
	{
		m_cInputDisableQueue.Remove(_cFreezeRequester.GetType());
	}


	public static void SerializePlayerState(CNetworkStream _cStream)
	{
		if (CGame.PlayerActor != null)
		{
			// Retrieve my actors head
			CPlayerHead cMyActorHead = CGame.PlayerActor.GetComponent<CPlayerHead>();

			// Write my head's x-rotation
			_cStream.Write(cMyActorHead.transform.localEulerAngles.x);
		}
	}


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		// Retrieve player actors head
		CPlayerHead cMyActorHead = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerHead>();

		// Write my head's x-rotation
		float fRotationX = _cStream.ReadFloat();

		cMyActorHead.m_fHeadEulerX.Set(fRotationX);
	}


	private void TransferPlayerPerspectiveToShipSpace()
	{
		CGame.CompositeCameraSystem.SetShipViewPerspective(m_cActorHead.transform);

		// Remove the galaxy observer component
		Destroy(gameObject.GetComponent<GalaxyObserver>());
	}
	
	private void TransferPlayerPerspectiveToGalaxySpace()
	{
		CGame.CompositeCameraSystem.SetGalaxyViewPerspective(m_cActorHead.transform);

		// Add the galaxy observer component
		gameObject.AddComponent<GalaxyObserver>();
	}


	private void OnMouseMoveY(float _fAmount)
	{
		if (!InputDisabled)
		{
			// Retrieve new rotations
			m_vRotation.x += _fAmount;

			// Clamp rotation
			m_vRotation.x = Mathf.Clamp(m_vRotation.x, m_vCameraMinRotation.x, m_vCameraMaxRotation.x);

			// Apply the pitch to the camera
			m_cActorHead.transform.localEulerAngles = new Vector3(m_vRotation.x, 0.0f, 0.0f);
		}
	}


// Member Fields


	List<Type> m_cInputDisableQueue = new List<Type>();


	CNetworkVar<float> m_fHeadEulerX = null;


	public GameObject m_cActorHead = null;
	GameObject m_cShipCamera = null;
	Vector3 m_vRotation = Vector3.zero;
	Vector2 m_vCameraMinRotation = new Vector2(-50.0f, -360.0f);
	Vector2 m_vCameraMaxRotation = new Vector2(60.0f, 360.0f);
	Vector2 m_vHeadMinRotation = new Vector2(-30, -60);
	Vector2 m_vHeadMaxRotation = new Vector2(30, 70); 
	
	
	float m_HeadYRotationLimit = 80.0f;


};
