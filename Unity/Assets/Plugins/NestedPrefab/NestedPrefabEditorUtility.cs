using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
// Nested prefab editor utility
public static class NestedPrefabEditorUtility
{
	// Add a component if not already there
	public static void DestroyComponentsInChildren<ComponentType>(GameObject a_rGameObjectOwner) where ComponentType : Component
	{
		// loop through the component of the searched type in children
		foreach(ComponentType rComponent in a_rGameObjectOwner.GetComponentsInChildren<ComponentType>())
		{
			// Destroy
			Editor.DestroyImmediate(rComponent);	
		}
	}
	
	// Add a component if not already there
	public static void DestroyComponentsInChildrenOnResource<ComponentType>(GameObject a_rResourceGameObject) where ComponentType : Component
	{
		// Destroy each components of searched type on object
		ComponentType[] rComponents = a_rResourceGameObject.GetComponents<ComponentType>();
		foreach(Component rComponent in rComponents)
		{
			// Destroy
			Editor.DestroyImmediate(rComponent, true);	
		}
		
		// Recursively destroy component in children
		foreach(Transform rChild in a_rResourceGameObject.transform)
		{	
			DestroyComponentsInChildrenOnResource<ComponentType>(rChild.gameObject);
		}
	}
	
	// Save as hierarchical prefab
	public static bool SaveAsHierarchicalPrefab(GameObject a_rGameObjectToSave, string a_rSavePath)
    {
		// Convert to hierarchical prefab
		HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = NestedPrefabEditorUtility.ConvertIntoHierarchicalPrefab(a_rGameObjectToSave);
		
		// The prefab to save into
		Object rPrefabObjectToSaveInto = AssetDatabase.LoadAssetAtPath(a_rSavePath, typeof(Object));
		
		// Check if the hierarchical prefab is cyclic
		if(IsHierarchicalPrefabInstanceCyclic(rHierarchicalPrefabInstanceModel, rPrefabObjectToSaveInto))
		{
			Debug.LogError("You can't create cyclic hierarchy!");
			rHierarchicalPrefabInstanceModel.RevertToHierarchicalPrefab();
			return false;
		}
		
		// Save into a prefab at save path
		if(rPrefabObjectToSaveInto == null)
		{
			rPrefabObjectToSaveInto = PrefabUtility.CreateEmptyPrefab(a_rSavePath);
		}
		else
		{
			// If an object replace a prefab copy the replaced prefab version number
			// To the one replacing
			if(rPrefabObjectToSaveInto != rHierarchicalPrefabInstanceModel.HierarchicalPrefab)
			{				
				// Try to get the hierarchical prefab instance model of the soon to de replaced prefab
				GameObject rPrefabToBeReplacedGameObject = NestedPrefabEditorUtility.GetPrefabGameObject(rPrefabObjectToSaveInto);
				if(rPrefabToBeReplacedGameObject != null)
				{
					HierarchicalPrefabInstance rHierarchicalPrefabInstanceModelToBeReplaced = rPrefabToBeReplacedGameObject.GetComponent<HierarchicalPrefabInstance>();
					if(rHierarchicalPrefabInstanceModelToBeReplaced != null)
					{
						// Copy the version number of the instance to be replaced
						rHierarchicalPrefabInstanceModel.CopyVersionNumber(rHierarchicalPrefabInstanceModelToBeReplaced);
					}
				}
			}
		}
		
		// Notify model instance of it's saving
		rHierarchicalPrefabInstanceModel.OnSaveModelInstanceBeforePrefabReplacement(PrefabUtility.GetPrefabObject(rPrefabObjectToSaveInto));
		
		EditorUtility.ReplacePrefab(a_rGameObjectToSave, rPrefabObjectToSaveInto, ReplacePrefabOptions.ConnectToPrefab);
		
		// Compilation is not up to date
		NestedPrefabEditorSettings.MustCompile = true;
		
		return true;
	}
	
