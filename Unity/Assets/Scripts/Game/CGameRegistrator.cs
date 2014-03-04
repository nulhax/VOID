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
        Asteroid_LAST = Asteroid_FIRST + 2,

        // Minerals
        Crystal,

		// Ships
		Ship,
		GalaxyShip,
		EnemyShip,
		
		// Facilities
		FacilityBridge,
        FacilityAirlock,
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
        Engine,

		
		// Components
		PanelFuseBox,
        CellSlot,

        // Parts
//        BlackMatterCell,
//        FuelCell,
//        PlasmaCell,
//        PowerCell,
//        BioCell,
//        ReplicatorCell,

        // Tools
        ToolTorch,
        ToolRatchet,
        ToolExtinguisher,
        ToolAk47,
        ToolMedical,
        ToolCircuitryKit,
		ToolModuleGun,
        ToolCalibrator,
        ToolFluidizer,
        ToolMiningDrill,

        // Hazards
        Fire,

		// User Interfaces
		DUITest,
		DUIFacilityExpansion,
		DUIModuleCreation,
		DUIPowerGenerator,
		DUIPowerCapacitor,
		DUIAtmosphereGenerator,
		DUIDispenser,
		DUIShipPower,
		DUIShipPropulsion,
		DUIShipResources,
		DUINaniteCapsule,
		DUIEngine,
        DUIAirlockInternal,
        DuiFacilityDoor,
		DUIFacilityControl,

		// NulOS
		DUINOSPanel,
		NOSMenuNode,

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
        RegisterKeyBindings();
		RegisterTools();
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
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityAirlock,              "Facilities/FacilityAirlock");
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
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.AtmosphereGenerator,			"Modules/Atmosphere Generator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.AtmosphereConditioner,		"Modules/Atmosphere Conditioner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlayerSpawner,				"Modules/Player Spawner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserCockpit,				"Modules/Turret Cockpit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserTurret,					"Modules/Laser Turret");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PilotCockpit,				"Modules/Pilot Cockpit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerGenerator,				"Modules/Power Generator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerCapacitor,				"Modules/Power Capacitor");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiningTurret,				"Modules/Mining Turret");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MiningCockpit,				"Modules/Mining Cockpit");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Dispenser,                   "Modules/Dispenser");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NaniteCapsule,               "Modules/Nanite Capsule");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Engine,                      "Modules/Engine");

        // Components
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PanelFuseBox,                "Accessories/FuseBox");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.CellSlot,                    "Accessories/FuseBox");

        // Parts
