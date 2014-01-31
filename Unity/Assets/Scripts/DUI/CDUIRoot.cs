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

[RequireComponent(typeof(CNetworkView))]
public class CDUIRoot : CNetworkMonoBehaviour
{
	// Member Types
	public enum EType
	{
		INVALID,

		ControlsTest,
		FacilityExpansion,
		ModuleCreation,

		MAX
	}

	// Member Delegates & Events


    // Member Fields
	public GameObject m_DUICamera2D = null;
	public GameObject m_DUICamera3D = null;
	public EType m_DUIType = EType.INVALID;
	public Vector2 m_RenderTexSize = Vector2.zero;

    private RenderTexture m_RenderTex = null; 

	private CNetworkVar<CNetworkViewId> m_ConsoleViewId = null;

	static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_RegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();

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
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_ConsoleViewId = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
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

	public void Update2()
	{
		if(m_DUICamera3D != null)
		{
			if(m_DUICamera3D.GetComponent<UICamera>().enabled)
				m_DUICamera3D.GetComponent<UICamera>().enabled = false;

			RenderTexture.active = m_DUICamera3D.camera.targetTexture;
		}

		if(m_DUICamera2D != null)
		{
			if(m_DUICamera2D.GetComponent<UICamera>().enabled) 
				m_DUICamera2D.GetComponent<UICamera>().enabled = false;
		}

		Camera firstCamera = null;
		Camera secondCamera = null;

		if(m_DUICamera3D != null && m_DUICamera2D != null)
		{
			firstCamera = m_DUICamera3D.camera.depth > m_DUICamera2D.camera.depth ? m_DUICamera2D.camera : m_DUICamera3D.camera;
			secondCamera = firstCamera == m_DUICamera3D.camera ? m_DUICamera2D.camera : m_DUICamera3D.camera;
		}
		else if(m_DUICamera2D != null)
		{
			firstCamera = m_DUICamera2D.camera;
		}
		else if(m_DUICamera3D != null)
		{
			firstCamera = m_DUICamera3D.camera;
		}

		RenderTexture temp = RenderTexture.active;
		RenderTexture.active = m_RenderTex;

		if(firstCamera != null)
			firstCamera.Render();

		if(secondCamera != null)
			secondCamera.Render();

		RenderTexture.active = temp;
	}

	public void UpdateCameraViewportPositions(Vector2 _screenTexCoord)
	{
		UICamera current = DUICamera2D.GetComponent<UICamera>();
		current.enabled = true;

		Vector3 viewPortPos = DUICameraViewportPos(_screenTexCoord);

		if(m_DUICamera2D != null) 
		{
			m_DUICamera2D.GetComponent<UICamera>().m_ViewPortPos = viewPortPos;
			m_DUICamera2D.GetComponent<UICamera>().enabled = true;
		}
		if(m_DUICamera3D != null) 
		{
			m_DUICamera3D.GetComponent<UICamera>().m_ViewPortPos = viewPortPos;
			m_DUICamera3D.GetComponent<UICamera>().enabled = true;
		}
	}

	public static void RegisterPrefab(EType _DUIType, CGameRegistrator.ENetworkPrefab _Prefab)
	{
		s_RegisteredPrefabs.Add(_DUIType, _Prefab);
	}

	public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _DUIType)
	{
		if (!s_RegisteredPrefabs.ContainsKey(_DUIType))
		{
			Debug.LogError(string.Format("DUI type ({0}) has not been registered a prefab", _DUIType));
			
			return (CGameRegistrator.ENetworkPrefab.INVALID);
		}
		
		return (s_RegisteredPrefabs[_DUIType]);
	}
	
	private void AttatchRenderTexture(Material _ScreenMaterial)
	{
		// Set the render text onto the material of the screen
		_ScreenMaterial.SetTexture("_MainTex", m_RenderTex); 
	}
	
	private void SetupRenderTexture()
	{	
		int width = (int)m_RenderTexSize.x;
		int height = (int)m_RenderTexSize.y;
		
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
			m_DUICamera2D.camera.enabled = false;
		}

		if(m_DUICamera3D != null)
		{
			m_DUICamera3D.camera.targetTexture = m_RenderTex;
			m_DUICamera3D.camera.enabled = false;
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
