//  Auckland
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


public class CDUI : MonoBehaviour
{
	// Member delegates
	public event Action<uint> SubviewChanged;
	
    // Member Fields
	public GameObject m_UICamera2D = null;
	public GameObject m_UICamera3D = null;

	private EQuality m_Quality = EQuality.VeryHigh;
	private GameObject m_Console = null;
    private RenderTexture m_RenderTex = null; 
	
	// Member Properties
	
	// Member Methods
	
    // Member Properties
	public GameObject Console 
	{ 
		get { return(m_Console); } 
		set { m_Console = value; } 
	}

	public GameObject UICamera2D 
	{ 
		get { return(m_UICamera2D); } 
	}

	public GameObject UICamera3D 
	{ 
		get { return(m_UICamera3D); } 
	}
	
    // Member Methods
    private void Update()
	{
//		// Render using the render cameras
//      UICamera2D.camera.Render();
//		UICamera3D.camera.Render();
    }

    public void SetupRenderTexture()
    {	
		// Figure out the pixels density for the screen based on quality setting
		float pd = 0.0f;
		switch (m_Quality)
		{
		case EQuality.VeryHigh: pd = 1000f; break;
		case EQuality.High: pd = 600.0f; break;
		case EQuality.Medium: pd = 350.0f; break;
		case EQuality.Low: pd = 200.0f; break;
		case EQuality.VeryLow: pd = 100.0f; break;
		default:break;
		}

		int width = (int)m_UICamera2D.camera.pixelWidth;
		int height = (int)m_UICamera2D.camera.pixelHeight;

		Debug.Log(width);
		Debug.Log(height);
		
		// Create a new render texture
		m_RenderTex = new RenderTexture(width, height, 16);
		m_RenderTex.name = name + " RT";
		m_RenderTex.Create();
    }

	public void SetupUICameras()
	{
		// Set the render target for the cameras
		m_UICamera2D.camera.targetTexture = m_RenderTex;
		m_UICamera3D.camera.targetTexture = m_RenderTex;

		// Set as DUI cameras
		m_UICamera2D.GetComponent<UICamera>().IsDUICamera = true;
		m_UICamera3D.GetComponent<UICamera>().IsDUICamera = true;
	}
	
    public void AttatchRenderTexture(Material _sharedScreenMat)
    {
        // Set the render text onto the material of the screen
        _sharedScreenMat.SetTexture("_MainTex", m_RenderTex); 
    }

	public void UpdateCameraViewportPositions(Vector2 _screenTexCoord)
	{
		Vector3 viewPortPos = DUICameraViewportPos(_screenTexCoord);

		UICamera2D.GetComponent<UICamera>().CurrentVeiwPortPos = viewPortPos;
		UICamera3D.GetComponent<UICamera>().CurrentVeiwPortPos = viewPortPos;
	}

	private Vector3 DUICameraViewportPos(Vector2 _screenTexCoord)
	{	
		Vector3 offset = new Vector3(_screenTexCoord.x * UICamera2D.camera.pixelWidth,
		                             _screenTexCoord.y * UICamera2D.camera.pixelHeight, 0.0f);
		
		offset = transform.rotation * offset;
		Vector3 rayOrigin = transform.position + offset + transform.forward * -1.0f;
		
		return(rayOrigin);
	}
	
	private void OnSubviewChange(uint _iActiveSubview)
	{
		if(SubviewChanged != null)
			SubviewChanged(_iActiveSubview);
	}
}
