//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCompoundCameraSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CGameCameras : MonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public bool m_UseOculusRift = false;

	private static GameObject s_MainCamera = null;
	private static GameObject s_ProjectedCamera = null;
	private static GameObject s_HUDCamera = null;

	private static bool s_IsObserverInsideShip = false;

	private static bool s_OculusRiftActive = false;

	private static Transform s_CachedMainCameraLeft = null;
	private static Transform s_CachedMainCameraRight = null;

	private static Transform s_CachedProjectedCameraLeft = null;
	private static Transform s_CachedProjectedCameraRight = null;

	private static Transform s_CachedHUDCameraLeft = null;
	private static Transform s_CachedHUDCameraRight = null;

	private static CGameCameras s_Instance = null;


	// Member Properties
	public static CGameCameras Instance
	{
		get { return(s_Instance); }
	}
	
	public static GameObject MainCamera
	{
		get { return(s_MainCamera); }
	}

	public static Transform MainCameraLeft
	{
		get { return(s_CachedMainCameraLeft); }
	}

	public static Transform MainCameraRight
	{
		get { return(s_CachedMainCameraRight); }
	}

	public static GameObject ProjectedCamera
	{
		get { return(s_ProjectedCamera);}
	}
	
	public static Transform ProjectedCameraLeft
	{
		get { return(s_CachedProjectedCameraLeft); }
	}
	
	public static Transform ProjectedCameraRight
	{
		get { return(s_CachedProjectedCameraRight); }
	}

	public static GameObject HUDCamera
	{
		get { return(s_HUDCamera); }
	}

	public static Transform HUDCameraLeft
	{
		get { return(s_CachedHUDCameraLeft); }
	}

	public static Transform HUDCameraRight
	{
		get { return(s_CachedHUDCameraRight); }
	}

	public static bool IsObserverInsideShip
	{
		get { return(s_IsObserverInsideShip); }
	}

	public static bool IsOculusRiftActive
	{
		get { return(s_OculusRiftActive); }
	}
	
	// Member Methods
	public void Start()
	{	
		s_Instance = this;

		CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnDisconnect);
	}
	
	public void Update()
	{
		if(s_OculusRiftActive)
		{
			if(Input.GetKeyDown(KeyCode.F2))
			{
				for(int i = 0; i < OVRDevice.SensorCount; ++i)
					OVRDevice.ResetOrientation(i);
			}
		}
	}
	
	public void LateUpdate()
	{
		// Make sure the client has the most up to date information
		if(!CNetwork.Connection.IsDownloadingInitialGameData &&
		   CNetwork.Connection.IsConnected)
		{
			// Update the transforms of the cameras
			UpdateCameraTransforms();
		}
	}

	public static void SetupCameras()
	{
		// If wanting to use oculus, check if the device is intialised
		if(s_Instance.m_UseOculusRift)
		{
			s_OculusRiftActive = OVRDevice.IsInitialized() && OVRDevice.SensorCount > 0;
		}
		
		// Instantiate the head camera
		if(s_OculusRiftActive)
		{
			s_MainCamera = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/CameraOVR"));
			s_MainCamera.name = "Camera_MainOVR";

			// Set the main camera parented to the player
			s_MainCamera.transform.parent = CGamePlayers.SelfActor.transform;

			// Set the rift to track the body rotations
			s_MainCamera.GetComponent<OVRCameraController>().FollowOrientation = CGamePlayers.SelfActorHead.transform;

			// Cache the left and right camera
			s_CachedMainCameraLeft = s_MainCamera.transform.FindChild("CameraLeft");
			s_CachedMainCameraRight = s_MainCamera.transform.FindChild("CameraRight");

			// Instantiate the projected camera (copy from head camera)
			s_ProjectedCamera = (GameObject)GameObject.Instantiate(s_MainCamera); 
			s_ProjectedCamera.name = s_ProjectedCamera.name = "Camera_ProjectedOVR";

			// Cache the left and right camera
			s_CachedProjectedCameraLeft = s_ProjectedCamera.transform.FindChild("CameraLeft");
			s_CachedProjectedCameraRight = s_ProjectedCamera.transform.FindChild("CameraRight");

			// Set to not update orientation automatically
			s_CachedProjectedCameraLeft.GetComponent<OVRCamera>().m_DontUpdateOrientation = true;
			s_CachedProjectedCameraRight.GetComponent<OVRCamera>().m_DontUpdateOrientation = true;
		}
		else
		{
			s_MainCamera = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/Camera"));
			s_MainCamera.name = "Camera_Main";

			// Set the main camera parented to the players head
			s_MainCamera.transform.parent = CGamePlayers.SelfActorHead.transform;

			// Instantiate the projected camera (copy from head camera)
			s_ProjectedCamera = (GameObject)GameObject.Instantiate(s_MainCamera); 
			s_ProjectedCamera.name = s_ProjectedCamera.name = "Camera_Projected";
		}

		// Move the camera to the head location
		s_MainCamera.transform.position = CGamePlayers.SelfActorHead.transform.position;
		s_MainCamera.transform.rotation = CGamePlayers.SelfActorHead.transform.rotation;
		
		// Set the defult view perspective
		SetPlayersViewPerspective(true);
		
		// Instantiate the 3D HUD
		if(s_OculusRiftActive)
		{
			s_HUDCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/User Interface/HUD/HUD3DOVR"))).transform.FindChild("CameraOVR").gameObject;
		}
		else
		{
			s_HUDCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/User Interface/HUD/HUD3D"))).transform.FindChild("Camera").gameObject;
		}

		// Cache the left and right camera
		s_CachedHUDCameraLeft = s_HUDCamera.transform.FindChild("CameraLeft");
		s_CachedHUDCameraRight = s_HUDCamera.transform.FindChild("CameraRight");
	}
	
	public static void SetPlayersViewPerspective(bool _IsInsideShip)
	{
		s_IsObserverInsideShip = _IsInsideShip;

		if(s_OculusRiftActive)
		{
			if(_IsInsideShip)
			{
				SetCameraDefaultValues(s_CachedMainCameraLeft.camera, 1.0f);
				SetCameraDefaultValues(s_CachedMainCameraRight.camera, 4.0f);

				SetCameraGalaxyValues(s_CachedProjectedCameraLeft.camera, 0.0f);
				SetCameraGalaxyValues(s_CachedProjectedCameraRight.camera, 3.0f);
			}
			else
			{
				SetCameraDefaultValues(s_CachedProjectedCameraLeft.camera, 1.0f);
				SetCameraDefaultValues(s_CachedProjectedCameraRight.camera, 4.0f);

				SetCameraGalaxyValues(s_CachedMainCameraLeft.camera, 0.0f);
				SetCameraGalaxyValues(s_CachedMainCameraRight.camera, 3.0f);
			}
		}
		else
		{
			if(_IsInsideShip)
			{
				SetCameraDefaultValues(s_MainCamera.camera, 1.0f);
				SetCameraGalaxyValues(s_ProjectedCamera.camera, 0.0f);
			}
			else
			{
				SetCameraDefaultValues(s_ProjectedCamera.camera, 1.0f);
				SetCameraGalaxyValues(s_MainCamera.camera, 0.0f);
			}
		}
	}

	private static void SetCameraGalaxyValues(Camera _Camera, float _Depth)
	{
		// Set the clear flags / culling mask
		_Camera.clearFlags = CameraClearFlags.SolidColor;
		_Camera.cullingMask = 1 << LayerMask.NameToLayer("Galaxy");

		// Set the depth
		_Camera.depth = _Depth;
	}
	
	private static void SetCameraDefaultValues(Camera _Camera, float _Depth)
	{
		// Set the clear flags / culling mask
		_Camera.clearFlags = CameraClearFlags.Nothing;
		_Camera.cullingMask = int.MaxValue;
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Galaxy"));
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("HUD"));
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI 2D"));
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI 3D"));

		// Set the depth
		_Camera.depth = _Depth;
	}
	
	private void UpdateCameraTransforms()
	{		
		// Transfer the projected camera based off the head camera
		if(s_IsObserverInsideShip)
		{
			CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(s_MainCamera.transform.position, s_MainCamera.transform.rotation, s_ProjectedCamera.transform);
		}
		else
		{
			CGameShips.ShipGalaxySimulator.TransferFromGalaxyToSimulation(s_MainCamera.transform.position, s_MainCamera.transform.rotation, s_ProjectedCamera.transform);	
		}

		// Update the HUD camera transform
		s_HUDCamera.transform.position = s_MainCamera.transform.position;
		s_HUDCamera.transform.rotation = s_MainCamera.transform.rotation;

		if(CGameCameras.IsOculusRiftActive)
		{
			// Update the left/right HUD cameras transform
			s_CachedHUDCameraLeft.position = s_CachedMainCameraLeft.position;
			s_CachedHUDCameraLeft.rotation = s_CachedMainCameraLeft.rotation;
			s_CachedHUDCameraRight.position = s_CachedMainCameraRight.position;
			s_CachedHUDCameraRight.rotation = s_CachedMainCameraRight.rotation;

			// Update the left/right galaxy cameras transform
			s_CachedProjectedCameraLeft.localPosition = s_CachedMainCameraLeft.localPosition;
			s_CachedProjectedCameraLeft.localRotation = s_CachedMainCameraLeft.localRotation;
			s_CachedProjectedCameraRight.localPosition = s_CachedMainCameraRight.localPosition;
			s_CachedProjectedCameraRight.localRotation = s_CachedMainCameraRight.localRotation;
		}
	}
	
	private void OnDisconnect()
	{
		if(s_ProjectedCamera != null)
		{
			Destroy(s_ProjectedCamera);
		}
		
		if(s_MainCamera != null)
		{
			Destroy(s_MainCamera);
		}

		if(CHUD3D.Instance.gameObject != null)
		{
			Destroy(CHUD3D.Instance.gameObject);
		}
	}
};
