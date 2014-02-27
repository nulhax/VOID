using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode()]
[AddComponentMenu("NestedPrefab/NestedPrefabInstantiator")]
//  The nested prefab instantiator
public class NestedPrefabsInstantiator : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	// The nested prefab datas
	private List<NestedPrefabData> m_rNestedPrefabDatas = new List<NestedPrefabData>();
	
	[SerializeField]
	[HideInInspector]
	// The nested prefab datas
	private bool m_bHasInstantiate;
	
	// Nested prefab accessor
	public List<NestedPrefabData> NestedPrefabDatas
	{
		get
		{
			return m_rNestedPrefabDatas;
		}
	}
	
#if UNITY_EDITOR
	// Awake
	private void Awake()
	{
		hideFlags = HideFlags.HideInInspector;
		if(Application.isPlaying)
		{
			Destroy(this);
		}
	}
	
	// Start
	private void Start()
	{
		hideFlags = HideFlags.HideInInspector;
	}
	
	// On Enable
	private void OnEnable()
	{
		hideFlags = HideFlags.HideInInspector;
	}
	
	// On Disable
	private void OnDisable()
	{
		hideFlags = HideFlags.HideInInspector;
	}
	
	// Clear
	public void Clear()
	{
		m_rNestedPrefabDatas.Clear();
		m_bHasInstantiate = false;
	}
	
	// Add a nested prefab to spawn
	public void Add(GameObject a_rNestedPrefabGameObject)
	{
		m_rNestedPrefabDatas.Add(NestedPrefabUtility.GetNestedPrefabData(a_rNestedPrefabGameObject));
	}
	
	// Instantiate the nested prefabs
	public void TriggerInstantiator()
	{
		InstantiateNestedPrefabs();
	}
	
	// Try to reinstantiate a hierarchical prefab
	public bool TryToReloadData(HierarchicalPrefabInstance a_rHierarchicalPrefabInstance)
	{
		// Try to find a prefab data corresponding to the hierarchical prefab
		NestedPrefabData rNestedPrefabData = TryGrabNestedPrefabData(a_rHierarchicalPrefabInstance.gameObject);
		if(rNestedPrefabData != null)
		{
			// Load its property modifications
		 	rNestedPrefabData.LoadModifications(a_rHierarchicalPrefabInstance.gameObject);
			
			return true;
		}
		
		return false;
	}
	
	// Try to grab the nested prefab data corresponding to a prefab object
	public NestedPrefabData TryGrabNestedPrefabData(GameObject a_rNestedPrefab)
	{
		foreach(NestedPrefabData rNestedPrefabData in m_rNestedPrefabDatas)
		{
			if(rNestedPrefabData.IsLastModifiedNestedPrefab(a_rNestedPrefab))
			{
				return rNestedPrefabData;
			}
		}
		
		return null;
	}
	
	// Instantiate the nested prefabs
	private void InstantiateNestedPrefabs()
	{
		if(m_bHasInstantiate == false)
		{
			DestroyNestedPrefabs();
			
			foreach(NestedPrefabData rNestedPrefabData in m_rNestedPrefabDatas)
			{
				InstantiateNestedPrefab(rNestedPrefabData);
			}
			m_bHasInstantiate = true;
		}
	}
	
	// Instantiate a nested prefab
	private void InstantiateNestedPrefab(NestedPrefabData a_rNestedPrefabData)
	{	
		// Instantiate the prefab
		GameObject rNestedPrefabInstantiated = PrefabUtility.InstantiatePrefab(a_rNestedPrefabData.Prefab) as GameObject;
		
		if(rNestedPrefabInstantiated != null)
		{
			// Load its property modifications
		 	a_rNestedPrefabData.LoadModifications(rNestedPrefabInstantiated);
			
			// Change the parent without changing the local transform information
			NestedPrefabUtility.ChangeParentAndKeepSameLocalTransform(rNestedPrefabInstantiated.transform, gameObject.transform);
		}
	}
	
	// Destroy the nested prefabs
	private void DestroyNestedPrefabs()
	{
		NestedPrefabEditorUtility.ClearHierarchicalPrefab(gameObject);
	}
#else
	// Awake
	private void Awake()
	{
		Destroy(this);
	}
#endif
}