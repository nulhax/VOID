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
		
		// Move the collider to identity transform
		Vector3 oldPos = m_CompoundCollider.transform.position;
		Quaternion oldRot = m_CompoundCollider.transform.rotation;
		m_CompoundCollider.transform.position = Vector3.zero;
		m_CompoundCollider.transform.rotation = Quaternion.identity;
		
		// Update the shield bounds
		Collider[] childColliders = m_CompoundCollider.GetComponentsInChildren<Collider>();
        Bounds shieldBounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach(Collider collider in childColliders)
        {
            shieldBounds.Encapsulate(collider.bounds);
        }
		
		// Update the shield bounds
		gameObject.GetComponent<CGalaxyShipShield>().UpdateShieldBounds(shieldBounds);
		
		// Move back to old transform
		m_CompoundCollider.transform.position = oldPos;
		m_CompoundCollider.transform.rotation = oldRot;
	}
	
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.F10))
		{
			// Update the shield bounds
			Renderer[] meshFilters = GetComponentsInChildren<Renderer>();
	        Bounds shieldBounds = new Bounds(transform.position, Vector3.zero);
	        foreach(Renderer meshFilter in meshFilters)
	        {
	            shieldBounds.Encapsulate(meshFilter.bounds);
	        }
			
			gameObject.GetComponent<CGalaxyShipShield>().UpdateShieldBounds(shieldBounds);
		}
	}
}
