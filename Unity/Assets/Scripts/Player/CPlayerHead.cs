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

	public bool InputFrozen
	{
		set { m_bInputFrozen.Set(value); }
		get { return (m_bInputFrozen.Get()); }
	}


// Member Methods


    public override void InstanceNetworkVars()
    {
		m_fHeadEulerX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_bInputFrozen = new CNetworkVar<bool>(OnNetworkVarSync, false);
    }
	
	
    public void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    {
		// Head Rotation
		if (CGame.PlayerActor != gameObject &&
			_cSyncedNetworkVar == m_fHeadEulerX)
	    {	
	        m_cActorHead.transform.eulerAngles = new Vector3(m_fHeadEulerX.Get(), 0.0f, 0.0f);
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
			CGame.Ship.GetComponent<CShipGalaxySimulatior>().AddPlayerActorGalaxyCamera(m_cShipCamera);
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
		_cStream.Write(cMyActorHead.m_vRotation.x);
	}


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		// Retrieve player actors head
		CPlayerHead cMyActorHead = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerHead>();

		// Write my head's x-rotation
		float fRotationX = _cStream.ReadFloat();

		cMyActorHead.m_fHeadEulerX.Set(fRotationX);
	}
	

    void UpdateInput()
	{
		// Retrieve new rotations
		m_vRotation.x += Input.GetAxis("Mouse Y") * m_fSensitivityX * -1.0f;
		m_vRotation.y += Input.GetAxis("Mouse X") * m_fSensitivityY;

		// Keep y rotation within 360 range
		m_vRotation.y -= (m_vRotation.y >=  360.0f) ? 360.0f : 0.0f;
		m_vRotation.y += (m_vRotation.y <= -360.0f) ? 360.0f : 0.0f;

		// Clamp rotation
		m_vRotation.x = Mathf.Clamp(m_vRotation.x, m_vCameraMinRotation.x, m_vCameraMaxRotation.x);
		m_vRotation.y = Mathf.Clamp(m_vRotation.y, m_vCameraMinRotation.y, m_vCameraMaxRotation.y);

		// Apply the pitch to the camera
		transform.localEulerAngles = new Vector3(0.0f, m_vRotation.y, 0.0f);
		m_cActorHead.transform.localEulerAngles = new Vector3(m_vRotation.x, 0.0f, 0.0f);
	}


// Member Fields


	CNetworkVar<float> m_fHeadEulerX = null;
	CNetworkVar<bool> m_bInputFrozen = null;


	public GameObject m_cActorHead = null;
	GameObject m_cShipCamera = null;
	Vector3 m_vRotation = Vector3.zero;
	Vector2 m_vCameraMinRotation = new Vector2(-50.0f, -360.0f); 
	Vector2 m_vCameraMaxRotation = new Vector2( 60.0f,  360.0f); 
	Vector2 m_vHeadMinRotation = new Vector2(-30, -60); 
	Vector2 m_vHeadMaxRotation = new Vector2( 30,  70); 


	float m_fSensitivityX = 10.0f;
	float m_fSensitivityY = 10.0f;


};
