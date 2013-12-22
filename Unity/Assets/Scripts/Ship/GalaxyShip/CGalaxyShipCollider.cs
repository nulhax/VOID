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


public class CGalaxyShipCollider : MonoBehaviour 
{
	// Member Types
	
	
	// Member Fields
	public GameObject m_CompoundCollider = null;
	
	private List<GameObject> m_ActorsJustExitedShip = new List<GameObject>();
	
	
	// Member Properies
	
	
	// Member Methods
	public void Start()
	{
		
	}
	
	public void AttachNewCollider(string _ColliderPrefab, Vector3 _RelativePos, Quaternion _RelativeRot)
	{
		GameObject newCollider = (GameObject)GameObject.Instantiate(Resources.Load(_ColliderPrefab, typeof(GameObject)));
		if(newCollider == null)
		{
			Debug.LogError("Collider prefab didn't exist! " + _ColliderPrefab);
		}
		
		newCollider.transform.parent = m_CompoundCollider.transform;
		newCollider.transform.localPosition = _RelativePos;
		newCollider.transform.localRotation = _RelativeRot;
		
		// Move the collider to identity transform
		Vector3 oldPos = m_CompoundCollider.transform.position;
		Quaternion oldRot = m_CompoundCollider.transform.rotation;
		m_CompoundCollider.transform.position = Vector3.zero;
		m_CompoundCollider.transform.rotation = Quaternion.identity;
		
		// Create a cube
		GameObject newSphere = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		MeshFilter mf = newSphere.GetComponent<MeshFilter>();
		
		// Get the mesh filters of the colliders
		MeshFilter[] meshFilters = m_CompoundCollider.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for(int i = 0; i < meshFilters.Length; ++i) 
		{
			Vector3 scale = meshFilters[i].collider.bounds.size + new Vector3(1.0f, 0.0f, 1.0f);
			scale.x = scale.x / Mathf.Sqrt(2.0f) * 2.0f;
			scale.z = scale.z / Mathf.Sqrt(2.0f) * 2.0f;
			scale.y = scale.y;
			
			newSphere.transform.localScale = scale;
			newSphere.transform.localPosition = meshFilters[i].collider.bounds.center;
			
            combine[i].mesh = mf.sharedMesh;
            combine[i].transform = mf.transform.localToWorldMatrix;
        }
		
		// Destroy the cube
		Destroy(newSphere);
		
		// Create a new mesh for the shield to use
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
	
		// Update the shield bounds
		gameObject.GetComponent<CGalaxyShipShield>().UpdateShieldBounds(mesh);
		
		// Move back to old transform
		m_CompoundCollider.transform.position = oldPos;
		m_CompoundCollider.transform.rotation = oldRot;
	}
}
