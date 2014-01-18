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
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		// DO NOT MAKE THIS LOOK SHIT - put your shit in a LOGICAL place please!
		
		//^^												^^
		//^^ Learn to spell Registrar and I'll consider it. ^^
		//^^												^^
		
		INVALID,
		
		// Ships
		Ship,
		GalaxyShip,
		
		// Ship: Facilities
		FacilityBridge, 
		FacilityFactory,
		FacilityScanner,
		FacilityReplicator,
		FacilityEngine,
		FacilityGravityGenerator, 
		FacilityShieldGenerator, 
		FacilityLifeSupport,
		
		// Ship: Doors
		Door,
		
		// Ship: Hallways
		HallwayStraight,
		HallwayCorner,
		HallwayTSection, 
		HallwayXSection,
		
		// Player
		PlayerActor,
		
		// Register prefabs: Tools
		ToolTorch, 
		ToolRachet, 
		ToolExtinguisher,
		ToolAk47,
		ToolMedical,
		
		// Galaxy
		Galaxy,
		Asteroid_FIRST,
		Asteroid_LAST = Asteroid_FIRST + 3,
		
		// Minerals
		Crystal,
		
		// Hazards
		Fire,
		
		// Modules: General
		BlackMatterCell,
		FuelCell,
		PlasmaCell,
		PowerCell,
		BioCell,
		ReplicatorCell,
		ControlConsole,
		
		// Facility Components
		PanelFuseBox,
		PlayerSpawner,
		Alarm,
		
		// Cockpits
		BridgeCockpit,
		TurretCockpit,
		
		// Turrets
		LaserTurret,
		
		// Turret: Projectile
		TurretLaserProjectile,
		
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
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


	void RegisterPrefabs()
	{
		// Ships
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Ship, "Ship/Ship");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.GalaxyShip, "Ship/GalaxyShip");
		
		// Ship: Facilities
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityBridge, "Ship/Facilities/Bridge/Bridge");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityFactory, "Ship/Facilities/Factory/Factory");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityScanner, "Ship/Facilities/Scanner/Scanner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityReplicator, "Ship/Facilities/Replicator/Replicator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityEngine, "Ship/Facilities/Engine/Engine");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityGravityGenerator, "Ship/Facilities/Gravity Generator/GravityGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityShieldGenerator, "Ship/Facilities/Shield Generator/ShieldGenerator");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FacilityLifeSupport, "Ship/Facilities/Life Support/LifeSupport");
		
		// Ship: Doors
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Door, "Ship/Doors/Door");
		
		// Ship: Hallways
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayStraight, "Ship/Hallways/HallwayStraight");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayCorner, "Ship/Hallways/HallwayCorner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayTSection, "Ship/Hallways/HallwayTSection");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.HallwayXSection, "Ship/Hallways/HallwayXSection");
		
		// Player
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlayerActor, "Player/Player Actor");
		
		// Register prefabs: Tools
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolTorch, "Tools/ToolTorch");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolRachet, "Tools/ToolRachet");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolExtinguisher, "Tools/ToolExtinguisher");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolAk47, "Tools/ToolAk47");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ToolMedical, "Tools/ToolMedical");
		
		
		// Galaxy
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Galaxy, "Galaxy/Galaxy");
		for(ushort us = 0; us <= ENetworkPrefab.Asteroid_LAST - ENetworkPrefab.Asteroid_FIRST; ++us)    // All asteroids.
			CNetwork.Factory.RegisterPrefab((ushort)((ushort)ENetworkPrefab.Asteroid_FIRST + us), "Galaxy/Asteroid" + us.ToString());
		
		// Minerals
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Crystal, "Minerals/Crystal");
		
		// Hazards
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Fire, "Hazards/Fire");
		
		// Modules: General
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.BlackMatterCell, "Modules/BlackMatterCell");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.FuelCell, "Modules/FuelCell");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlasmaCell, "Modules/PlasmaCell");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PowerCell, "Modules/PowerCell");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.BioCell, "Modules/BioCell");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ReplicatorCell, "Modules/ReplicatorCell");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.ControlConsole, "Modules/DUI/CurvedMonitor_wide");
		
		// Facility Components
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PanelFuseBox, "Ship/Facilities/Components/FuseBox");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.PlayerSpawner, "Ship/Facilities/Components/PlayerSpawner");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.Alarm, "Ship/Facilities/Components/Alarm");
		
		// Cockpits
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.BridgeCockpit, "Ship/Facilities/Bridge/Cockpit");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretCockpit, "Ship/Facilities/Weapons System/Turret Cockpit");
		
		// Un categorized
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.LaserTurret, "Ship/Facilities/Weapons System/Turret");
		CNetwork.Factory.RegisterPrefab(ENetworkPrefab.TurretLaserProjectile, "Ship/Facilities/Weapons System/TurretLaserProjectile");
	}


	void RegisterSerailizationTargets()
	{
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerGroundMotor.SerializePlayerState, CPlayerGroundMotor.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerHead.SerializePlayerState, CPlayerHead.UnserializePlayerState);
		CNetworkConnection.RegisterThrottledSerializationTarget(CBridgeCockpit.SerializeCockpitInteractions, CBridgeCockpit.UnserializeCockpitInteractions);
		CNetworkConnection.RegisterThrottledSerializationTarget(CDUIInteraction.SerializeDUIInteractions, CDUIInteraction.UnserializeDUIInteraction);
		CNetworkConnection.RegisterThrottledSerializationTarget(CCockpit.SerializeOutbound, CCockpit.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CTurretController.SerializeOutbound, CTurretController.UnserializeInbound);
		CNetworkConnection.RegisterThrottledSerializationTarget(CPlayerAirMotor.SerializeOutbound, CPlayerAirMotor.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBelt.SerializeBeltState, CPlayerBelt.UnserializeBeltState);
		CNetworkConnection.RegisterSerializationTarget(CPlayerBackPack.SerializeOutbound, CPlayerBackPack.UnserializeInbound);
		CNetworkConnection.RegisterSerializationTarget(CPlayerIKController.SerializeIKTarget, CPlayerIKController.UnserializeIKTarget);
	}


// Member Fields


};
