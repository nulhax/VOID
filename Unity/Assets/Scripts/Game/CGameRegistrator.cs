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
        Asteroid_LAST = Asteroid_FIRST/* + 3*/,

        // Minerals
        Crystal,

		// Ships
		Ship,
		GalaxyShip,
		EnemyShip,
		
		// Facilities
		FacilityBridge, 
        HallwayStraight,
        HallwayCorner,
        HallwayTSection,
        HallwayXSection,

		// Facility Miniature
		MiniFacilityBridge, 
		MiniHallwayStraight,
		MiniHallwayCorner,
		MiniHallwayTSection,
		MiniHallwayXSection,
		
		// Accessories
        Alarm,
        ControlConsole,
		Door,
        DuiMontior,
		
		// Modules
		AtmosphereGenerator,
		PlayerSpawner,
		LaserCockpit,
		LaserTurret,
		PilotCockpit,
		PowerGenerator,
		PowerCapacitor,
		MiningTurret,
		MiningCockpit,
		AtmosphereConditioner,
        OxygenRefiller,
        Dispenser,
        NaniteCapsule,

		
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
		ToolModuleGun,

        // Hazards
        Fire,

		// User Interfaces
		UITest,
		UIFacilityExpansion,
		UIModuleCreation,
		UIPowerGenerator,

		// Other
		LaserTurretProjectile,
		LaserHitParticles,

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
		RegisterUserInterfaces();
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
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Ship, "Ship/Ship");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.GalaxyShip, "Ship/GalaxyShip");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.EnemyShip, "Enemy Ship/Enemy Ship");
		
		// Facilities
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityBridge,               "Facilities/Bridge/Bridge");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayStraight,              "Facilities/Hallways/HallwayStraight");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayCorner,                "Facilities/Hallways/HallwayCorner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayTSection,              "Facilities/Hallways/HallwayTSection");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayXSection,              "Facilities/Hallways/HallwayXSection");

		// Facilities Mini
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiniFacilityBridge,           "Facilities/Bridge/BridgeMini");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiniHallwayStraight,          "Facilities/Hallways/HallwayStraightMini");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiniHallwayCorner,            "Facilities/Hallways/HallwayCornerMini");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiniHallwayTSection,          "Facilities/Hallways/HallwayTSectionMini");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiniHallwayXSection,          "Facilities/Hallways/HallwayXSectionMini");

        // Accessories
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ControlConsole,              "Accessories/Monitors/CurvedMonitor_wide");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Door,                        "Accessories/Doors/Door");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Alarm,                       "Accessories/Alarm");
		
		// Modules
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.AtmosphereGenerator,			"Modules/Atmosphere/Atmosphere Generator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.AtmosphereConditioner,		"Modules/Atmosphere/Atmosphere Conditioner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlayerSpawner,				"Modules/Crew/Player Spawner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserCockpit,				"Modules/Defence/Turret Cockpit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserTurret,					"Modules/Defence/Laser Turret/Laser Turret");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PilotCockpit,				"Modules/Exploration/Pilot Cockpit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerGenerator,				"Modules/Power/Power Generator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerCapacitor,				"Modules/Power/Power Capacitor");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiningTurret,				"Modules/Resources/Mining Turret/Mining Turret");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiningCockpit,				"Modules/Resources/Mining Cockpit");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Dispenser,                   "Modules/Production/Dispenser/Dispenser");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NaniteCapsule,               "Modules/Resources/Nanite Capsule/Nanite Capsule");

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
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolModuleGun,               "Tools/Module Gun/ToolModuleGun");

        // Hazards
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Fire,                        "Hazards/Fire/Fire");

		// User Interface
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.UITest,						"DUI/DUIControlsTest");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.UIFacilityExpansion,			"DUI/DUIFacilityExpansion");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.UIModuleCreation,			"DUI/DUIModuleCreation");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.UIPowerGenerator,			"DUI/Modules/DUIPowerGenerator");

		// Other
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserTurretProjectile,		"Modules/Defence/Laser Turret/Laser Turret Projectile");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserHitParticles,			"Modules/Defence/Laser Turret/Laser Hit Particles");
	}


	void RegisterSerailizationTargets()
	{
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerGroundMotor.SerializePlayerState     , CPlayerGroundMotor.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerHead.SerializePlayerState            , CPlayerHead.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CBridgeCockpit.SerializeCockpitInteractions , CBridgeCockpit.UnserializeCockpitInteractions);
		CNetworkConnection.RegisterThrottledSerializationTarget(CCockpit.SerializeOutbound                  , CCockpit.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CTurretBehaviour.SerializeOutbound          , CTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerAirMotor.SerializeOutbound           , CPlayerAirMotor.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerIKController.SerializeIKTarget		, CPlayerIKController.UnserializeIKTarget);
		CNetworkConnection.RegisterThrottledSerializationTarget(CGamePlayers.SerializeData					, CGamePlayers.UnserializeData);
        CNetworkConnection.RegisterThrottledSerializationTarget(CGameChat.SerializeData                     , CGameChat.UnserializeData);
		CNetworkConnection.RegisterThrottledSerializationTarget(CLaserTurretBehaviour.SerializeOutbound     , CLaserTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CMiningTurretBehaviour.SerializeOutbound    , CMiningTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBelt.SerializeBeltState                       , CPlayerBelt.UnserializeBeltState);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBackPack.SerializeOutbound                    , CPlayerBackPack.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CDUIElement.SerializeElementEvents    		, CDUIElement.UnserializeElementEvents);
		CNetworkConnection.RegisterThrottledSerializationTarget(CDUISlider.SerializeSliderEvents    		, CDUISlider.UnserializeSliderEvents);
        CNetworkConnection.RegisterThrottledSerializationTarget(CDispenserBehaviour.SerializeData           , CDispenserBehaviour.UnserializeData);
		CNetworkConnection.RegisterThrottledSerializationTarget(CMiningTurretBehaviour.SerializeOutbound    , CMiningTurretBehaviour.UnserializeInbound);
	}



    void RegisterFacilities()
    {
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.Bridge, CGameRegistrator.ENetworkPrefab.FacilityBridge);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayStraight, CGameRegistrator.ENetworkPrefab.HallwayStraight);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayCorner, CGameRegistrator.ENetworkPrefab.HallwayCorner);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayTSection, CGameRegistrator.ENetworkPrefab.HallwayTSection);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayXSection, CGameRegistrator.ENetworkPrefab.HallwayXSection);

		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.Bridge, CGameRegistrator.ENetworkPrefab.MiniFacilityBridge);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayStraight, CGameRegistrator.ENetworkPrefab.MiniHallwayStraight);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayCorner, CGameRegistrator.ENetworkPrefab.MiniHallwayCorner);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayTSection, CGameRegistrator.ENetworkPrefab.MiniHallwayTSection);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayXSection, CGameRegistrator.ENetworkPrefab.MiniHallwayXSection);
    }


    void RegisterAccessories()
    {
        CAccessoryInterface.RegisterPrefab(CAccessoryInterface.EType.Alarm, ENetworkPrefab.Alarm);
    }


    void RegisterModules()
    {
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.AtmosphereGenerator, ENetworkPrefab.AtmosphereGenerator);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PlayerSpawner, ENetworkPrefab.PlayerSpawner);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.LaserCockpit, ENetworkPrefab.LaserCockpit);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.LaserTurret, ENetworkPrefab.LaserTurret);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PilotCockpit, ENetworkPrefab.PilotCockpit);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PowerGenerator, ENetworkPrefab.PowerGenerator);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PowerCapacitor, ENetworkPrefab.PowerCapacitor);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.MiningTurret, ENetworkPrefab.MiningTurret);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.MiningCockpit, ENetworkPrefab.MiningCockpit);	
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.AtmosphereConditioner, ENetworkPrefab.AtmosphereConditioner);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.Dispenser, ENetworkPrefab.Dispenser);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.NaniteCapsule, ENetworkPrefab.NaniteCapsule);
    }


    void RegisterComponents()
    {
        //CComponentInterface.RegisterPrefab(CComponentInterface.EType.CellSlot, ENetworkPrefab.CellSlot);
        //CComponentInterface.RegisterPrefab(CComponentInterface.EType.FuseBox, ENetworkPrefab.PanelFuseBox);
    }


	void RegisterUserInterfaces()
	{
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.ControlsTest, ENetworkPrefab.UITest);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.FacilityExpansion, ENetworkPrefab.UIFacilityExpansion);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.ModuleCreation, ENetworkPrefab.UIModuleCreation);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.PowerGenerator, ENetworkPrefab.UIPowerGenerator);
	}


// Member Fields


};
