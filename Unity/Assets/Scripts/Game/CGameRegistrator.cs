//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGameResourceLoader.cs
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


public class CGameRegistrator : MonoBehaviour
{

// Member Types


	public enum ENetworkPrefab : ushort
	{
		//^^												^^
		//^^ Learn to spell Registrar and I'll consider it. ^^
		//^^												^^
		
		INVALID,

        // Player
        PlayerActor,

        // Galaxy
        Galaxy,
        Asteroid_FIRST,
        Asteroid_LAST = Asteroid_FIRST + 3,

        // Minerals
        Crystal,

		// Ships
		Ship,
		GalaxyShip,
		
		// Facilities
		FacilityBridge, 
		FacilityFactory,
		FacilityScanner,
		FacilityReplicator,
		FacilityEngine,
		FacilityGravityGenerator, 
		FacilityShieldGenerator, 
		FacilityLifeSupport,
        HallwayStraight,
        HallwayCorner,
        HallwayTSection,
        HallwayXSection,
		
		// Accessories
        Alarm,
        ControlConsole,
		Door,
        DuiMontior,
		
        // Modules
        PilotCockpit,
        TurretCockpit,
        PlayerSpawner,
        LaserTurret,
        LaserTurretProjectile,
		MiningTurret,
		MiningCockpit,
		
		// Components
		PanelFuseBox,
        CellSlot,

        // Parts
        BlackMatterCell,
        FuelCell,
        PlasmaCell,
        PowerCell,
        BioCell,
        ReplicatorCell,

        // Tools
        ToolTorch,
        ToolRachet,
        ToolExtinguisher,
        ToolAk47,
        ToolMedical,
        ToolWiringKit,

        // Hazards
        Fire,
		
		MAX
	}


// Member Delegates & Events


// Member Properties


// Member Methods


	void Awake()
	{
		RegisterPrefabs();
	}


	void Start()
	{
		RegisterSerailizationTargets();
        RegisterFacilities();
        RegisterAccessories();
        RegisterModules();
        RegisterComponents();
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


	void RegisterPrefabs()
	{
        // Player
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlayerActor,                 "Player/Player Actor");

        // Galaxy
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Galaxy,                      "Galaxy/Galaxy");
        for (int i = 0; i <= ENetworkPrefab.Asteroid_LAST - ENetworkPrefab.Asteroid_FIRST; ++ i)
            CNetwork.Factory.RegisterPrefab((ushort)((int)ENetworkPrefab.Asteroid_FIRST + i), "Galaxy/Asteroid" + i.ToString());

