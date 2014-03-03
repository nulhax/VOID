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


public class CHUDLocator : CHUDToolTip
{
	// Member Types


	// Member Delegates & Events

	
	// Member Fields
	public GameObject m_LocatorIcon = null;

	
	// Member Properties


	// Member methods
	protected void LateUpdate()
	{
		base.LateUpdate();

		// Rotate the outofbounds icon in the correct direction
		if(m_OutsideBounds)
		{
			// Calculate the difference vector
			Vector3 dir = (m_Target.position - m_GameCamera.position).normalized;
			
			// Project the vector onto the cameras facing plane
			Vector3 projRight = Vector3.Project(dir, m_GameCamera.right);
			Vector3 projUp = Vector3.Project(dir, m_GameCamera.up);
			Vector3 projTot = (projRight + projUp).normalized;
			
			// Find the new position from the totalprojection normalised rotated by the camera facing direction
			Vector3 pos = Quaternion.Inverse(m_GameCamera.rotation) * projTot;

			// Update the rotation of the icon
			Vector3 euler = Vector3.zero;
			euler.z = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg + 45.0f;
			m_LocatorIcon.transform.localEulerAngles = euler;

			// Set this -1, 1 space pos back to 0 1 space.
			pos.x = ((pos.x * m_MaxDistance) + 1.0f) / 2.0f;
			pos.y = ((pos.y * m_MaxDistance) + 1.0f) / 2.0f;

			// Set the z to be appropriate pos away from the camera
			pos.z = transform.parent.localPosition.z;
			
			// Set the position
			transform.position = m_HUDCamera.ViewportToWorldPoint(pos);
			
			// Zero the z component
			pos = transform.localPosition;
			pos.z = 0f;
			transform.localPosition = pos;
		}
	}

	protected override void LeftBounds()
	{
		base.LeftBounds();

		if(m_LocatorIcon != null)
			m_LocatorIcon.SetActive(true);
	}
	
	protected override void EnteredBounds()
	{
		if(m_LocatorIcon != null)
			m_LocatorIcon.SetActive(false);

		base.EnteredBounds();
	}
}
