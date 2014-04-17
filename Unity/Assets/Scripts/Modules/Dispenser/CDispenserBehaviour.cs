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
public class CDispenserBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	void Start()
    {
        if (CNetwork.IsServer)
        {
            // Register the event for building a tool
            m_cDuiConsole.DUIRoot.GetComponent<CDUIDispenserRoot>().EventBuildToolButtonPressed += OnEventDuiButtonPressed;

            // Register for parent facility power active change
            GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerActiveChange += OnEventFacilityPowerActiveChange;
        }
    }


    void OnDestory()
    {
        if (CNetwork.IsServer)
        {
            GetComponent<CModuleInterface>().ParentFacility.GetComponent<CFacilityPower>().EventFacilityPowerActiveChange -= OnEventFacilityPowerActiveChange;
        }
    }
	

    [AServerOnly]
    void SpawnTool(CToolInterface.EType _ToolType)
    {
        // Create a new object
		GameObject NewTool = CNetwork.Factory.CreateObject(CToolInterface.GetPrefabType(_ToolType));

        gameObject.GetComponent<CAudioCue>().Play(0.3f, false, 0);

        // Set the tool's position
		NewTool.GetComponent<CNetworkView>().SetPosition(m_cToolSpawnLocation.position);
		NewTool.GetComponent<CNetworkView>().SetEuler(m_cToolSpawnLocation.eulerAngles);
    }


    [AServerOnly]
    void OnEventDuiButtonPressed(CDUIDispenserRoot _cDui)
    {
        CShipNaniteSystem cShipNaniteSystem = CGameShips.Ship.GetComponent<CShipNaniteSystem>();

        // Check there is enough nanites for the selected tool
        if (cShipNaniteSystem.NanaiteQuanity > _cDui.SelectedToolCost)
        {
            // Deduct the amount
            cShipNaniteSystem.ChangeQuanity(-_cDui.SelectedToolCost);

            // Spawn the selected tool
            SpawnTool(_cDui.SelectedToolType);
        }
    }


    [AServerOnly]
    void OnEventFacilityPowerActiveChange(GameObject _cFacility, bool _bActive)
    {
        // Empty
    }


// Member Fields


    public CDUIConsole m_cDuiConsole = null;
    public Transform m_cToolSpawnLocation = null;

    public CComponentInterface m_cCircuitryComponent = null;
    public CComponentInterface m_cMechanicalComponent = null;


    CDUIDispenserRoot m_DUIDispenser = null;


};