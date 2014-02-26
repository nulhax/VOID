//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDLocator.cs
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


public class CHUDLocator : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events

	
	// Member Fields
	public GameObject m_WithinBoundsIcon = null;
	public GameObject m_OutOfBoundsIcon = null;

	public bool m_UseOutOfBounds = true;

	private Transform m_Target = null;
	private bool m_OutsideBounds = false;
	private float m_MaxDistance = 0.9f;

	
	// Member Properties
	public Transform Target
	{
		set { m_Target = value; }
	}

	public bool IsOutsideBounds
	{
		get { return(m_OutsideBounds); }
	}


	private void Start()
	{
		// If m_WithinBoundsIcon is null, select self
		if(m_WithinBoundsIcon == null)
			m_WithinBoundsIcon = gameObject;

		// Default in bounds
		EnteredBounds();

		// Set the max distance much closer for oculus
		if(CGameCameras.IsOculusRiftActive)
			m_MaxDistance = 0.6f;
	}

	// Member Methods
	private void LateUpdate()
	{
		UpdateIconTracking();
	}

	private void UpdateIconTracking()
	{
		// Ensure there is a target
		if(m_Target == null)
			return;
		
		// Select the camera depending on which layer the target is
		GameObject gameCamera = m_Target.gameObject.layer == LayerMask.NameToLayer("Galaxy") ?
			CGameCameras.CameraRenderingGalaxy : CGameCameras.CameraRenderingDefault;
		
		Vector3 pos = Vector3.zero;
		Camera HUDCamera = CHUD3D.HUDCamera.camera;
		
		if(CGameCameras.IsOculusRiftActive)
		{
			Camera left = gameCamera.transform.FindChild("CameraLeft").camera;
			Camera right = gameCamera.transform.FindChild("CameraRight").camera;
			
			Vector3 leftPos = left.WorldToViewportPoint(m_Target.position);
			Vector3 rightPos = right.WorldToViewportPoint(m_Target.position);
			
			pos = (leftPos + rightPos) / 2.0f;
		}
		else
		{
			pos = gameCamera.camera.WorldToViewportPoint(m_Target.position);
		}
		
		// Set this 0, 1 space to -1 1 space to check bounds.
		Vector2 outsideTest = Vector2.zero;
		outsideTest.x = (pos.x * 2.0f) - 1.0f;
		outsideTest.y = (pos.y * 2.0f) - 1.0f;
		
		// Check the bounds
		if(outsideTest.sqrMagnitude > (m_MaxDistance * m_MaxDistance) || pos.z < 0.0f)
		{
			if(!m_OutsideBounds && m_UseOutOfBounds && m_OutOfBoundsIcon != null)
			{
				LeftBounds();
				m_OutsideBounds = true;
			}
		}
		else
		{
			if(m_OutsideBounds)
			{
				EnteredBounds();
				m_OutsideBounds = false;
			}
		}
		
		// Rotate the outofbounds icon in the correct direction
		if(m_OutsideBounds)
		{
			// Calculate the difference vector
			Vector3 dir = (m_Target.position - gameCamera.transform.position).normalized;
			
			// Project the vector onto the cameras facing plane
			Vector3 projRight = Vector3.Project(dir, gameCamera.transform.right);
			Vector3 projUp = Vector3.Project(dir, gameCamera.transform.up);
			Vector3 projTot = (projRight + projUp).normalized;
			
			// Find the new position from the totalprojection normalised rotated by the camera facing direction
			Vector3 newPos = Quaternion.Inverse(gameCamera.transform.rotation) * projTot;
			
			// Set this -1, 1 space back to 0 1 space.
			pos.x = ((newPos.x * m_MaxDistance) + 1.0f) / 2.0f;
			pos.y = ((newPos.y * m_MaxDistance) + 1.0f) / 2.0f;
			
			// Update the rotation of the icon
			Vector3 euler = Vector3.zero;
			euler.z = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg + 45.0f;
			m_OutOfBoundsIcon.transform.localEulerAngles = euler;
		}
		
		// Set the z to be appropriate pos away from the camera
		pos.z = transform.parent.localPosition.z;
		
		// Set the position
		transform.position = HUDCamera.ViewportToWorldPoint(pos);
		
		// Zero the z component
		pos = transform.localPosition;
		pos.z = 0f;
		transform.localPosition = pos;
	}

	private void LeftBounds()
	{
		m_WithinBoundsIcon.SetActive(false);

		if(m_OutOfBoundsIcon != null)
			m_OutOfBoundsIcon.SetActive(true);

		m_OutOfBoundsIcon.transform.localEulerAngles = Vector3.zero;
	}

	private void EnteredBounds()
	{
		if(m_OutOfBoundsIcon != null)
			m_OutOfBoundsIcon.SetActive(false);

		m_WithinBoundsIcon.SetActive(true);
	}
}
