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
        CGalaxy.SCellPos parentAbsoluteCell = CGalaxy.instance.RelativePointToAbsoluteCell(CGameShips.GalaxyShip.transform.position);
        Vector3 pos = (CGameShips.GalaxyShip.transform.position - CGalaxy.instance.RelativeCellToRelativePoint(parentAbsoluteCell - CGalaxy.instance.centreCell)) + Random.onUnitSphere * CGalaxy.instance.cellRadius/*Fog end*/;
        CGalaxy.instance.LoadGubbin(new CGalaxy.SGubbinMeta((CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1), parentAbsoluteCell, Random.Range(10.0f, 30.0f), pos, Random.rotationUniform, (CGameShips.GalaxyShip.transform.position - pos).normalized * 100.0f, Random.onUnitSphere * 50.0f, 0.125f, true, true));
    }
}