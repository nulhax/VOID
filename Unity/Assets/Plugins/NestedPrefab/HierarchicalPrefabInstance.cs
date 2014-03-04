using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[ExecuteInEditMode()]
[AddComponentMenu("NestedPrefab/HierarchicalPrefab")]
//  The hierarchical prefab
public class HierarchicalPrefabInstance : MonoBehaviour
{
	// The compiled prefab folder path
	private const string mc_oCompiledHierarchicalPrefabFolderPath = "Assets/NestedPrefab/Compiled"; 
	
	// Is the instance out of date
	private bool m_bIsOutOfDate;
	
	// Can instantiate in runtime?
	public bool CanBeInstantiatedAtRuntime
	{
		get
		{
			return m_bCanBeInstantiatedAtRuntime;
		}
		
		set
		{
#if UNITY_EDITOR
			if(m_bCanBeInstantiatedAtRuntime != value)
			{
				m_bCanBeInstantiatedAtRuntime = value;
				OnCanBeInstantiateAtRuntimeChange();
			}
#endif		
		}
	}
	
	// Can instantiate in runtime?
	[HideInInspector]
	[SerializeField]
	private bool m_bCanBeInstantiatedAtRuntime;
	
	[HideInInspector]
	[SerializeField]
	// The hierarchical prefab id
	private int m_iHierarchicalPrefabVersionNumber;
	
	[HideInInspector]
	[SerializeField]
	// The prefab object
	private Object m_rPrefabObject;
	
	[HideInInspector]
	[SerializeField]
	// The hierarchical prefab id
	private GameObject m_rCompiledHierarchicalPrefab;
	
	[HideInInspector]
	[SerializeField]
	// Is this an scene instance?
	private bool m_bSceneInstance;
	
	// Instantiate compiled Version
	public GameObject InstantiateCompiledVersion()
	{
		if(CanBeInstantiatedAtRuntime)
		{
			if(m_rCompiledHierarchicalPrefab == null)
			{
				DisplayUncompiledPrefabError();
				return null;
			}
			else
			{	
				return Instantiate(m_rCompiledHierarchicalPrefab) as GameObject;
			}
		}
		else
		{
			DisplayCantInstantiateAtRuntimeError();
			return null;
		}
	}
	
	// Instantiate compiled Version
	public GameObject InstantiateCompiledVersion(Vector3 a_f3Position, Quaternion a_oRotation)
	{
		if(CanBeInstantiatedAtRuntime)
		{
			if(m_rCompiledHierarchicalPrefab == null)
			{
				DisplayUncompiledPrefabError();
				return null;
			}
			else
			{
				return Instantiate(m_rCompiledHierarchicalPrefab, a_f3Position, a_oRotation) as GameObject;
			}
		}
		else
		{
			DisplayCantInstantiateAtRuntimeError();
			return null;
		}
	}
	
	// Display the uncompiled prefab error
	private void DisplayUncompiledPrefabError()
	{
		Debug.LogError("You can't instantiate an uncompiled hierarchical prefab! Use the Compile button on the nested prefab editor.");
	}
	
	// Display the cant instantiate at runtime error
	private void DisplayCantInstantiateAtRuntimeError()
	{
		Debug.LogError("You try to instantiate a hierarchical prefab don't marked as 'Can Be Instantiated'. To do so check the corresponding check box in the inspector of the HierarchicalPrefabInstance component.");
	}
	
#if UNITY_EDITOR
	// The hierarchical prefab
	public GameObject HierarchicalPrefab
	{
		get
		{
			GameObject rPrefabParent = PrefabUtility.GetPrefabParent(gameObject) as GameObject;
			
			// The hierarchical prefab has to be the root
			if(rPrefabParent == PrefabUtility.FindPrefabRoot(rPrefabParent))
			{
				return rPrefabParent;
			}
			else
			// Corrupted prefab
			{
				return null;
			}
		}
	}
	
	// The hierarchical prefab
	public Object PrefabObject
	{
		get
		{
			return m_rPrefabObject;
		}
	}
	
