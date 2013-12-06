//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShipStatus.cs
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


public class CShipStatus : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


	float TotalPowerOutput { get { return (m_fTotalPowerOutput.Get()); } set { TotalPowerOutput = value; } }
	float TotalPowerConsumption { get { return (m_fTotalPowerConsumption.Get()); } set { TotalPowerConsumption = value; } }
	float TotalOxygenOutput { get { return (m_fTotalOxygenOutput.Get()); } set { TotalOxygenOutput = value; } }
	float TotalOxygenConsumption { get { return (m_fTotalOxygenConsumption.Get()); } set { TotalOxygenConsumption = value; } }


// Member Methods


// Member Properties


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
	//		switch (Room.GetComponent<CFacilityInterface>().RoomType())
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
	//	m_RoomDictionary.Add(CFacilityInterface.ERoomType.Bridge, m_RoomsBridge);
//		m_RoomDictionary.Add(CFacilityInterface.ERoomType.Factory, m_RoomsFactory);
//		m_RoomDictionary.Add(CFacilityInterface.ERoomType.LifeSupportDome, m_RoomsLifeSupport);
	//	m_RoomDictionary.Add(CFacilityInterface.ERoomType.GravityGenerator, m_RoomsGravityGen);
	//	m_RoomDictionary.Add(CFacilityInterface.ERoomType.Engine, m_RoomsEngine);
	}


    public void Update()
    {
		UpdateOxygen();


//		for (CFacilityInterface.ERoomType RoomType = CFacilityInterface.ERoomType.Bridge;
		//	 RoomType < CFacilityInterface.ERoomType.MAX;
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
		//		case CFacilityInterface.ERoomType.LifeSupportDome: 
		//		{
					// m_fTotalOxygenOutput += Current room's oxygen output
		//		}
		//	}
		//}
    }


	void UpdateOxygen()
	{

	}


    void OnNetworkVarSync(INetworkVar _cVarInstance)
	{

	}


// Member Fields


	CNetworkVar<float> m_fTotalPowerOutput;
	CNetworkVar<float> m_fTotalPowerConsumption;
	CNetworkVar<float> m_fTotalOxygenOutput;
	CNetworkVar<float> m_fTotalOxygenConsumption;


	Dictionary<CFacilityInterface.EType, List<GameObject>> m_RoomDictionary = new Dictionary<CFacilityInterface.EType, List<GameObject>>();
	List<GameObject> m_RoomsBridge = new List<GameObject>();
	List<GameObject> m_RoomsFactory = new List<GameObject>();
	List<GameObject> m_RoomsLifeSupport = new List<GameObject>();
	List<GameObject> m_RoomsGravityGen = new List<GameObject>();
	List<GameObject> m_RoomsEngine = new List<GameObject>();


};