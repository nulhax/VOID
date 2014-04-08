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

	public GameObject m_VariantItemTemplate = null;
	public UILabel m_SelectionLabel = null;
	
	public UIGrid m_FloorVartiationsGrid = null;
	public UIGrid m_WallVariationsGrid = null;
	public UIGrid m_CeilingVariationsGrid = null;

	private Dictionary<ETileVariant, GameObject> m_CurrentFloorVariations = new Dictionary<ETileVariant, GameObject>();
	private Dictionary<ETileVariant, GameObject> m_CurrentWallVariations = new Dictionary<ETileVariant, GameObject>();
	private Dictionary<ETileVariant, GameObject> m_CurrentCeilingVariations = new Dictionary<ETileVariant, GameObject>();


	// Member Properties
	
	
	// Member Methods
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
		m_GridUI.m_CurrentMode = CGridUI.EToolMode.ModifyTileVariants;
	}

	public void ExportGridTilesToShip()
	{
		CGameShips.Ship.GetComponent<CShipFacilities>().m_ShipGrid.ImportTileInformation(m_Grid.Tiles.ToArray());
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
			foreach(GameObject item in m_CurrentFloorVariations.Values)
				Destroy(item);
			m_CurrentFloorVariations.Clear();

			foreach(GameObject item in m_CurrentWallVariations.Values)
				Destroy(item);
			m_CurrentWallVariations.Clear();

			foreach(GameObject item in m_CurrentCeilingVariations.Values)
				Destroy(item);
			m_CurrentCeilingVariations.Clear();

			return;
		}

		// Instance all nothing variants
		if(!m_CurrentFloorVariations.ContainsKey(ETileVariant.Nothing))
			InstanceNewFloorTileVariantSelection(ETileVariant.Nothing, "Item 01");

		if(!m_CurrentWallVariations.ContainsKey(ETileVariant.Nothing))
			InstanceNewWallTileVariantSelection(ETileVariant.Nothing, "Item 01");

		if(!m_CurrentCeilingVariations.ContainsKey(ETileVariant.Nothing))
			InstanceNewCeilingTileVariantSelection(ETileVariant.Nothing, "Item 01");

		// Instance all normal variants
		if(!m_CurrentFloorVariations.ContainsKey(ETileVariant.Default))
			InstanceNewFloorTileVariantSelection(ETileVariant.Default, "Item 02");
		
		if(!m_CurrentWallVariations.ContainsKey(ETileVariant.Default))
			InstanceNewWallTileVariantSelection(ETileVariant.Default, "Item 02");
		
		if(!m_CurrentCeilingVariations.ContainsKey(ETileVariant.Default))
			InstanceNewCeilingTileVariantSelection(ETileVariant.Default, "Item 02");

		// Reposition all items
		m_FloorVartiationsGrid.Reposition();
		m_WallVariationsGrid.Reposition();
		m_CeilingVariationsGrid.Reposition();
	}

	private void InstanceNewFloorTileVariantSelection(ETileVariant _TileVariant, string _ItemName)
	{
		GameObject newSelection = (GameObject)GameObject.Instantiate(m_VariantItemTemplate);
		newSelection.name = _ItemName;
		newSelection.transform.parent = m_FloorVartiationsGrid.transform;
		newSelection.transform.localPosition = Vector3.zero;
		newSelection.transform.localScale = Vector3.one;

		newSelection.GetComponentInChildren<UILabel>().text = _TileVariant.ToString();
		EventDelegate.Add(newSelection.GetComponent<UIButton>().onClick, OnFloorVariantModification);

		m_CurrentFloorVariations.Add(_TileVariant, newSelection);
	}
	
	private void InstanceNewWallTileVariantSelection(ETileVariant _TileVariant, string _ItemName)
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

	private void InstanceNewCeilingTileVariantSelection(ETileVariant _TileVariant, string _ItemName)
	{
		GameObject newSelection = (GameObject)GameObject.Instantiate(m_VariantItemTemplate);
		newSelection.name = _ItemName;
		newSelection.transform.parent = m_CeilingVariationsGrid.transform;
		newSelection.transform.localPosition = Vector3.zero;
		newSelection.transform.localScale = Vector3.one;
		
		newSelection.GetComponentInChildren<UILabel>().text = _TileVariant.ToString();
		EventDelegate.Add(newSelection.GetComponent<UIButton>().onClick, OnCeilingVariantModification);
		
		m_CurrentCeilingVariations.Add(_TileVariant, newSelection);
	}

	private void OnFloorVariantModification()
	{
		GameObject button = UIButton.current.gameObject;

		// Find the variant type
		foreach(KeyValuePair<ETileVariant, GameObject> pair in m_CurrentFloorVariations)
		{
			// If the button was found
			if(pair.Value == button)
			{
				// Iterate each tile and set its variation
				foreach(CTile tile in m_GridUI.m_SelectedTiles)
				{
					tile.SetTileTypeVariant(ETileType.Floor, pair.Key);
				}
				return;
			}
		}
	}

	private void OnWallVariantModification()
	{
		GameObject button = UIButton.current.gameObject;
		
		// Find the variant type
		foreach(KeyValuePair<ETileVariant, GameObject> pair in m_CurrentWallVariations)
		{
			// If the button was found
			if(pair.Value == button)
			{
				// Iterate each tile and set its variation
				foreach(CTile tile in m_GridUI.m_SelectedTiles)
				{
					tile.SetTileTypeVariant(ETileType.Wall_Ext, pair.Key);
				}
				return;
			}
		}
	}

	private void OnCeilingVariantModification()
	{
		GameObject button = UIButton.current.gameObject;
		
		// Find the variant type
		foreach(KeyValuePair<ETileVariant, GameObject> pair in m_CurrentCeilingVariations)
		{
			// If the button was found
			if(pair.Value == button)
			{
				// Iterate each tile and set its variation
				foreach(CTile tile in m_GridUI.m_SelectedTiles)
				{
					tile.SetTileTypeVariant(ETileType.Ceiling, pair.Key);
				}
				return;
			}
		}
	}
}
