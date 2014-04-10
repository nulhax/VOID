using UnityEngine;
using System.Collections;

public class DynamicEventEnemyShip
{
	float mLowestCost = 200.0f;	// Where difficulty is 100%, it would take X seconds before the DM can afford this event at its lowest cost. Todo: Favourably, this would scale based on the density of asteroids in the area.
	float mTimeWhenCooldownLastUpdated = float.PositiveInfinity;
	float mCooldownCostPerUse = 100.0f;	// Adds X to cooldown per use.
	float mCooldown = 0.0f;	// Time until the event reaches its lowest cost - is used to scale the cost until then.

	public DynamicEventEnemyShip()
	{
		DungeonMaster.instance.AddDynamicEvent(new DungeonMaster.DynamicEvent(Cost, Behaviour));
	}

	public void Cost(out float _cost)
	{
		// Local variables
		if (mTimeWhenCooldownLastUpdated == float.PositiveInfinity) { mTimeWhenCooldownLastUpdated = Time.time; }

		// Update the cooldown before calculating the cost.
		float currentTime = Time.time;
		float deltaTime = currentTime - mTimeWhenCooldownLastUpdated;	// Time since cooldown was last updated.
		mTimeWhenCooldownLastUpdated = currentTime;

		mCooldown -= deltaTime;	// Cool the cooldown.
		if (mCooldown < 0.0f) mCooldown = 0.0f;	// Cooldown can't be less than zero, else the cost will be lower than the lowest cost.

		// Return the cost at the present time.
		_cost = mLowestCost + mCooldown;	// Simple cooldown effect where the cost to deploy this event increases by 50% of the base cost per use. More math could make it scale exponentially or linearly or such.
	}

	public void Behaviour()
	{
		mCooldown += mCooldownCostPerUse;	// Increase cooldown by a fixed amount with each use.

		CGalaxy galaxy = CGalaxy.instance;
        uint uiTriesToPlace = 5;
        bool created;
        do
        {
			Vector3 approachVectorNorm = Random.onUnitSphere;
			Vector3 relativePosition = CGameShips.GalaxyShip.transform.position + approachVectorNorm * galaxy.backdrop.fogEnd;
			CGalaxy.SCellPos absoluteParentCell = galaxy.RelativePointToAbsoluteCell(relativePosition);
			Vector3 relativePositionToParent = relativePosition - galaxy.AbsoluteCellToRelativePoint(absoluteParentCell);
			created = CGalaxy.instance.LoadGubbin(new CGalaxy.CGubbinMeta(
				(CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.EnemyShip_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.EnemyShip_LAST + 1),   // PrefabID
				absoluteParentCell, // Parent cell.
				relativePositionToParent,	// Position relative to parent.
				Random.rotationUniform,	// Rotation.
				Vector3.one,	// Scale
				Vector3.zero,	// Linear velocity.
				Vector3.zero,	// Angular velocity.
				true,	// Has NetworkedEntity script.
				true));	// Has a rigid body.
        } while (created != true && --uiTriesToPlace > 0);
	}
}