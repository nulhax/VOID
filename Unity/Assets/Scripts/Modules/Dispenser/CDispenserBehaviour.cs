//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDispenserBehaviour.cs
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


[RequireComponent(typeof(CModuleInterface))]
public class CDispenserBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_tDuiConsoleViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
    }


	void Start()
    {
        if (CNetwork.IsServer)
        {
            // Register the event for building a tool
            //m_cDuiConsole.DUIRoot.GetComponent<CDUIDispenserRoot>().EventBuildToolButtonPressed += OnEventDuiButtonPressed;

            // Register for parent facility power active change
            //GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerActiveChange += OnEventFacilityPowerActiveChange;
        }

        GetComponent<CModuleInterface>().EventBuilt += OnEventBuilt;
    }


    void OnDestory()
    {
        if (CNetwork.IsServer)
        {
            //GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerActiveChange -= OnEventFacilityPowerActiveChange;
        }
    }


    void Update()
    {
        if (m_cPreviewTool != null)
        {
            m_cPreviewTool.transform.Rotate(Vector3.up, 20.0f * Time.deltaTime);
        }
    }
	

    [AServerOnly]
    void SpawnTool(CToolInterface.EType _ToolType)
    {
        // Create a new object
		GameObject NewTool = CNetwork.Factory.CreateGameObject(CToolInterface.GetPrefabType(_ToolType));

        gameObject.GetComponent<CAudioCue>().Play(0.3f, false, 0);

        // Set the tool's position
		NewTool.GetComponent<CNetworkView>().SetPosition(m_cTransToolSpawn.position);
		NewTool.GetComponent<CNetworkView>().SetEuler(m_cTransToolSpawn.eulerAngles);
    }


    void DestroyToolPreview()
    {
        if (m_cPreviewTool != null)
        {
            Destroy(m_cPreviewTool);
            m_cPreviewTool = null;
        }
    }


    [AServerOnly]
    void OnEventDuiButtonPressed(CDuiDispenserBehaviour _cDui)
    {
        /*
        CShipNaniteSystem cShipNaniteSystem = CGameShips.Ship.GetComponent<CShipNaniteSystem>();

        // Check there is enough nanites for the selected tool
		if(cShipNaniteSystem.NanaiteQuanity >= (float)_cDui.SelectedToolCost)
        {
            // Deduct the amount
            cShipNaniteSystem.ChangeQuanity(-_cDui.SelectedToolCost);

            // Spawn the selected tool
            SpawnTool(_cDui.SelectedToolType);
        }
         * */
    }


    void OnEventBuilt(CModuleInterface _cSender)
    {
        // Create console on build
        if (CNetwork.IsServer)
        {
            m_tDuiConsoleViewId.Value = m_cDuiScreen.GetComponent<CDUIConsole>().CreateUserInterface();
        }
    }


    void OnEventDuiToolSelectChange(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType)
    {
        DestroyToolPreview();

        string sToolPrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CToolInterface.GetPrefabType(_eType));

        m_cPreviewTool = Resources.Load(sToolPrefabFile, typeof(GameObject)) as GameObject;
        m_cPreviewTool = GameObject.Instantiate(m_cPreviewTool.GetComponent<CToolInterface>().m_cModel) as GameObject;
        m_cPreviewTool.transform.parent = m_cTransToolSpawn;
        m_cPreviewTool.transform.localPosition = Vector3.zero;
        m_cPreviewTool.transform.localScale = Vector3.one / 2;
    }


    [AServerOnly]
    void OnEventDuiToolBuild(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType)
    {
        GameObject cTool = CNetwork.Factory.CreateGameObject(CToolInterface.GetPrefabType(_eType));
        cTool.GetComponent<CNetworkView>().SetPosition(m_cTransToolSpawn.position);
    }


    [AServerOnly]
    void OnEventFacilityPowerActiveChange(GameObject _cFacility, bool _bActive)
    {
        // Empty
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_tDuiConsoleViewId)
        {
            if (CNetwork.IsServer)
            {
                m_tDuiConsoleViewId.Value.GameObject.GetComponent<CDuiDispenserBehaviour>().EventToolSelect += OnEventDuiToolSelectChange;
                m_tDuiConsoleViewId.Value.GameObject.GetComponent<CDuiDispenserBehaviour>().EventToolBuild += OnEventDuiToolBuild;
            }
        }
    }


// Member Fields


    public GameObject m_cDuiScreen = null;
    public Transform m_cTransToolSpawn = null;


    CNetworkVar<TNetworkViewId> m_tDuiConsoleViewId = null;

    GameObject m_cPreviewTool = null;
    CDuiDispenserBehaviour m_DUIDispenser = null;


};