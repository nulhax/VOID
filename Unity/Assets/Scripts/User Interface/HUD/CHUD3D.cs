//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDRoot.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CHUD3D : MonoBehaviour
{
	// Member Types
	

	// Member Delegates & Events
	
	
	// Member Fields
	public GameObject m_HUDCamera = null;

	private Transform m_CachedHUDCameraLeft = null;
	private Transform m_CachedHUDCameraRight = null;

	public CHUDToolTip m_ReticleOuter = null;

	public UILabel m_O2Value = null;
	public UIProgressBar m_02Bar = null;
	public UILabel m_Status = null;

	private Transform m_CachedReticlePanel = null;

	private bool m_HUDActive = false;


	// Member Properties
	public GameObject HUDCamera
	{
		get { return(m_HUDCamera); }
	}
	
	public Transform HUDCameraLeft
	{
		get { return(m_CachedHUDCameraLeft); }
	}
	
	public Transform HUDCameraRight
	{
		get { return(m_CachedHUDCameraRight); }
	}

	public bool IsHUDActive
	{
		get { return(m_HUDActive); }
	}
	
	// Member Methods
	public void Awake()
	{
		// Cache the left and right camera
		m_CachedHUDCameraLeft = m_HUDCamera.transform.FindChild("CameraLeft");
		m_CachedHUDCameraRight = m_HUDCamera.transform.FindChild("CameraRight");
	}

	public void Start()
	{
		// Set the locator on the players aiming position
		m_ReticleOuter.Target = CGamePlayers.SelfActorHead.transform.FindChild("Aim");

		if(CGameCameras.IsOculusRiftActive)
		{
			// Cache the reticle panel
			m_CachedReticlePanel = m_ReticleOuter.transform.parent;
			m_CachedReticlePanel.GetComponent<CHUDPanel>().ContinuouslyUpdateScale = true;
		}

		// Register events for UI activation from the visor
		CGameHud.Visor.EventVisorHUDActivated += OnActivateHUD;
		CGameHud.Visor.EventVisorHUDDeactivated += OnDeactivateHUD;
	}

	public void Update()
	{
		if(m_HUDActive)
		{
			UpdateUI();
		}
	}

	private void UpdateUI()
	{
		if(CGamePlayers.SelfActor == null)
		{
			return;
		}

		// Update reticle panel position for the rift
		if(CGameCameras.IsOculusRiftActive)
		{
			float fovCoefficient = CGameCameras.MainCameraLeft.camera.fieldOfView / CGameHud.Hud3D.HUDCameraLeft.camera.fieldOfView;
			
			if(CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().TargetActorObject != null)
			{
				RaycastHit rayHit = CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().TargetRaycastHit;
				
				// Update the reticle panel z position to be the same distance to the current target
				if(rayHit.distance < CPlayerInteractor.s_InteractionRange)
				{
					m_CachedReticlePanel.localPosition = Vector3.forward * rayHit.distance * fovCoefficient;
				}
				else
				{
					m_CachedReticlePanel.localPosition = Vector3.forward * CPlayerInteractor.s_InteractionRange * fovCoefficient;
				}
			}
			else
			{
				m_CachedReticlePanel.localPosition = Vector3.forward * CPlayerInteractor.s_InteractionRange * fovCoefficient;
			}
		}
		
		// Get the player oxygen supplu
		float oxygenSupply = CGamePlayers.SelfActor.GetComponent<CPlayerSuit>().OxygenSupply;
		float oxygenCapacity = CGamePlayers.SelfActor.GetComponent<CPlayerSuit>().OxygenCapacity;
		
		// Calculate the value ratio
		float value = oxygenSupply/oxygenCapacity;
		
		// Update the bar
		CDUIUtilites.LerpBarColor(value, m_02Bar);
		m_02Bar.value = value;
		
		// Update the label
		m_O2Value.color = CDUIUtilites.LerpColor(value);
		m_O2Value.text = Mathf.RoundToInt(oxygenSupply) + " / " + Mathf.RoundToInt(oxygenCapacity);
		
		// Update the status label
		if(value == 0.0f)
		{
			m_Status.color = Color.red;
			m_Status.text = "CRITICAL:\nOXYGEN DEPLETED!";
			
			m_Status.GetComponent<UITweener>().enabled = true;
		}
		else if(value < 0.20f)
		{
			m_Status.color = Color.red;
			m_Status.text = "Critical:\nLow Oxygen!";
			
			m_Status.GetComponent<UITweener>().enabled = true;
		}
		else if(value < 0.50f)
		{
			m_Status.color = Color.yellow;
			m_Status.text = "Warning:\nLow Oxygen!";
			
			if(m_Status.GetComponent<UITweener>().enabled)
			{
				m_Status.GetComponent<UITweener>().ResetToBeginning();
				m_Status.GetComponent<UITweener>().enabled = false;
			}
		}
		else
		{
			m_Status.text = "";
			
			if(m_Status.GetComponent<UITweener>().enabled)
			{
				m_Status.GetComponent<UITweener>().ResetToBeginning();
				m_Status.GetComponent<UITweener>().enabled = false;
			}
		}
	}

	private void OnActivateHUD()
	{
		m_HUDActive = true;

		// Play the tweeners
		gameObject.GetComponent<UIPlayTween>().playDirection = AnimationOrTween.Direction.Forward;
		gameObject.GetComponent<UIPlayTween>().Play(true);
	}

	private void OnDeactivateHUD()
	{
		m_HUDActive = false;

		// Play the tweeners
		gameObject.GetComponent<UIPlayTween>().playDirection = AnimationOrTween.Direction.Reverse;
		gameObject.GetComponent<UIPlayTween>().Play(true);
	}
}