	/// \brief  Load a prefab at the place of an object
	static public void LoadPrefab(ref GameObject a_rObjectToReplace, string a_rPath)
	{
		// Get the prefab to load
		GameObject rPrefabToLoad = AssetDatabase.LoadAssetAtPath(a_rPath, typeof(GameObject)) as GameObject;
		
		// Instantiate the prefab
		GameObject rLoadedObject = PrefabUtility.InstantiatePrefab(rPrefabToLoad) as GameObject; 
		
		// if there is an object to replace
		if(a_rObjectToReplace != null)
		{
			rLoadedObject.transform.parent = a_rObjectToReplace.transform.parent;
			rLoadedObject.transform.localPosition = a_rObjectToReplace.transform.localPosition;
			rLoadedObject.transform.localRotation = a_rObjectToReplace.transform.localRotation;
			rLoadedObject.transform.localScale = a_rObjectToReplace.transform.localScale;
			
			// Change the ref
			GameObject a_rReplacedObject = a_rObjectToReplace;
					
			// Destroy the replaced object
			Editor.DestroyImmediate(a_rReplacedObject);
		}
		
		// Return the loaded object
		a_rObjectToReplace = rLoadedObject;
	}
	
	// Convert into hierarchical prefab
	public static HierarchicalPrefabInstance ConvertIntoHierarchicalPrefab(GameObject a_rGameObjectToConvert)
    {	
		// Add the hierarchical prefab if not there
		HierarchicalPrefabInstance rHierarchicalPrefabInstance = NestedPrefabComponentUtility.GetOrCreate<HierarchicalPrefabInstance>(a_rGameObjectToConvert);
		
		// Clear the hierarchy from the prefab and add instantiator to respawn them when needed
		TrimHierarchicalPrefab(a_rGameObjectToConvert.transform);
		
		return rHierarchicalPrefabInstance;
    }
	
	// Trim the hierarchical prefab
	public static void TrimHierarchicalPrefab(Transform a_rTreeToTrimRoot)
    {	
		// Begin by the Hierarchical prefab instance leaf
		// Go all the way down first, then climb and trim recursively
		foreach(Transform rChildTransform in a_rTreeToTrimRoot)
		{
			// Wait to be at the hierarchy trim to actualy execute the trim
			TrimHierarchicalPrefab(rChildTransform);
		}
		
		// If we are on a hierarchical prefab transform it
		HierarchicalPrefabInstance rHierarchicalPrefabInstance = a_rTreeToTrimRoot.GetComponent<HierarchicalPrefabInstance>();
		if(rHierarchicalPrefabInstance != null)
		{
			// Clear the hierarchy from the prefab and add instantiator to respawn them when needed
			ReplacePrefabInHierarchyByInstantiator(rHierarchicalPrefabInstance.gameObject);
		}
    }
	
	// Convert into hierarchical prefab
	public static void ReplacePrefabInHierarchyByInstantiator(GameObject a_rGameObjectToConvert)
    {	
		// Loop through the child transforms
		// and search the nested prefab
		Transform a_rGameObjectToConvertTransform = a_rGameObjectToConvert.transform;;
		
		// If there already is a instantiator remove it
		NestedPrefabsInstantiator rNestedPrefabsInstantiator = a_rGameObjectToConvert.GetComponent<NestedPrefabsInstantiator>();
		if(rNestedPrefabsInstantiator != null)
		{
			rNestedPrefabsInstantiator.Clear();
		}
		
		// Loop in reverse to allow direct child destruction
		for(int i = a_rGameObjectToConvertTransform.GetChildCount() - 1; i >= 0; i--)
		{	
			// Check if Prefab replacement hasn't destroy the parent
			if(a_rGameObjectToConvertTransform != null)
			{
				Transform rChildTransform = a_rGameObjectToConvertTransform.GetChild(i);
				
				GameObject rChildGameObject = rChildTransform.gameObject;
				
				// If we encounter a nested prefab
				if(IsNestedPrefab(rChildGameObject, a_rGameObjectToConvert))
				{
					// Removing the child from the scene graph in order to avoid
					// unwanted prefab reconnection propagation 
					// (A prefab instance nested into an other instance of the same prefab will
					// reconnect the the top most instance thus destroying the nested prefab too soon (i.e. destroying the current child)
					// Keep the local Transform in order to keep the override properties
					NestedPrefabUtility.ChangeParentAndKeepSameLocalTransform(rChildTransform, null);
					
					// Force the prefab reconnection to have an up to date property modifications
					PrefabUtility.ReconnectToLastPrefab(rChildGameObject);
					
					// Reconnect to the scene graph
					NestedPrefabUtility.ChangeParentAndKeepSameLocalTransform(rChildTransform, a_rGameObjectToConvertTransform);
					
					// Create the nested prefab instantiator on his parent
					NestedPrefabsInstantiator rNestedPrefabInstantiator = NestedPrefabComponentUtility.GetOrCreate<NestedPrefabsInstantiator>(a_rGameObjectToConvert);
					
					// Add the current nested prefab game object
					rNestedPrefabInstantiator.Add(rChildGameObject);
					
					// Destroy the nested prefab game object
					Editor.DestroyImmediate(rChildGameObject);
				}
				else
				// If not a prefab go deeper
				{
					ReplacePrefabInHierarchyByInstantiator(rChildGameObject);
				}
			}
		}
    }
	
