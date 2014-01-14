//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomFactoryToolSpawner.cs
//  Description :   Class script for the factory facility
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Notes:
// About tool storage; once a tool is spawned, can another tool be spawned,
// or must the factory wait before spawning another tool? Suggestion would be
// to change the spawn position/rotation to allow for tools to spawn up to a specified limit.

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Implementation
public class CFactoryToolSpawner : CNetworkMonoBehaviour
{
    // Member Data
    CNetworkVar<float> m_fRecharge;
    CNetworkVar<ushort> m_sCurrentToolID;

    // Member Properties
    float Recharge        { get { return (m_fRecharge.Get()); } }
    ushort CurrentToolID  { get { return (m_sCurrentToolID.Get()); } }

    // Member Functions
    public override void InstanceNetworkVars()
    {
        m_fRecharge      = new CNetworkVar<float>(OnNetworkVarSync);
        m_sCurrentToolID = new CNetworkVar<ushort>(OnNetworkVarSync);
    }
	
	public void Start()
	{
		// Placeholder DUI stuff ************************
		CDUI consoleDUI = transform.parent.gameObject.GetComponent<CFacilityGeneral>().FacilityControlConsole.GetComponent<CDUIConsole>().DUI;
		
		CDUISubView factory = consoleDUI.AddSubView();
		
		CDUIButton but = factory.AddButton("SpawnTool");
		
		but.MiddleCenterViewPos = new Vector2(0.5f, 0.5f);
		but.PressDown += SpawnTool;	
	}
	
	[AServerOnly]
    void SpawnTool(CDUIButton _sender)
    {
        // Create a new prefab and tool
        CGame.ENetworkRegisteredPrefab ToolPrefab = CGame.ENetworkRegisteredPrefab.ToolRachet;
        GameObject newTool = CNetwork.Factory.CreateObject(ToolPrefab);		
		
		Quaternion TempQuat = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
		
		newTool.transform.position = transform.position;
		newTool.transform.rotation = TempQuat;
		newTool.transform.parent   = transform.parent;
		
		newTool.GetComponent<CNetworkView>().SyncParent();
		newTool.GetComponent<CNetworkView>().SyncTransformPosition();
		newTool.GetComponent<CNetworkView>().SyncTransformRotation();
    }

    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        // Empty
	}
	
	public void Update(){}
};
