//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDPanel.cs
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


public class CHUDPanel : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private Camera m_CachedCamera = null;

	private bool m_ContinuouslyUpdateScale = false;


	// Member Properties
	public bool ContinuouslyUpdateScale
	{
		set { m_ContinuouslyUpdateScale = value; }
	}

	
	// Member Methods
	private void Start()
	{
		// Cache the HUD camera
		m_CachedCamera = CGameHUD.HUD3D.HUDCamera.camera;

		// Update the scale
		UpdateScale();
	}

	private void Update()
	{
		if(m_ContinuouslyUpdateScale)
		{
			UpdateScale();
		}
	}

	private void UpdateScale()
	{
		// Find the pixel size of this panel
		float zDistance = Mathf.Abs(transform.localPosition.z);
		float pixelSize = m_CachedCamera.pixelHeight / 2.0f / zDistance / Mathf.Tan(m_CachedCamera.fieldOfView / 2.0f * Mathf.Deg2Rad);
		
		// Adjust the local scale to be such
		transform.localScale = Vector3.one * 1.0f / pixelSize;
	}
}
