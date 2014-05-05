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
	private static GameObject s_BackgroundCamera = null;

	private static GameObject s_SpaceFog = null;

	private static bool s_IsObserverInsideShip = false;

	private static bool s_OculusRiftActive = false;

	private static Transform s_CachedMainCameraLeft = null;
	private static Transform s_CachedMainCameraRight = null;

	private static Transform s_CachedProjectedCameraLeft = null;
	private static Transform s_CachedProjectedCameraRight = null;

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

	public static bool IsObserverInsideShip
	{
		get { return(s_IsObserverInsideShip); }
	}

	public static bool IsOculusRiftActive
	{
		get { return(s_OculusRiftActive); }
	}


	// Member Methods
	public void Awake()
	{	
		s_Instance = this;
	}

	public void Start()
	{
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
		   CNetwork.Connection.IsConnected &&
            s_MainCamera != null)
		{
			// Update the transforms of the cameras
			UpdateCameraTransforms();
		}
	}

	public static void SetMainCameraParent(GameObject _parent)
	{
		s_MainCamera.transform.parent = _parent.transform;
        s_MainCamera.transform.localPosition = s_MainCamera.transform.localPosition + new Vector3(0.0f, 0.0f, 0.1f);
	}

	public static void ResetCamera()
	{
		s_MainCamera.transform.localRotation = Quaternion.identity;
		s_MainCamera.transform.localPosition = new Vector3 (0, 0, 0);
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

			// Instantiate the projected camera (copy from head camera)
			s_ProjectedCamera = (GameObject)GameObject.Instantiate(s_MainCamera); 
			s_ProjectedCamera.name = s_ProjectedCamera.name = "Camera_Projected";

			//// Instantiate the background camera
			//s_BackgroundCamera = (GameObject)GameObject.Instantiate(s_ProjectedCamera); 
			//s_BackgroundCamera.name = s_BackgroundCamera.name = "Camera_Background";

			//// Remove all image effects for background camera
			//foreach(PostEffectsBase ieb in s_BackgroundCamera.GetComponents<PostEffectsBase>())
			//    Destroy(ieb);

			//// Set up the values for the bg camera
			//s_BackgroundCamera.transform.position = Vector3.zero;
			//s_BackgroundCamera.camera.clearFlags = CameraClearFlags.Skybox;
			//s_BackgroundCamera.camera.cullingMask = 1 << LayerMask.NameToLayer("Background");
			//s_BackgroundCamera.camera.depth = 49;
		}

		// Move the camera to the head location
		s_MainCamera.transform.position = CGamePlayers.SelfActorHead.transform.position;
		s_MainCamera.transform.rotation = CGamePlayers.SelfActorHead.transform.rotation;

		//Debug.Log("Creating space fog");
		s_SpaceFog = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/SpaceFog"));

		// Set the defult view perspective
		SetObserverSpace(true);
		SetObserverPerspective(CGamePlayers.SelfActorHead);
	}

	public static void SetObserverPerspective(GameObject _LookingFrom)
	{
		// Parent the camera to the looking from object
		s_MainCamera.transform.parent = _LookingFrom.transform;
		s_MainCamera.transform.localPosition = Vector3.zero;
		s_MainCamera.transform.localRotation = Quaternion.identity;
	}
	
	public static void SetObserverSpace(bool _IsInsideShip)
	{
		s_IsObserverInsideShip = _IsInsideShip;

		if(s_OculusRiftActive)
		{
			if(_IsInsideShip)
			{
				SetCameraDefaultValues(s_CachedMainCameraLeft.camera, 51.0f);
				SetCameraDefaultValues(s_CachedMainCameraRight.camera, 54.0f);

				SetCameraGalaxyValues(s_CachedProjectedCameraLeft.camera, 50.0f);
				SetCameraGalaxyValues(s_CachedProjectedCameraRight.camera, 53.0f);
			}
			else
			{
				SetCameraDefaultValues(s_CachedProjectedCameraLeft.camera, 51.0f);
				SetCameraDefaultValues(s_CachedProjectedCameraRight.camera, 54.0f);

				SetCameraGalaxyValues(s_CachedMainCameraLeft.camera, 50.0f);
				SetCameraGalaxyValues(s_CachedMainCameraRight.camera, 53.0f);
			}
		}
		else
		{
			if(_IsInsideShip)
			{
				SetCameraDefaultValues(s_MainCamera.camera, 51.0f);
				SetCameraGalaxyValues(s_ProjectedCamera.camera, 50.0f);
			}
			else
			{
				SetCameraDefaultValues(s_ProjectedCamera.camera, 51.0f);
				SetCameraGalaxyValues(s_MainCamera.camera, 50.0f);
			}
		}
	}

	public static void SetCameraGalaxyValues(Camera _Camera, float _Depth)
	{
		// Set the clear flags / culling mask
		_Camera.clearFlags = CameraClearFlags.Skybox;
		_Camera.cullingMask = CGalaxy.layerBit_All;

		// Set the depth
		_Camera.depth = _Depth;

		// Disable image effects
		foreach(PostEffectsBase ieb in _Camera.GetComponents<PostEffectsBase>())
			ieb.enabled = false;
	}

    public static void SetCameraDefaultValues(Camera _Camera, float _Depth)
	{
		// Set the clear flags / culling mask
		_Camera.clearFlags = CameraClearFlags.Nothing;
		_Camera.cullingMask = int.MaxValue;
		_Camera.cullingMask &= ~CGalaxy.layerBit_All;
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Background"));
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("HUD"));
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI 2D"));
		_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI 3D"));

		// Set the depth
		_Camera.depth = _Depth;

		// Enable image effects
		foreach(PostEffectsBase ieb in _Camera.GetComponents<PostEffectsBase>())
			ieb.enabled = true;
	}
	
	private void UpdateCameraTransforms()
	{		
		// Transfer the projected camera based off the head camera
		if(s_IsObserverInsideShip)
		{
			CGameShips.ShipGalaxySimulator.TransferFromSimulationToGalaxy(s_MainCamera.transform.position, s_MainCamera.transform.rotation, s_ProjectedCamera.transform);

			// Update the background camera rotation
			//s_BackgroundCamera.transform.rotation = s_ProjectedCamera.transform.rotation;

			// Move fog to projected camera
			s_SpaceFog.transform.position = s_ProjectedCamera.transform.position;
		}
		else
		{
			CGameShips.ShipGalaxySimulator.TransferFromGalaxyToSimulation(s_MainCamera.transform.position, s_MainCamera.transform.rotation, s_ProjectedCamera.transform);	

			// Update the background camera rotation
			//s_BackgroundCamera.transform.rotation = s_MainCamera.transform.rotation;

			// Move fog to projected camera
			s_SpaceFog.transform.position = s_MainCamera.transform.position;
		}

		if(CGameCameras.IsOculusRiftActive)
		{
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
	}
};
