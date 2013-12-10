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


public class CTurretCockpitController : CNetworkMonoBehaviour
{

// Member Types


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
		gameObject.GetComponent<CCockpit>().EventPlayerEnter += new CCockpit.HandlePlayerEnter(OnPlayerEnterCockpit);
		gameObject.GetComponent<CCockpit>().EventPlayerLeave += new CCockpit.HandlePlayerLeave(OnPlayerLeaveCockpit);


		s_cRenderTexture = new RenderTexture(Screen.width, Screen.height, 32);
		s_cRenderTexture.name = "TurretRenderTexture";
		s_cRenderTexture.Create();


		s_cOverlayTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);

		for (int x = 0; x < Screen.width; ++ x)
		{
			for (int y = 0; y < Screen.height; ++y)
			{
				s_cOverlayTexture.SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f, 0.90f));
			}
		}

		s_cOverlayTexture.Apply();
	}


	public void OnDestroy()
	{
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
	void OnPlayerEnterCockpit()
	{
		if (gameObject.GetComponent<CCockpit>().ContainedPlayerActorViewId == CGame.PlayerActorViewId)
		{
			CGame.UserInput.EventMouseMoveX += new CUserInput.NotifyMouseInput(OnMouseMoveX);
			CGame.UserInput.EventMouseMoveY += new CUserInput.NotifyMouseInput(OnMouseMoveY);

			m_bInCockpit = true;

			m_cTurretViewId.Set(5);
		}
	}


	[AClientMethod]
	void OnPlayerLeaveCockpit()
	{
		if (m_bInCockpit)
		{
			CGame.UserInput.EventMouseMoveX -= new CUserInput.NotifyMouseInput(OnMouseMoveX);
			CGame.UserInput.EventMouseMoveY -= new CUserInput.NotifyMouseInput(OnMouseMoveY);

			m_bInCockpit = false;
		}
	}


	[AClientMethod]
	void OnMouseMoveX(float _fAmount)
	{
		m_vRotation.y += _fAmount;

		// Keep y rotation within 360 range
		m_vRotation.y -= (m_vRotation.y >= 360.0f) ? 360.0f : 0.0f;
		m_vRotation.y += (m_vRotation.y <= -360.0f) ? 360.0f : 0.0f;

		// Clamp rotation
		m_vRotation.y = Mathf.Clamp(m_vRotation.y, m_vMinRotationY.y, m_vMaxRotationY.y);

		TurretObject.transform.localEulerAngles = new Vector3(0.0f, m_vRotation.y, 0.0f);
	}


	[AClientMethod]
	void OnMouseMoveY(float _fAmount)
	{
		// Retrieve new rotations
		m_vRotation.x += _fAmount;

		// Clamp rotation
		m_vRotation.x = Mathf.Clamp(m_vRotation.x, m_vMinRotationX.x, m_vMaxRotationX.x);

		// Apply the pitch to the camera
		TurretObject.transform.FindChild("TurretBarrels").localEulerAngles = new Vector3(m_vRotation.x, 0.0f, 0.0f);
	}


	[AClientMethod]
	void UpdateActiveTurret()
	{
		if (m_bInCockpit)
		{
			CNetwork.Factory.FindObject(m_cTurretViewId.Get()).GetComponent<CTurretController>().TurretCamera.camera.targetTexture = s_cRenderTexture;
		}
	}


	[AClientMethod]
	void OnGUI()
	{
		if (m_bInCockpit)
		{
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), s_cOverlayTexture, ScaleMode.StretchToFill, true);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), s_cRenderTexture, ScaleMode.StretchToFill, true);
		}
	}


// Member Fields


	CNetworkVar<ushort> m_cTurretViewId = null;


	Vector3 m_vRotation = Vector3.zero;
	Vector2 m_vMinRotationY = new Vector2(-50.0f, -360.0f);
	Vector2 m_vMaxRotationY = new Vector2( 60.0f,  360.0f);
	Vector2 m_vMinRotationX = new Vector2( -80, -60);
	Vector2 m_vMaxRotationX = new Vector2( 0,  70); 


	bool m_bInCockpit = false;
	bool m_bUpdateRotation = false;


	static RenderTexture s_cRenderTexture = null;
	static Texture2D s_cOverlayTexture = null;



};
