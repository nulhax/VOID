﻿using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class CMeshCombinerWizard : ScriptableWizard 
{
	private GameObject m_FacilityObject = null;

	[MenuItem ("VOID/Mesh Combiner")]
	static void CreateWizard() 
	{
		ScriptableWizard.DisplayWizard<CMeshCombinerWizard>("Create Combined Mesh Instance", "Create");
	}

	void Update()
	{
		m_FacilityObject = UnityEditor.Selection.activeGameObject;

		helpString = "Creates a combined mesh instance of the given object.\n\nThe mesh will be saved to \"Assets/Models/_Combined/\"\n\n";
		if(!m_FacilityObject) 
		{
			helpString += "\t\t\tPlease select a GameObject.\n";
			isValid = false;
		} 
		else 
		{
			helpString += "\t\t\tSelected: [" + m_FacilityObject.name + "]\n";
			isValid = true;	
		}


	}

	void OnWizardUpdate() 
	{

	}

	void OnWizardCreate() 
	{
		Vector3 oldPos = m_FacilityObject.transform.position;
		m_FacilityObject.transform.position = Vector3.zero;
		
		GameObject combinationMesh = new GameObject(m_FacilityObject.name + "_Combined");
		combinationMesh.transform.localPosition = Vector3.zero;
		combinationMesh.transform.localRotation = Quaternion.identity;
		
		MeshFilter[] meshFilters = m_FacilityObject.GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		
		for(int i = 0; i < meshFilters.Length; ++i) 
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
		}
		
		Mesh mesh = new Mesh();
		mesh.name = m_FacilityObject.name + "_Combined";
		mesh.CombineMeshes(combine);
		
		// Add the mesh renderer
		MeshRenderer mr = combinationMesh.AddComponent<MeshRenderer>();
		mr.sharedMaterial = new Material(Shader.Find("Diffuse"));

		// Add the mesh filter
		MeshFilter mf = combinationMesh.AddComponent<MeshFilter>();
		mf.sharedMesh = mesh;

		// Save the mesh into the file path
		AssetDatabase.CreateAsset(mesh, "Assets/Models/_Combined/" + mesh.name + ".asset");

		m_FacilityObject.transform.position = oldPos;

		AssetDatabase.SaveAssets();
	}
}