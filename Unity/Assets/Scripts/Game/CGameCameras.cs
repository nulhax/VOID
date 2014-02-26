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

	private static bool s_IsPlayerInsideShip = false;

	private static bool s_OculusRiftActive = false;

	private static CGameCameras s_Instance = null;
	
	// Member Properties
	public static CGameCameras Instance
	{
		get { return(s_Instance); }
	}
	
	public static GameObject ProjectedCamera
	{
		get { return(s_ProjectedCamera);}
	}
	
	public static GameObject MainCamera
	{
		get { return(s_MainCamera); }
	}

	public static GameObject CameraRenderingGalaxy
	{
		get 
		{ 
			if(!s_IsPlayerInsideShip)
				return(s_MainCamera); 
			else
				return(s_ProjectedCamera); 
		}
	}
	
	public static GameObject CameraRenderingDefault
	{
		get 
		{ 
			if(!s_IsPlayerInsideShip)
				return(s_ProjectedCamera); 
			else
				return(s_MainCamera); 
		}
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
		}
		else
		{
			s_MainCamera = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/Camera"));
			s_MainCamera.name = "Camera_Main";

			// Set the main camera parented to the players head
			s_MainCamera.transform.parent = CGamePlayers.SelfActorHead.transform;
		}

		// Move the camera to the head location
		s_MainCamera.transform.position = CGamePlayers.SelfActorHead.transform.position;
		s_MainCamera.transform.rotation = CGamePlayers.SelfActorHead.transform.rotation;
		
		// Instantiate the projected camera (copy from head camera)
		s_ProjectedCamera = (GameObject)GameObject.Instantiate(s_MainCamera); 

		// Clean name up
		s_ProjectedCamera.name = s_ProjectedCamera.name.Replace("Main", "Proj");
		s_ProjectedCamera.name = s_ProjectedCamera.name.Replace("(Clone)", "");
		
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
	}
	
	public static void SetPlayersViewPerspective(bool _IsInsideShip)
	{
		s_IsPlayerInsideShip = _IsInsideShip;

		if(s_OculusRiftActive)
		{
			if(_IsInsideShip)
			{
				SetCameraDefaultValues(s_MainCamera.transform.FindChild("CameraLeft").camera, 1.0f);
				SetCameraDefaultValues(s_MainCamera.transform.FindChild("CameraRight").camera, 4.0f);

				SetCameraGalaxyValues(s_ProjectedCamera.transform.FindChild("CameraLeft").camera, 0.0f);
				SetCameraGalaxyValues(s_ProjectedCamera.transform.FindChild("CameraRight").camera, 3.0f);
			}
			else
			{
				SetCameraDefaultValues(s_ProjectedCamera.transform.FindChild("CameraLeft").camera, 1.0f);
				SetCameraDefaultValues(s_ProjectedCamera.transform.FindChild("CameraRight").camera, 4.0f);

				SetCameraGalaxyValues(s_MainCamera.transform.FindChild("CameraLeft").camera, 0.0f);
				SetCameraGalaxyValues(s_MainCamera.transform.FindChild("CameraRight").camera, 3.0f);
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

//		// Remove the current audio listener
//		if(_Camera.gameObject.GetComponent<AudioListener>() != null)
//			Destroy(_Camera.gameObject.GetComponent<AudioListener>());
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

//		// Set as the current audio listener
//		if(_Camera.gameObject.GetComponent<AudioListener>() == null)
//			_Camera.gameObject.AddComponent<AudioListener>();
	}
	
	private void UpdateCameraTransforms()
	{		
		// Transfer the projected camera based off the head camera
		if(s_IsPlayerInsideShip)
		{
			CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(s_MainCamera.transform.position, s_MainCamera.transform.rotation, s_ProjectedCamera.transform);
		}
		else
		{
			CGameShips.ShipGalaxySimulator.TransferFromGalaxyToSimulation(s_MainCamera.transform.position, s_MainCamera.transform.rotation, s_ProjectedCamera.transform);	
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
