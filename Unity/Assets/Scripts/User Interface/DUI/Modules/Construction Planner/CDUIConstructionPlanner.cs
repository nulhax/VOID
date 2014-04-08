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


public class CDUIConstructionPlanner : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	private CGridUI m_GridUI = null;
	private CGrid m_Grid = null;

	public GameObject m_SelectionGridItemTemplate = null;
	public UILabel m_SelectionLabel = null;
	
	public UIGrid m_FloorSelectionGridVariations = null;
	public UIGrid m_WallExtSelectionGridVariations = null;
	public UIGrid m_WallIntSelectionGridVariations = null;
	public UIGrid m_CeilingSelectionGridVariations = null;
	
	private Dictionary<CTile.ETileType, UIGrid> m_SelectionVaritantGrids = new Dictionary<CTile.ETileType, UIGrid>();


	// Member Properties
	
	
	// Member Methods
	public void Awake()
	{
		m_SelectionVaritantGrids.Add(CTile.ETileType.Floor, m_FloorSelectionGridVariations);
		m_SelectionVaritantGrids.Add(CTile.ETileType.Wall_Ext, m_WallExtSelectionGridVariations);
		m_SelectionVaritantGrids.Add(CTile.ETileType.Wall_Int, m_WallIntSelectionGridVariations);
		m_SelectionVaritantGrids.Add(CTile.ETileType.Ceiling, m_CeilingSelectionGridVariations);
	}

	public void RegisterGridUI(CGridUI _GridUI, CGrid _Grid)
	{
		m_GridUI = _GridUI;
		m_Grid = _Grid;

		m_GridUI.EventTileSelectionChange += OnTileSelectionChange;
	}

	public void ResetMode()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Nothing;
	}

	public void EnableTilePainterExterior()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Paint_Exterior;
	}

	public void EnableTilePainterInteriorWalls()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Paint_Interior_Walls;
	}

	public void EnableTilePainterInteriorFloors()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Paint_Interior_Floors;
	}

	public void EnableTileModifierSelection()
	{
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.Select_Tiles;
	}

	public void ExportGridTilesToShip()
	{
		CGameShips.Ship.GetComponent<CShipFacilities>().m_ShipGrid.ImportTileInformation(m_Grid.Tiles.ToArray());
	}

	public void OnTileSelectionChange()
	{
		int count = m_GridUI.m_SelectedTiles.Count;
		if(count == 0)
		{
			m_SelectionLabel.text = "No Tile(s) Selected.";
			return;
		}

		if(count == 1)
		{
			m_SelectionLabel.text = "1 Tile Selected.";
			return;
		}

		if(count > 1)
		{
			m_SelectionLabel.text = count + " Tiles Selected.";
			return;
		}
	}
}
