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
		
		INVALID 				= 0,

        // Player
        PlayerActor 			= 1,

        // Galaxy
        Galaxy					= 10,
        Asteroid_FIRST,
        Asteroid_LAST 			= Asteroid_FIRST + 2,

        // Minerals
        Crystal					= 100,

		// Ships
		Ship					= 200,
		GalaxyShip,
		EnemyShip_FIRST,
		EnemyShip_LAST			= EnemyShip_FIRST + 13,
		
		// Facilities
		Facility				= 300,
		Facility_Preplaced,

		// Tiles
		Tile					= 350,

		// Facility Miniature
		MiniFacilityBridge		= 400, 
		MiniHallwayStraight,
		MiniHallwayCorner,
		MiniHallwayTSection,
		MiniHallwayXSection,
		
		// Accessories
        Alarm					= 500,
        ControlConsole,
		Door,
        DuiMontior,
		
		// Modules
		AtmosphereGenerator		= 600,
		PlayerSpawner,
		TurretCockpit,
		PilotCockpit,
		PowerGenerator,
		PowerCapacitor,
        Dispenser,
		NaniteSilo,
        Engine,
		Starter,
		Prefabricator,
        TurretPulseSmall        = 640,
        TurretPulseMedium,
        TurretMissileSmall      = 645,
        TurretMissileMedium,
        MissileProjectile                 = 660,
        MissileHitParticles     = 662,

		// Components
		PanelFuseBox			= 700,
        CellSlot,

        // Tools
        ToolTorch				= 800,
        ToolRatchet,
        ToolExtinguisher,
        ToolAk47,
        ToolMedical,
        ToolCircuitryKit,
        ToolMiningDrill,

        // Hazards
        Fire					= 900,

		// User Interfaces
		DUITest					= 1000,
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
		DUIPrefabricator,

		// NulOS
		NOSPanelWide			= 1100,

		// NulOS Widgets
		NOSWFacilityControl		= 1200,
		NOSWShipPropulsion,
		NOSWShipPower,
		NOSWShipNanites,
		NOSWShipCrew,

		// Other
		LaserProjectile	= 1300,
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
        RegisterAccessories();
        RegisterModules();
        RegisterComponents();
		RegisterUserInterfaces();
		RegisterNulOSWidgets();
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
		for (int i = 0; i <= ENetworkPrefab.EnemyShip_LAST - ENetworkPrefab.EnemyShip_FIRST; ++i)
			CNetwork.Factory.RegisterPrefab((ushort)((int)ENetworkPrefab.EnemyShip_FIRST + i), "EnemyShips/EnemyShip" + i.ToString());
		
		// Facilities
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Facility,              		"Facilities/Facility");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Facility_Preplaced,          "Facilities/Preplaced_Facility");

		// Tile
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Tile,              			"Tiles/Tile");

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
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.AtmosphereGenerator,			"Modules/Atmosphere Generator Small");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlayerSpawner,				"Modules/Player Spawner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretCockpit,				"Modules/Turret Cockpit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PilotCockpit,				"Modules/Pilot Cockpit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerGenerator,				"Modules/Power Generator Small");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerCapacitor,				"Modules/Power Battery");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Dispenser,                   "Modules/Dispenser");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NaniteSilo,               	"Modules/Nanite Silo Small");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Engine,                      "Modules/Engine Small");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Starter,                     "Modules/Starter");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Prefabricator,               "Modules/Prefabricator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretPulseSmall,            "Modules/Pulse Turret Small");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretPulseMedium,           "Modules/Pulse Turret Medium");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretMissileSmall,          "Modules/Missile Turret Small");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretMissileMedium,         "Modules/Missile Turret Medium");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MissileProjectile,           "Modules/Turrets/Missile Projectile");
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.MissileHitParticles,         "Modules/Turrets/Missile Hit Particles");

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
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolMiningDrill,             "Tools/Mining Drill/ToolMiningDrill");

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
        CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DuiFacilityDoor,     	    "User Interface/DUI/DuiDoorControl");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIFacilityControl,     	    "User Interface/DUI/DUIFacilityControl");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.DUIPrefabricator,     	    "User Interface/DUI/Modules/DUIPrefabricator");


		// NulOS
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NOSPanelWide,				"User Interface/NulOS/NOSPanelWide");

		// NulOS Widgets
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NOSWFacilityControl,			"User Interface/NulOS/Widgets/WidgetFacilityControl");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NOSWShipPropulsion,			"User Interface/NulOS/Widgets/WidgetShipPropulsion");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NOSWShipPower,				"User Interface/NulOS/Widgets/WidgetShipPower");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NOSWShipNanites,				"User Interface/NulOS/Widgets/WidgetShipNanites");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.NOSWShipCrew,				"User Interface/NulOS/Widgets/WidgetShipCrew");

		// Other
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserProjectile,		        "Modules/Turrets/Laser Projectile");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserHitParticles,			"Modules/Turrets/Laser Hit Particles");
	}


	void RegisterSerailizationTargets()
	{
		CNetworkConnection.RegisterSerializationTarget(CPlayerMotor.SerializeOutbound               , CPlayerMotor.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerHead.SerializeOutbound                , CPlayerHead.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CBridgeCockpit.SerializeCockpitInteractions  , CBridgeCockpit.UnserializeCockpitInteractions);
		CNetworkConnection.RegisterSerializationTarget(CCockpitBehaviour.SerializeOutbound          , CCockpitBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerIKController.SerializeIKTarget	    , CPlayerIKController.UnserializeIKTarget);
		CNetworkConnection.RegisterSerializationTarget(CGamePlayers.SerializeData                   , CGamePlayers.UnserializeData);
        CNetworkConnection.RegisterSerializationTarget(CGameChat.SerializeData                      , CGameChat.UnserializeData);
		CNetworkConnection.RegisterSerializationTarget(CLaserTurretBehaviour.SerializeOutbound      , CLaserTurretBehaviour.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CMiningTurretBehaviour.SerializeOutbound     , CMiningTurretBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CPlayerBelt.SerializeOutbound                , CPlayerBelt.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CDUIElement.SerializeElementEvents    		, CDUIElement.UnserializeElementEvents);
		CNetworkConnection.RegisterSerializationTarget(CDUISlider.SerializeSliderEvents    		    , CDUISlider.UnserializeSliderEvents);
		CNetworkConnection.RegisterSerializationTarget(CMiningTurretBehaviour.SerializeOutbound     , CMiningTurretBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CUserInput.SerializeOutbound                 , CUserInput.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CRatchetBehaviour.Serialize                	, CRatchetBehaviour.Unserialize);
		CNetworkConnection.RegisterSerializationTarget(CPlayerRagdoll.Serialize                		, CPlayerRagdoll.Unserialize);
        CNetworkConnection.RegisterSerializationTarget(CTurretCockpitBehaviour.SerializeOutbound    , CTurretCockpitBehaviour.UnserializeInbound);


        CNetworkConnection.RegisterSerializationTarget(CTurretBehaviour.SerializeOutbound             , CTurretBehaviour.UnserializeInbound); // Process first so rotations can be updated before turret specialized behaviours process
        CNetworkConnection.RegisterSerializationTarget(CPulseTurretSmallBehaviour.SerializeOutbound   , CPulseTurretSmallBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CPulseTurretMediumBehaviour.SerializeOutbound  , CPulseTurretMediumBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CMissileTurretSmallBehaviour.SerializeOutbound , CMissileTurretSmallBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CMissileTurretMediumBehaviour.SerializeOutbound, CMissileTurretMediumBehaviour.UnserializeInbound);



        // Player
        CNetworkConnection.RegisterSerializationTarget(CPlayerInteractor.SerializeOutbound          , CPlayerInteractor.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CPlayerNaniteLaser.SerializeOutbound         , CPlayerNaniteLaser.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CPlayerModuleMenu.SerializeOutbound          , CPlayerModuleMenu.UnserializeInbound);
        
        // Tools
        CNetworkConnection.RegisterSerializationTarget(CAk47Behaviour.SerializeOutbound             , CAk47Behaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CFireExtinguisherSpray.SerializeOutbound     , CFireExtinguisherSpray.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CMedicalSpray.SerializeOutbound              , CMedicalSpray.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CTorchLight.SerializeOutbound                , CTorchLight.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CModuleGunBehaviour.SerializeOutbound        , CModuleGunBehaviour.UnserializeInbound);
        CNetworkConnection.RegisterSerializationTarget(CMiningDrillBehaviour.SerializeOutbound      , CMiningDrillBehaviour.UnserializeInbound);
	}


    void RegisterAccessories()
    {
        CAccessoryInterface.RegisterPrefab(CAccessoryInterface.EType.Alarm                          , ENetworkPrefab.Alarm);
    }



	void RegisterTools()
	{
		CToolInterface.RegisterPrefab(CToolInterface.EType.Ratchet         , ENetworkPrefab.ToolRatchet);
		CToolInterface.RegisterPrefab(CToolInterface.EType.CircuitryKit    , ENetworkPrefab.ToolCircuitryKit);
		CToolInterface.RegisterPrefab(CToolInterface.EType.FireExtinguisher, ENetworkPrefab.ToolExtinguisher);
		CToolInterface.RegisterPrefab(CToolInterface.EType.AK47            , ENetworkPrefab.ToolAk47);   
		CToolInterface.RegisterPrefab(CToolInterface.EType.MiningDrill     , ENetworkPrefab.ToolMiningDrill);
	}


    void RegisterModules()
    {
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.AtmosphereGenerator  , ENetworkPrefab.AtmosphereGenerator);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PlayerSpawner        , ENetworkPrefab.PlayerSpawner);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.TurretCockpit        , ENetworkPrefab.TurretCockpit);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PilotCockpit         , ENetworkPrefab.PilotCockpit);		
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PowerGenerator       , ENetworkPrefab.PowerGenerator);			
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.PowerBattery         , ENetworkPrefab.PowerCapacitor);			
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.Dispenser            , ENetworkPrefab.Dispenser);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.NaniteSilo        	, ENetworkPrefab.NaniteSilo);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.Engine               , ENetworkPrefab.Engine);
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.Starter              , ENetworkPrefab.Starter);
		CModuleInterface.RegisterPrefab(CModuleInterface.EType.Prefabricator        , ENetworkPrefab.Prefabricator);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.TurretPulseSmall     , ENetworkPrefab.TurretPulseSmall);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.TurretPulseMedium    , ENetworkPrefab.TurretPulseMedium);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.TurretMissleSmall    , ENetworkPrefab.TurretMissileSmall);
        CModuleInterface.RegisterPrefab(CModuleInterface.EType.TurretMissileMedium  , ENetworkPrefab.TurretMissileMedium);

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
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.NOSPanelWide 		, ENetworkPrefab.NOSPanelWide);
		CDUIRoot.RegisterPrefab(CDUIRoot.EType.Prefabricator  		, ENetworkPrefab.DUIPrefabricator);
	}


	void RegisterNulOSWidgets()
	{
		CNOSWidget.RegisterPrefab(CNOSWidget.EType.FacilityControl 	, ENetworkPrefab.NOSWFacilityControl);
		CNOSWidget.RegisterPrefab(CNOSWidget.EType.ShipPropulsion 	, ENetworkPrefab.NOSWShipPropulsion);
		CNOSWidget.RegisterPrefab(CNOSWidget.EType.ShipPower 		, ENetworkPrefab.NOSWShipPower);
		CNOSWidget.RegisterPrefab(CNOSWidget.EType.ShipNanites 		, ENetworkPrefab.NOSWShipNanites);
		CNOSWidget.RegisterPrefab(CNOSWidget.EType.ShipCrew 		, ENetworkPrefab.NOSWShipCrew);
	}


    void RegisterKeyBindings()
    {
        CUserInput.SetKeyBinding(CUserInput.EInput.Primary, KeyCode.Mouse0);
        CUserInput.SetKeyBinding(CUserInput.EInput.Secondary, KeyCode.Mouse1);

        CUserInput.SetKeyBinding(CUserInput.EInput.Use, KeyCode.F);
		CUserInput.SetKeyBinding(CUserInput.EInput.Visor, KeyCode.C);

        CUserInput.SetKeyBinding(CUserInput.EInput.Move_Forward, KeyCode.W);
        CUserInput.SetKeyBinding(CUserInput.EInput.Move_Backwards, KeyCode.S);
        CUserInput.SetKeyBinding(CUserInput.EInput.Move_StrafeLeft, KeyCode.A);
        CUserInput.SetKeyBinding(CUserInput.EInput.Move_StrafeRight, KeyCode.D);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveGround_Jump, KeyCode.Space);
        CUserInput.SetKeyBinding(CUserInput.EInput.Move_Crouch, KeyCode.LeftControl);
        CUserInput.SetKeyBinding(CUserInput.EInput.Move_Run, KeyCode.LeftShift);

        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_Up, KeyCode.Space);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_Down, KeyCode.LeftControl);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_RollLeft, KeyCode.E);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_RollRight, KeyCode.Q);
        CUserInput.SetKeyBinding(CUserInput.EInput.MoveFly_Stabilize, KeyCode.LeftShift);
        

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
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_RollLeft, KeyCode.Q);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_RollRight, KeyCode.E);
        CUserInput.SetKeyBinding(CUserInput.EInput.GalaxyShip_Turbo, KeyCode.LeftShift);// Shift

        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_EquipToolSlot1, KeyCode.Alpha1);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_EquipToolSlot2, KeyCode.Alpha2);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_EquipToolSlot3, KeyCode.Alpha3);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_EquipToolSlot4, KeyCode.Alpha4);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_Reload, KeyCode.R);
        CUserInput.SetKeyBinding(CUserInput.EInput.Tool_Drop, KeyCode.G);

        CUserInput.SetKeyBinding(CUserInput.EInput.ModuleMenu_ToggleDisplay, KeyCode.B);
        CUserInput.SetKeyBinding(CUserInput.EInput.TurretMenu_ToggleDisplay, KeyCode.Tab);

        CUserInput.SetKeyBinding(CUserInput.EInput.ReturnKey, KeyCode.Return);
        CUserInput.SetKeyBinding(CUserInput.EInput.Escape, KeyCode.Escape);
    }


// Member Fields


};
