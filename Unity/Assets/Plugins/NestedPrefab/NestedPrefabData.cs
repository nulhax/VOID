using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
//  The nested prefab data
public class NestedPrefabData
{
	[SerializeField]
	// The last instance 
	private GameObject m_rLastModifiedNestedPrefab;
	
	[SerializeField]
	// The prefab 
	private UnityEngine.Object m_rPrefabObject;
	
	[SerializeField]
	// The property modifications
	public NestedPrefabPropertyModifications m_oPropertyModificationsFromPrefab = new  NestedPrefabPropertyModifications();

#if UNITY_EDITOR
	// The prefab accessor
	public GameObject Prefab
	{
		get
		{
			return NestedPrefabEditorUtility.GetPrefabGameObject(m_rPrefabObject);
		}
	}
	
	// The prefab object accessor
	public UnityEngine.Object PrefabObject
	{
		get
		{
			return m_rPrefabObject;
		}
	}
	
	// Save modifications from prefab
	public void SaveModifications(GameObject a_rPrefabInstance)
	{
		// Save the prefab
		m_rPrefabObject = PrefabUtility.GetPrefabObject(PrefabUtility.GetPrefabParent(a_rPrefabInstance));
		
		// Save the modifications
		m_oPropertyModificationsFromPrefab.SavePropertyModifications(a_rPrefabInstance);
	}
	
	// Load modifications to prefab
	public void LoadModifications(GameObject a_rPrefabInstance)
	{
		m_oPropertyModificationsFromPrefab.LoadPropertyModifications(a_rPrefabInstance);
		m_rLastModifiedNestedPrefab = a_rPrefabInstance;
	}
	
	// Is the last modified nested prefab
	public bool IsLastModifiedNestedPrefab(GameObject a_rNestedPrefab)
	{
		return a_rNestedPrefab == m_rLastModifiedNestedPrefab;
	}
#endif
}