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
		PowerGenerator,
		PowerCapacitor,
		AtmosphereGenerator,
		Dispenser,
		ShipPower,
		ShipPropulsion,
		ShipResources,
		NaniteCapsule,
		Engine,
        AirlockInternal,
        FacilityDoor,
		FacilityControl,
		NOSPanelWide,

		MAX
	}

	public enum EAspectRatio
	{
		Standard,
		Widescreen,
	}

	// Member Delegates & Events


    // Member Fields
	public GameObject m_DUICamera2D = null;
	public GameObject m_DUICamera3D = null;
	public EType m_DUIType = EType.INVALID;
	public EAspectRatio m_AspectRatio = EAspectRatio.Standard;

    private RenderTexture m_RenderTex = null; 
	private CNetworkVar<TNetworkViewId> m_ConsoleViewId = null;

	private UICamera m_Cached2DCamera = null;
	private UICamera m_Cached3DCamera = null;

	static float s_UIOffset = 0.0f;
	static Dictionary<CDUIRoot.EType, CGameRegistrator.ENetworkPrefab> s_RegisteredPrefabs = new Dictionary<CDUIRoot.EType, CGameRegistrator.ENetworkPrefab>();

	// Member Properties
	public TNetworkViewId ConsoleViewId 
	{ 
		get { return(m_ConsoleViewId.Get()); } 

		[AServerOnly]
		set { m_ConsoleViewId.Set(value); }
	}

	public GameObject Console 
	{ 
		get { return(m_ConsoleViewId.Get().GameObject); } 
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
		m_ConsoleViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
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

	public void Start()
	{
		if(CNetwork.IsServer)
		{
			// Offset its position
			gameObject.GetComponent<CNetworkView>().SetPosition(new Vector3(0.0f, 0.0f, s_UIOffset));
			gameObject.GetComponent<CNetworkView>().SetEulerAngles(Quaternion.identity.eulerAngles);

			// Increment the offset
			s_UIOffset += 10.0f;
		}

		if(m_DUICamera3D != null)
		{
			m_Cached3DCamera = m_DUICamera3D.GetComponent<UICamera>();
			m_Cached3DCamera.m_IsDUI = true;
		}
		
		if(m_DUICamera2D != null)
		{
			m_Cached2DCamera = m_DUICamera2D.GetComponent<UICamera>();
			m_Cached2DCamera.m_IsDUI = true;
		}
	}

	public void Update()
	{
		if(m_Cached3DCamera != null)
		{
			if(m_Cached3DCamera.enabled)
				m_Cached3DCamera.enabled = false;
		}

		if(m_Cached2DCamera != null)
		{
			if(m_Cached2DCamera.enabled) 
				m_Cached2DCamera.enabled = false;
		}
	}

	public void SetCamerasRenderingState(bool _Render)
	{
//		if(m_DUICamera2D != null)
//			m_DUICamera2D.camera.enabled = _Render;
//		
//		if(m_DUICamera3D != null)
//			m_DUICamera3D.camera.enabled = _Render;
	}

	public void UpdateCameraViewportPositions(Vector2 _screenTexCoord)
	{
		Vector3 viewPortPos = DUICameraViewportPos(_screenTexCoord);

		if(m_Cached2DCamera != null) 
		{
			m_Cached2DCamera.m_ViewPortPos = viewPortPos;
			m_Cached2DCamera.enabled = true;
		}
		if(m_Cached3DCamera != null) 
		{
			m_Cached3DCamera.m_ViewPortPos = viewPortPos;
			m_Cached3DCamera.enabled = true;
		}
	}
	
	private void AttatchRenderTexture(Material _ScreenMaterial)
	{
		// Set the render text onto the material of the screen
		_ScreenMaterial.SetTexture("_UI", m_RenderTex); 
	}
	
	private void SetupRenderTexture()
	{	
		int width = 0;
		int height = 0;

		if(m_AspectRatio == EAspectRatio.Standard)
		{
			width = 1280;
			height = 1024;
		}
		else if(m_AspectRatio == EAspectRatio.Widescreen)
		{
			width = 1280;
			height = 720;
		}
		
		// Create a new render texture
		m_RenderTex = new RenderTexture(width, height, 16);
		m_RenderTex.name = name + " RT";
		m_RenderTex.Create();
	}
	
	private void SetupUICameras()
	{
		UpdateCameraAspect();

		if(m_DUICamera2D != null)
		{
			m_DUICamera2D.camera.targetTexture = m_RenderTex;
		}

		if(m_DUICamera3D != null)
		{
			m_DUICamera3D.camera.targetTexture = m_RenderTex;
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

	public static void RegisterPrefab(EType _DUIType, CGameRegistrator.ENetworkPrefab _NetworkPrefab)
	{
		s_RegisteredPrefabs.Add(_DUIType, _NetworkPrefab);
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

	[ContextMenu("Update Camera Aspect")]
	public void UpdateCameraAspect()
	{
		// Get the aspect ratio
		float rtAspect = 0.0f;
		if(m_AspectRatio == EAspectRatio.Standard)
		{
			rtAspect = 1.333f;
		}
		else if(m_AspectRatio == EAspectRatio.Widescreen)
		{
			rtAspect = 16.0f / 9.0f;
		}

		// Apply this ratio to the cameras
		if(m_DUICamera2D != null)
		{
			m_DUICamera2D.camera.aspect = rtAspect;
		}

		if(m_DUICamera3D != null)
		{
			m_DUICamera3D.camera.aspect = rtAspect;
		}
	}
}
