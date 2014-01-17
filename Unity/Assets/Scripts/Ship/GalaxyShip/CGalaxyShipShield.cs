//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipShield.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CGalaxyShipShield : MonoBehaviour 
{
	// Member Types
	public enum EShieldState
	{
		INVALID,
		
		PoweredUp,
		PoweredDown,
		Reacting,
		Charging,
		
		MAX
	}
	
	// Member Fields
	public GameObject m_Shield = null;
	
	private float m_ShieldPower;
	private float m_MaxShieldPower;
	
	private bool m_Active = true;
	private EShieldState m_ShieldState = EShieldState.PoweredDown;
	
	// Member Properies
	
	
	// Member Methods
	public void UpdateShieldBounds(Mesh _ShieldMesh)
	{	
		Mesh workingMesh = MeshUtils.CloneMesh(_ShieldMesh);
		
		// Apply Laplacian Smoothing Filter to Mesh
		int iterations = 1;
		for(int i = 0; i < iterations; ++i)
		{
			//workingMesh.vertices = SmoothFilter.laplacianFilter(_ShieldMesh.vertices, workingMesh.triangles);
		}
		
		m_Shield.GetComponent<MeshFilter>().sharedMesh = workingMesh;
	}
}
