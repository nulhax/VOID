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
            m_cDuiScreen.GetComponent<CDUIConsole>().CreateUserInterface();
        }
    }


    [AServerOnly]
    void OnEventFacilityPowerActiveChange(GameObject _cFacility, bool _bActive)
    {
        // Empty
    }


// Member Fields


    public GameObject m_cDuiScreen = null;
    public Transform m_cTransToolSpawn = null;


    CDuiDispenserBehaviour m_DUIDispenser = null;


};