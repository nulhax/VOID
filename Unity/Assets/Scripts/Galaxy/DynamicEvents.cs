using UnityEngine;
using System.Collections;

public class DynamicEvent_RogueAsteroid
{
    public DynamicEvent_RogueAsteroid()
    {
        DungeonMaster.instance.AddDynamicEvent(new DungeonMaster.DynamicEvent(2.0f, Behaviour));
    }

    public static void Behaviour()
    {
        CGalaxy.SCellPos parentAbsoluteCell = CGalaxy.instance.RelativePointToAbsoluteCell(CGame.GalaxyShip.transform.position);
        Vector3 pos = (CGame.GalaxyShip.transform.position - CGalaxy.instance.RelativeCellToRelativePoint(parentAbsoluteCell - CGalaxy.instance.centreCell)) + Random.onUnitSphere * CGalaxy.instance.cellRadius/*Fog end*/;
        CGalaxy.instance.LoadGubbin(new CGalaxy.SGubbinMeta((CGame.ENetworkRegisteredPrefab)Random.Range((ushort)CGame.ENetworkRegisteredPrefab.Asteroid_FIRST, (ushort)CGame.ENetworkRegisteredPrefab.Asteroid_LAST + 1), parentAbsoluteCell, Random.Range(10.0f, 30.0f), pos, Random.rotationUniform, (CGame.GalaxyShip.transform.position - pos).normalized * 100.0f, Random.onUnitSphere * 50.0f, 0.125f, true, true));
    }
}