	// Convert into hierarchical prefab
	public static void ClearHierarchicalPrefab(GameObject a_rGameObjectToConvert)
    {	
		// Loop through the child transforms
		// and search the nested prefab
		Transform a_rGameObjectToConvertTransform = a_rGameObjectToConvert.transform;;
		// Loop in reverse to allow direct child destruction
		for(int i = a_rGameObjectToConvertTransform.GetChildCount() - 1; i >= 0; i--)
		{	
			Transform rChildTransform = a_rGameObjectToConvertTransform.GetChild(i);
			GameObject rChildGameObject = rChildTransform.gameObject;
			
			// If we encounter a nested prefab
			if(IsNestedPrefab(rChildGameObject, a_rGameObjectToConvert))
			{
				// Destroy the nested prefab game object
				Editor.DestroyImmediate(rChildGameObject);
			}
		}
    }
	
	// Is a nested prefab
	public static bool IsNestedPrefab(GameObject a_rChild, GameObject a_rRoot)
	{
		HierarchicalPrefabInstance rHierarchicalPrefabInstance = a_rChild.GetComponent<HierarchicalPrefabInstance>();
		if(rHierarchicalPrefabInstance != null)
		{
			return rHierarchicalPrefabInstance.HierarchicalPrefab != null;
		}
		else
		{
			Object rPrefabOfChild = PrefabUtility.GetPrefabObject(a_rChild);
			Object rPrefabOfRoot = PrefabUtility.GetPrefabObject(a_rRoot);
			return a_rChild.transform.parent != null && rPrefabOfChild != null && rPrefabOfChild != rPrefabOfRoot;
		}
	}
	
	// Notify the other instances of update
	public static void NotifyInstancesOfHierarchicalPrefabUpdate(Object a_rHierarchicalPrefab)
    {
		NotifyInstancesOfHierarchicalPrefabUpdate(a_rHierarchicalPrefab, null);
	}
	
	// Notify the other instances of update
	public static void NotifyInstancesOfHierarchicalPrefabUpdate(Object a_rHierarchicalPrefab, HierarchicalPrefabInstance a_rHierarchicalInstanceCaller)
    {
		// Loop through the instances of this prefab and notify them
		foreach(HierarchicalPrefabInstance rHierarchicalPrefabInstance in GetAllInstancesOfHierarchicalPrefab(a_rHierarchicalPrefab))
		{
			rHierarchicalPrefabInstance.OnHierarchicalPrefabUpdate(a_rHierarchicalInstanceCaller);
		}
	}
	
	// Get all the hierarchical prefab instances
	public static List<HierarchicalPrefabInstance> GetAllInstancesOfHierarchicalPrefab(Object a_rHierarchicalPrefab)
    {
		List<HierarchicalPrefabInstance> oHierarchicalPrefabInstances = new List<HierarchicalPrefabInstance>();
		
		if(a_rHierarchicalPrefab != null)
		{
			// Loop through the hierarchical prefab instances
			foreach(HierarchicalPrefabInstance rHierarchicalPrefabInstance in GameObject.FindObjectsOfType(typeof(HierarchicalPrefabInstance)))
			{
				// If it's an instance of the searched prefab
				if(rHierarchicalPrefabInstance.PrefabObject == a_rHierarchicalPrefab)
				{
					oHierarchicalPrefabInstances.Add(rHierarchicalPrefabInstance);
				}
			}
		}
		
		return oHierarchicalPrefabInstances;
	}
	
	// Is Cyclic?
	public static bool IsHierarchicalPrefabInstanceCyclic(HierarchicalPrefabInstance a_rTestedHierarchicalPrefabInstance, Object a_rPrefabToSaveInto)
    {	
		return IsHierarchicalPrefabTreeContainAnInstantiatorOf(a_rTestedHierarchicalPrefabInstance.transform, PrefabUtility.GetPrefabObject(a_rPrefabToSaveInto));
    }
	