	// Awake
	private void Awake()
	{
		if(Application.isPlaying)
		{
			if(m_bSceneInstance == false)
			{
				Debug.LogWarning("Warning : Using GameObject.Instantiate on hierarchical prefabs at runtime won't work on a build player! Use HierarchicalPrefabUtility.Instantiate instead.");
				if(CanBeInstantiatedAtRuntime == false || m_rCompiledHierarchicalPrefab == null)
				{
					Debug.LogWarning("And remember to check the 'Can Be Instantiated' box, then compile the last version of the hierarchical prefab.");
				}
					
				// Auto destroy
				Destroy(gameObject);
			}
			Destroy(this);
		}
	}
	
	// Start
	private void Start()
	{
		if(Application.isPlaying == false)
		{
			// If we were just added to the scene
			// Deploy the hierarchy
			if(m_bSceneInstance == false)
			{
				DeployHierarchy();
			}
			
			DisconnectFromClassicPrefab();
			
			// If the resource has changed
			if(HasPrefabChanged())
			{
				OnHierarchicalPrefabUpdate();
			}
		}
	}
	
	// Update
	private void Update()
	{
		if(m_bIsOutOfDate)
		{
			OnHierarchicalPrefabUpdate();
			m_bIsOutOfDate = false;
		}
	}
	
	// OnEnable
	private void OnEnable()
	{
		if(Application.isPlaying == false)
		{
			// If we are a scene instance
			if(m_bSceneInstance)
			{
				if(HasPrefabChanged())
				{
					m_bIsOutOfDate = true;
				}
			}
		}
	}
	
	// Has prefab changed?
	private bool HasPrefabChanged()
	{
		// If the resource has changed
		GameObject rHierarchicalPrefab = HierarchicalPrefab;
		if(rHierarchicalPrefab != null)
		{
			HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = rHierarchicalPrefab.GetComponent<HierarchicalPrefabInstance>();
			
			if(rHierarchicalPrefabInstanceModel != null && m_iHierarchicalPrefabVersionNumber != rHierarchicalPrefabInstanceModel.m_iHierarchicalPrefabVersionNumber)
			{
				return true;
			}
		}
		else
		{
			// The resource can have been overrided
			return true;
		}
		
		return false;
	}
	
	// Is prefab a valid instance
	public bool IsValid()
	{
		return m_bSceneInstance == false;
	}
	
	// On Hierarchical prefab update
	public void OnHierarchicalPrefabUpdate()
	{
		OnHierarchicalPrefabUpdate(null);
	}
		
	// On Hierarchical prefab update
	public void OnHierarchicalPrefabUpdate(HierarchicalPrefabInstance a_rHierarchicalInstanceCaller)
	{
		if(this != null)
		{
			// if the connection with the hierarchical prefab is broken
			if(HierarchicalPrefab == null)
			{
				// If the prefab is still there and has just been replaced
				if(m_rPrefabObject != null)
				{
					// Try to grab the nested prefab data from the potential instantiator
					NestedPrefabData rNestedPrefabData = TryGrabNestedPrefabData();
					
					// Replace the current object by an instance of the new prefab
					GameObject rNewInstance = PrefabUtility.InstantiatePrefab(NestedPrefabEditorUtility.GetPrefabGameObject(m_rPrefabObject)) as GameObject;
					HierarchicalPrefabInstance rNewHierarchicalPrefabInstance = rNewInstance.GetComponent<HierarchicalPrefabInstance>();
					// If nested
					if(rNestedPrefabData != null && rNewHierarchicalPrefabInstance != null && transform.parent != null)
					{
						rNewHierarchicalPrefabInstance.ReloadNestedPrefabData(rNestedPrefabData);
						// Change the parent without changing the local transform information
						NestedPrefabUtility.ChangeParentAndKeepSameLocalTransform(rNewHierarchicalPrefabInstance.transform, transform.parent);
					}
					else
					{
						Vector3 f3LocalScaleSave = rNewInstance.transform.localScale;
						rNewInstance.transform.parent = transform.parent;
						rNewInstance.transform.localPosition = transform.localPosition;
						rNewInstance.transform.localRotation = transform.localRotation;
						rNewInstance.transform.localScale = f3LocalScaleSave;
					}
					
					// Auto destruction
					Editor.DestroyImmediate(gameObject);
				}
			}
			else
			{
				// Revert to hierarchical prefab
				//RevertToHierarchicalPrefab();
				if(this == a_rHierarchicalInstanceCaller)
				{
					// if it's the updated instance we just redeploy the hierarchy
					DeployHierarchy();
				}
				else
				{
					// Revert to hierarchical prefab
					RevertToHierarchicalPrefab();
				}
			}
		}
	}
	
