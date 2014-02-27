using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
// The nested prefab utility
public static class NestedPrefabUtility
{
	// Get the nested prefab data
	public static NestedPrefabData GetNestedPrefabData(GameObject a_rNestedPrefabGameObject)
	{
		NestedPrefabData rNestedPrefabData = new NestedPrefabData();
		
		rNestedPrefabData.SaveModifications(a_rNestedPrefabGameObject);
		
		return rNestedPrefabData;
	}
	
	// Change parent and keep the same local transform
	public static void ChangeParentAndKeepSameLocalTransform(Transform a_rChild, Transform a_rNewParent)
	{
		// Save local transform
		Vector3 f3LocalPositionSave = a_rChild.localPosition;
		Quaternion oLocalRotationSave = a_rChild.localRotation;
		Vector3 f3LocalScaleSave = a_rChild.localScale;
		
		// Place it on the object
		a_rChild.parent = a_rNewParent;
		
		// Restore local transform
		a_rChild.localPosition = f3LocalPositionSave;
		a_rChild.localRotation = oLocalRotationSave;
		a_rChild.localScale = f3LocalScaleSave;
	}	
}
#endif