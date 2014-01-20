﻿//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
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

[RequireComponent(typeof(CNetworkView))]
public class CDUI : CNetworkMonoBehaviour
{
	// Member Types


	// Member Delegates & Events


    // Member Fields
	public GameObject m_DUICamera2D = null;
	public GameObject m_DUICamera3D = null;

    private RenderTexture m_RenderTex = null; 

	private CNetworkVar<CNetworkViewId> m_ConsoleViewId = null;


	// Member Properties
	public CNetworkViewId ConsoleViewId 
	{ 
		get { return(m_ConsoleViewId.Get()); } 

		[AServerOnly]
		set { m_ConsoleViewId.Set(value); }
	}

	public GameObject Console 
	{ 
		get { return(CNetwork.Factory.FindObject(m_ConsoleViewId.Get())); } 
	}

	public GameObject DUICamera2D 
	{ 
		get { return(m_DUICamera2D); } 
	}

	public GameObject DUICamera3D 
	{ 
		get { return(m_DUICamera3D); } 
	}


    // Member Methods
	public override void InstanceNetworkVars()
	{
		m_ConsoleViewId = new CNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
	}
	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if(_cSyncedVar == m_ConsoleViewId)
		{
			// Remake the render texture and assign cameras
			SetupRenderTexture();
			SetupUICameras();

			// Attach the camera to the consoles screen
			AttatchRenderTexture(Console.GetComponent<CDUIConsole>().ConsoleScreen.renderer.material);
		}
	}

    public void Update()
	{
//		// Render using the render cameras
//      UICamera2D.camera.Render();
//		UICamera3D.camera.Render();
    }

	public void UpdateCameraViewportPositions(Vector2 _screenTexCoord)
	{
		Vector3 viewPortPos = DUICameraViewportPos(_screenTexCoord);

		if(DUICamera2D != null) 
			DUICamera2D.GetComponent<UICamera>().CurrentVeiwPortPos = viewPortPos;
		if(DUICamera3D != null) 
			DUICamera3D.GetComponent<UICamera>().CurrentVeiwPortPos = viewPortPos;
	}
	
	private void AttatchRenderTexture(Material _ScreenMaterial)
	{
		// Set the render text onto the material of the screen
		_ScreenMaterial.SetTexture("_MainTex", m_RenderTex); 
	}
	
	private void SetupRenderTexture()
	{	
		int width = (int)m_DUICamera2D.camera.pixelWidth;
		int height = (int)m_DUICamera2D.camera.pixelHeight;
		
		// Create a new render texture
		m_RenderTex = new RenderTexture(width, height, 16);
		m_RenderTex.name = name + " RT";
		m_RenderTex.Create();
	}
	
	private void SetupUICameras()
	{
		if(m_DUICamera2D != null)
		{
			m_DUICamera2D.camera.targetTexture = m_RenderTex;
			m_DUICamera2D.GetComponent<UICamera>().IsDUICamera = true;
		}

		if(m_DUICamera3D != null)
		{
			m_DUICamera3D.camera.targetTexture = m_RenderTex;
			m_DUICamera3D.GetComponent<UICamera>().IsDUICamera = true;
		}
	}
	
	private Vector3 DUICameraViewportPos(Vector2 _screenTexCoord)
	{	
		Vector3 offset = new Vector3(_screenTexCoord.x * DUICamera2D.camera.pixelWidth,
		                             _screenTexCoord.y * DUICamera2D.camera.pixelHeight, 0.0f);
		
		offset = transform.rotation * offset;
		Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;
		
		return(rayOrigin);
	}
}