	// Revert to the hierarchical prefab
	public void ApplyChangesToHierarchicalPrefab()
	{
		// Try to get the prefab parent
		GameObject rHierarchicalPrefab = HierarchicalPrefab;
		if(rHierarchicalPrefab != null)
		{
			// Get the prefab path
			string oPrefabPath = AssetDatabase.GetAssetPath(rHierarchicalPrefab);
			
			// Save the prefab at this path
			if(NestedPrefabEditorUtility.SaveAsHierarchicalPrefab(gameObject, oPrefabPath))
			{
				// Ensure the disconnection
				PrefabUtility.DisconnectPrefabInstance(gameObject);
				
				// Notify the other instances
				NestedPrefabEditorUtility.NotifyInstancesOfHierarchicalPrefabUpdate(m_rPrefabObject, this);
			}
		}
	}
	
	// Revert to the hierarchical prefab
	public void RevertToHierarchicalPrefab()
	{
		// If it is nested in an instance of same prefab take some care
		Transform rSamePrefabParent;
		Vector3 f3LocalPositionSave;
		Quaternion oLocalRotationSave;
		SaveNestedPlaceIfCyclic(out rSamePrefabParent, out f3LocalPositionSave, out oLocalRotationSave);
		
		// Remove nested hierarchical instance of same prefab
		RemoveNestedHierarchicalInstanceOfSamePrefab();
		
		// Try to grab the nested prefab data from the potential instantiator
		NestedPrefabData rNestedPrefabData = TryGrabNestedPrefabData();
		
		PrefabUtility.ReconnectToLastPrefab(gameObject);
		if(this != null)
		{
			ReloadNestedPrefabData(rNestedPrefabData);
			PrefabUtility.DisconnectPrefabInstance(gameObject);
			ForcePrefabInformationUpdate();
			TriggerAllInstantiators();

			// If we were working on a hierarchical instance nested in a instance of same prefab
			RestoreNestedPlaceIfCyclic(rSamePrefabParent, f3LocalPositionSave, oLocalRotationSave);
		}
	}
	
	// Revert to the hierarchical prefab
	public void ResetToHierarchicalPrefab()
	{
		// If it is nested in an instance of same prefab take some care
		Transform rSamePrefabParent;
		Vector3 f3LocalPositionSave;
		Quaternion oLocalRotationSave;
		SaveNestedPlaceIfCyclic(out rSamePrefabParent, out f3LocalPositionSave, out oLocalRotationSave);
		
		// Remove nested hierarchical instance of same prefab
		RemoveNestedHierarchicalInstanceOfSamePrefab();
		
		PrefabUtility.ReconnectToLastPrefab(gameObject);
		if(this != null)
		{
			PrefabUtility.RevertPrefabInstance(gameObject);
			PrefabUtility.DisconnectPrefabInstance(gameObject);
			ForcePrefabInformationUpdate();
			TriggerAllInstantiators();
			
			// If we were working on a hierarchical instance nested in a instance of same prefab
			RestoreNestedPlaceIfCyclic(rSamePrefabParent, f3LocalPositionSave, oLocalRotationSave);
		}
	}
	
	// Save cyclic nested place
	private void SaveNestedPlaceIfCyclic(out Transform a_rParentIfCyclic, out Vector3 a_f3LocalPositionSave, out Quaternion a_oLocalRotationSave)
	{
		a_rParentIfCyclic = null;
		a_f3LocalPositionSave = Vector3.zero;
		a_oLocalRotationSave = Quaternion.identity;
		
		// If it is nested in an instance of same prefab take some care
		if(IsNestedInHierarchicalInstanceOfSamePrefab())
		{
			// Save
			a_rParentIfCyclic = transform.parent;
			a_f3LocalPositionSave = transform.localPosition;
			a_oLocalRotationSave = transform.localRotation;
			
			// Avoid prefab recursive prefab restoration
			transform.parent = null;
		}
	}
	
