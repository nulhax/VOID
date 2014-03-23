using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Post process prefab
public class NestedPrefabAssetPostProcessor : AssetPostprocessor
{
	// Disable?
    private static bool m_bDisable = false;
	
	// On post process all assets
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {			
		if(m_bDisable)
		{
			return;
		}
		
		// Loop through all the imported prefabs
		foreach(string a_rAssetPath in importedAssets)
        {
			// Get the prefab
	        GameObject rImportedPrefab = AssetDatabase.LoadAssetAtPath(a_rAssetPath, typeof(GameObject)) as GameObject;
	        if(rImportedPrefab != null)
	        {
				// Ensure the prefab validity
				// Either a functionning hierarchical prefab or a clean classic prefab
				EnsurePrefabValidity(rImportedPrefab);
	        }	
		}
    }
	
	// Ensure the prefab validity :
	// Either a functionning hierarchical prefab or a clean classic prefab.
	public static void EnsurePrefabValidity(GameObject a_rPrefabGameObjectToCheck)
    {
		// If the prefab is still valid don't bother updating it
		HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = a_rPrefabGameObjectToCheck.GetComponent<HierarchicalPrefabInstance>();
		if(rHierarchicalPrefabInstanceModel == null || rHierarchicalPrefabInstanceModel.IsValid() == false)
		{
			// If it's not a hierarchical prefab, ensure that the prefab is clean
			NestedPrefabEditorUtility.DisconnectResourceFromHierarchicalPrefab(a_rPrefabGameObjectToCheck);
		}
	}
}
