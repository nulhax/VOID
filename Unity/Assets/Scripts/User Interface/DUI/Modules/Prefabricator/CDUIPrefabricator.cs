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

	public GameObject m_VariantItemTemplate = null;
	public UILabel m_SelectionLabel = null;
	
	public UIGrid m_FloorVartiationsGrid = null;
	public UIGrid m_WallVariationsGrid = null;
	public UIGrid m_CeilingVariationsGrid = null;

	private Dictionary<int, GameObject> m_CurrentFloorVariations = new Dictionary<int, GameObject>();
	private Dictionary<int, GameObject> m_CurrentWallVariations = new Dictionary<int, GameObject>();
	private Dictionary<int, GameObject> m_CurrentCeilingVariations = new Dictionary<int, GameObject>();


	// Member Properties


	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{

	}

	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{

	}

	public void RegisterGridUI(CPrefabricatorGridUI _GridUI, CGrid _Grid)
	{
		m_GridUI = _GridUI;
		m_Grid = _Grid;

		m_GridUI.EventTileSelectionChange += OnTileSelectionChange;
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

	public void EnableTileModifierSelection()
	{
		m_GridUI.m_CurrentMode = CPrefabricatorGridUI.EToolMode.Modify_Tile_Variants;
	}

	public void ExportGridTilesToShip()
	{
		m_GridUI.ExportTilesToShip();
	}

	public void OnTileSelectionChange()
	{
		// Update the selection widget
		UpdateSelectionWidget();

		// Update the tile type variants
		UpdateTileTypeVariants();
	}

	private void UpdateSelectionWidget()
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

	private void UpdateTileTypeVariants()
	{
		// If there are none selected then remove all items
		if(m_GridUI.m_SelectedTiles.Count == 0)
		{
			foreach(GameObject item in m_CurrentWallVariations.Values)
				Destroy(item);
			m_CurrentWallVariations.Clear();
			return;
		}

		// Instance all default variants
		if(!m_CurrentWallVariations.ContainsKey(0))
			InstanceNewWallTileVariantSelection(0, "Item 01");

		// Reposition all items
		m_WallVariationsGrid.Reposition();
	}

	private void InstanceNewWallTileVariantSelection(int _TileVariant, string _ItemName)
	{
		GameObject newSelection = (GameObject)GameObject.Instantiate(m_VariantItemTemplate);
		newSelection.name = _ItemName;
		newSelection.transform.parent = m_WallVariationsGrid.transform;
		newSelection.transform.localPosition = Vector3.zero;
		newSelection.transform.localScale = Vector3.one;
		
		newSelection.GetComponentInChildren<UILabel>().text = _TileVariant.ToString();
		EventDelegate.Add(newSelection.GetComponent<UIButton>().onClick, OnWallVariantModification);
		
		m_CurrentWallVariations.Add(_TileVariant, newSelection);
	}

	private void OnWallVariantModification()
	{
//		GameObject button = UIButton.current.gameObject;
//		
//		// Find the variant type
//		foreach(KeyValuePair<ETileVariant, GameObject> pair in m_CurrentWallVariations)
//		{
//			// If the button was found
//			if(pair.Value == button)
//			{
//				// Iterate each tile and set its variation
//				foreach(CTileInterface tile in m_GridUI.m_SelectedTiles)
//				{
//					tile.SetTileTypeVariant(CTile.EType.ExteriorWall, pair.Key);
//				}
//				return;
//			}
//		}
	}
}
