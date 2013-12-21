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


public class CCompositeCameraSystem : MonoBehaviour
{

// Member Types


// Member Delegates & Events

	
// Member Fields
	private Camera m_ShipCamera = null;
	private Camera m_GalaxyCamera = null;

	private bool m_IsObserverOutside = false;
	
// Member Properties

	
// Member Methods
	public void Start()
	{	
		// Instantiate the galaxy camera (must be first!)
		m_GalaxyCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/GalaxyCamera"))).camera;

		// Instantiate the ship camera
		m_ShipCamera = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Cameras/ShipCamera"))).camera;
	}

	public void Update()
	{
		// Update the transforms of the cameras
		UpdateCameraTransforms();
	}
	
	public void SetShipViewPerspective(Transform _ShipPerspective)
	{
		m_IsObserverOutside = false;
		
		// Set the perspective of the ship camera
		m_ShipCamera.transform.parent = _ShipPerspective;
		m_ShipCamera.transform.localPosition = Vector3.zero;
		m_ShipCamera.transform.localRotation = Quaternion.identity;
		
		// Unparent the galaxy camera
		m_GalaxyCamera.transform.parent = null;
		
		// Destroy the galaxy observer/shiftable components
        Destroy(m_GalaxyCamera.gameObject.GetComponent<GalaxyObserver>());
        Destroy(m_GalaxyCamera.gameObject.GetComponent<GalaxyShiftable>());
	}
	
	public void SetGalaxyViewPerspective(Transform _GalaxyPerspective)
	{
		m_IsObserverOutside = true;
		
		// Set the perspective of the galaxy camera
		m_GalaxyCamera.transform.parent = _GalaxyPerspective;
		m_GalaxyCamera.transform.localPosition = Vector3.zero;
		m_GalaxyCamera.transform.localRotation = Quaternion.identity;
		
		// Unparent the ship camera
		m_ShipCamera.transform.parent = null;

		// Add the galaxy observer/shiftable components
        m_GalaxyCamera.gameObject.AddComponent<GalaxyObserver>();
        m_GalaxyCamera.gameObject.AddComponent<GalaxyShiftable>();
	}
	
	private void UpdateCameraTransforms()
	{	
		Transform shipTransform = CGame.Ship.transform;
		Transform galaxyShipTransform = CGame.GalaxyShip.transform;
		
		if(!m_IsObserverOutside)
		{
			// Update the galaxy camera transform based off the ship camera relative to the ship
			m_GalaxyCamera.transform.position = galaxyShipTransform.rotation * (m_ShipCamera.transform.position - shipTransform.position) + galaxyShipTransform.position;
			m_GalaxyCamera.transform.rotation = galaxyShipTransform.rotation * m_ShipCamera.transform.rotation;
		}
		else
		{
			// Update the ship camera transform based off the galaxy camera relative to the galaxy ship
			m_ShipCamera.transform.position = Quaternion.Inverse(galaxyShipTransform.rotation) * (m_GalaxyCamera.transform.position - galaxyShipTransform.transform.position) + shipTransform.position;
			m_ShipCamera.transform.rotation = Quaternion.Inverse(galaxyShipTransform.rotation) * m_GalaxyCamera.transform.rotation;	
		}
	}
};