	// Is the hierarchical prefab tree contain an instantiator Of a hierarchical prefab
	public static bool IsHierarchicalPrefabTreeContainAnInstantiatorOf(Transform a_rHierarchicaPrefabTreeRoot, Object a_rPrefabObject)
    {
		// Try to get an instantiator on the current sub tree root
		NestedPrefabsInstantiator rNestedPrefabsInstantiator = a_rHierarchicaPrefabTreeRoot.GetComponent<NestedPrefabsInstantiator>();
		if(rNestedPrefabsInstantiator != null)
		{
			// Loop through the to be instantiated prefab
			foreach(NestedPrefabData rNestedPrefabDatas in rNestedPrefabsInstantiator.NestedPrefabDatas)
			{
				// If we found our prefab in this instantiator
				if(rNestedPrefabDatas.PrefabObject == a_rPrefabObject)
				{
					return true;
				}
				// Else keep searching on the nested prefab
				else
				{
					// Try to jump onto the prefab to spawn
					GameObject rNestedPrefab = rNestedPrefabDatas.Prefab;
					if(rNestedPrefab != null)
					{
						if(IsHierarchicalPrefabTreeContainAnInstantiatorOf(rNestedPrefab.transform, a_rPrefabObject))
						{
							return true;
						}
					}
				}
			}
		}
		
		// Try in the children
		foreach(Transform rChild in a_rHierarchicaPrefabTreeRoot.transform)
		{	
			if(IsHierarchicalPrefabTreeContainAnInstantiatorOf(rChild, a_rPrefabObject))
			{
				return true;
			}
		}
		
		return false;
    }
	
	// Get the game object contained in a prefab object
	public static GameObject GetPrefabGameObject(Object a_rPrefabObject)
	{
		return AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(a_rPrefabObject), typeof(GameObject)) as GameObject;
	}
	
	// Get the game object contained in a prefab object
	public static Object GetPrefabObject(Object a_rPrefabGameObject)
	{
		return PrefabUtility.GetPrefabObject(PrefabUtility.GetPrefabParent(a_rPrefabGameObject));
	}
	
	// Disconnect button action
	public static void DisconnectFromHierarchicalPrefabAndFromClassicPrefab(ref GameObject a_rGameObjectToDisconnect)
	{ 
		// Erase all nested prefabs specific components
		NestedPrefabEditorUtility.DestroyComponentsInChildren<HierarchicalPrefabInstance>(a_rGameObjectToDisconnect);
		NestedPrefabEditorUtility.DestroyComponentsInChildren<NestedPrefabsInstantiator>(a_rGameObjectToDisconnect);
		
		// Duplicate
		GameObject rGameObjectDisconnectedClone = Editor.Instantiate(a_rGameObjectToDisconnect) as GameObject;
		rGameObjectDisconnectedClone.name = a_rGameObjectToDisconnect.name;
		rGameObjectDisconnectedClone.transform.parent = a_rGameObjectToDisconnect.transform.parent;
		rGameObjectDisconnectedClone.transform.localPosition = a_rGameObjectToDisconnect.transform.localPosition;
		rGameObjectDisconnectedClone.transform.localRotation = a_rGameObjectToDisconnect.transform.localRotation;
		rGameObjectDisconnectedClone.transform.localScale = a_rGameObjectToDisconnect.transform.localScale;
		
		// Destroy the game object to disconnect
		Editor.DestroyImmediate(a_rGameObjectToDisconnect);
		
		// Change the ref to the cloned object
		a_rGameObjectToDisconnect = rGameObjectDisconnectedClone;
    }
	
	// Disconnect button action
	public static void DisconnectFromHierarchicalPrefab(GameObject a_rGameObjectToDisconnect)
	{ 
		// Erase all nested prefabs specific components
		NestedPrefabEditorUtility.DestroyComponentsInChildren<HierarchicalPrefabInstance>(a_rGameObjectToDisconnect);
		NestedPrefabEditorUtility.DestroyComponentsInChildren<NestedPrefabsInstantiator>(a_rGameObjectToDisconnect);
    }
	
	// Disconnect button action
	public static void DisconnectResourceFromHierarchicalPrefab(GameObject a_rGameObjectToDisconnect)
	{ 
		// Erase all nested prefabs specific components
		NestedPrefabEditorUtility.DestroyComponentsInChildrenOnResource<HierarchicalPrefabInstance>(a_rGameObjectToDisconnect);
		NestedPrefabEditorUtility.DestroyComponentsInChildrenOnResource<NestedPrefabsInstantiator>(a_rGameObjectToDisconnect);
    }
}
#endif