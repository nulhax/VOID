﻿//  Auckland
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
	private CGridUI m_GridUI = null;
	private CGrid m_Grid = null;

	public GameObject m_VariantItemTemplate = null;
	public UILabel m_SelectionLabel = null;
	
	public UIGrid m_FloorVartiationsGrid = null;
	public UIGrid m_WallVariationsGrid = null;
	public UIGrid m_CeilingVariationsGrid = null;

	public UILabel m_ModuleNameLabel = null;
	public UILabel m_ModuleDescLabel = null;
	public UILabel m_ModuleCategoryLabel = null;
	public UILabel m_ModuleCostLabel = null;

	private CModuleInterface.EType m_SelectedModuleType = CModuleInterface.EType.INVALID;
	private CModuleInterface.ECategory m_SelectedModuleCategory = CModuleInterface.ECategory.INVALID; 
	private CModuleInterface.ESize m_SelectedModuleSize = CModuleInterface.ESize.INVALID;
	private int m_SelectedModuleCost = 0;

	private Dictionary<ETileVariant, GameObject> m_CurrentFloorVariations = new Dictionary<ETileVariant, GameObject>();
	private Dictionary<ETileVariant, GameObject> m_CurrentWallVariations = new Dictionary<ETileVariant, GameObject>();
	private Dictionary<ETileVariant, GameObject> m_CurrentCeilingVariations = new Dictionary<ETileVariant, GameObject>();

	private CNetworkVar<CModuleInterface.EType> m_CurrentModuleType = null;


	// Member Properties
	public CModuleInterface.EType SelectedModuleType
	{
		get { return(m_SelectedModuleType); }
	}

	public CModuleInterface.ECategory SelectedModuleCategory
	{
		get { return(m_SelectedModuleCategory); }
	}

	public CModuleInterface.ESize SelectedModuleSize
	{
		get { return(m_SelectedModuleSize); }
	}

	public int SelectedModuleCost
	{
		get { return(m_SelectedModuleCost); }
	}
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_CurrentModuleType = _cRegistrar.CreateReliableNetworkVar<CModuleInterface.EType>(OnNetworkVarSync, CModuleInterface.EType.INVALID);
	}

	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		if(_SyncedNetworkVar == m_CurrentModuleType)
		{
			UpdateModuleInfo();
		}
	}

	public void RegisterGridUI(CGridUI _GridUI, CGrid _Grid)
	{
		m_GridUI = _GridUI;
		m_Grid = _Grid;

		m_GridUI.EventTileSelectionChange += OnTileSelectionChange;
	}

	public void ResetCursorMode()
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
		CGameShips.Ship.GetComponent<CShipTiles>().m_ShipGrid.ImportTileInformation(m_Grid.Tiles.ToArray());
	}
	
	[AServerOnly]
	public void SetSelectedModuleType(CModuleInterface.EType _ModuleType)
	{
		m_CurrentModuleType.Value = _ModuleType;
	}
	
	public void ResetModuleInfo()
	{
		if(CNetwork.IsServer)
			m_CurrentModuleType.Value = CModuleInterface.EType.INVALID;
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

		// Instance all default variants
		if(!m_CurrentFloorVariations.ContainsKey(ETileVariant.Default))
			InstanceNewFloorTileVariantSelection(ETileVariant.Default, "Item 01");
		
		if(!m_CurrentWallVariations.ContainsKey(ETileVariant.Default))
			InstanceNewWallTileVariantSelection(ETileVariant.Default, "Item 01");
		
		if(!m_CurrentCeilingVariations.ContainsKey(ETileVariant.Default))
			InstanceNewCeilingTileVariantSelection(ETileVariant.Default, "Item 01");

		// Reposition all items
		m_FloorVartiationsGrid.Reposition();
		m_WallVariationsGrid.Reposition();
		m_CeilingVariationsGrid.Reposition();
	}

	private void UpdateModuleInfo()
	{
		if(m_CurrentModuleType.Value == CModuleInterface.EType.INVALID)
		{
			m_SelectedModuleType = CModuleInterface.EType.INVALID;
			m_SelectedModuleCategory = CModuleInterface.ECategory.INVALID; 
			m_SelectedModuleSize = CModuleInterface.ESize.INVALID;
			m_SelectedModuleCost = 0;
		
			// Set the label values
			m_ModuleNameLabel.text = "N/A";
			m_ModuleCategoryLabel.text = "N/A";
			m_ModuleDescLabel.text = "N/A";
			m_ModuleCostLabel.text = "N/A";
			return;
		}

		// Create a temp module
		string modulePrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CModuleInterface.GetPrefabType(m_CurrentModuleType.Get()));
		GameObject moduleObject = (GameObject)Resources.Load("Prefabs/" + modulePrefabFile);
		CModuleInterface tempModuleInterface = moduleObject.GetComponent<CModuleInterface>();
		
		m_SelectedModuleType = tempModuleInterface.ModuleType;
		m_SelectedModuleCategory = tempModuleInterface.ModuleCategory; 
		m_SelectedModuleSize = tempModuleInterface.ModuleSize;
		m_SelectedModuleCost = m_SelectedModuleSize == CModuleInterface.ESize.Small ? 400 : 800;

		// Set the prefabricator cursor based on module size
		switch(m_SelectedModuleSize)
		{
		case CModuleInterface.ESize.Small:
			m_GridUI.m_CurrentMode = CGridUI.EToolMode.PlaceModulePort; break;

		case CModuleInterface.ESize.Medium:
			m_GridUI.m_CurrentMode = CGridUI.EToolMode.PlaceModulePort; break;

		case CModuleInterface.ESize.Large:
			m_GridUI.m_CurrentMode = CGridUI.EToolMode.PlaceModulePort; break;
		}

		// Set the label values
		m_ModuleNameLabel.text = CUtility.SplitCamelCase(m_SelectedModuleType.ToString());
		m_ModuleCategoryLabel.text = m_SelectedModuleCategory.ToString();
		m_ModuleDescLabel.text = CUtility.LoremIpsum(6, 12, 2, 4, 1);
		m_ModuleCostLabel.text = m_SelectedModuleCost.ToString() + "N";
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
