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
	public event NotifyDUIEvent EventBuildModuleButtonPressed;

	
	// Member Fields
	public UILabel m_ModuleNameLabel = null;
	public UILabel m_ModuleDescLabel = null;
	public UILabel m_ModuleCategoryLabel = null;
	public UILabel m_ModuleCostLabel = null;
	
	public UISprite m_SmallModuleSprite = null;
	public UISprite m_MediumModuleSprite = null;
	public UISprite m_LargeModuleSprite = null;

	public UISprite m_SmallPortSprite = null;
	public UISprite m_MediumPortSprite = null;
	public UISprite m_LargePortSprite = null;

	public CModuleInterface.EType m_StartingModuleType = CModuleInterface.EType.INVALID;
	public GameObject m_ParentModuleObject = null;

	private CModuleInterface.EType m_SelectedModuleType = CModuleInterface.EType.INVALID;
	private CModuleInterface.ECategory m_SelectedModuleCategory = CModuleInterface.ECategory.INVALID; 
	private CModuleInterface.ESize m_SelectedModuleSize = CModuleInterface.ESize.INVALID;
	private int m_SelectedModuleCost = 0;

	private CModuleInterface.ESize m_SelectedPortSize = CModuleInterface.ESize.INVALID;

	private CNetworkVar<CModuleInterface.EType> m_CurrentModuleType = null;
	private CNetworkVar<CNetworkViewId> m_CurrentPortSelected = null;
	
	
	// Member Properties
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

	public GameObject CurrentPortSelected
	{
		get {return(CNetwork.Factory.FindObject(m_CurrentPortSelected.Get())); }
	}
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_CurrentModuleType = _cRegistrar.CreateNetworkVar<CModuleInterface.EType>(OnNetworkVarSync, CModuleInterface.EType.INVALID);
		m_CurrentPortSelected = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		if(_SyncedNetworkVar == m_CurrentModuleType)
		{
			UpdateModulePresentation();
		}
		else if(_SyncedNetworkVar == m_CurrentPortSelected)
		{
			UpdatePortPresentation();
		}
	}
	
	public void Start()
	{
		if(CNetwork.IsServer)
			SetSelectedModuleType(m_StartingModuleType);
	}

	public void Update()
	{
		// Update the color based on nanite availability
		if(CGameShips.Ship.GetComponent<CShipNaniteSystem>().IsEnoughNanites(m_SelectedModuleCost))
			m_ModuleCostLabel.color = Color.white;
		else
			m_ModuleCostLabel.color = Color.red;
	}

	public void ButtonBuildModulePressed()
	{
		if(CNetwork.IsServer)
		{
			if(EventBuildModuleButtonPressed != null)
				EventBuildModuleButtonPressed();
		}
	}
	
	[AServerOnly]
	public void SetSelectedModuleType(CModuleInterface.EType _ModuleType)
	{
		m_CurrentModuleType.Set(_ModuleType);
	}

	[AServerOnly]
	public void SetSelectedPort(CNetworkViewId _PortViewId)
	{
		m_CurrentPortSelected.Set(_PortViewId);
	}

	private void UpdateModulePresentation()
	{
		// Create a temp module
		string modulePrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CModuleInterface.GetPrefabType(m_CurrentModuleType.Get()));
		GameObject moduleObject = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/" + modulePrefabFile));
		
		// Destroy the old module
		if(m_ParentModuleObject.transform.childCount != 0)
			Destroy(m_ParentModuleObject.transform.GetChild(0).gameObject);

		// Update the info
		UpdateModuleInfo(moduleObject.GetComponent<CModuleInterface>());

		// Destroy all non rendering components
		CUtility.DestroyAllNonRenderingComponents(moduleObject);
		
		// Add it to the child object
		moduleObject.transform.parent = m_ParentModuleObject.transform;
		
		// Reset some values
		CUtility.SetLayerRecursively(moduleObject, LayerMask.NameToLayer("UI 3D"));
		moduleObject.transform.localPosition = new Vector3(0.0f, -0.3f, 0.0f);
		moduleObject.transform.localRotation = Quaternion.identity;
		
		// Set the scale a lot smaller
		moduleObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
	}

	private void UpdatePortPresentation()
	{
		// Get the port that was selected
		GameObject port = CNetwork.Factory.FindObject(m_CurrentPortSelected.Get());

		// Update the port info in the DUI
		UpdatePortInfo(port.GetComponent<CModulePortInterface>());
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
		UpdateSizeInfo(m_SelectedModuleSize, m_SmallModuleSprite, m_MediumModuleSprite, m_LargeModuleSprite);
		
		// Set the desc
		m_ModuleDescLabel.text = desc;
		
		// Set the cost
		m_ModuleCostLabel.text = m_SelectedModuleCost.ToString() + "N";
	}

	private void UpdatePortInfo(CModulePortInterface _ModulePortInterface)
	{
		m_SelectedPortSize = _ModulePortInterface.PortSize;

		// Set the size
		UpdateSizeInfo(m_SelectedPortSize, m_SmallPortSprite, m_MediumPortSprite, m_LargePortSprite);
	}
	
	private void UpdateSizeInfo(CModuleInterface.ESize _ModuleSize, UISprite _Small, UISprite _Medium, UISprite _Large)
	{
		// Reset all colors to default
		_Small.color = Color.green;
		_Medium.color = Color.yellow;
		_Large.color = Color.red;
		
		if(_ModuleSize == CModuleInterface.ESize.Small)
		{
			_Medium.color = Color.gray;
			_Large.color = Color.gray;
		}
		else if(_ModuleSize == CModuleInterface.ESize.Medium)
		{
			_Small.color = Color.gray;
			_Large.color = Color.gray;
		}
		else if(_ModuleSize == CModuleInterface.ESize.Large)
		{
			_Small.color = Color.gray;
			_Medium.color = Color.gray;
		}
	}
}
