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


public class CDispenserBehaviour : MonoBehaviour
{
    // Member Types


    // Member Delegates & Events


    // Member Properties


    // Member Methods

    void Start()
    {
		// Get the DUI of the dispenser
		m_DUIDispenser = m_DUIConsole.DUI.GetComponent<CDUIDispenserRoot>();

		// Register the event for building a tool
		m_DUIDispenser.EventBuildToolButtonPressed += HandleDUIButtonPressed;

        GetComponent<CActorInteractable>().EventHover += OnHover;
    }

	[AServerOnly]
	private void HandleDUIButtonPressed(CDUIDispenserRoot _DUI)
	{
		// Check there is enough nanites for the selected tool
		CShipNaniteSystem sns = CGameShips.Ship.GetComponent<CShipNaniteSystem>();
		//if(sns.IsEnoughNanites(_DUI.SelectedToolCost))
		{
			// Spawn the selected tool
			SpawnTool(_DUI.SelectedToolType);

			// Negate the nantites from the ship pool
			//sns.DeductNanites(_DUI.SelectedToolCost);
		}
	}
	
    [AServerOnly]
    private void SpawnTool(CToolInterface.EType _ToolType)
    {
        // Create a new object
		GameObject NewTool = CNetwork.Factory.CreateObject(CToolInterface.GetPrefabType(_ToolType));

        // Set the tool's position
		NewTool.GetComponent<CNetworkView>().SetPosition(m_ToolSpawnLocation.position);
		NewTool.GetComponent<CNetworkView>().SetEulerAngles(m_ToolSpawnLocation.eulerAngles);
    }


    void OnDestroy()
    {
        GetComponent<CActorInteractable>().EventHover -= OnHover;
    }

    void Update()
    {
        // Interpolate health
		// Martin: Ill replace this with a UI to show status' :)
        //transform.FindChild("Cube").renderer.material.color = Color.Lerp(Color.red, Color.green, m_fHealth / 100.0f);
    }

    // TEMPORARY //
    //
    // Hover text logic that needs revision. OnGUI + Copy/Paste code = Terribad
    //
    // TEMPORARY //
    bool bShowName = false;
    bool bOnGUIHit = false;
    void OnHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
    {
        bShowName = true;
    }


    public void OnGUI()
    {
        float fScreenCenterX = Screen.width / 2;
        float fScreenCenterY = Screen.height / 2;
        float fWidth = 150.0f;
        float fHeight = 20.0f;
        float fOriginX = fScreenCenterX + 25.0f;
        float fOriginY = fScreenCenterY - 10.0f;

        if (bShowName && !bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Dispenser Module");
            bOnGUIHit = true;
        }
        else if (bShowName && bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Dispenser Module");
            bShowName = false;
            bOnGUIHit = false;
        }
    }
    // TEMPORARY //
    //
    // 
    //
    // TEMPORARY //
	

	public CDUIConsole m_DUIConsole = null;
	public Transform m_ToolSpawnLocation = null;

	public CComponentInterface m_CircuitryComponent = null;
	public CComponentInterface m_MechanicalComponent = null;

	private CDUIDispenserRoot m_DUIDispenser = null;
	
};