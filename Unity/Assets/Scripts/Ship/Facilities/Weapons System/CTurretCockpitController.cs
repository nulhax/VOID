//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTurretCockpitController.cs
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


[RequireComponent(typeof(CNetworkView))]
[RequireComponent(typeof(CCockpit))]
public class CTurretCockpitController : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		FireLasers
	}


// Member Delegates & Events


// Member Properties


	public GameObject TurretObject
	{
		get { return (CNetwork.Factory.FindObject(m_cTurretViewId.Get())); }
	}


// Member Methods
	

	public override void InstanceNetworkVars()
    {
		m_cTurretViewId = new CNetworkVar<ushort>(OnNetworkVarSync, 0);
    }

	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cTurretViewId)
		{
			UpdateActiveTurret();
		}
	}


	public void Start()
	{
		m_cCockpit = gameObject.GetComponent<CCockpit>();

		// Subscribe to cockpit events
		m_cCockpit.EventPlayerEnter += new CCockpit.HandlePlayerEnter(OnPlayerEnterCockpit);
		m_cCockpit.EventPlayerLeave += new CCockpit.HandlePlayerLeave(OnPlayerLeaveCockpit);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		CCockpit cCockpit = gameObject.GetComponent<CCockpit>();

		if (cCockpit.IsMounted &&
			cCockpit.ContainedPlayerActorViewId == CGame.PlayerActorViewId)
		{
			//if (Input.GetKeyDown(KeyCode.Space))
		}
	}


	[AClientMethod]
	void OnPlayerEnterCockpit(ushort _usEnteringPlayerActorViewId)
	{
		// Check the player was myself
		if (_usEnteringPlayerActorViewId == CGame.PlayerActorViewId)
		{
			// Subscribe to mouse input
			CGame.UserInput.EventMouseMoveX += new CUserInput.NotifyMouseInput(OnMouseMoveX);
			CGame.UserInput.EventMouseMoveY += new CUserInput.NotifyMouseInput(OnMouseMoveY);

			// Debug - Testing
			m_cTurretViewId.Set(5);
		}
	}


	[AClientMethod]
	void OnPlayerLeaveCockpit(ushort _usLeavingPlayerActorViewId)
	{
		if (_usLeavingPlayerActorViewId == CGame.PlayerActorViewId)
		{
			// Unsubscriber to mouse input
			CGame.UserInput.EventMouseMoveX -= new CUserInput.NotifyMouseInput(OnMouseMoveX);
			CGame.UserInput.EventMouseMoveY -= new CUserInput.NotifyMouseInput(OnMouseMoveY);
		}
	}


	[AClientMethod]
	void OnMouseMoveX(float _fAmount)
	{
		// Rotate turret around X
		TurretObject.GetComponent<CTurretController>().RotateX(_fAmount);
	}


	[AClientMethod]
	void OnMouseMoveY(float _fAmount)
	{
		// Rotate turret around Y
		TurretObject.GetComponent<CTurretController>().RotateY(_fAmount);
	}


	[AClientMethod]
	void UpdateActiveTurret()
	{
		if (m_cCockpit.ContainedPlayerActorViewId == CGame.PlayerActorViewId)
		{
			// Set the turret camera to enabled
			TurretObject.transform.FindChild("TurretBarrels").FindChild("TurretCamera").camera.enabled = true;
		}
	}


// Member Fields


	CNetworkVar<ushort> m_cTurretViewId = null;


	CCockpit m_cCockpit = null;


	Vector3 m_vRotation = Vector3.zero;
	Vector2 m_vMinRotationY = new Vector2(-50.0f, -360.0f);
	Vector2 m_vMaxRotationY = new Vector2( 60.0f,  360.0f);
	Vector2 m_vMinRotationX = new Vector2( -80, -60);
	Vector2 m_vMaxRotationX = new Vector2( 0,  70);


	float m_fFireTimer		= 0.0f;
	float m_fFireInterval	= 0.2f;


	bool m_bUpdateRotation = false;



};
