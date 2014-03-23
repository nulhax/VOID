using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
//  The nested prefab property modifications
public class NestedPrefabPropertyModifications
{
	[SerializeField]
	private List<NestedPrefabPropertyModification> m_oNestedPrefabPropertyModifications = new List<NestedPrefabPropertyModification>();
	
	// Accessor to avoid warning on player build
	public List<NestedPrefabPropertyModification> NestedPrefabPropertyModificationsList
	{
		get
		{
			return m_oNestedPrefabPropertyModifications;
		}
	}
	
#if UNITY_EDITOR
	public void LoadPropertyModifications(UnityEngine.Object a_rTargetPrefab)
	{
		PropertyModification[] oPropertyModifications;
		CopyTo(out oPropertyModifications);
		PrefabUtility.SetPropertyModifications(a_rTargetPrefab, oPropertyModifications);
	}
	
	public void SavePropertyModifications(UnityEngine.Object a_rTargetPrefabInstance)
	{		
		PropertyModification[] oPropertyModifications;
		oPropertyModifications = PrefabUtility.GetPropertyModifications(a_rTargetPrefabInstance);
		CopyFrom(oPropertyModifications, a_rTargetPrefabInstance);
	}
	
	public void CopyTo(out PropertyModification[] a_rPropertyModifications)
	{	
		a_rPropertyModifications = new PropertyModification[m_oNestedPrefabPropertyModifications.Count];
		for(int i = 0; i < a_rPropertyModifications.Length; i++)
		{
			PropertyModification rPropertyModification = new PropertyModification();				
			NestedPrefabPropertyModification rNestedPrefabPropertyModification = m_oNestedPrefabPropertyModifications[i];
			
			rNestedPrefabPropertyModification.CopyTo(ref rPropertyModification);
			a_rPropertyModifications[i] = rPropertyModification;
		}
	}
	
	public void CopyFrom(PropertyModification[] a_rPropertModifications, UnityEngine.Object a_rTargetPrefabInstance)
	{
		m_oNestedPrefabPropertyModifications.Clear();
		
		if(a_rPropertModifications != null)
		{
			foreach(PropertyModification rPropertyModification in a_rPropertModifications)
			{
				NestedPrefabPropertyModification rNestedPrefabPropertyModification = new NestedPrefabPropertyModification();
				rNestedPrefabPropertyModification.CopyFrom(rPropertyModification);
				if(rNestedPrefabPropertyModification.CanUse(a_rTargetPrefabInstance))
				{
					m_oNestedPrefabPropertyModifications.Add(rNestedPrefabPropertyModification);
				}
			}
		}
	}
#endif
}