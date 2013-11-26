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


/* Implementation */


public class CGalaxyShipCollider : MonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	public GameObject m_CompoundCollider = null;
	
	
	// Member Properies
	
	
	// Member Methods
	public void AttachNewCollider(string _ColliderPrefab, Vector3 _RelativePos, Quaternion _RelativeRot)
	{
		GameObject newCollider = (GameObject)GameObject.Instantiate(Resources.Load(_ColliderPrefab, typeof(GameObject)));
		newCollider.transform.parent = m_CompoundCollider.transform;
		newCollider.transform.localPosition = _RelativePos;
		newCollider.transform.localRotation = _RelativeRot;
	}
}
