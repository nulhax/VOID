//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIConstructionPlanner.cs
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


public class CDUIPrefabricator : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private CPrefabricatorGridUI m_GridUI = null;
	private CGrid m_Grid = null;


	// Member Properties


	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{

	}

	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{

	}

	public void RegisterGridUI(CPrefabricatorGridUI _GridUI, CGrid _Grid)
	{
		m_GridUI = _GridUI;
		m_Grid = _Grid;
	}

	public void ResetCursorMode()
	{
		m_GridUI.m_CurrentMode = CPrefabricatorGridUI.EToolMode.Nothing;
	}

	public void EnableTilePainterExterior()
	{
		m_GridUI.m_CurrentMode = CPrefabricatorGridUI.EToolMode.Paint_Exterior;
	}

	public void EnableTilePainterInteriorWalls()
	{
		m_GridUI.m_CurrentMode = CPrefabricatorGridUI.EToolMode.Paint_Interior_Walls;
	}

	public void EnableTilePainterInteriorFloors()
	{
		m_GridUI.m_CurrentMode = CPrefabricatorGridUI.EToolMode.Paint_Interior_Floors;
	}

	public void EnableTileModifierDoor()
	{
		m_GridUI.m_CurrentMode = CPrefabricatorGridUI.EToolMode.Modify_Tile_Door;
	}

	public void EnableTileModifierWindow()
	{
		m_GridUI.m_CurrentMode = CPrefabricatorGridUI.EToolMode.Modify_Tile_Window;
	}

	public void ExportGridTilesToShip()
	{
		m_GridUI.ExportTilesToShip();
	}
}
