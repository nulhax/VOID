using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

[CustomEditor(typeof(HierarchicalPrefabInstance))]
// The hierarchical prefab instance editor
public class HierarchicalPrefabInstanceEditor : Editor
{
	// On inspector gui
	public override void OnInspectorGUI()
    {			
		base.OnInspectorGUI();
		
		HierarchicalPrefabInstance rHierarchicalPrefabInstance = target as HierarchicalPrefabInstance;
		
		rHierarchicalPrefabInstance.CanBeInstantiatedAtRuntime = EditorGUILayout.Toggle("Can Be Instantiated", rHierarchicalPrefabInstance.CanBeInstantiatedAtRuntime);
    }
}