	// Restore cyclic nested place
	private void RestoreNestedPlaceIfCyclic(Transform a_rParentIfCyclic, Vector3 a_f3LocalPositionSave, Quaternion a_oLocalRotationSave)
	{
		// If we were working on a hierarchical instance nested in a instance of same prefab
		if(a_rParentIfCyclic != null)
		{
			// Restore
			transform.parent = a_rParentIfCyclic;
			transform.localPosition = a_f3LocalPositionSave;
			transform.localRotation = a_oLocalRotationSave;
		}
	}
	
	// Try to grab the nested prefab data of the instance
	private NestedPrefabData TryGrabNestedPrefabData()
	{
		// Try to get a parent hierarchical prefab
		NestedPrefabsInstantiator rHierarchicalPrefabInstantiator = TryGrabHierarchicalPrefabInstantiator();
		if(rHierarchicalPrefabInstantiator != null)
		{
			// Try to grab the nested prefab data
			return rHierarchicalPrefabInstantiator.TryGrabNestedPrefabData(gameObject);
		}
		return null;
	}
	
	// Is the instance nested (directly or not) on an other instance of the same prefab?
	private bool IsNestedInHierarchicalInstanceOfSamePrefab()
	{
		// Climb up the hierarchy and test if there is a hierarchical instance of the same prefab
		Transform rCurrentParent = transform.parent; 
		while(rCurrentParent != null)
		{
			// Is the parent a hierarchical instance?
			HierarchicalPrefabInstance rParentHierarchicalPrefabInstance = rCurrentParent.GetComponent<HierarchicalPrefabInstance>();
			if(rParentHierarchicalPrefabInstance != null)
			{
				// Is it an instance of the same prefab
				if(rParentHierarchicalPrefabInstance.PrefabObject == PrefabObject)
				{
					return true;
				}
			}
			
			// Climb up one level
			rCurrentParent = rCurrentParent.parent;
		}
		
		return false;
	}
	
	// Remove nested hierarchical instance of same prefab
	private void RemoveNestedHierarchicalInstanceOfSamePrefab()
	{
		// Go down the hierarchy and test if there is a hierarchical instance of the same prefab 
		foreach(HierarchicalPrefabInstance rNestedHierarchicalInstance in GetComponentsInChildren<HierarchicalPrefabInstance>())
		{
			if(rNestedHierarchicalInstance != null && rNestedHierarchicalInstance != this)
			{
				// Is it an instance of the same prefab
				if(rNestedHierarchicalInstance.PrefabObject == PrefabObject)
				{
					// Destroy it
					Editor.DestroyImmediate(rNestedHierarchicalInstance.gameObject);
				}
			}
		}
	}
	
	// Reload the nested prefab data (either by its reloading instantiator prefab data or revert to prefab resource)
	private void ReloadNestedPrefabData(NestedPrefabData a_rNestedPrefabData)
	{
		// If there is a nested prefab data
		if(a_rNestedPrefabData != null)
		{
			// Reload property modification
			a_rNestedPrefabData.LoadModifications(gameObject);
			
			return;
		}
		
		// If it doesn't has a instantiator revert to resource
		PrefabUtility.RevertPrefabInstance(gameObject);
	}
	
	// Reinstantiate the hierarchical prefab (either by its instantiator or revert to prefab resource)
	private NestedPrefabsInstantiator TryGrabHierarchicalPrefabInstantiator()
	{
		Transform rParentTransform = transform.parent;
		if(rParentTransform != null)
		{
			return rParentTransform.GetComponent<NestedPrefabsInstantiator>();	
		}
		return null;
	}
	
	// Start the prefab instantiators
	public void TriggerAllInstantiators()
	{
		// loop through all the instantiators and trigger them
		foreach(NestedPrefabsInstantiator rNestedPrefabsInstantiator in gameObject.GetComponentsInChildren<NestedPrefabsInstantiator>())
		{
			rNestedPrefabsInstantiator.TriggerInstantiator();
		}
	}
	
