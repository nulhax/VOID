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
		
		// Move the collider to identity transform
		Vector3 oldPos = transform.position;
		Quaternion oldRot = transform.rotation;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		
		// Create a cube
		GameObject newSphere = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		MeshFilter mf = newSphere.GetComponent<MeshFilter>();
		
		// Get the mesh filters of the colliders
		MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
		List<CombineInstance> combines = new List<CombineInstance>();
        for(int i = 0; i < meshFilters.Length; ++i) 
		{
			if(meshFilters[i].collider == null)
				continue;

			Vector3 scale = meshFilters[i].collider.bounds.size + new Vector3(1.0f, 0.0f, 1.0f);
			scale.x = scale.x / Mathf.Sqrt(2.0f) * 2.0f;
			scale.z = scale.z / Mathf.Sqrt(2.0f) * 2.0f;
			scale.y = scale.y;
			
			newSphere.transform.localScale = scale;
			newSphere.transform.localPosition = meshFilters[i].collider.bounds.center;

			CombineInstance combine = new CombineInstance();
			combine.mesh = mf.sharedMesh;
			combine.transform = mf.transform.localToWorldMatrix;
			combines.Add(combine);
        }
		
		// Destroy the cube
		Destroy(newSphere);
		
		// Create a new mesh for the shield to use
        Mesh mesh = new Mesh();
		mesh.name = "Shield";
		mesh.CombineMeshes(combines.ToArray(), true, true);
		mesh.Optimize();
	
		// Update the shield bounds
		gameObject.GetComponent<CGalaxyShipShield>().UpdateShieldBounds(mesh);
		
		// Move back to old transform
		transform.position = oldPos;
		transform.rotation = oldRot;
	}
}
