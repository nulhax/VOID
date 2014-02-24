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
	public Camera m_HUDCamera = null;

	public UISprite m_WithinBoundsIcon = null;
	public UISprite m_OutOfBoundsIcon = null;

	private GameObject m_GameCamera = null;
	private Transform m_Tracker = null;
	private bool m_OutsideBounds = false;

	private float m_MaxDistance = 0.9f;

	
	// Member Properties
	public GameObject GameCamera
	{
		set { m_GameCamera = value; }
	}

	public Transform Tracker
	{
		set { m_Tracker = value; }
	}

	public bool IsOutsideBounds
	{
		get { return(m_OutsideBounds); }
	}


	public void Start()
	{
		// Default in bounds
		EnteredBounds();

		// Set the max distance much closer for oculus
		if(CGameCameras.IsOculusRiftActive)
			m_MaxDistance = 0.6f;
	}

	// Member Methods
	public void LateUpdate()
	{
		Vector3 pos = Vector3.zero;

		if(CGameCameras.IsOculusRiftActive)
		{
			Camera left = m_GameCamera.transform.FindChild("CameraLeft").camera;
			Camera right = m_GameCamera.transform.FindChild("CameraRight").camera;

			Vector3 leftPos = left.WorldToViewportPoint(m_Tracker.position);
			Vector3 rightPos = right.WorldToViewportPoint(m_Tracker.position);

			pos = (leftPos + rightPos) / 2.0f;
		}
		else
		{
			pos = m_GameCamera.camera.WorldToViewportPoint(m_Tracker.position);
		}

		Debug.Log(pos);

		// Clamp the position within the frame of the view port
		pos.x = Mathf.Clamp01(pos.x);
		pos.y = Mathf.Clamp01(pos.y);

		// Set this 0, 1 space to -1 1 space to check bounds.
		Vector2 outsideTest = Vector2.zero;
		outsideTest.x = (pos.x * 2.0f) - 1.0f;
		outsideTest.y = (pos.y * 2.0f) - 1.0f;

		// Check the bounds
		if(outsideTest.sqrMagnitude > (m_MaxDistance * m_MaxDistance) || pos.z < 0.0f)
		{
			if(!m_OutsideBounds)
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
			Vector3 dir = (m_Tracker.position - m_GameCamera.transform.position).normalized;

			// Project the vector onto the cameras facing plane
			Vector3 projRight = Vector3.Project(dir, m_GameCamera.transform.right);
			Vector3 projUp = Vector3.Project(dir, m_GameCamera.transform.up);
			Vector3 projTot = (projRight + projUp).normalized;

			// Find the new position from the totalprojection normalised rotated by the camera facing direction
			Vector3 newPos = Quaternion.Inverse(m_GameCamera.transform.rotation) * projTot;

			// Set this -1, 1 space back to 0 1 space.
			pos.x = ((newPos.x * m_MaxDistance) + 1.0f) / 2.0f;
			pos.y = ((newPos.y * m_MaxDistance) + 1.0f) / 2.0f;

			// Update the rotation of the icon
			Vector3 euler = Vector3.zero;
			euler.z = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg + 45.0f;
			m_OutOfBoundsIcon.transform.localEulerAngles = euler;
		}

		// Set the z to be appropriate pos away form camera
		pos.z = transform.parent.localPosition.z;

		// Set the position
		transform.position = m_HUDCamera.ViewportToWorldPoint(pos);
		pos = transform.localPosition;
		pos.x = pos.x;
		pos.y = pos.y;
		pos.z = 0f;
		transform.localPosition = pos;
	}

	private void LeftBounds()
	{
		m_WithinBoundsIcon.enabled = false;
		m_OutOfBoundsIcon.enabled = true;
	}

	private void EnteredBounds()
	{
		m_WithinBoundsIcon.enabled = true;
		m_OutOfBoundsIcon.enabled = false;
	}
}
