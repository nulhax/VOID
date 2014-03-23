//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipCamera.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CGalaxyShipCamera : MonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	public float m_InitialZoom = 60.0f;
	public float m_ZoomSpeed = 20.0f;
	public float m_MovementDampening = 10.0f;
	public float m_RotationDampening = 6.0f;

	private float m_Distance = 0.0f;
	private GameObject m_PilotCameraPosition = null;

	private Transform m_CachedCamera = null;
	private Transform m_CachedGalaxyShip = null;

	private Bounds m_CurrentShipBounds;


	// Member Properies
	public GameObject PilotCameraPosition
	{
		get { return(m_PilotCameraPosition); }
	}
	
	
	// Member Methods
	public void Start()
	{
		m_PilotCameraPosition = CUtility.CreateNewGameObject(null, "CameraPosition_Piloting");
		m_PilotCameraPosition.AddComponent<GalaxyShiftable>();

		m_CachedCamera = m_PilotCameraPosition.transform;
		m_CachedGalaxyShip = transform;
		m_Distance = m_InitialZoom;
	}

	public void LateUpdate()
	{
		if(!CNetwork.IsServer)
		{
			UpdateInterpolation();
		}
	}

	public void FixedUpdate()
	{
		if(CNetwork.IsServer)
		{
			UpdateInterpolation();
		}
	}

	private void UpdateInterpolation()
	{
		// Smooth lookat interpolation
		Quaternion look = m_CachedGalaxyShip.rotation;
		m_CachedCamera.rotation = Quaternion.Lerp(m_CachedCamera.rotation, look, Time.deltaTime * m_RotationDampening);		
		
		// Smooth follow interpolation
		Vector3 offset = m_CachedGalaxyShip.up * m_CurrentShipBounds.extents.y + -m_CachedGalaxyShip.forward * (m_CurrentShipBounds.extents.z + m_Distance);
		Vector3 dest = (m_CachedGalaxyShip.position + m_CurrentShipBounds.center) + offset;
		m_CachedCamera.position = Vector3.Lerp(m_CachedCamera.position, dest, Time.deltaTime * m_MovementDampening);
	}

	public void UpdateCameraBounds()
	{
		// Take the bounds of the shield
		m_CurrentShipBounds = gameObject.GetComponent<CGalaxyShipShield>().m_Shield.GetComponent<MeshFilter>().mesh.bounds;
	}

	public void AdjustZoom(float _Delta)
	{
		// Increment the distance
		m_Distance -= _Delta * m_ZoomSpeed;
	}
}
