using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Nested prefab editor window
public class NestedPrefabEditorWindow : EditorWindow
{
	// The selection mode
	private enum ESelectionMode
	{
		None,
		InPlayMode,
		IsAHierarchicalPrefabResource,
		IsAResourceButNotAHierarchicalPrefab,
		IsNotAHierarchicalPrefabInstance,
		IsAConnectedHierarchicalInstancePrefab,
		IsAHierarchicalInstancePrefabWithoutResource
	}
	
	// The last save file path id 
	public const string mc_oLastSaveFilePathId = "NestedEditor_LastFilePath";
	
	// The button dimension rectangle
	public readonly Rect mc_oButtonDimensionRectangle = new Rect(5, 5, 100, 20);
	
	// The selection mode
	private ESelectionMode m_eSelectionMode;
	
	// The selected game object
	private GameObject m_rSelectedGameObject;
	
	// The selected game object
	private HierarchicalPrefabInstance m_rSelectedHierarchicalPrefabInstance;
	
	// Must compile
	private bool m_bMustCompile;
	
	[MenuItem ("Nested Prefab/Open Editor")]
	// Open the nested prefab editor
    static void OpenNestedPrefabEditor()
	{		
    	EditorWindow.GetWindow(typeof(NestedPrefabEditorWindow), false, "Nested Prefab");
    }
	
	// Update
	private void Update()
	{
		// Update the selection
		if(UpdateSelection())
		{
			// Force repaint
			Repaint();
		}
	}
	
	// Called on gui refresh
	private void OnGUI()
	{		
		// Ensure the selection is up to date
		UpdateSelection();
			
		// Display the editor
		DisplayEditor();
    }
	
