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
	
	public bool InputFrozen
	{
		set { m_bInputFrozen = value; }
		get { return (m_bInputFrozen); }
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
		// Add components for owned actor
		if(CGame.PlayerActor == gameObject)
		{
			// Disable any main camera currently rendering
			GameObject.Find("Main Camera").camera.enabled = false;
			
			// Add the ship camera to the actor observing the ship
			m_cShipCamera = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Player/Cameras/PlayerShipCamera"));
			m_cShipCamera.transform.parent = ActorHead.transform;
			m_cShipCamera.transform.localPosition = Vector3.zero;
			m_cShipCamera.transform.localRotation = Quaternion.identity;
			
			// Add the galaxy camera to the ship galaxy simulator
			CGame.Ship.GetComponent<CShipGalaxySimulatior>().AddPlayerActorGalaxyCamera();
			
			// Register event handler for entering/exiting ship
			gameObject.GetComponent<CDynamicActor>().EventEnteredShip += new CDynamicActor.EnterExitShipHandler(PlayerActorEnteredShip);
			gameObject.GetComponent<CDynamicActor>().EventExitedShip += new CDynamicActor.EnterExitShipHandler(PlayerActorExitedShip);
		}
	}


    public void Update()
    {	
		// Only update input for client
		if(!InputFrozen &&
		   CGame.PlayerActor == gameObject)
		{
			UpdateInput();
		}
    }


	public static void SerializePlayerState(CNetworkStream _cStream)
	{
		// Retrieve my actors head
		CPlayerHead cMyActorHead = CGame.PlayerActor.GetComponent<CPlayerHead>();

		// Write my head's x-rotation
		_cStream.Write(cMyActorHead.transform.localEulerAngles.x);
	}


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		// Retrieve player actors head
		CPlayerHead cMyActorHead = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerHead>();

		// Write my head's x-rotation
		float fRotationX = _cStream.ReadFloat();

		cMyActorHead.m_fHeadEulerX.Set(fRotationX);
	}
	

    private void UpdateInput()
	{
		// Retrieve new rotations
		m_vRotation.x = Input.GetAxis("Mouse Y") * m_fSensitivityX * -1.0f;
		m_vRotation.y = Input.GetAxis("Mouse X") * m_fSensitivityY;

		// Apply yaw to the actor
		transform.Rotate(0.0f, m_vRotation.y, 0.0f, Space.Self);
		
		// Apply the pitch to the camera
		m_cActorHead.transform.Rotate(m_vRotation.x, 0.0f, 0.0f, Space.Self);
		
		// Clamp the head rotation
		float clampedHeadX = m_cActorHead.transform.localEulerAngles.x;
		if(clampedHeadX > 180.0f)
			clampedHeadX = clampedHeadX - 360.0f;
		clampedHeadX = Mathf.Clamp(clampedHeadX, -m_HeadYRotationLimit, m_HeadYRotationLimit);
		
		// Apply the clamp
		m_cActorHead.transform.localEulerAngles = new Vector3(clampedHeadX, m_cActorHead.transform.localEulerAngles.y, m_cActorHead.transform.localEulerAngles.z);
	}
	
	private void PlayerActorEnteredShip()
	{
		CShipGalaxySimulatior shipGalaxySim = CGame.Ship.GetComponent<CShipGalaxySimulatior>();
		GameObject playerGalaxyCamera = shipGalaxySim.PlayerGalaxyCamera;
	
		if(playerGalaxyCamera.transform.parent == ActorHead.transform)
		{
			// Swap the cameras parenthood
			playerGalaxyCamera.transform.parent = shipGalaxySim.gameObject.transform;
			PlayerShipCamera.transform.parent = ActorHead.transform;
			
			// Update the transform of the player ship camera
			PlayerShipCamera.transform.localPosition = Vector3.zero;
			PlayerShipCamera.transform.localRotation = Quaternion.identity;
		}
	}
	
	private void PlayerActorExitedShip()
	{
		CShipGalaxySimulatior shipGalaxySim = CGame.Ship.GetComponent<CShipGalaxySimulatior>();
		GameObject playerGalaxyCamera = shipGalaxySim.PlayerGalaxyCamera;
	
		// Swap the cameras parenthood
		playerGalaxyCamera.transform.parent = ActorHead.transform;
		PlayerShipCamera.transform.parent = shipGalaxySim.gameObject.transform;
		
		// Update the transform of the player galaxy camera
		playerGalaxyCamera.transform.localPosition = Vector3.zero;
		playerGalaxyCamera.transform.localRotation = Quaternion.identity;
	}

// Member Fields
	CNetworkVar<float> m_fHeadEulerX = null;
	
	
	bool m_bInputFrozen = false;


	public GameObject m_cActorHead = null;
	GameObject m_cShipCamera = null;
	Vector3 m_vRotation = Vector3.zero;
	
	
	float m_HeadYRotationLimit = 80.0f;


	float m_fSensitivityX = 10.0f;
	float m_fSensitivityY = 10.0f;


};
