//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomFactory.cs
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
public class CRoomFactory : CNetworkMonoBehaviour
{
    // Member Data
    CNetworkVar<float> m_fHealth;
    CNetworkVar<float> m_fPowerCost;
    CNetworkVar<float> m_fRecharge;
    CNetworkVar<float> m_fRadiationLevel;
    CNetworkVar<float> m_fRadiationRadius;
    CNetworkVar<ushort> m_sCurrentToolID;

    // Member Properties
    float Health          { get { return (m_fHealth.Get()); } }
    float PowerCost       { get { return (m_fPowerCost.Get()); } }
    float Recharge        { get { return (m_fRecharge.Get()); } }
    float RadiationLevel  { get { return (m_fRadiationLevel.Get()); } }
    float RadiationRadius { get { return (m_fRadiationRadius.Get()); } }
    ushort CurrentToolID  { get { return (m_sCurrentToolID.Get()); } }

    // Member Functions
    public override void InstanceNetworkVars()
    {
        m_fHealth          = new CNetworkVar<float>(OnNetworkVarSync);
        m_fPowerCost       = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRecharge        = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRadiationLevel  = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRadiationRadius = new CNetworkVar<float>(OnNetworkVarSync);
        m_sCurrentToolID   = new CNetworkVar<ushort>(OnNetworkVarSync);
    }
	
	public void Start()
	{
		// Placeholder DUI stuff ************************
		
//		CDUIMainView consoleMainView = GetComponent<CRoomGeneral>().RoomControlConsole.GetComponent<CDUIConsole>().MainView;
//		
//		CDUISubView factoryButton = consoleMainView.AddSubView().GetComponent<CDUISubView>();
//		
//		CDUIButton but = factoryButton.AddButton("SpawnTool");
//		but.PressDown += SpawnTool;
//		
//		but.m_ViewPos = new Vector2(0.5f, 0.5f);
	}

    public void Update()
    {
        
    }

    void SpawnTool(CDUIButton _sender)
    {
        // Create a new prefab and tool
        CGame.ENetworkRegisteredPrefab TorchPrefab = CGame.ENetworkRegisteredPrefab.ToolTorch;
        GameObject newTool = CNetwork.Factory.CreateObject(TorchPrefab);
		
		newTool.transform.position = transform.position;
		newTool.transform.rotation = transform.rotation;
		newTool.transform.parent = transform.parent;	
		
		newTool.GetComponent<CNetworkView>().SyncParent();
		newTool.GetComponent<CNetworkView>().SyncTransformPosition();
		newTool.GetComponent<CNetworkView>().SyncTransformRotation();
    }

    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        // Empty
	}
};