	// Ensure that the hierarchical prefab is deconnected of the classic prefab
	private void DisconnectFromClassicPrefab()
	{
		PrefabUtility.DisconnectPrefabInstance(gameObject);
	}
	
	// Save prefab id
	private void IncrementHierarchicalPrefabVersion()
	{
		m_iHierarchicalPrefabVersionNumber++;
	}
	
	// On save
	public void OnSaveModelInstanceBeforePrefabReplacement(Object a_rPrefabObject)
	{
		// Save the prefab object for recuperation (mainly to react after an other object override our prefab)
		m_rPrefabObject = a_rPrefabObject;
		
		// Increment version
		IncrementHierarchicalPrefabVersion();	
		
		// Clear the scene instance flag
		m_bSceneInstance = false;
		
		// Conserve the compiled prefab
		GameObject rPrefabGameObject = HierarchicalPrefab;
		if(rPrefabGameObject)
		{
			HierarchicalPrefabInstance rHierarchicalPrefabInstance = rPrefabGameObject.GetComponent<HierarchicalPrefabInstance>();
			if(rHierarchicalPrefabInstance != null)
			{
				m_rCompiledHierarchicalPrefab = rHierarchicalPrefabInstance.m_rCompiledHierarchicalPrefab;
			}
		}
	}
	
	// Copy version number
	public void CopyVersionNumber(HierarchicalPrefabInstance a_rHierarchicalPrefabInstanceToCopy)
	{
		m_iHierarchicalPrefabVersionNumber = a_rHierarchicalPrefabInstanceToCopy.m_iHierarchicalPrefabVersionNumber;
	}
	
	// Force the intern prefab update to avoid instance override to get in the way
	private void ForcePrefabInformationUpdate()
	{
		GameObject rHierarchicalPrefab = HierarchicalPrefab;
		if(rHierarchicalPrefab != null)
		{
			HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = rHierarchicalPrefab.GetComponent<HierarchicalPrefabInstance>();
			if(rHierarchicalPrefabInstanceModel != null)
			{
				m_iHierarchicalPrefabVersionNumber = rHierarchicalPrefabInstanceModel.m_iHierarchicalPrefabVersionNumber;
				m_rPrefabObject = rHierarchicalPrefabInstanceModel.m_rPrefabObject;
				m_rCompiledHierarchicalPrefab = rHierarchicalPrefabInstanceModel.m_rCompiledHierarchicalPrefab;
				m_bSceneInstance = true;
			}
		}
	}
	
	// Deploy the hierarchy
	private void DeployHierarchy()
	{
		m_bSceneInstance = true;
		
		// Trigger Instantiators
		TriggerAllInstantiators();
		
		// Deploy the newly created nested hierarchical prefab instances
		foreach(HierarchicalPrefabInstance rHierarchicalInstance in GetComponentsInChildren<HierarchicalPrefabInstance>())
		{
			if(rHierarchicalInstance != this)
			{
				rHierarchicalInstance.DeployHierarchy();
			}
		}
	}
	
	// Compile the hierarchical prefab
	public void Compile()
	{
		// Instantiate a copy of us
		GameObject rHierarchicalPrefabToBeCompiledGameObject = PrefabUtility.InstantiatePrefab(gameObject) as GameObject;
		
		// Force the hierarchy to deploy
		HierarchicalPrefabInstance rHierarchicalPrefabToBeCompiled = rHierarchicalPrefabToBeCompiledGameObject.GetComponent<HierarchicalPrefabInstance>();
		rHierarchicalPrefabToBeCompiled.DeployHierarchy();
		
		// Disconnect from the hierarchical prefab world
		NestedPrefabEditorUtility.DisconnectFromHierarchicalPrefab(rHierarchicalPrefabToBeCompiledGameObject);
		
		// Ensure the compilation directory exist
		Directory.CreateDirectory(Application.dataPath.Replace("Assets", "") + mc_oCompiledHierarchicalPrefabFolderPath);
		
		// Prefab object into which to save
		Object rPrefabObject;
		if(m_rCompiledHierarchicalPrefab == null)
		{
			// Ensure we have a unique save path
			string oSavingPath = mc_oCompiledHierarchicalPrefabFolderPath + "/" + rHierarchicalPrefabToBeCompiledGameObject.name + ".prefab";
			oSavingPath = AssetDatabase.GenerateUniqueAssetPath(oSavingPath);
			
			// Save it into a prefab
			rPrefabObject = PrefabUtility.CreateEmptyPrefab(oSavingPath);
		}
		else
		{
			rPrefabObject = PrefabUtility.GetPrefabObject(m_rCompiledHierarchicalPrefab);
		}
		GameObject rPrefabGameObject = PrefabUtility.ReplacePrefab(rHierarchicalPrefabToBeCompiledGameObject, rPrefabObject);
		
		// Link the instance model to the prefab
		m_rCompiledHierarchicalPrefab = rPrefabGameObject;
		
		// Destroy the instance
		Editor.DestroyImmediate(rHierarchicalPrefabToBeCompiledGameObject);
	}
	
