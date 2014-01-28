//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CButtonSelectFacility.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */
using System;


[RequireComponent(typeof(CNetworkView))]
public class CDUIStageModuleCreation : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CModuleInterface.EType m_StartingModuleType = CModuleInterface.EType.INVALID;
	public GameObject m_ParentModuleObject = null;
	public GameObject m_ParentPortObject = null;

	private CNetworkVar<CModuleInterface.EType> m_CurrentModuleType = null;
	
	
	// Member Properties
	public CModuleInterface.EType CurrentModuleType
	{
		get { return(m_CurrentModuleType.Get()); }
	}
	
	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_CurrentModuleType = new CNetworkVar<CModuleInterface.EType>(OnNetworkVarSync, CModuleInterface.EType.INVALID);
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		if(_SyncedNetworkVar == m_CurrentModuleType)
		{
			UpdateModulePresentation();
		}
	}
	
	public void Start()
	{
		if(CNetwork.IsServer)
			ChangeModuleType(m_StartingModuleType);
	}
	
	[AServerOnly]
	public void ChangeModuleType(CModuleInterface.EType _ModuleType)
	{
		m_CurrentModuleType.Set(_ModuleType);
	}
	
	public void UpdateModulePresentation()
	{
		// Create a temp module
		string modulePrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CModuleInterface.GetPrefabType(CurrentModuleType));
		GameObject moduleObject = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/" + modulePrefabFile));
		
		// Destroy the old module
		if(m_ParentModuleObject.transform.childCount != 0)
			Destroy(m_ParentModuleObject.transform.GetChild(0).gameObject);

		// Get all the monobehaviours that exsist on the prefab, reverse the order to delete dependant components first
		List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>(moduleObject.GetComponents<MonoBehaviour>());
		monoBehaviours.Reverse();

		// Get all the monobehaviours of all of the children too
		List<MonoBehaviour> childrenMonoBehaviours = new List<MonoBehaviour>(moduleObject.GetComponentsInChildren<MonoBehaviour>());
		childrenMonoBehaviours.Reverse();
		monoBehaviours.AddRange(childrenMonoBehaviours);

		// Remove any scripts that arent rendering related
		foreach(MonoBehaviour mb in monoBehaviours)
		{
			Type behaviourType = mb.GetType();

			if(behaviourType != typeof(MeshRenderer) ||
			   behaviourType != typeof(MeshFilter))
			{
				Destroy(mb);
			}
		}

		// Add it to the child object
		moduleObject.transform.parent = m_ParentModuleObject.transform;
		
		// Reset some values
		CUtility.SetLayerRecursively(moduleObject, LayerMask.NameToLayer("UI 3D"));
		moduleObject.transform.localPosition = new Vector3(0.0f, -0.3f, 0.0f);
		moduleObject.transform.localRotation = Quaternion.identity;

		// Set the scale a lot smaller
		moduleObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
	}
}
