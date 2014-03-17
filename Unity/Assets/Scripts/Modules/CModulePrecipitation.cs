//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModulePrecipitation.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CModulePrecipitation : MonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	public GameObject m_PrecipitationMesh = null;
	public float m_BuildTime = 20.0f;

	private float m_Timer = 0.0f;


	// Member Properties
	public bool IsModuleBuilt
	{
		get { return(m_PrecipitationMesh == null); }
	}

	
	// Member Methods
	void Start()
	{
		// Disable all children except for the precipitation mesh
		foreach(Transform child in transform)
		{
			if(child.gameObject != m_PrecipitationMesh)
				child.gameObject.SetActive(false);
		}
	}
	
	void Update()
	{
		if(m_Timer != m_BuildTime)
		{
			m_Timer += Time.deltaTime;
			m_Timer = Mathf.Clamp(m_Timer, 0.0f, m_BuildTime);

			m_PrecipitationMesh.renderer.material.SetFloat("_Amount", m_Timer/m_BuildTime);
		}
		else
		{
			OnPrecipitationFinish();
		}
	}

	[ContextMenu("Create Precipitation Object")]
	void CreatePrecipitationObject()
	{
		Vector3 oldPos = transform.position;
		transform.position = Vector3.zero;
		
		m_PrecipitationMesh = new GameObject("_PrecipitaionObject");
		m_PrecipitationMesh.transform.parent = transform;
		m_PrecipitationMesh.transform.localPosition = Vector3.zero;
		m_PrecipitationMesh.transform.localRotation = Quaternion.identity;
		
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		
		for(int i = 0; i < meshFilters.Length; ++i) 
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
		}
		
		MeshFilter mf = m_PrecipitationMesh.AddComponent<MeshFilter>();
		mf.sharedMesh = new Mesh();
		mf.sharedMesh.CombineMeshes(combine);
		
		MeshRenderer mr = m_PrecipitationMesh.AddComponent<MeshRenderer>();
		mr.sharedMaterial = new Material(Shader.Find("VOID/Module Precipitate"));
		
		AssetDatabase.CreateAsset(mf.sharedMesh, "Assets/Models/Modules/_PrecipitationMeshes/" + gameObject.name + ".asset");
		AssetDatabase.CreateAsset(mr.sharedMaterial, "Assets/Models/Modules/_PrecipitationMeshes/Materials/" + gameObject.name + ".mat");
		AssetDatabase.SaveAssets();
		
		transform.position = oldPos;
	}
	
	
	void OnPrecipitationFinish()
	{
		// Enable all the children
		foreach(Transform child in transform)
		{
			child.gameObject.SetActive(true);
		}

		// Destroy the precipitation mesh
		Destroy(m_PrecipitationMesh);
		m_PrecipitationMesh = null;
	}
};