	// Update selection
	private bool UpdateSelection()
	{	
		// Save the current selection
		ESelectionMode eSelectionModeLast = m_eSelectionMode;
		GameObject rSelectedGameObjectLast = m_rSelectedGameObject;
		HierarchicalPrefabInstance rSelectedHierarchicalPrefabInstanceLast = m_rSelectedHierarchicalPrefabInstance;
		
		// Update the selected game object
		m_rSelectedGameObject = Selection.activeGameObject;
		
		// Update the selection mode
		if(Application.isPlaying)
		{
			m_eSelectionMode = ESelectionMode.InPlayMode;
			m_rSelectedGameObject = null;
		}
		else if(m_rSelectedGameObject == null)
		{
			m_eSelectionMode = ESelectionMode.None;
		}
		else if(EditorUtility.IsPersistent(m_rSelectedGameObject))
		{
			m_rSelectedHierarchicalPrefabInstance = m_rSelectedGameObject.GetComponent<HierarchicalPrefabInstance>();
			
			if(m_rSelectedHierarchicalPrefabInstance == null)
			{
				m_eSelectionMode = ESelectionMode.IsAResourceButNotAHierarchicalPrefab;
			}
			else
			{
				m_eSelectionMode = ESelectionMode.IsAHierarchicalPrefabResource;
			}
		}
		else 
		{	
			m_rSelectedHierarchicalPrefabInstance = m_rSelectedGameObject.GetComponent<HierarchicalPrefabInstance>();
			
			if(m_rSelectedHierarchicalPrefabInstance == null)
			{
				m_eSelectionMode = ESelectionMode.IsNotAHierarchicalPrefabInstance;
			}
			else
			{
				if(m_rSelectedHierarchicalPrefabInstance.HierarchicalPrefab == null)
				{
					m_eSelectionMode = ESelectionMode.IsAHierarchicalInstancePrefabWithoutResource;
				}
				else
				{
					m_eSelectionMode = ESelectionMode.IsAConnectedHierarchicalInstancePrefab;
				}
			}
		}
		
		bool bMustCompileLast = m_bMustCompile;
		m_bMustCompile = NestedPrefabEditorSettings.MustCompile;
		
		// Check if there was change
		if(	eSelectionModeLast != m_eSelectionMode
		||	rSelectedGameObjectLast != m_rSelectedGameObject
		||	rSelectedHierarchicalPrefabInstanceLast != m_rSelectedHierarchicalPrefabInstance
		|| 	bMustCompileLast != m_bMustCompile)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	// Display the editor
	private Vector2 m_f2ScrollPositionEditor;
	private void DisplayEditor()
	{
		m_f2ScrollPositionEditor = GUILayout.BeginScrollView(m_f2ScrollPositionEditor);
		GUILayout.BeginVertical();
		{
			DisplayEditorSettings();
			EditorGUILayout.Space();
			DisplayHierarchicalPrefabInspector();
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}
	
	// Display the editor settings
	void DisplayEditorSettings()
	{
		bool bGUIEnabled = GUI.enabled;
		GUI.enabled = bGUIEnabled && Application.isPlaying == false;
		GUILayout.BeginVertical();
		{
			GUILayout.Label("Project");
			
			EditorGUILayout.Space();

			EditorGUI.indentLevel++;
			DisplayCompileButtons();
			EditorGUI.indentLevel--;
		}
		GUILayout.EndVertical();
		GUI.enabled = bGUIEnabled;
	}
	
	// Display the hierarchical prefab inspector
	private void DisplayHierarchicalPrefabInspector()
	{	
		GUILayout.BeginVertical();
		{
			GUILayout.Label("Inspector");
			
			EditorGUILayout.Space();
			
			DisplayTitle();
		
			DisplaySaveGUI();
		}
		GUILayout.EndVertical();
	}
	
	// Display save GUI
	void DisplaySaveGUI()
	{	
		switch(m_eSelectionMode)
		{
			case ESelectionMode.None:
			{
				GUI.enabled = false;
				{
					GUILayout.BeginHorizontal();
					{
						DisplaySaveAsButton();
						
						GUI.enabled = true;
						{
							DisplayLoadButton();
						}
						GUI.enabled = false;
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					{
						DisplayApplyButton();
						DisplayRevertButton();
						DisplayResetButton();
					}
					GUILayout.EndHorizontal();
				
					DisplayDisconnectButton();
				}
				GUI.enabled = true;
			}
			break;
			
			case ESelectionMode.InPlayMode:
			{
				GUI.enabled = false;
				{
					GUILayout.BeginHorizontal();
					{
						DisplaySaveAsButton();
						
						DisplayLoadButton();
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					{
						DisplayApplyButton();
						DisplayRevertButton();
						DisplayResetButton();
					}
					GUILayout.EndHorizontal();
				
					DisplayDisconnectButton();
				}
				GUI.enabled = true;
			}
			break;
			
			case ESelectionMode.IsAHierarchicalPrefabResource:
			{
				GUI.enabled = false;
				{
					GUILayout.BeginHorizontal();
					{
						DisplaySaveAsButton();
						DisplayLoadButton();
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					{
						DisplayApplyButton();
						DisplayRevertButton();
						DisplayResetButton();
					}
					GUILayout.EndHorizontal();
				
					DisplayDisconnectButton();
				}
				GUI.enabled = true;
			}
			break;
			
			case ESelectionMode.IsAResourceButNotAHierarchicalPrefab:
			{
				GUI.enabled = false;
				{
					GUILayout.BeginHorizontal();
					{
						DisplaySaveAsButton();
						DisplayLoadButton();
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					{
						DisplayApplyButton();
						DisplayRevertButton();
						DisplayResetButton();
					}
					GUILayout.EndHorizontal();
				
					DisplayDisconnectButton();
				}
				GUI.enabled = true;
			}
			break;
			
			case ESelectionMode.IsNotAHierarchicalPrefabInstance:
			{
				GUILayout.BeginHorizontal();
				{
					DisplaySaveAsButton();
					DisplayLoadButton();
				}
				GUILayout.EndHorizontal();
				
				GUI.enabled = false;
				{
					GUILayout.BeginHorizontal();
					{
						DisplayApplyButton();
						DisplayRevertButton();
						DisplayResetButton();
					}
					GUILayout.EndHorizontal();
				
					DisplayDisconnectButton();
				}
				GUI.enabled = true;
			}
			break;
			
			case ESelectionMode.IsAConnectedHierarchicalInstancePrefab:
			{
				GUILayout.BeginHorizontal();
				{
					DisplaySaveAsButton();
					DisplayLoadButton();
				}
				GUILayout.EndHorizontal();
			
				GUILayout.BeginHorizontal();
				{
					DisplayApplyButton();
					DisplayRevertButton();
					DisplayResetButton();
				}
				GUILayout.EndHorizontal();
			
				DisplayDisconnectButton();
			}
			break;
			
			case ESelectionMode.IsAHierarchicalInstancePrefabWithoutResource:
			{
				GUILayout.BeginHorizontal();
				{
					DisplaySaveAsButton();
					DisplayLoadButton();
				}
				GUILayout.EndHorizontal();
				
				GUI.enabled = false;
				{
					GUILayout.BeginHorizontal();
					{
						DisplayApplyButton();
						DisplayRevertButton();
						DisplayResetButton();
					}
					GUILayout.EndHorizontal();
				}
				GUI.enabled = true;
			
				DisplayDisconnectButton();
			}
			break;
		}
	}
	
	// Display title
	void DisplayTitle()
	{
		switch(m_eSelectionMode)
		{
			case ESelectionMode.None:
			{
				Color oColorSave = GUI.color;
				Color oContentColorSave = GUI.contentColor;
				GUI.color = Color.grey;
				GUI.contentColor = Color.white;
				
				GUILayout.TextArea("Select a Game Object.");
			
				GUI.color = oColorSave;
				GUI.contentColor = oContentColorSave;
			}
			break;
			
			case ESelectionMode.InPlayMode:
			{
				Color oColorSave = GUI.color;
				Color oContentColorSave = GUI.contentColor;
				GUI.color = Color.grey;
				GUI.contentColor = Color.white;
				
				GUILayout.TextArea("Can't Edit in Play.");
			
				GUI.color = oColorSave;
				GUI.contentColor = oContentColorSave;
			}
			break;
			
			case ESelectionMode.IsNotAHierarchicalPrefabInstance:
			case ESelectionMode.IsAResourceButNotAHierarchicalPrefab:
			{
				Color oColorSave = GUI.color;
				Color oContentColorSave = GUI.contentColor;
				GUI.color = Color.yellow;
				GUI.contentColor = Color.white;
				
				GUILayout.TextArea("Not a Hierarchical Prefab.");
			
				GUI.color = oColorSave;
				GUI.contentColor = oContentColorSave;
			}
			break;
			
			case ESelectionMode.IsAConnectedHierarchicalInstancePrefab:
			{
				Color oColorSave = GUI.color;
				Color oContentColorSave = GUI.contentColor;
				GUI.color = Color.green;
				GUI.contentColor = Color.white;
				
				GUILayout.TextArea("Instance of " + m_rSelectedHierarchicalPrefabInstance.HierarchicalPrefab.name + ".");
			
				GUI.color = oColorSave;
				GUI.contentColor = oContentColorSave;
			}
			break;
			
			case ESelectionMode.IsAHierarchicalPrefabResource:
			{
				Color oColorSave = GUI.color;
				Color oContentColorSave = GUI.contentColor;
				GUI.color = Color.green;
				GUI.contentColor = Color.white;
				
				GUILayout.TextArea(m_rSelectedHierarchicalPrefabInstance.name);
			
				GUI.color = oColorSave;
				GUI.contentColor = oContentColorSave;
			}
			break;
			
			case ESelectionMode.IsAHierarchicalInstancePrefabWithoutResource:
			{
				Color oColorSave = GUI.color;
				Color oContentColorSave = GUI.contentColor;
				GUI.color = Color.red;
				GUI.contentColor = Color.white;
			
				GUILayout.TextArea("Resource lost!");
				
				GUI.color = oColorSave;
				GUI.contentColor = oContentColorSave;
			}
			break;
		}
	}
	
	// Display the save as button
	void DisplaySaveAsButton()
	{
		// Button save as...
		if(GUILayout.Button(new GUIContent("Save as...", null, "Save the Game Object as a hierarchical prefab."),
			GUILayout.Width(mc_oButtonDimensionRectangle.width),
			GUILayout.Height(mc_oButtonDimensionRectangle.height),
			GUILayout.ExpandWidth(true)))
		{
			SaveAsButtonAction();
		}
	}
	
	// Display the save as button
	void DisplayLoadButton()
	{
		// Button load
		if(GUILayout.Button(new GUIContent("Load", null, "Load a prefab to replace the selected object."),
			GUILayout.Width(mc_oButtonDimensionRectangle.width),
			GUILayout.Height(mc_oButtonDimensionRectangle.height),
			GUILayout.ExpandWidth(true)))
		{
			LoadButtonAction();
		}
	}
	
	// Display the apply button
	void DisplayApplyButton()
	{
		// Apply button
		if(GUILayout.Button(new GUIContent("Apply", null, "Apply the changes made to the hierarchical prefab."),
			GUILayout.Width(mc_oButtonDimensionRectangle.width),
			GUILayout.Height(mc_oButtonDimensionRectangle.height),
			GUILayout.ExpandWidth(true)))
		{
			ApplyButtonAction();
		}
	}
	
	// Display the revert button
	void DisplayRevertButton()
	{
		// Revert button
		if(GUILayout.Button(new GUIContent("Revert", null, "Revert the changes made to the nested prefab and keep the hierarchy overrides."),
			GUILayout.Width(mc_oButtonDimensionRectangle.width),
			GUILayout.Height(mc_oButtonDimensionRectangle.height),
			GUILayout.ExpandWidth(true)))
		{
			RevertButtonAction();
		}
	}
	
	// Display the reset button
	void DisplayResetButton()
	{
		// Reset button
		if(GUILayout.Button(new GUIContent("Reset", null, "Reset the nested prefab to the prefab state and doesn't keep hierarchy overrides."),
			GUILayout.Width(mc_oButtonDimensionRectangle.width),
			GUILayout.Height(mc_oButtonDimensionRectangle.height),
			GUILayout.ExpandWidth(true)))
		{
			ResetButtonAction();
		}
	}
	
	// Display the deconnect button
	void DisplayDisconnectButton()
	{
		EditorGUILayout.Space();
		
		GUILayout.BeginHorizontal();
		{	
			// Disconnect button
			if(GUILayout.Button(new GUIContent("Disconnect", null, "Disconnect from the hierarchical prefab."),
				GUILayout.Width(mc_oButtonDimensionRectangle.width),
				GUILayout.Height(mc_oButtonDimensionRectangle.height),
				GUILayout.ExpandWidth(true)))
			{
				DisconnectButtonAction();
			}
		}
		GUILayout.EndHorizontal();
	}
	
	// Display the Compile button
	void DisplayCompileButtons()
	{
		GUILayout.BeginVertical();
		{	
			GUILayout.BeginHorizontal();
			{
				DisplayCompileSelectedButton();
				DisplayCleanProjectButton();
			}
			GUILayout.EndHorizontal();
				
			DisplayCompileAllButton();
		}
		GUILayout.EndVertical();
	}
	
	// Display the Compile Selected button
	void DisplayCompileSelectedButton()
	{
		bool bGUIEnabledSave = GUI.enabled;
		GUI.enabled = GUI.enabled && NestedPrefabEditorSettings.MustCompile;
		{
			// Compile button
			if(GUILayout.Button(new GUIContent("Compile Selected", null, "Compile the selected hierarchical prefabs"),
				GUILayout.Width(mc_oButtonDimensionRectangle.width),
				GUILayout.Height(mc_oButtonDimensionRectangle.height),
				GUILayout.ExpandWidth(true)))
			{
				CompileSelectedButtonAction();
			}
		}
		GUI.enabled = bGUIEnabledSave;
	}
	
	// Display the Compile button
	void DisplayCompileAllButton()
	{
		bool bGUIEnabledSave = GUI.enabled;
		GUI.enabled = GUI.enabled && NestedPrefabEditorSettings.MustCompile;
		{
			// Compile button
			if(GUILayout.Button(new GUIContent("Compile All", null, "Compile all the hierarchical prefabs of the project."),
				GUILayout.Width(mc_oButtonDimensionRectangle.width),
				GUILayout.Height(mc_oButtonDimensionRectangle.height),
				GUILayout.ExpandWidth(true)))
			{
				CompileAllButtonAction();
			}
		}
		GUI.enabled = bGUIEnabledSave;
	}
	
	// Display the clean project button
	void DisplayCleanProjectButton()
	{
		// Compile button
		if(GUILayout.Button(new GUIContent("Clean Project", null, "Erase all the compiled hierarchical prefabs of the project."),
			GUILayout.Width(mc_oButtonDimensionRectangle.width),
			GUILayout.Height(mc_oButtonDimensionRectangle.height),
			GUILayout.ExpandWidth(true)))
		{
			CleanProjectButtonAction();
		}
	}
	
	// Save as button action
	private void SaveAsButtonAction()
	{ 
		// Chose path
		string oChosenPath = EditorUtility.SaveFilePanel(
                "Save as ...",
                GetLastFilePath(),
                m_rSelectedGameObject.name + ".prefab",
                "prefab");
		
		// If a path has been selected
        if(oChosenPath.Length != 0) 
		{
			// Check if the path is in the asset folder
			if(oChosenPath.Contains(Application.dataPath))
			{
				string oAssetPath = Application.dataPath;
				oAssetPath = oAssetPath.Replace("Assets", "");
				
				string oLocalChosenPath = oChosenPath;
				oLocalChosenPath = oLocalChosenPath.Replace(oAssetPath, "");
			
				// Save
				if(NestedPrefabEditorUtility.SaveAsHierarchicalPrefab(m_rSelectedGameObject, oLocalChosenPath))
				{
					// Ensure the disconnection
					PrefabUtility.DisconnectPrefabInstance(m_rSelectedGameObject);
					
					// Notify the other instances
					NestedPrefabEditorUtility.NotifyInstancesOfHierarchicalPrefabUpdate(m_rSelectedGameObject.GetComponent<HierarchicalPrefabInstance>().PrefabObject);
				}
				
				// Remember the chosen path
				SetLastFilePath(oChosenPath);
			}
			else
			{
				Debug.LogError("Please choose an emplacement included in the Asset folder.");
			}
		}
    }
	
	// Load button action
	private void LoadButtonAction()
	{ 
		string oChosenPath = EditorUtility.OpenFilePanel(
                "Load a prefab to replace the selected object",
                GetLastFilePath(),
                "prefab");
		
		if(oChosenPath.Length != 0) 
		{
			// Check if the path is in the asset folder
			if(oChosenPath.Contains(Application.dataPath))
			{    
				string oAssetPath = Application.dataPath;
				oAssetPath = oAssetPath.Replace("Assets", "");
				string oLocalChosenPath = oChosenPath;
				oLocalChosenPath = oLocalChosenPath.Replace(oAssetPath, "");
			
	            // Load
				NestedPrefabEditorUtility.LoadPrefab(ref m_rSelectedGameObject, oLocalChosenPath);
				Selection.activeGameObject = m_rSelectedGameObject;
				
				// Remember the chosen path relative to asset path
				SetLastFilePath(oChosenPath);
			}
			else
			{
				Debug.LogError("Please choose an emplacement included in the Asset folder.");	
			}
		}
    }
	
	// Apply button action
	private void ApplyButtonAction()
	{ 
		// Save
		m_rSelectedHierarchicalPrefabInstance.ApplyChangesToHierarchicalPrefab();
    }
	
	// Revert button action
	private void RevertButtonAction()
	{ 
		// Revert to the hierarchical instance
		m_rSelectedHierarchicalPrefabInstance.RevertToHierarchicalPrefab();
    }
	
	// Reset button action
	private void ResetButtonAction()
	{ 
		// Reset to the hierarchical instance
		m_rSelectedHierarchicalPrefabInstance.ResetToHierarchicalPrefab();
    }
	
	// Disconnect button action
	private void DisconnectButtonAction()
	{ 
		if(EditorUtility.IsPersistent(m_rSelectedGameObject))
		{
			NestedPrefabEditorUtility.DisconnectResourceFromHierarchicalPrefab(m_rSelectedGameObject);
		}
		else
		{
			NestedPrefabEditorUtility.DisconnectFromHierarchicalPrefabAndFromClassicPrefab(ref m_rSelectedGameObject);
			Selection.activeGameObject = m_rSelectedGameObject;
		}
    }
	
	// Compile Selected button action
	private void CompileSelectedButtonAction()
	{
		HierarchicalPrefabInstance.CompileSelectedHierarchicalPrefabs();
    }
	
	// Compile All button action
	private void CompileAllButtonAction()
	{
		HierarchicalPrefabInstance.CompileAllHierarchicalPrefabs();
    }
	
	// Clean project button action
	private void CleanProjectButtonAction()
	{
		HierarchicalPrefabInstance.ClearAllCompiledHierarchicalPrefabs();
    }
	
	// Get the last file path
	private string GetLastFilePath()
	{ 
		return Application.dataPath + EditorPrefs.GetString(mc_oLastSaveFilePathId, "");
	}
	
	// Get the last file path
	private void SetLastFilePath(string a_rPath)
	{ 
		// Remenber it relative to the project path
		string oPathRelativeToProject = a_rPath.Replace(Application.dataPath, "");
		
		EditorPrefs.SetString(mc_oLastSaveFilePathId, oPathRelativeToProject);
	}
}