	// Clear all compiled hierarchical prefabs
	public static void ClearAllCompiledHierarchicalPrefabs()
	{
		DestroyAllCompiledHierarchicalPrefabs();
	}
	
	// Get all selected hierarchical prefabs
	public static List<HierarchicalPrefabInstance> GetSelectedHierarchicalPrefabInstanceModels()
	{
		// The selected hierarchical prefab
		List<HierarchicalPrefabInstance> oSelectedHierarchicalPrefabInstanceModels = new List<HierarchicalPrefabInstance>();
		
		// loop through the selected game object
		foreach(GameObject rSelectedGameObject in Selection.gameObjects)
		{
			// Try to get the prefab game object
			GameObject rPrefabGameObject;
			if(EditorUtility.IsPersistent(rSelectedGameObject))
			{
				rPrefabGameObject = rSelectedGameObject;
			}
			else
			{
				rPrefabGameObject = PrefabUtility.GetPrefabParent(rSelectedGameObject) as GameObject;
			}
			rPrefabGameObject = PrefabUtility.FindPrefabRoot(rPrefabGameObject);
			if(rPrefabGameObject != null)
			{
				// If it's a hierarchical prefab
				HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = rPrefabGameObject.GetComponent<HierarchicalPrefabInstance>();
				if(rHierarchicalPrefabInstanceModel != null)
				{
					// Avoid duplicate
					if(oSelectedHierarchicalPrefabInstanceModels.Contains(rHierarchicalPrefabInstanceModel) == false)
					{
						oSelectedHierarchicalPrefabInstanceModels.Add(rHierarchicalPrefabInstanceModel);
					}
				}
			}
		}
		
		return oSelectedHierarchicalPrefabInstanceModels;
	}
	
	// Compile selected hierarchical prefabs
	public static void CompileSelectedHierarchicalPrefabs()
	{
		// Ensure the compilation directory exist
		Directory.CreateDirectory(GetCompiledHierarchicaPrefabFolderGlobalPath());
	
		// Will keep track of the dirty instance model
		// Because if we set the resource to dirty too soon it will only save the root and let the children in the scene
		List<HierarchicalPrefabInstance> oDirtyHierarchicalPrefabInstanceModels = new List<HierarchicalPrefabInstance>();
		
		// loop through all the hierarchical prefabs in the selection
		bool bAtLeastOnePrefabCompiled = false;
		foreach(HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel in GetSelectedHierarchicalPrefabInstanceModels())
		{
			// If need to be compile
			if(rHierarchicalPrefabInstanceModel.CanBeInstantiatedAtRuntime)
			{
				// Compile it
				rHierarchicalPrefabInstanceModel.Compile();
			
				// Add it to the dirty list
				oDirtyHierarchicalPrefabInstanceModels.Add(rHierarchicalPrefabInstanceModel);
				
				// Notify the user
				Debug.Log("Compile : " + AssetDatabase.GetAssetPath(rHierarchicalPrefabInstanceModel.gameObject));
				
				bAtLeastOnePrefabCompiled = true;
			}
		}
		
		// loop through all the hierarchical prefabs in the resources
		foreach(HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel in oDirtyHierarchicalPrefabInstanceModels)
		{
			// Set dirty
			EditorUtility.SetDirty(rHierarchicalPrefabInstanceModel);
		}
		
		if(bAtLeastOnePrefabCompiled == false)
		{
			DisplayNothingToCompileMessage();
		}
	}
	
