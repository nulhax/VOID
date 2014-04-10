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


public class CDUIDispenserRoot : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	public delegate void NotifyDUIEvent(CDUIDispenserRoot _Sender);
	public event NotifyDUIEvent EventBuildToolButtonPressed;
	
	
	// Member Fields
	public UILabel m_ToolNameLabel = null;
	public UILabel m_ToolDescLabel = null;
	public UILabel m_ToolCostLabel = null;
	
	public CToolInterface.EType m_StartingToolType = CToolInterface.EType.INVALID;
	public GameObject m_ParentToolObject = null;
	
	private CToolInterface.EType m_SelectedToolType = CToolInterface.EType.INVALID;
	private int m_SelectedToolCost = 0;
	
	private CNetworkVar<CToolInterface.EType> m_CurrentToolType = null;
	
	
	// Member Properties
	public CToolInterface.EType SelectedToolType
	{
		get {return(m_SelectedToolType); }
	}
	
	public int SelectedToolCost
	{
		get
        {
        #if UNITY_EDITOR

            return (0);

        #endif

            return(m_SelectedToolCost); 
        }
	}
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_CurrentToolType = _cRegistrar.CreateReliableNetworkVar<CToolInterface.EType>(OnNetworkVarSync, CToolInterface.EType.INVALID);
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		if(_SyncedNetworkVar == m_CurrentToolType)
		{
			UpdateToolPresentation();
		}
	}
	
	public void Start()
	{
		if(CNetwork.IsServer)
			SetSelectedToolType(m_StartingToolType);
	}
	
	public void Update()
	{
		// Update the color based on nanite availability
//		if(CGameShips.Ship.GetComponent<CShipNaniteSystem>().IsEnoughNanites(m_SelectedToolCost))
//			m_ToolCostLabel.color = Color.white;
//		else
//			m_ToolCostLabel.color = Color.red;
	}
	
	public void ButtonBuildToolPressed()
	{
		if(CNetwork.IsServer)
		{
			if(EventBuildToolButtonPressed != null)
				EventBuildToolButtonPressed(this);
		}
	}
	
	[AServerOnly]
	public void SetSelectedToolType(CToolInterface.EType _ToolType)
	{
		m_CurrentToolType.Set(_ToolType);
	}
	
	private void UpdateToolPresentation()
	{
		// Create a temp module
		string toolPrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CToolInterface.GetPrefabType(m_CurrentToolType.Get()));
		GameObject toolObject = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/" + toolPrefabFile));
		
		// Destroy the old module
		if(m_ParentToolObject.transform.childCount != 0)
			Destroy(m_ParentToolObject.transform.GetChild(0).gameObject);

		// Update the tool info
		UpdateToolInfo(toolObject.GetComponent<CToolInterface>());

		// Destroy the non rendering 
		CUtility.DestroyAllNonRenderingComponents(toolObject);
		
		// Add it to the child object
		toolObject.transform.parent = m_ParentToolObject.transform;
		
		// Reset some values
		CUtility.SetLayerRecursively(toolObject, LayerMask.NameToLayer("UI 3D"));
		toolObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		toolObject.transform.localRotation = Quaternion.identity;
		
		// Set the scale a lot smaller
		toolObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
	}
	
	private void UpdateToolInfo(CToolInterface _tempToolInterface)
	{
		// DEBUG: Make a random sentance to describe it
		string desc = CUtility.LoremIpsum(6, 12, 2, 4, 1);
		
		m_SelectedToolType = _tempToolInterface.ToolType;
		m_SelectedToolCost = 100;
		
		// Set the name
		string name = CUtility.SplitCamelCase(m_SelectedToolType.ToString());
		m_ToolNameLabel.text = name;
		
		// Set the desc
		m_ToolDescLabel.text = desc;
		
		// Set the cost
		m_ToolCostLabel.text = m_SelectedToolCost.ToString() + "N";
	}
}
