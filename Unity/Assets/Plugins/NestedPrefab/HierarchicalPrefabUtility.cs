using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The hierarchical prefab utility
public static class HierarchicalPrefabUtility
{
	private static bool s_WarningSent = false;

	// Summary : Instantiate a Prefab GameObject 
	// Note : Same as the GameObject.Instantiate (You can use this method as replacement)
	// But add the capability to instantiate the compiled version of a Hierarchical Prefab
	// Return : The created instance
	public static GameObject Instantiate(GameObject a_rPrefabGameObject)
	{
		DisplayCompilationNotUpToDateWarningIfNeeded();
			
		// Try to get the hierarchical component of the prefab game object
		HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = a_rPrefabGameObject.GetComponent<HierarchicalPrefabInstance>();
		
		// If it's a hierarchical prefab
		if(rHierarchicalPrefabInstanceModel != null)
		{	
			// Instantiate the compiled hierarchic prefab
			return rHierarchicalPrefabInstanceModel.InstantiateCompiledVersion();
		}
		// If it's a normal prefab
		else
		{
			// Instantiate the prefab
			return GameObject.Instantiate(a_rPrefabGameObject) as GameObject;
		}
	}
	
	// Summary : Instantiate a Prefab GameObject 
	// Note : Same as the GameObject.Instantiate (You can use this method as replacement)
	// But add the capability to instantiate the compiled version of a Hierarchical Prefab
	// Return : The created instance
	public static GameObject Instantiate(GameObject a_rPrefabGameObject, Vector3 a_f3Position, Quaternion a_oRotation)
	{
		DisplayCompilationNotUpToDateWarningIfNeeded();
			
		// Try to get the hierarchical component of the prefab game object
		HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = a_rPrefabGameObject.GetComponent<HierarchicalPrefabInstance>();
		
		// If it's a hierarchical prefab
		if(rHierarchicalPrefabInstanceModel != null)
		{	
			// Instantiate the compiled hierarchic prefab
			return rHierarchicalPrefabInstanceModel.InstantiateCompiledVersion(a_f3Position, a_oRotation);
		}
		// If it's a normal prefab
		else
		{
			// Instantiate the prefab
			return GameObject.Instantiate(a_rPrefabGameObject, a_f3Position, a_oRotation) as GameObject;
		}
	}
	
	// Display the compilation not up to date warning
	static private void DisplayCompilationNotUpToDateWarningIfNeeded()
	{
#if UNITY_EDITOR
		if(NestedPrefabEditorSettings.MustCompile && !s_WarningSent)
		{
			Debug.LogWarning("Warning : Nested prefab Compilation is not up to date. Please use the Compile button on the nested prefab Editor.");
			s_WarningSent = true;
		}
#endif
	}
}