	// Compile all hierarchical prefabs
	public static void CompileAllHierarchicalPrefabs()
	{
		// Ensure the compilation directory exist
		Directory.CreateDirectory(GetCompiledHierarchicaPrefabFolderGlobalPath());
		
		// Empty it
		DestroyAllCompiledHierarchicalPrefabs();
	
		// Will keep track of the dirty instance model
		// Because if we set the resource to dirty too soon it will only save the root and let the children in the scene
		List<HierarchicalPrefabInstance> oDirtyHierarchicalPrefabInstanceModels = new List<HierarchicalPrefabInstance>();
		
		// loop through all the hierarchical prefabs in the resources
		bool bAtLeastOnePrefabCompiled = false;
		foreach(string rAssetPath in AssetDatabase.GetAllAssetPaths())
		{
			// Is this a prefab?
			GameObject rGameObjectPrefab = AssetDatabase.LoadAssetAtPath(rAssetPath, typeof(GameObject)) as GameObject;
			if(rGameObjectPrefab != null)
			{
				// Try to grab a hierarchical prefab
				HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel = rGameObjectPrefab.GetComponent<HierarchicalPrefabInstance>();
				if(rHierarchicalPrefabInstanceModel != null)
				{
					// If need to be compile
					if(rHierarchicalPrefabInstanceModel.CanBeInstantiatedAtRuntime)
					{
						// Compile it
						rHierarchicalPrefabInstanceModel.Compile();
						// Add it to the dirty list
						oDirtyHierarchicalPrefabInstanceModels.Add(rHierarchicalPrefabInstanceModel);
						
						bAtLeastOnePrefabCompiled = true;
					}
				}
			}
		}
		
		// loop through all the hierarchical prefabs in the resources
		foreach(HierarchicalPrefabInstance rHierarchicalPrefabInstanceModel in oDirtyHierarchicalPrefabInstanceModels)
		{
			// Set dirty
			EditorUtility.SetDirty(rHierarchicalPrefabInstanceModel);
		}
		
		// Compilation is up to date
		NestedPrefabEditorSettings.MustCompile = false;
		
		if(bAtLeastOnePrefabCompiled)
		{
			// Notify the user of the compilation completion 
			Debug.Log("Compilation Successful!");
		}
		else
		{
			DisplayNothingToCompileMessage();
		}
	}
	
	// Display the nothing to compile message
	private static void DisplayNothingToCompileMessage()
	{
		Debug.Log("Nothing to compile (Only the hierarchical prefab marked as 'Can Be Instantiated' need to be compiled).");
	}
	
	// Destroy all compiled hierarchical prefabs
	private static void DestroyAllCompiledHierarchicalPrefabs()
	{
		// loop through all the prefabs in the compiled folder
		foreach(string rGlobalAssetPath in Directory.GetFiles(GetCompiledHierarchicaPrefabFolderGlobalPath()))
		{
			string rLoacalAssetPath = "Assets" + rGlobalAssetPath.Replace(Application.dataPath, "");
			
			// Is this a prefab?
			GameObject rCompiledGameObject = AssetDatabase.LoadAssetAtPath(rLoacalAssetPath, typeof(GameObject)) as GameObject;
			if(rCompiledGameObject != null)
			{
				// Delete it
				AssetDatabase.DeleteAsset(rLoacalAssetPath);
			}
		}
		
		NestedPrefabEditorSettings.MustCompile = true;
	}
		
	// Compile all hierarchical prefabs
	private static string GetCompiledHierarchicaPrefabFolderGlobalPath()
	{
		return Application.dataPath.Replace("Assets", "") + mc_oCompiledHierarchicalPrefabFolderPath;
	}
	
	// On can be instantiate at runtime change
	private void OnCanBeInstantiateAtRuntimeChange()
	{
		if(m_bCanBeInstantiatedAtRuntime)
		{
			NestedPrefabEditorSettings.MustCompile = true;
		}
	}
	
#else
	
	// Awake
	private void Awake()
	{
		Destroy(this);
	}
	
#endif
}