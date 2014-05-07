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
using System;


/* Implementation */


public class CDuiDispenserBehaviour : CNetworkMonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events


    public delegate void BuildToolHandler(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType);
    public event BuildToolHandler EventToolBuild;


    public delegate void ToolSelectHandler(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType);
    public event ToolSelectHandler EventToolSelect;
	
	
// Member Properties


// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_eSelectedToolType = _cRegistrar.CreateReliableNetworkVar<CToolInterface.EType>(OnNetworkVarSync, CToolInterface.EType.INVALID);
	}
	

	public void ButtonBuildToolPressed()
	{
		if (CNetwork.IsServer)
		{
            if (EventToolBuild != null)
                EventToolBuild(this, m_eSelectedToolType.Value);
		}
	}

	
	public void OnEventSelectTool(GameObject _cItem)
	{
        if (CNetwork.IsServer)
        {
            m_eSelectedToolType.Value = _cItem.GetComponent<CMetaData>().GetMeta<CToolInterface.EType>("ToolType");
        }

        m_ToolNameLabel.text = _cItem.GetComponent<CMetaData>().GetMeta<string>("Name");
        m_ToolDescLabel.text = _cItem.GetComponent<CMetaData>().GetMeta<string>("Description");
        m_ToolCostLabel.text = _cItem.GetComponent<CMetaData>().GetMeta<float>("NaniteCost").ToString() + " Nanites";
	}


    void Start()
    {
        // Hide tool item template
        m_cTemplateToolItem.transform.parent = null;
        m_cTemplateToolItem.SetActive(false);

        LoadToolGridItems();
    }


    void Update()
    {
        // Update the color based on nanite availability
        //		if(CGameShips.Ship.GetComponent<CShipNaniteSystem>().IsEnoughNanites(m_SelectedToolCost))
        //			m_ToolCostLabel.color = Color.white;
        //		else
        //			m_ToolCostLabel.color = Color.red;
    }
	

	void UpdateToolPresentation()
	{
        /*
		// Create a temp module
		string toolPrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CToolInterface.GetPrefabType(m_eSelectedToolType.Get()));
		GameObject toolObject = (GameObject)Resources.Load("Prefabs/" + toolPrefabFile);
		GameObject toolObjectMesh = new GameObject(toolObject.name);

		// Get the children of the tool
		foreach(Transform child in toolObject.transform)
		{
			Transform childTrans = ((GameObject)GameObject.Instantiate(child.gameObject)).transform;
			childTrans.transform.parent = toolObjectMesh.transform;
			childTrans.transform.localPosition = child.localPosition;
			childTrans.transform.localRotation = child.localRotation;
			childTrans.transform.localScale = child.localScale;

			CUtility.DestroyAllNonRenderingComponents(childTrans.gameObject);
		}

		// Destroy old tools
		if(m_ToolObject.transform.childCount > 0)
			Destroy(m_ToolObject.transform.GetChild(0).gameObject);

		// Parent to the tool object
		toolObjectMesh.transform.parent = m_ToolObject.transform;

		// Update the tool info
		UpdateToolInfo(toolObject.GetComponent<CToolInterface>());
		
		// Reset some values
		CUtility.SetLayerRecursively(toolObjectMesh, LayerMask.NameToLayer("UI 3D"));
		toolObjectMesh.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		toolObjectMesh.transform.localRotation = Quaternion.identity;
		
		// Set the scale a lot smaller
		toolObjectMesh.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
         * */
	}


    void LoadToolGridItems()
    {
        // Load every tool
        foreach (CToolInterface.EType eToolType in Enum.GetValues(typeof(CToolInterface.EType)))
        {
            string sToolPrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CToolInterface.GetPrefabType(eToolType));

            if (sToolPrefabFile == null)
                continue;

            GameObject cToolPrefab = Resources.Load(sToolPrefabFile, typeof(GameObject)) as GameObject;
            CToolInterface cToolInferface = cToolPrefab.GetComponent<CToolInterface>();

            // Skip non dispensable tools
            if (cToolInferface.m_bDispensable == false)
                continue;

            // Clone template item
            GameObject cNewItem = GameObject.Instantiate(m_cTemplateToolItem) as GameObject;
            cNewItem.SetActive(true);

            // Set tool name
            cNewItem.transform.FindChild("Label").GetComponent<UILabel>().text = cToolInferface.m_sName;

            // Append item to grid
            cNewItem.transform.parent = m_cGrid.gameObject.transform;
            cNewItem.transform.localPosition = Vector3.zero;
            cNewItem.transform.localScale = Vector3.one;
            cNewItem.transform.localRotation = Quaternion.identity;

            // Set meta data
            cNewItem.GetComponent<CMetaData>().SetMeta("ToolType", cToolInferface.m_eToolType);
            cNewItem.GetComponent<CMetaData>().SetMeta("Name", cToolInferface.m_sName);
            cNewItem.GetComponent<CMetaData>().SetMeta("Description", cToolInferface.m_sDescription);
            cNewItem.GetComponent<CMetaData>().SetMeta("NaniteCost", cToolInferface.m_fNaniteCost);

            // Refresh grid
            m_cGrid.Reposition();
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_eSelectedToolType)
        {
            if (EventToolSelect != null)
                EventToolSelect(this, m_eSelectedToolType.Value);
        }
    }


// Member Fields


    public UILabel m_ToolNameLabel = null;
    public UILabel m_ToolDescLabel = null;
    public UILabel m_ToolCostLabel = null;


    public GameObject m_ToolObject = null;
    public GameObject m_cTemplateToolItem = null;
    public UIGrid m_cGrid = null;


    CNetworkVar<CToolInterface.EType> m_eSelectedToolType = null;

    GameObject m_cPreviewTool = null;
    

}