        // Minerals
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Crystal,                     "Minerals/Crystal");

		// Ships
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Ship,                        "Ship/Ship");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.GalaxyShip,                  "Ship/GalaxyShip");
		
		// Facilities
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityBridge,              "Facilities/Bridge/Bridge");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityFactory,             "Facilities/Factory/Factory");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityScanner,             "Facilities/Scanner/Scanner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityReplicator,          "Facilities/Replicator/Replicator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityEngine,              "Facilities/Engine/Engine");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityGravityGenerator,    "Facilities/Gravity Generator/GravityGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityShieldGenerator,     "Facilities/Shield Generator/ShieldGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityLifeSupport,         "Facilities/Life Support/LifeSupport");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayStraight,             "Facilities/Hallways/HallwayStraight");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayCorner,               "Facilities/Hallways/HallwayCorner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayTSection,             "Facilities/Hallways/HallwayTSection");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayXSection,             "Facilities/Hallways/HallwayXSection");

        // Accessories
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ControlConsole,              "Accessories/DUI/CurvedMonitor_wide");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Door,                        "Accessories/Doors/Door");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Alarm,                       "Accessories/Alarm");
		
		// Modules
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PilotCockpit,                "Modules/Pilot Cockpit/Cockpit");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlayerSpawner,               "Modules/Player Spawner/PlayerSpawner");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretCockpit,               "Modules/Turret Cockpit/TurretCockpit");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserTurret,                 "Modules/Laser Turret/LaserTurret");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserTurretProjectile,       "Modules/Laser Turret/LaserTurretProjectile");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiningTurret, 			    "Modules/Mining Turret/MiningTurret");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiningCockpit, 			    "Modules/Mining Cockpit/MiningCockpit");

        // Components
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PanelFuseBox,                "Accessories/FuseBox");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.CellSlot,                    "Accessories/FuseBox");

        // Parts
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.BlackMatterCell,             "Parts/Cells/BlackMatterCell");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FuelCell,                    "Parts/Cells/FuelCell");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlasmaCell,                  "Parts/Cells/PlasmaCell");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerCell,                   "Parts/Cells/PowerCell");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.BioCell,                     "Parts/Cells/BioCell");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ReplicatorCell,              "Parts/Cells/ReplicatorCell");

		// Tools
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolAk47,                    "Tools/Ak47/ToolAk47");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolExtinguisher,            "Tools/Fire Extinguisher/ToolExtinguisher");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolMedical,                 "Tools/Medical Gun/ToolMedical");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolRachet,                  "Tools/Ratchet/ToolRachet");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolTorch,                   "Tools/Torch/ToolTorch");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolWiringKit,               "Tools/Wiring Kit/ToolWiringKit");

        // Hazards
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Fire,                        "Hazards/Fire/Fire");
	}


	void RegisterSerailizationTargets()
	{
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerGroundMotor.SerializePlayerState     , CPlayerGroundMotor.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerHead.SerializePlayerState            , CPlayerHead.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CBridgeCockpit.SerializeCockpitInteractions , CBridgeCockpit.UnserializeCockpitInteractions);
		//CNetworkConnection.RegisterThrottledSerializationTarget(CDUIInteraction.SerializeDUIInteractions  , CDUIInteraction.UnserializeDUIInteraction);
		CNetworkConnection.RegisterThrottledSerializationTarget(CCockpit.SerializeOutbound                  , CCockpit.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CTurretBehaviour.SerializeOutbound          , CTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerAirMotor.SerializeOutbound           , CPlayerAirMotor.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerIKController.SerializeIKTarget		, CPlayerIKController.UnserializeIKTarget);
		CNetworkConnection.RegisterThrottledSerializationTarget(CGamePlayers.SerializeData					, CGamePlayers.UnserializeData);
		CNetworkConnection.RegisterThrottledSerializationTarget(CLaserTurretBehaviour.SerializeOutbound     , CLaserTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CMiningTurretBehaviour.SerializeOutbound    , CMiningTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBelt.SerializeBeltState                       , CPlayerBelt.UnserializeBeltState);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBackPack.SerializeOutbound                    , CPlayerBackPack.UnserializeInbound);
	}



    void RegisterFacilities()
    {
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.Bridge, CGameRegistrator.ENetworkPrefab.FacilityBridge);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayStraight, CGameRegistrator.ENetworkPrefab.HallwayStraight);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayCorner, CGameRegistrator.ENetworkPrefab.HallwayCorner);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayTSection, CGameRegistrator.ENetworkPrefab.HallwayTSection);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayXSection, CGameRegistrator.ENetworkPrefab.HallwayXSection);
    }


    void RegisterAccessories()
    {
        CAccessoryInterface.RegisterPrefab(CAccessoryInterface.EType.Alarm, ENetworkPrefab.Alarm);
        //CModuleInterface.RegisterPrefab(CAccessoryInterface.EType.DuiMonitor, ENetworkPrefab.);
    }


    void RegisterModules()
    {
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.PilotCockpit, ENetworkPrefab.PilotCockpit);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.PlayerSpawner, ENetworkPrefab.PlayerSpawner);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.LaserCockpit, ENetworkPrefab.TurretCockpit);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.LaserTurret, ENetworkPrefab.LaserTurret);
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.MiningTurret, ENetworkPrefab.MiningTurret);
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.MiningCockpit, ENetworkPrefab.MiningCockpit);
    }


    void RegisterComponents()
    {
        CComponentInterface.RegisterPrefab(CComponentInterface.EType.CellSlot, ENetworkPrefab.CellSlot);
        CComponentInterface.RegisterPrefab(CComponentInterface.EType.FuseBox, ENetworkPrefab.PanelFuseBox);
    }


// Member Fields


};
