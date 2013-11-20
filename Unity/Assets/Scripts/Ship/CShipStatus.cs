//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipStatus.cs
//  Description :   Class script for maintaining and updating the stasus of the ship's resources.
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Notes:
// Need to 'register' for event of new room
// creation, to add that room to the lists

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Implementation
public class CShipStatus : CNetworkMonoBehaviour
{
    // Member Data
    CNetworkVar<float> m_fTotalPowerOutput;
    CNetworkVar<float> m_fTotalPowerConsumption;
    CNetworkVar<float> m_fTotalOxygenOutput;
    CNetworkVar<float> m_fTotalOxygenConsumption;
	
	Dictionary<CRoomInterface.ERoomType, List<GameObject>> m_RoomDictionary = new Dictionary<CRoomInterface.ERoomType, List<GameObject>>();
	List<GameObject> m_RoomsBridge      = new List<GameObject>();
	List<GameObject> m_RoomsFactory     = new List<GameObject>();
	List<GameObject> m_RoomsLifeSupport = new List<GameObject>();
	List<GameObject> m_RoomsGravityGen  = new List<GameObject>();
	List<GameObject> m_RoomsEngine      = new List<GameObject>();

    // Member Properties
    float TotalPowerOutput       { get { return (m_fTotalPowerOutput.Get()); }       set { TotalPowerOutput = value; } }
    float TotalPowerConsumption  { get { return (m_fTotalPowerConsumption.Get()); }  set { TotalPowerConsumption = value; } }
    float TotalOxygenOutput      { get { return (m_fTotalOxygenOutput.Get()); }      set { TotalOxygenOutput = value; } }
    float TotalOxygenConsumption { get { return (m_fTotalOxygenConsumption.Get()); } set { TotalOxygenConsumption = value; } }

    // Member Functions
    public override void InstanceNetworkVars()
    {
        m_fTotalPowerOutput       = new CNetworkVar<float>(OnNetworkVarSync);
        m_fTotalPowerConsumption  = new CNetworkVar<float>(OnNetworkVarSync);
        m_fTotalOxygenOutput      = new CNetworkVar<float>(OnNetworkVarSync);
        m_fTotalOxygenConsumption = new CNetworkVar<float>(OnNetworkVarSync);
    }
	
	public void Start()
	{
		// Save a list of every room
	//	List<GameObject> TempList = CGame.Instance.GetComponent<CShipRooms>().GetAllRooms();
		
		// Iterate through rooms, adding each type to a specific list of that type
	//	foreach (GameObject Room in TempList)
	//	{
			// Get the current room's type
	//		switch (Room.GetComponent<CRoomInterface>().RoomType())
	//		{
	//		case Bridge:           { m_RoomsBridge.Add(Room); break; }
	//		case Factory:          { m_RoomsFactory.Add(Room); break; }
	//		case LifeSupportDome:  { m_RoomsLifeSupport.Add(Room); break; }
	//		case GravityGenerator: { m_RoomsGravityGen.Add(Room); break; }
	//		case Engine:           { m_RoomsEngine.Add(Room); break; }
	//		default: break;
	//		}
	//	}
		
		// Add each 'type' list into a dictionary, keyed to that type
	//	m_RoomDictionary.Add(CRoomInterface.ERoomType.Bridge, m_RoomsBridge);
//		m_RoomDictionary.Add(CRoomInterface.ERoomType.Factory, m_RoomsFactory);
//		m_RoomDictionary.Add(CRoomInterface.ERoomType.LifeSupportDome, m_RoomsLifeSupport);
	//	m_RoomDictionary.Add(CRoomInterface.ERoomType.GravityGenerator, m_RoomsGravityGen);
	//	m_RoomDictionary.Add(CRoomInterface.ERoomType.Engine, m_RoomsEngine);
	}

    public void Update()
    {
//		for (CRoomInterface.ERoomType RoomType = CRoomInterface.ERoomType.Bridge;
		//	 RoomType < CRoomInterface.ERoomType.MAX;
		//	 ++RoomType)
		//{
			// Total up every room's power consumpotion
		//	foreach (GameObject Room in m_RoomDictionary[RoomType])
		//	{
				// m_fTotalPowerConsumption += This room's power consumption
		//	}
			
		//	switch (RoomType)
		//	{
				// Total up all life support system's output
		//		case CRoomInterface.ERoomType.LifeSupportDome: 
		//		{
					// m_fTotalOxygenOutput += Current room's oxygen output
		//		}
		//	}
		//}
    }

    void OnNetworkVarSync(INetworkVar _cVarInstance) {}
};