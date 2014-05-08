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

        if (m_cDuiDispenserBehaviour != null &&
            m_cDuiDispenserBehaviour.CurrentPanel == CDuiDispenserBehaviour.EPanel.BuildProgress)
        {
            CPrecipitativeMeshBehaviour cPrecipitativeMeshBehaviour = m_cPreviewTool.GetComponent<CPrecipitativeMeshBehaviour>();
            cPrecipitativeMeshBehaviour.SetProgressRatio(m_cDuiDispenserBehaviour.BuildProgressRatio);
        }
    }


    void DestroyToolPreview()
    {
        if (m_cPreviewTool != null)
        {
            Destroy(m_cPreviewTool);
            m_cPreviewTool = null;
        }
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

        m_cPreviewTool = CNetwork.Factory.LoadPrefab(CToolInterface.GetPrefabType(_eType));
        m_cPreviewTool = GameObject.Instantiate(m_cPreviewTool.GetComponent<CToolInterface>().m_cPrecipitativeModel) as GameObject;
        m_cPreviewTool.transform.parent = m_cTransToolSpawn;
        m_cPreviewTool.transform.localPosition = Vector3.zero;
        //m_cPreviewTool.transform.localScale = Vector3.one / 2;
    }


    void OnEventDuiToolBuild(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType)
    {
        m_cPreviewTool.GetComponent<CPrecipitativeMeshBehaviour>().m_cParticles.Play();

        if ( CNetwork.IsServer &&
            !m_bBuildingTool)
        {
            m_cDuiDispenserBehaviour.SimulateBuildProgress(_eType);
            m_bBuildingTool = true;
        }
    }


    void OnEventDuiBuildProgressFinished(CDuiDispenserBehaviour _cSender, CToolInterface.EType _eType)
    {
        gameObject.GetComponent<CAudioCue>().Play(0.3f, false, 0);

        m_cPreviewTool.GetComponent<CPrecipitativeMeshBehaviour>().m_cParticles.Stop();
        m_cPreviewTool.GetComponent<CPrecipitativeMeshBehaviour>().SetProgressRatio(0.0f);

        if (CNetwork.IsServer)
        {
            GameObject cTool = CNetwork.Factory.CreateGameObject(CToolInterface.GetPrefabType(_eType));
            cTool.GetComponent<CNetworkView>().SetPosition(m_cTransToolSpawn.position);

            m_bBuildingTool = false;
        }
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
            m_cDuiDispenserBehaviour = m_tDuiConsoleViewId.Value.GameObject.GetComponent<CDuiDispenserBehaviour>();

            m_cDuiDispenserBehaviour.EventToolBuild += OnEventDuiToolBuild;
            m_cDuiDispenserBehaviour.EventToolSelect += OnEventDuiToolSelectChange;
            m_cDuiDispenserBehaviour.EventBuildProgressFinished += OnEventDuiBuildProgressFinished;
        }
    }


// Member Fields


    public GameObject m_cDuiScreen = null;
    public Transform m_cTransToolSpawn = null;


    CNetworkVar<TNetworkViewId> m_tDuiConsoleViewId = null;

    GameObject m_cPreviewTool = null;
    CDuiDispenserBehaviour m_cDuiDispenserBehaviour = null;

    bool m_bBuildingTool = false;


};