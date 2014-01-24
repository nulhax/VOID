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


[RequireComponent(typeof(CNetworkView))]
public class CDUIStageFacilityExpansion : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CFacilityInterface.EType m_StartingFacilityType = CFacilityInterface.EType.INVALID;

	private CNetworkVar<CFacilityInterface.EType> m_CurrentFacilityType = null;
	private GameObject m_FacilityObject = null;


	// Member Properties
	public CFacilityInterface.EType CurrentFacilityType
	{
		get { return(m_CurrentFacilityType.Get()); }
	}

	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_CurrentFacilityType = new CNetworkVar<CFacilityInterface.EType>(OnNetworkVarSync, CFacilityInterface.EType.INVALID);
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		if(_SyncedNetworkVar == m_CurrentFacilityType)
		{
			UpdateChildFacilityPresentation();
		}
	}

	public void Awake()
	{
		m_FacilityObject = transform.GetChild(0).gameObject;
	}

	public void Start()
	{
		if(CNetwork.IsServer)
			ChangeFacilityType(m_StartingFacilityType);
	}

	[AServerOnly]
	public void ChangeFacilityType(CFacilityInterface.EType _FacilityType)
	{
		m_CurrentFacilityType.Set(_FacilityType);
	}

	public void UpdateChildFacilityPresentation()
	{
		// Create a temp miniature facility
		string faciltyPrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetMiniaturePrefabType(CurrentFacilityType));
		GameObject tempFacilityObject = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/" + faciltyPrefabFile));

		// Destroy the old facility
		if(m_FacilityObject.transform.childCount != 0)
			Destroy(m_FacilityObject.transform.GetChild(0).gameObject);

		// Add it to the child object
		tempFacilityObject.transform.parent = m_FacilityObject.transform;

		// Reset some values
		tempFacilityObject.layer = LayerMask.NameToLayer("UI 3D");
		tempFacilityObject.transform.localPosition =  Vector3.zero;
		tempFacilityObject.transform.localRotation = Quaternion.identity;
	}
}
