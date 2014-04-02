using UnityEngine;
using System.Collections;

public class DynamicEvent_RogueAsteroid
{
	float mLowestCost = 60.0f;	// Where difficulty is 100%, it would take X seconds before the DM can afford this event at its lowest cost. Todo: Favourably, this would scale based on the density of asteroids in the area.
	float mTimeWhenCooldownLastUpdated = float.PositiveInfinity;
	float mCooldownCostPerUse = 30.0f;	// Adds X to cooldown per use.
	float mCooldown = 0.0f;	// Time until the event reaches its lowest cost - is used to scale the cost until then.

	public DynamicEvent_RogueAsteroid()
	{
		DungeonMaster.instance.AddDynamicEvent(new DungeonMaster.DynamicEvent(Cost, Behaviour));
	}

	public void Cost(out float _cost)
	{
		// Update the cooldown before calculating the cost.
		float currentTime = Time.time;
		float deltaTime = currentTime - mTimeWhenCooldownLastUpdated;	// Time since cooldown was last updated.
		mTimeWhenCooldownLastUpdated = currentTime;

		mCooldown -= deltaTime;	// Cool the cooldown.
		if(mCooldown < 0.0f) mCooldown = 0.0f;	// Cooldown can't be less than zero, else the cost will be lower than the lowest cost.

		// Return the cost at the present time.
		_cost = mLowestCost + mCooldown;	// Simple cooldown effect where the cost to deploy this event increases by 50% of the base cost per use. More math could make it scale exponentially or linearly or such.
	}

	public void Behaviour()
	{
		mCooldown += mCooldownCostPerUse;	// Increase cooldown by a fixed amount with each use.

		CGalaxy.SCellPos parentAbsoluteCell = CGalaxy.instance.RelativePointToAbsoluteCell(CGameShips.GalaxyShip.transform.position);
		uint uiTriesToPlaceRogueAsteroid = 5;
		bool created;
		do
		{
			Vector3 asteroidPosition = (CGameShips.GalaxyShip.transform.position - CGalaxy.instance.RelativeCellToRelativePoint(parentAbsoluteCell - CGalaxy.instance.centreCell)) + Random.onUnitSphere * CGalaxy.instance.cellRadius/*Fog end*/;
			created = CGalaxy.instance.LoadGubbin(new CGalaxy.CGubbinMeta(
				(CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1),   // PrefabID
				parentAbsoluteCell, // Parent cell.
				asteroidPosition,   // Position relative to parent.
				Random.rotationUniform, // Rotation.
				Vector3.one,
				(CGameShips.GalaxyShip.transform.position - asteroidPosition).normalized * 50.0f,  // Linear velocity.
				Random.onUnitSphere * 0.1f,    // Angular velocity.
				true,   // Has networked entity script.
				true)); // Has rigid body.
		} while (created != true && --uiTriesToPlaceRogueAsteroid > 0);
	}
}