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


public class CDUIModuleCreationRoot : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	public delegate void NotifyDUIEvent();
	public event NotifyDUIEvent EventSelectNodeButtonPressed;

	
	// Member Fields
	public UILabel m_ModuleNameLabel = null;
	public UILabel m_ModuleDescLabel = null;
	public UILabel m_ModuleCategoryLabel = null;
	public UILabel m_ModuleCostLabel = null;
	
	public UISprite m_SmallModuleSprite = null;
	public UISprite m_MediumModuleSprite = null;
	public UISprite m_LargeModuleSprite = null;

	public CModuleInterface.EType m_StartingModuleType = CModuleInterface.EType.INVALID;
	public GameObject m_ParentModuleObject = null;

	private CModuleInterface.EType m_SelectedModuleType = CModuleInterface.EType.INVALID;
	private CModuleInterface.ECategory m_SelectedModuleCategory = CModuleInterface.ECategory.INVALID; 
	private CModuleInterface.ESize m_SelectedModuleSize = CModuleInterface.ESize.INVALID;
	private int m_SelectedModuleCost = 0;

	private CNetworkVar<CModuleInterface.EType> m_CurrentModuleType = null;
	
	
	// Member Properties
	public CModuleInterface.EType CurrentModuleType
	{
		get { return(m_CurrentModuleType.Get()); }
	}

	public CModuleInterface.EType SelectedModuleType
	{
		get {return(m_SelectedModuleType); }
	}

	public CModuleInterface.ECategory SelectedModuleCategory
	{
		get {return(m_SelectedModuleCategory); }
	}

	public CModuleInterface.ESize SelectedModuleSize
	{
		get {return(m_SelectedModuleSize); }
	}

	public int SelectedModuleCost
	{
		get {return(m_SelectedModuleCost); }
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

	public void SelectNodeButtonPressed()
	{
		if(EventSelectNodeButtonPressed != null)
			EventSelectNodeButtonPressed();
	}
	
	[AServerOnly]
	public void ChangeModuleType(CModuleInterface.EType _ModuleType)
	{
		m_CurrentModuleType.Set(_ModuleType);
	}

	private void UpdateModulePresentation()
	{
		// Create a temp module
		string modulePrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CModuleInterface.GetPrefabType(CurrentModuleType));
		GameObject moduleObject = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/" + modulePrefabFile));
		
		// Destroy the old module
		if(m_ParentModuleObject.transform.childCount != 0)
			Destroy(m_ParentModuleObject.transform.GetChild(0).gameObject);
		
		UpdateModuleInfo(moduleObject.GetComponent<CModuleInterface>());
		
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
			System.Type behaviourType = mb.GetType();
			
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

	private void UpdateModuleInfo(CModuleInterface _tempModuleInterface)
	{
		// DEBUG: Make a random sentance to describe it
		string desc = CUtility.LoremIpsum(6, 12, 2, 4, 1);

		m_SelectedModuleType = _tempModuleInterface.ModuleType;
		m_SelectedModuleCategory = _tempModuleInterface.ModuleCategory; 
		m_SelectedModuleSize = _tempModuleInterface.ModuleSize;
		m_SelectedModuleCost = UnityEngine.Random.Range(100, 400) * 1000;

		// Set the name
		m_ModuleNameLabel.text = m_SelectedModuleType.ToString();
		
		// Set the category
		m_ModuleCategoryLabel.text = m_SelectedModuleCategory.ToString();
		
		// Set the size
		UpdateSize(m_SelectedModuleSize);
		
		// Set the desc
		m_ModuleDescLabel.text = desc;
		
		// Set the cost
		m_ModuleCostLabel.text = m_SelectedModuleCost.ToString() + "N";
	}
	
	private void UpdateSize(CModuleInterface.ESize _ModuleSize)
	{
		// Reset all colors to default
		m_SmallModuleSprite.color = Color.green;
		m_MediumModuleSprite.color = Color.yellow;
		m_LargeModuleSprite.color = Color.red;
		
		if(_ModuleSize == CModuleInterface.ESize.Small)
		{
			m_MediumModuleSprite.color = Color.gray;
			m_LargeModuleSprite.color = Color.gray;
		}
		else if(_ModuleSize == CModuleInterface.ESize.Medium)
		{
			m_SmallModuleSprite.color = Color.gray;
			m_LargeModuleSprite.color = Color.gray;
		}
		else if(_ModuleSize == CModuleInterface.ESize.Large)
		{
			m_SmallModuleSprite.color = Color.gray;
			m_MediumModuleSprite.color = Color.gray;
		}
	}
}
