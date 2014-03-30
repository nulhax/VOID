using UnityEngine;
using System.Collections;

public class DynamicEvent_RogueAsteroid
{
    public DynamicEvent_RogueAsteroid()
    {
        DungeonMaster.instance.AddDynamicEvent(new DungeonMaster.DynamicEvent(60.0f, Behaviour));
    }

    public static void Behaviour()
    {
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