//        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.BlackMatterCell,             "Parts/Cells/BlackMatterCell");
//        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FuelCell,                    "Parts/Cells/FuelCell");
//        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlasmaCell,                  "Parts/Cells/PlasmaCell");
//        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerCell,                   "Parts/Cells/PowerCell");
//        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.BioCell,                     "Parts/Cells/BioCell");
//        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ReplicatorCell,              "Parts/Cells/ReplicatorCell");

		// Tools
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolAk47,                    "Tools/Ak47/ToolAk47");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolExtinguisher,            "Tools/Fire Extinguisher/ToolExtinguisher");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolMedical,                 "Tools/Medical Gun/ToolMedical");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolRatchet,                 "Tools/Ratchet/ToolRachet");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolTorch,                   "Tools/Torch/ToolTorch");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolCircuitryKit,            "Tools/Circuitry Kit/ToolCircuitryKit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolModuleGun,               "Tools/Module Gun/ToolModuleGun");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolCalibrator,              "Tools/Calibrator/ToolCalibrator");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolFluidizer,               "Tools/FluidTool/ToolFluid");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolMiningDrill,             "Tools/ToolMiningDrill");

        // Hazards
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Fire,                        "Hazards/Fire/Fire_Old");

		// User Interface
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUITest,						"User Interface/DUI/DUIControlsTest");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIFacilityExpansion,		"User Interface/DUI/DUIFacilityExpansion");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIModuleCreation,			"User Interface/DUI/DUIModuleCreation");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIPowerGenerator,			"User Interface/DUI/Modules/DUIPowerGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIPowerCapacitor,			"User Interface/DUI/Modules/DUIPowerCapacitor");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIAtmosphereGenerator,		"User Interface/DUI/Modules/DUIAtmosphereGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIDispenser,				"User Interface/DUI/Modules/DUIDispenser");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIShipPower,				"User Interface/DUI/Ship/DUIShipPower");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIShipPropulsion,			"User Interface/DUI/Ship/DUIShipPropulsion");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIShipResources,			"User Interface/DUI/Ship/DUIShipResources");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUINaniteCapsule,			"User Interface/DUI/Modules/DUINaniteCapsule");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIEngine,					"User Interface/DUI/Modules/DUIPropulsionEngine");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIAirlockInternal,          "User Interface/DUI/DuiAirlockInternal");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DuiFacilityDoor,     	    "User Interface/DUI/DuiFacilityDoor");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIFacilityControl,     	    "User Interface/DUI/DUIFacilityControl");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUINOSPanel,					"User Interface/DUI/DUINOSMain");

		// NulOS
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NOSMenuNode,					"User Interface/DUI/NulOS/MenuNode");


		// Other
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserTurretProjectile,		"Modules/Other/Laser Turret Projectile");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserHitParticles,			"Modules/Other/Laser Hit Particles");
	}


	void RegisterSerailizationTargets()
	{
		CNetworkConnection.RegisterSerializationTarget(CPlayerGroundMotor.SerializePlayerState      , CPlayerGroundMotor.UnserializePlayerState);
		CNetworkConnection.RegisterSerializationTarget(CPlayerHead.SerializePlayerState             , CPlayerHead.UnserializePlayerState);
		CNetworkConnection.RegisterSerializationTarget(CBridgeCockpit.SerializeCockpitInteractions  , CBridgeCockpit.UnserializeCockpitInteractions);
		CNetworkConnection.RegisterSerializationTarget(CCockpit.SerializeOutbound                   , CCockpit.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerAirMotor.SerializeOutbound            , CPlayerAirMotor.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerIKController.SerializeIKTarget	    , CPlayerIKController.UnserializeIKTarget);
		CNetworkConnection.RegisterSerializationTarget(CGamePlayers.SerializeData                   , CGamePlayers.UnserializeData);
        CNetworkConnection.RegisterSerializationTarget(CGameChat.SerializeData                      , CGameChat.UnserializeData);
		CNetworkConnection.RegisterSerializationTarget(CLaserTurretBehaviour.SerializeOutbound      , CLaserTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CMiningTurretBehaviour.SerializeOutbound     , CMiningTurretBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CPlayerBelt.SerializeBeltState               , CPlayerBelt.UnserializeBeltState);
        CNetworkConnection.RegisterSerializationTarget(CPlayerBackPack.SerializeOutbound            , CPlayerBackPack.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CDUIElement.SerializeElementEvents    		, CDUIElement.UnserializeElementEvents);
		CNetworkConnection.RegisterSerializationTarget(CDUISlider.SerializeSliderEvents    		    , CDUISlider.UnserializeSliderEvents);
		CNetworkConnection.RegisterSerializationTarget(CMiningTurretBehaviour.SerializeOutbound     , CMiningTurretBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CUserInput.SerializeOutbound                 , CUserInput.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CRatchetBehaviour.Serialize                	, CRatchetBehaviour.Unserialize);
        
        // Tools
        CNetworkConnection.RegisterSerializationTarget(CPlayerInteractor.SerializeOutbound          , CPlayerInteractor.UnserializeInbound);
        
        CNetworkConnection.RegisterSerializationTarget(CAk47Behaviour.SerializeOutbound             , CAk47Behaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CFireExtinguisherSpray.SerializeOutbound     , CFireExtinguisherSpray.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CMedicalSpray.SerializeOutbound              , CMedicalSpray.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CTorchLight.SerializeOutbound                , CTorchLight.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CModuleGunBehaviour.SerializeOutbound        , CModuleGunBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CMiningDrillBehaviour.SerializeOutbound      , CMiningDrillBehaviour.UnserializeInbound);
	}



    void RegisterFacilities()
    {
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.Bridge                           , CGameRegistrator.ENetworkPrefab.FacilityBridge);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayStraight                  , CGameRegistrator.ENetworkPrefab.HallwayStraight);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayCorner                    , CGameRegistrator.ENetworkPrefab.HallwayCorner);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayTSection                  , CGameRegistrator.ENetworkPrefab.HallwayTSection);
        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.HallwayXSection                  , CGameRegistrator.ENetworkPrefab.HallwayXSection);

		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.Bridge                    , CGameRegistrator.ENetworkPrefab.MiniFacilityBridge);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayStraight           , CGameRegistrator.ENetworkPrefab.MiniHallwayStraight);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayCorner             , CGameRegistrator.ENetworkPrefab.MiniHallwayCorner);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayTSection           , CGameRegistrator.ENetworkPrefab.MiniHallwayTSection);
		CFacilityInterface.RegistMiniaturePrefab(CFacilityInterface.EType.HallwayXSection           , CGameRegistrator.ENetworkPrefab.MiniHallwayXSection);

        CFacilityInterface.RegisterPrefab(CFacilityInterface.EType.Airlock                          , CGameRegistrator.ENetworkPrefab.FacilityAirlock);
    }


    void RegisterAccessories()
    {
        CAccessoryInterface.RegisterPrefab(CAccessoryInterface.EType.Alarm                          , ENetworkPrefab.Alarm);
    }



	void RegisterTools()
	{
		CToolInterface.RegisterPrefab(CToolInterface.EType.Ratchet         , ENetworkPrefab.ToolRatchet);
		CToolInterface.RegisterPrefab(CToolInterface.EType.CircuitryKit    , ENetworkPrefab.ToolCircuitryKit);
		CToolInterface.RegisterPrefab(CToolInterface.EType.Calibrator      , ENetworkPrefab.ToolCalibrator);
		CToolInterface.RegisterPrefab(CToolInterface.EType.Fluidizer       , ENetworkPrefab.ToolFluidizer);
		CToolInterface.RegisterPrefab(CToolInterface.EType.ModuleCreator   , ENetworkPrefab.ToolModuleGun);
		CToolInterface.RegisterPrefab(CToolInterface.EType.FireExtinguisher, ENetworkPrefab.ToolExtinguisher);
		CToolInterface.RegisterPrefab(CToolInterface.EType.Norbert         , ENetworkPrefab.ToolModuleGun);
		CToolInterface.RegisterPrefab(CToolInterface.EType.HealingKit      , ENetworkPrefab.ToolMedical);
		CToolInterface.RegisterPrefab(CToolInterface.EType.AK47            , ENetworkPrefab.ToolAk47);   
		CToolInterface.RegisterPrefab(CToolInterface.EType.MiningDrill     , ENetworkPrefab.ToolMiningDrill);  
	}


    void RegisterModules()
    {
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.AtmosphereGenerator  , ENetworkPrefab.AtmosphereGenerator);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PlayerSpawner        , ENetworkPrefab.PlayerSpawner);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.LaserCockpit         , ENetworkPrefab.LaserCockpit);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.LaserTurret          , ENetworkPrefab.LaserTurret);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PilotCockpit         , ENetworkPrefab.PilotCockpit);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PowerGenerator       , ENetworkPrefab.PowerGenerator);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PowerCapacitor       , ENetworkPrefab.PowerCapacitor);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.MiningTurret         , ENetworkPrefab.MiningTurret);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.MiningCockpit        , ENetworkPrefab.MiningCockpit);	
		//CModuleInterface.RegisterPrefab(CModuleInterface.EType.AtmosphereConditioner, ENetworkPrefab.AtmosphereConditioner);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.Dispenser            , ENetworkPrefab.Dispenser);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.NaniteCapsule        , ENetworkPrefab.NaniteCapsule);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.Engine               , ENetworkPrefab.Engine);
    }


    void RegisterComponents()
    {
        //CComponentInterface.RegisterPrefab(CComponentInterface.EType.CellSlot, ENetworkPrefab.CellSlot);
        //CComponentInterface.RegisterPrefab(CComponentInterface.EType.FuseBox, ENetworkPrefab.PanelFuseBox);
    }


	void RegisterUserInterfaces()
	{
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.ControlsTest         , ENetworkPrefab.DUITest);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.FacilityExpansion    , ENetworkPrefab.DUIFacilityExpansion);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.ModuleCreation       , ENetworkPrefab.DUIModuleCreation);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.PowerGenerator       , ENetworkPrefab.DUIPowerGenerator);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.PowerCapacitor       , ENetworkPrefab.DUIPowerCapacitor);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.AtmosphereGenerator  , ENetworkPrefab.DUIAtmosphereGenerator);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.Dispenser            , ENetworkPrefab.DUIDispenser);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.ShipPower            , ENetworkPrefab.DUIShipPower);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.ShipPropulsion       , ENetworkPrefab.DUIShipPropulsion);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.ShipResources        , ENetworkPrefab.DUIShipResources);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.NaniteCapsule        , ENetworkPrefab.DUINaniteCapsule);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.Engine               , ENetworkPrefab.DUIEngine);
        CDUIRoot.RegisterPrefab(CDUIRoot.EType.AirlockInternal      , ENetworkPrefab.DUIAirlockInternal);
        CDUIRoot.RegisterPrefab(CDUIRoot.EType.FacilityDoor         , ENetworkPrefab.DuiFacilityDoor);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.FacilityControl 		, ENetworkPrefab.DUIFacilityControl);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.NOSPanel 			, ENetworkPrefab.DUINOSPanel);
	}


    void RegisterKeyBindings()
    {
        CUserInput.SetKeyBinding(CUserInput.EInput.Primary, KeyCode.Mouse0);
        CUserInput.SetKeyBinding(CUserInput.EInput.Secondary, KeyCode.Mouse1);

        CUserInput.SetKeyBinding(CUserInput.EInput.Use, KeyCode.F);
		CUserInput.SetKeyBinding(CUserInput.EInput.Visor, KeyCode.C);

        CUserInput.SetKeyBinding(CUserInput.EInput.MoveGround_Forward, KeyCode.W);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveGround_Backwards, KeyCode.S);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveGround_StrafeLeft, KeyCode.A);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveGround_StrafeRight, KeyCode.D);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveGround_Jump, KeyCode.Space);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveGround_Crouch, KeyCode.LeftControl);

        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_Up, KeyCode.Space);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_Down, KeyCode.LeftControl);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_RollLeft, KeyCode.E);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_RollRight, KeyCode.Q);

        CUserInput.SetKeyBinding(CUserInput.EInput.Move_Turbo, KeyCode.LeftShift);

        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_Forward, KeyCode.W);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_Backward, KeyCode.S);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_Up, KeyCode.Space);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_Down, KeyCode.LeftControl);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_StrafeLeft, KeyCode.A);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_StrafeRight, KeyCode.D);
        //CUserInput.SetKeyBinding(CUserInput.axyShip_YawLeft]                = KeyCode.Mouse0);   // Mouse X
        //CUserInput.SetKeyBinding(CUserInput.axyShip_YawRight]               = KeyCode.);         // Mouse X
        //CUserInput.SetKeyBinding(CUserInput.axyShip_PitchUp]                = KeyCode);          // Mouse Y
        //CUserInput.SetKeyBinding(CUserInput.axyShip_PitchDown]              = KeyCode);          // Mouse Y
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_YawLeft, KeyCode.Q);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_YawRight, KeyCode.E);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_Turbo, KeyCode.LeftShift);// Shift

        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_SelectSlot1, KeyCode.Alpha1);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_SelectSlot2, KeyCode.Alpha2);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_SelectSlot3, KeyCode.Alpha3);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_SelectSlot4, KeyCode.Alpha4);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_Reload, KeyCode.R);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_Drop, KeyCode.G);

        CUserInput.SetKeyBinding(CUserInput.EInput.ReturnKey, KeyCode.Return);
    }


// Member Fields


};
