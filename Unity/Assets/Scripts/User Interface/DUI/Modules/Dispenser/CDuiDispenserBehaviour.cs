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


    public enum EPanel
    {
        INVALID,

        ToolMenu,
        BuildProgress
    }
	
	
// Member Delegates & Events


    public delegate void BuildToolHandler(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType);
    public event BuildToolHandler EventToolBuild;


    public delegate void ToolSelectHandler(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType);
    public event ToolSelectHandler EventToolSelect;


    public delegate void BuildProgressFinishHandler(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType);
    public event BuildProgressFinishHandler EventBuildProgressFinished;
	
	
// Member Properties


    public EPanel CurrentPanel
    {
        get { return (m_eCurrentPanel); }
    }


    public float BuildProgressRatio
    {
        get { return (m_fBuildTimer / m_fBuildDuration); }
    }


// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        _cRegistrar.RegisterRpc(this, "SimulateBuildProgress");

		m_eSelectedToolType = _cRegistrar.CreateReliableNetworkVar<CToolInterface.EType>(OnNetworkVarSync, CToolInterface.EType.INVALID);
	}
	

	public void ButtonBuildToolPressed()
	{
        if (EventToolBuild != null)
            EventToolBuild(this, m_eSelectedToolType.Value);
	}


    [ANetworkRpc]
    public void SimulateBuildProgress(CToolInterface.EType _eToolType)
    {
        if (CNetwork.IsServer)
        {
            InvokeRpcAllButServer("SimulateBuildProgress", _eToolType);
        }

        GameObject cToolPrefab = CNetwork.Factory.LoadPrefab(CToolInterface.GetPrefabType(_eToolType));

        m_cLabelBuildingToolName.text = cToolPrefab.GetComponent<CToolInterface>().m_sName;
        m_fBuildDuration = cToolPrefab.GetComponent<CToolInterface>().m_fBuildDuration;
        m_fBuildTimer = 0.0f;

        SetPanel(EPanel.BuildProgress);
    }

	
	public void OnEventSelectTool(GameObject _cItem)
	{
        if (CNetwork.IsServer)
        {
            m_eSelectedToolType.Value = _cItem.GetComponent<CMetaData>().GetMeta<CToolInterface.EType>("ToolType");
        }

        m_cLabelToolName.text = _cItem.GetComponent<CMetaData>().GetMeta<string>("Name");
        m_cLabelToolDesc.text = _cItem.GetComponent<CMetaData>().GetMeta<string>("Description");
        m_cLabelToolCost.text = _cItem.GetComponent<CMetaData>().GetMeta<float>("NaniteCost").ToString() + " Nanites";
	}


    void Start()
    {
        // Hide tool item template
        m_cTemplateGridToolItem.transform.parent = null;
        m_cTemplateGridToolItem.SetActive(false);

        LoadToolGridItems();

        SetPanel(EPanel.ToolMenu);
    }


    void Update()
    {
        if (m_eCurrentPanel == EPanel.BuildProgress)
        {
            m_fBuildTimer += Time.deltaTime;

            if (m_fBuildTimer > m_fBuildDuration)
            {
                SetPanel(EPanel.ToolMenu);

                if (EventBuildProgressFinished != null)
                    EventBuildProgressFinished(this, m_eSelectedToolType.Value);
            }

            m_cProgressBarBuildProgress.value = BuildProgressRatio;
        }

        // Update the color based on nanite availability
        //		if(CGameShips.Ship.GetComponent<CShipNaniteSystem>().IsEnoughNanites(m_SelectedToolCost))
        //			m_ToolCostLabel.color = Color.white;
        //		else
        //			m_ToolCostLabel.color = Color.red;
    }


    void SetPanel(EPanel _ePanel)
    {
        switch (_ePanel)
        {
            case EPanel.BuildProgress:
                m_cPanelToolMenu.gameObject.SetActive(false);
                m_cPanelBuildProgress.gameObject.SetActive(true);
                break;

            case EPanel.ToolMenu:
                m_cPanelToolMenu.gameObject.SetActive(true);
                m_cPanelBuildProgress.gameObject.SetActive(false);
                break;

            default:
                Debug.LogError("Unknown panel: " + _ePanel);
                break;
        }

        m_eCurrentPanel = _ePanel;
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
            GameObject cNewItem = GameObject.Instantiate(m_cTemplateGridToolItem) as GameObject;
            cNewItem.SetActive(true);

            // Set tool name
            cNewItem.transform.FindChild("Label").GetComponent<UILabel>().text = cToolInferface.m_sName;

            // Append item to grid
            cNewItem.transform.parent = m_cGridTools.gameObject.transform;
            cNewItem.transform.localPosition = Vector3.zero;
            cNewItem.transform.localScale = Vector3.one;
            cNewItem.transform.localRotation = Quaternion.identity;

            // Set meta data
            cNewItem.GetComponent<CMetaData>().SetMeta("ToolType", cToolInferface.m_eToolType);
            cNewItem.GetComponent<CMetaData>().SetMeta("Name", cToolInferface.m_sName);
            cNewItem.GetComponent<CMetaData>().SetMeta("Description", cToolInferface.m_sDescription);
            cNewItem.GetComponent<CMetaData>().SetMeta("NaniteCost", cToolInferface.m_fNaniteCost);

            // Refresh grid
            m_cGridTools.Reposition();
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


    public UILabel m_cLabelToolName = null;
    public UILabel m_cLabelToolDesc = null;
    public UILabel m_cLabelToolCost = null;
    public UILabel m_cLabelBuildingToolName = null;

    public UIGrid m_cGridTools = null;
    public GameObject m_cTemplateGridToolItem = null;

    public UIPanel m_cPanelToolMenu = null;
    public UIPanel m_cPanelBuildProgress = null;

    public UIProgressBar m_cProgressBarBuildProgress = null;


    CNetworkVar<CToolInterface.EType> m_eSelectedToolType = null;

    GameObject m_cPreviewTool = null;

    EPanel m_eCurrentPanel = EPanel.INVALID;

    float m_fBuildDuration = 0.0f;
    float m_fBuildTimer = 0.0f;
    

}
