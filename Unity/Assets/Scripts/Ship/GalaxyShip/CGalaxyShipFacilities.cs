//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipCollider.cs
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


public class CGalaxyShipFacilities : MonoBehaviour 
{
	// Member Types
	
	
	// Member Fields


	// Member Properies

	
	// Member Methods
	public void AttachNewFacility(GameObject _FacilityExterior, Vector3 _RelativePos, Quaternion _RelativeRot)
	{	
		_FacilityExterior.transform.parent = transform;
		_FacilityExterior.transform.localPosition = _RelativePos;
		_FacilityExterior.transform.localRotation = _RelativeRot;

		// Update the shield bounds
		//gameObject.GetComponent<CGalaxyShipShield>().UpdateShieldBounds();

		// Update the camera bounds
		gameObject.GetComponent<CGalaxyShipCamera>().UpdateCameraBounds();
	}
}
