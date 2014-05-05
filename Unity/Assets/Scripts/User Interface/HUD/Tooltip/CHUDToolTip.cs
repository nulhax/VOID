//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDLocator.cs
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


public class CHUDToolTip : MonoBehaviour
{
	// Member Types
	public enum EDominantEye
	{
		BOTH,
		RIGHT,
		LEFT
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	public GameObject m_ToolTip = null;
	public EDominantEye m_OVRDominantEye = EDominantEye.BOTH;

	public float m_MaxDistance = 0.9f;

	protected Transform m_Target = null;
	protected bool m_OutsideBounds = false;

	protected Transform m_GameCamera = null;
	protected Camera m_HUDCamera = null;


	// Member Properties
	public Transform Target
	{
		set { m_Target = value; }
	}
	
	public bool IsOutsideBounds
	{
		get { return(m_OutsideBounds); }
	}
	
	// Member Methods
	protected void Start()
	{
		// Default in bounds
		EnteredBounds();
	}
	
	protected void LateUpdate()
	{
		// Ensure there is a target
		if(m_Target == null)
			return;

		// Update the selected cameras
		UpdateSelectedCameras();

		// Update the tooltip position and rotation
		UpdateToolTipTracking();
	}

	protected void UpdateSelectedCameras()
	{
		// Figure out which camera to use
		// Use main camera if the observer and target are both inside or both outside the ship.
		bool useMainCamera = CGameCameras.IsObserverInsideShip == (((1 << m_Target.gameObject.layer) & CGalaxy.layerBit_All) == 0);
		
		// Select the camera depending on which layer the target is
		Transform gameCamera = null;
		Camera HUDCamera = null;

		// If using the rift, need to take into consideration eye dominance
		if(CGameCameras.IsOculusRiftActive && m_OVRDominantEye != EDominantEye.BOTH)
		{
			if(m_OVRDominantEye == EDominantEye.RIGHT)
			{
				gameCamera = useMainCamera ? CGameCameras.MainCameraRight : CGameCameras.ProjectedCameraRight;
				HUDCamera = CGameHUD.HUD3D.HUDCameraRight.camera;
			}
			else if(m_OVRDominantEye == EDominantEye.LEFT)
			{
				gameCamera = useMainCamera ? CGameCameras.MainCameraLeft : CGameCameras.ProjectedCameraLeft;
				HUDCamera = CGameHUD.HUD3D.HUDCameraLeft.camera;
			}
		}
		else
		{
			gameCamera = useMainCamera ? CGameCameras.MainCamera.transform : CGameCameras.ProjectedCamera.transform;
			HUDCamera = CGameHUD.HUD3D.HUDCamera.camera;
		}

		m_HUDCamera = HUDCamera;
		m_GameCamera = gameCamera;
	}

	protected void UpdateToolTipTracking()
	{
		// Get the position from the world - viewport
		Vector3 pos = m_GameCamera.camera.WorldToViewportPoint(m_Target.position);
		
		// Set this 0, 1 space to -1 1 space to check bounds.
		Vector2 outsideTest = Vector2.zero;
		outsideTest.x = (pos.x * 2.0f) - 1.0f;
		outsideTest.y = (pos.y * 2.0f) - 1.0f;
		
		// Check the bounds
		if(outsideTest.sqrMagnitude > (m_MaxDistance * m_MaxDistance) || pos.z < 0.0f)
		{
			if(!m_OutsideBounds)
			{
				LeftBounds();
				m_OutsideBounds = true;
			}
		}
		else
		{
			if(m_OutsideBounds)
			{
				EnteredBounds();
				m_OutsideBounds = false;
			}
		}

		// Update the tooltip position within the HUD
		if(!m_OutsideBounds)
		{
			// Set the z to be appropriate pos away from the camera
			pos.z = transform.parent.localPosition.z;
			
			// Set the position
			transform.position = m_HUDCamera.ViewportToWorldPoint(pos);
			
			// Zero the z component
			pos = transform.localPosition;
			pos.z = 0f;
			transform.localPosition = pos;
		}
	}
	
	protected virtual void LeftBounds()
	{
		if(m_ToolTip != null)
			m_ToolTip.SetActive(false);
	}
	
	protected virtual void EnteredBounds()
	{
		if(m_ToolTip != null)
			m_ToolTip.SetActive(true);
	}
}
