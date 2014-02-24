//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDPanel.cs
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


public class CHUDPanel : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields

	
	// Member Properties

	
	// Member Methods
	public void Start()
	{
		// Get the HUD camera
		Camera HUDCamera = CHUD3D.HUDCamera.camera;

		// Find the pixel size of this panel
		float zDistance = Mathf.Abs(HUDCamera.transform.position.z - transform.position.z);
		float pixelSize = HUDCamera.pixelHeight / 2.0f / zDistance / Mathf.Tan(HUDCamera.fieldOfView / 2.0f * Mathf.Deg2Rad);

		// Adjust the local scale to be such
		transform.localScale = Vector3.one * 1.0f / pixelSize;
	}

}
