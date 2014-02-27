using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
//  The nested prefab property modifications
public class NestedPrefabPropertyModification
{
	public UnityEngine.Object objectReference;
	public string propertyPath;
	public UnityEngine.Object target;
	public string value;
	
	// Used to recuperate transform override even though the prefab have been replace by an other 
	public bool isRootTransform;
	public UnityEngine.Object targetTransformPrefabObject;

#if UNITY_EDITOR
	public void CopyTo(ref PropertyModification a_rPropertModification)
	{
		// If the target is a transform on an replaced prefab
		if(isRootTransform && target == null && targetTransformPrefabObject != null)
		{
			target = (AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(targetTransformPrefabObject), typeof(GameObject)) as GameObject).GetComponent<Transform>();
		}
		
		a_rPropertModification.objectReference = objectReference;
		a_rPropertModification.propertyPath = propertyPath;
		a_rPropertModification.value = value;
		a_rPropertModification.target = target;
	}
	
	public void CopyFrom(PropertyModification a_rPropertModification)
	{
		objectReference = a_rPropertModification.objectReference;
		propertyPath = a_rPropertModification.propertyPath;
		value = a_rPropertModification.value;
		target = a_rPropertModification.target;
		
		// If the object reference is not persistent make it
		if(objectReference != null && EditorUtility.IsPersistent(objectReference) == false)
		{
			objectReference = PrefabUtility.GetPrefabParent(objectReference);
		}
		
		// If the target is a transform component
		if(target is Transform)
		{
			GameObject rTransformGameObject = (target as Transform).gameObject;
			// If the target is the prefab root transform
			if(rTransformGameObject == PrefabUtility.FindPrefabRoot(rTransformGameObject))
			{
				isRootTransform = true;	
				targetTransformPrefabObject = PrefabUtility.GetPrefabObject(target);
			}
		}
	}
	
	
	// Can we use this modification
	public bool CanUse(UnityEngine.Object a_rTargetPrefabInstance)
	{
		if(	propertyPath.Contains("m_bSceneInstance")
		||	propertyPath.Contains("m_rNestedPrefabDatas"))
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	
	// Can we use this modification
	private bool IsAnInstanceProperty(UnityEngine.Object a_rTargetPrefabInstance)
	{
		return true;
	}
#endif
}