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


	// Member Fields
	public CDUIConsole m_DUIConsole = null;
	public Transform m_ToolSpawnLocation = null;
	
	public CComponentInterface m_CircuitryComponent = null;
	public CComponentInterface m_MechanicalComponent = null;
	public bool m_Debug = false;

	private CDUIDispenserRoot m_DUIDispenser = null;


    // Member Properties


    // Member Methods
	public void Start()
    {
		// Get the DUI of the dispenser
		m_DUIDispenser = m_DUIConsole.DUIRoot.GetComponent<CDUIDispenserRoot>();

		// Register the event for building a tool
		m_DUIDispenser.EventBuildToolButtonPressed += HandleDUIButtonPressed;
    }

	[AServerOnly]
	private void HandleDUIButtonPressed(CDUIDispenserRoot _DUI)
	{
		// Check there is enough nanites for the selected tool
		CShipNaniteSystem sns = CGameShips.Ship.GetComponent<CShipNaniteSystem>();
		if(sns.IsEnoughNanites(_DUI.SelectedToolCost) || m_Debug)
		{
			// Deduct the amount
			if(!m_Debug)
				sns.DeductNanites(_DUI.SelectedToolCost);

			// Spawn the selected tool
			SpawnTool(_DUI.SelectedToolType);
		}
	}
	
    [AServerOnly]
    private void SpawnTool(CToolInterface.EType _ToolType)
    {
        // Create a new object
		GameObject NewTool = CNetwork.Factory.CreateObject(CToolInterface.GetPrefabType(_ToolType));

        gameObject.GetComponent<CAudioCue>().Play(0.3f, false, 0);

        // Set the tool's position
		NewTool.GetComponent<CNetworkView>().SetPosition(m_ToolSpawnLocation.position);
		NewTool.GetComponent<CNetworkView>().SetEulerAngles(m_ToolSpawnLocation.eulerAngles);
    }

};