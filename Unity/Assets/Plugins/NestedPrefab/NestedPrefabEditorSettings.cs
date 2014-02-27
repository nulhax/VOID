using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
// Nested prefab editor settings
public static class NestedPrefabEditorSettings
{
	// The must compile save id
	private const string mc_oMustCompileSaveId = "NestedPrefabEditor_MustCompile"; 
	
	// Editor mode accessor
	public static bool MustCompile
	{
		get
		{
			return EditorPrefs.GetBool(mc_oMustCompileSaveId, true);	
		}
		
		set
		{
			EditorPrefs.SetBool(mc_oMustCompileSaveId, value);
		}
	}
}
#endif