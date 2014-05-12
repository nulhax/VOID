
//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGridManager.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


/* Implementation */


[RequireComponent(typeof(CGrid))]
public class CPrefabricatorGridUI : MonoBehaviour 
{
	// Member Types
	public enum EToolMode
	{
		INVALID,

		Nothing,
		Paint_Exterior,
		Paint_Interior_Walls,
		Paint_Interior_Floors,
		Modify_Tile_Variants,
	}

	public enum EPlaneInteraction
	{
		INVALID,

		Nothing,
		DragRotation,
		DragMovement,
	}

	
	// Member Delegates & Events
	public delegate void HandleGridUIEvent();

	public event HandleGridUIEvent EventTileSelectionChange;

	
	// Member Fields
	private CGrid m_Grid = null;

	public GameObject m_GridPlane = null;
	public GameObject m_GridCursor = null;

	public float m_GridScale = 0.1f;
	public Vector2 m_GridScaleLimits = new Vector2(0.05f, 0.2f);
	public Vector3 m_TilesOffset = Vector3.zero;

	public EToolMode m_CurrentMode = EToolMode.INVALID;
	public EPlaneInteraction m_CurrentPlaneInteraction = EPlaneInteraction.INVALID;
	public int m_CurrentVerticalLayer = 0;
	
	public Vector3 m_CurrentMouseHitPoint = Vector3.zero;
	public CGridPoint m_CurrentMouseGridPoint;

	public Vector3 m_MouseDownHitPoint = Vector3.zero;
	public CGridPoint m_MouseDownGridPoint;

	public Material m_TileMaterial = null;

	private List<RaycastHit> m_RaycastHits;
	private RaycastHit m_PlaneHit;

	private Quaternion m_DragRotateStart = Quaternion.identity;
	private Vector3 m_DragMovementStart = Vector3.zero;


	// Member Properties
	public bool IsShiftKeyDown
	{
		get { return(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)); }
	}
	
	public bool IsCtrlKeyDown
	{
		get { return(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)); }
	}
	
	public bool IsAltKeyDown
	{
		get { return(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)); }
	}


	// Member Methods
	private void Awake()
	{
		m_Grid = gameObject.GetComponent<CGrid>();

		// Register events
		m_Grid.EventTileInterfaceCreated += OnTileInterfaceCreated;
	}
	
	private void Start() 
	{
		if(!CNetwork.IsServer)
			return;
		
		// Update scale and clamp
		m_GridScale = Mathf.Clamp(m_GridScale, m_GridScaleLimits.x, m_GridScaleLimits.y);
		UpdateGridScale(m_GridScale);

		// Default enums
		m_CurrentMode = EToolMode.Nothing;
		m_CurrentPlaneInteraction = EPlaneInteraction.Nothing;

		// Instance new material
		m_TileMaterial = new Material(m_TileMaterial);

		// Import the tiles from the current ship
		ImportPreplacedTiles();

		// Register for when players join
		CGamePlayers.Instance.EventPlayerJoin += OnPlayerJoin;
	}

	private void ImportPreplacedTiles()
	{
		m_Grid.ImportTileInformation(CGameShips.Ship.GetComponent<CShipFacilities>().m_ShipGrid.TileInterfaces);
		foreach(CTileInterface tile in m_Grid.TileInterfaces)
		{
			// Register events
			tile.EventTileGeometryChanged += OnTileGeometryChange;
		}
	}
	
	private void Update() 
	{
		if(!CNetwork.IsServer)
			return;

        if (CGamePlayers.SelfActor == null)
            return;

		// Get the raycast hits against all grid objects
		RaycastHit[] lastRaycastHits = CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().LastRaycastHits;
		if(lastRaycastHits == null)
			return;

		m_RaycastHits = lastRaycastHits.ToList();
        m_RaycastHits.RemoveAll(hit => hit.collider != null && CUtility.FindInParents<CGrid>(hit.collider.transform) == null);

		// Get the plane hit
        m_PlaneHit = m_RaycastHits.Find(hit => hit.collider != null && hit.collider.gameObject == m_GridPlane);

		// Get the current grid point and hit point
		if(m_PlaneHit.collider != null)
		{
			m_CurrentMouseHitPoint = m_PlaneHit.point;
			m_CurrentMouseGridPoint = m_Grid.GetGridPoint(m_CurrentMouseHitPoint - (m_Grid.TileContainer.transform.rotation * m_TilesOffset * m_GridScale));
		}
		
		// Update cursor
		UpdateCursor();

		// Update input
		UpdateInput();

		// Update the material variables
		Vector3 up = m_GridPlane.transform.up;
		Vector3 pos = m_GridPlane.transform.position;
		m_TileMaterial.SetVector("_PlaneNormal", new Vector4(up.x, up.y, up.z));
		m_TileMaterial.SetVector("_PlanePoint", new Vector4(pos.x, pos.y + 2.0f * m_GridScale, pos.z));
		m_TileMaterial.SetFloat("_GlowDist", 0.3f * m_GridScale);
	}

	[AServerOnly]
	private void OnPlayerJoin(ulong _PlayerId)
	{
		gameObject.GetComponent<CNetworkView>().SyncTransformScale();
		gameObject.GetComponent<CNetworkView>().SyncTransformPosition();
		gameObject.GetComponent<CNetworkView>().SyncTransformRotation();
	}

	[AServerOnly]
	private void UpdateInput()
	{
		// Toggle modes
		if(Input.GetKeyDown(KeyCode.Alpha1))
			m_CurrentMode = EToolMode.Paint_Exterior;

		else if(Input.GetKeyDown(KeyCode.Alpha2))
			m_CurrentMode = EToolMode.Paint_Interior_Walls;

		else if(Input.GetKeyDown(KeyCode.Alpha3))
			m_CurrentMode = EToolMode.Modify_Tile_Variants;

		// Left Click
		if(Input.GetMouseButtonDown(0))
			HandleLeftClickDown();

		if(Input.GetMouseButton(0))
			HandleLeftClickHold();

		if(Input.GetMouseButtonUp(0))
			HandleLeftClickUp();

		// Right Click
		if(Input.GetMouseButtonDown(1))
			HandleRightClickDown();

		if(Input.GetMouseButton(1))
			HandleRightClick();

		if(Input.GetMouseButtonUp(1))
			HandleRightClickUp();

		// Middle Click
		if(Input.GetMouseButtonDown(2))
			HandleMiddleClickDown();

		if(Input.GetMouseButton(2))
			HandleMiddleClick();

		if(Input.GetMouseButtonUp(2))
			HandleMiddleClickUp();

		// Mouse Scroll
		float sw = Input.GetAxis("Mouse ScrollWheel");
		if(sw != 0.0f)
			HandleMiddleScroll(sw);
	}

	private void HandleLeftClickDown()
	{
		if(m_PlaneHit.collider == null)
			return;

		m_MouseDownHitPoint = m_CurrentMouseHitPoint;
		m_MouseDownGridPoint = m_CurrentMouseGridPoint;
	}
	
	private void HandleLeftClickHold()
	{
		if(m_PlaneHit.collider == null)
			return;

		if(m_CurrentMode == EToolMode.Paint_Exterior)
		{
			if(!IsCtrlKeyDown)
				CreateInternalTile(m_CurrentMouseGridPoint);
			else
				DestroyInternalTile(m_CurrentMouseGridPoint);
			return;
		}
		
		if(m_CurrentMode == EToolMode.Paint_Interior_Walls)
		{
			Vector3 cursorPos = (m_GridCursor.transform.localPosition - m_TilesOffset) / m_Grid.m_TileSize;
			
			CGridPoint tilePos1 = new CGridPoint(Mathf.FloorToInt(cursorPos.x), Mathf.FloorToInt(cursorPos.y), Mathf.FloorToInt(cursorPos.z));
			CGridPoint tilePos2 = new CGridPoint(Mathf.CeilToInt(cursorPos.x), Mathf.FloorToInt(cursorPos.y), Mathf.CeilToInt(cursorPos.z));

			CTileInterface tile1 = m_Grid.GetTileInterface(tilePos1);
			CTileInterface tile2 = m_Grid.GetTileInterface(tilePos2);
			
			if(tile1 == null || tile2 == null)
				return;

			if(IsShiftKeyDown)
				ModifyWallModification(!IsCtrlKeyDown, CTile_InteriorWall.EModification.Door, tile1, tile2);
			else if(IsAltKeyDown)
				ModifyWallModification(!IsCtrlKeyDown, CTile_InteriorWall.EModification.Window, tile1, tile2);
			else
				ModifyInteriorWall(!IsCtrlKeyDown, tile1, tile2);

			tile1.UpdateAllCurrentTileMetaData();
			tile2.UpdateAllCurrentTileMetaData();
			return;
		}

		if(m_CurrentMode == EToolMode.Paint_Interior_Floors)
		{
			Vector3 cursorPos = (m_GridCursor.transform.localPosition - m_TilesOffset) / m_Grid.m_TileSize;

			cursorPos.x = Mathf.Round(cursorPos.x * 2.0f) * 0.5f;
			cursorPos.y = Mathf.Round(cursorPos.y * 2.0f) * 0.5f;
			cursorPos.z = Mathf.Round(cursorPos.z * 2.0f) * 0.5f;

			CGridPoint tilePosUpper1 = new CGridPoint(Mathf.FloorToInt(cursorPos.x), Mathf.FloorToInt(cursorPos.y), Mathf.FloorToInt(cursorPos.z));
			CGridPoint tilePosUpper2 = new CGridPoint(Mathf.CeilToInt(cursorPos.x), Mathf.FloorToInt(cursorPos.y), Mathf.CeilToInt(cursorPos.z));
			CGridPoint tilePosLower1 = new CGridPoint(Mathf.FloorToInt(cursorPos.x), Mathf.FloorToInt(cursorPos.y) - 1, Mathf.FloorToInt(cursorPos.z));
			CGridPoint tilePosLower2 = new CGridPoint(Mathf.CeilToInt(cursorPos.x), Mathf.FloorToInt(cursorPos.y) - 1, Mathf.CeilToInt(cursorPos.z));

			CTileInterface tileUpper1 = m_Grid.GetTileInterface(tilePosUpper1);
			CTileInterface tileUpper2 = m_Grid.GetTileInterface(tilePosUpper2);
			CTileInterface tileLower1 = m_Grid.GetTileInterface(tilePosLower1);
			CTileInterface tileLower2 = m_Grid.GetTileInterface(tilePosLower2);

			if(tileUpper1 == null || tileUpper2 == null || tileLower1 == null || tileLower2 == null)
				return;
			
			CTile tileInteriorFloor1 = tileUpper1.GetTile(CTile.EType.Interior_Floor);
			CTile tileInteriorFloor2 = tileUpper2.GetTile(CTile.EType.Interior_Floor);
			CTile tileInteriorCeiling1 = tileLower1.GetTile(CTile.EType.Interior_Ceiling);
			CTile tileInteriorCeiling2 = tileLower2.GetTile(CTile.EType.Interior_Ceiling);

			if(tileInteriorFloor1 == null || tileInteriorFloor2 == null || tileInteriorCeiling1 == null || tileInteriorCeiling2 == null)
				return;

			ModifyInteriorFloorAndCeiling(!IsCtrlKeyDown, tileInteriorFloor1, tileInteriorFloor2, tileInteriorCeiling1, tileInteriorCeiling2);

			tileUpper1.UpdateAllCurrentTileMetaData();
			tileUpper2.UpdateAllCurrentTileMetaData();
			tileLower1.UpdateAllCurrentTileMetaData();
			tileLower2.UpdateAllCurrentTileMetaData();
			return;
		}
	}
	
	private void HandleLeftClickUp()
	{
		if (m_PlaneHit.collider == null)
			return;

		if(!IsShiftKeyDown)
			HandleLeftClickUpSingle();
		else
			HandleLeftClickUpMulti();
	}

	private void HandleLeftClickUpSingle()
	{
//		if(m_CurrentMode == EToolMode.Modify_Tile_Variants) 
//		{
//			if(!IsCtrlKeyDown)
//				m_SelectedTiles.Clear();
//
//			SelectTile(m_CurrentMouseGridPoint);
//
//			if(EventTileSelectionChange != null)
//				EventTileSelectionChange();
//
//			return;
//		}
	}

	private void HandleLeftClickUpMulti()
	{
		switch(m_CurrentMode) 
		{
//			case EToolMode.Modify_Tile_Variants: 
//			{
//				if (!IsCtrlKeyDown)
//					m_SelectedTiles.Clear();
//
//				SelectMultipleTiles();
//				
//				if(EventTileSelectionChange != null)
//					EventTileSelectionChange();
//
//				break;
//			}
			case EToolMode.Paint_Exterior: 
			{
				SelectionManipulateTiles(IsCtrlKeyDown);
				break;
			}
		}
	}

	private void HandleRightClickDown()
	{
		if(m_PlaneHit.collider == null)
			return;

		m_MouseDownHitPoint = m_CurrentMouseHitPoint;
		m_DragRotateStart = m_Grid.transform.rotation;
		
		m_CurrentPlaneInteraction = EPlaneInteraction.DragRotation;
	}

	private void HandleRightClick()
	{
		if(m_CurrentPlaneInteraction == EPlaneInteraction.DragRotation)
			DragRotateGrid();
	}

	private void HandleRightClickUp()
	{
		if(m_CurrentPlaneInteraction == EPlaneInteraction.DragRotation)
			m_CurrentPlaneInteraction = EPlaneInteraction.Nothing;
	}

	private void HandleMiddleClickDown()
	{
		if(m_PlaneHit.collider == null)
			return;

		m_MouseDownHitPoint = m_CurrentMouseHitPoint;
		m_DragMovementStart = m_TilesOffset;
		
		m_CurrentPlaneInteraction = EPlaneInteraction.DragMovement;
	}
	
	private void HandleMiddleClick()
	{
		if(m_CurrentPlaneInteraction == EPlaneInteraction.DragMovement)
			DragMoveTiles();
	}
	
	private void HandleMiddleClickUp()
	{
		if(m_CurrentPlaneInteraction == EPlaneInteraction.DragMovement)
			m_CurrentPlaneInteraction = EPlaneInteraction.Nothing;
	}

	private void HandleMiddleScroll(float _ScrollWheel)
	{
		if(m_PlaneHit.collider == null)
			return;

		if(IsShiftKeyDown)
		{
			int direction = (int)Mathf.Sign(_ScrollWheel);
			ChangeVerticalLayer(direction);
		}
		else
		{
			UpdateGridScale(Mathf.Clamp(m_GridScale + _ScrollWheel * 0.1f, m_GridScaleLimits.x, m_GridScaleLimits.y));
		}
	}

	[AServerOnly]
	private void UpdateGridScale(float _GridScale)
	{
		m_GridScale = _GridScale;

		// Update the grid root scale
		m_Grid.transform.localScale = Vector3.one * _GridScale;
		
		// Update the raycast plane to be the same
		m_GridPlane.transform.localScale = Vector3.one * 0.25f / _GridScale;

		// Sync the new scale
		gameObject.GetComponent<CNetworkView>().SyncTransformScale();
	}

	[AServerOnly]
	private void ChangeVerticalLayer(int _Direction)
	{
		m_CurrentVerticalLayer += _Direction;

		// Update the tiles offset
		m_TilesOffset -= Vector3.up * (float)_Direction * m_Grid.m_TileSize;
		m_Grid.TileContainer.transform.localPosition = m_TilesOffset;
	}

	[AServerOnly]
	private void UpdateCursor()
	{
		if(m_CurrentPlaneInteraction != EPlaneInteraction.Nothing)
			return;

		if(m_CurrentMode == EToolMode.Paint_Exterior ||
		   m_CurrentMode == EToolMode.Paint_Interior_Floors ||
		   m_CurrentMode == EToolMode.Paint_Interior_Walls ||
		   m_CurrentMode == EToolMode.Modify_Tile_Variants)
		{
			m_GridCursor.renderer.enabled = true;
		}
		else
		{
			m_GridCursor.renderer.enabled = false;
		}

		if(m_CurrentMode == EToolMode.Paint_Exterior)
		{
			Vector3 centerPos = m_Grid.GetLocalPosition(m_CurrentMouseGridPoint.ToVector) + m_TilesOffset;
			centerPos.y += m_Grid.m_TileSize * 0.5f;
			
			m_GridCursor.transform.localScale = Vector3.one * m_Grid.m_TileSize;
			m_GridCursor.transform.localPosition = centerPos;
			return;
		}

		if(m_CurrentMode == EToolMode.Paint_Interior_Walls ||
		   m_CurrentMode == EToolMode.Paint_Interior_Floors)
		{
			Vector3 tilePos = m_Grid.GetGridPosition(m_CurrentMouseHitPoint - (m_Grid.TileContainer.transform.rotation * m_TilesOffset * m_GridScale));
			Vector3 centerPos = m_CurrentMouseGridPoint.ToVector;
			centerPos.x += Mathf.Sign(tilePos.x - centerPos.x) * 0.5f;
			centerPos.z += Mathf.Sign(tilePos.z - centerPos.z) * 0.5f;

			Vector3[] directions = new Vector3[]{ 
				new Vector3(0.5f, 0.0f, 0.0f),
				new Vector3(-0.5f, 0.0f, 0.0f),
				new Vector3(0.0f, 0.0f, 0.5f), 
				new Vector3(0.0f, 0.0f, -0.5f) };

			Vector3 closest = Vector3.zero;
			float cloststDist = float.PositiveInfinity;
			foreach(Vector3 direction in directions)
			{
				float dist = Vector3.Distance(tilePos, (centerPos + direction));
				if(dist < cloststDist)
				{
					closest = direction;
					cloststDist = dist;
				}
			}

			centerPos = m_Grid.GetLocalPosition(centerPos + closest) + m_TilesOffset;
			centerPos.y += m_Grid.m_TileSize * 0.5f;

			m_GridCursor.transform.localScale = Vector3.one * m_Grid.m_TileSize;
			m_GridCursor.transform.localPosition = centerPos;
			return;
		}
	}

	[AServerOnly]
	private void DragRotateGrid()
	{
		// Get the screen mouse positions
		Vector3 point1 = m_MouseDownHitPoint;
		Vector3 point2 = m_CurrentMouseHitPoint;
		
		// Get the difference of the two
		Vector3 diff = (point1 - point2) * 100.0f;
		
		// Rotate plane using the axis of camera to rotate pitch and yaw
		Quaternion rotPitch = Quaternion.AngleAxis(diff.z, Camera.main.transform.right);
		Quaternion rotYaw = Quaternion.AngleAxis(diff.x, m_Grid.transform.parent.up);
		
		// Lerp to the final rotation
		m_Grid.transform.rotation = /*rotPitch **/ m_DragRotateStart * rotYaw;
	}

	[AServerOnly]
	private void DragMoveTiles()
	{
		// Get the hit positions on the plane
		Vector3 point1 = m_MouseDownHitPoint;
		Vector3 point2 = m_CurrentMouseHitPoint;

		// Get the difference of the two
		Vector3 diff = (point1 - point2);

		// Get the yaw rotation component of the grid
		Quaternion rotY = Quaternion.Euler(0.0f,  m_Grid.transform.eulerAngles.y, 0.0f);

		// Move the tiles along the x, z
		m_TilesOffset = m_DragMovementStart - (Quaternion.Inverse(rotY) * new Vector3(diff.x, 0.0f, diff.z)) / m_GridScale;
		m_Grid.TileContainer.transform.localPosition = m_TilesOffset;
	}

	[AServerOnly]
	private void SelectionManipulateTiles(bool _Remove)
	{
		// Get the diagonal corner points
		CGridPoint point1 = m_MouseDownGridPoint;
		CGridPoint point2 = m_CurrentMouseGridPoint;
		
		// Determine the rect properties
		int left = point1.x < point2.x ? point1.x : point2.x;
		int bottom = point1.y < point2.y ? point1.y : point2.y;
		int back = point1.z < point2.z ? point1.z : point2.z;
		int width = Mathf.Abs(point1.x - point2.x) + 1;
		int height = Mathf.Abs(point1.y - point2.y) + 1;
		int depth = Mathf.Abs(point1.z - point2.z) + 1;

		// Itterate through the positions
		for(int x = left; x < (left + width); ++x)
		{
			for(int y = bottom; y < (bottom + height); ++y)
			{
				for(int z = back; z < (back + depth); ++z)
				{
					if(_Remove) DestroyInternalTile(new CGridPoint(x, y, z));
					else CreateInternalTile(new CGridPoint(x, y, z));
				}
			}
		}	
	}

	[AServerOnly]
	private void SelectTile(CGridPoint _GridPoint)
	{
//		CTileInterface tile = m_Grid.GetTileInterface(_GridPoint);
//
//		if(tile != null)
//			m_SelectedTiles.Add(tile);
	}

	[AServerOnly]
	private void CreateInternalTile(CGridPoint _GridPoint)
	{
		// Place the interior tile
		CTileInterface tileInterface = m_Grid.PlaceTile(_GridPoint);

		// Set the tile types for this tile
		List<CTile.EType> tileTypes = new CTile.EType[]{ 
			CTile.EType.Interior_Floor, CTile.EType.Interior_Floor_Inverse_Corner,
			CTile.EType.Interior_Wall, CTile.EType.Interior_Wall_Inverse_Corner, 
			CTile.EType.Interior_Ceiling, CTile.EType.Interior_Ceiling_Inverse_Corner }.ToList();

		foreach(CTile.EType tileType in tileTypes)
			tileInterface.SetTileTypeState(tileType, true);

		// Get the surrounding tiles
		List<CTileInterface> surroundingTiles = GetAllSurroundingTiles(_GridPoint, true);

		// Detirmine the exterior tiles
		foreach(CTileInterface surroundingTile in surroundingTiles)
			DetirmineExternalTileTypes(surroundingTile);

		// Update the meta data
		tileInterface.UpdateAllCurrentTileMetaData();

		foreach(CTileInterface surroundingTile in surroundingTiles)
			surroundingTile.UpdateAllCurrentTileMetaData();
	}

	[AServerOnly]
	private void DestroyInternalTile(CGridPoint _GridPoint)
	{
		// Only internal tile can be erased
		CTileInterface tileInterface = m_Grid.GetTileInterface(_GridPoint);
		if(tileInterface == null || !tileInterface.GetTileTypeState(CTile.EType.Interior_Wall))
			return;

		// Replace the tile
		tileInterface = m_Grid.PlaceTile(tileInterface.m_GridPosition);
		DetirmineExternalTileTypes(tileInterface);

		// Get the surrounding tiles
		List<CTileInterface> surroundingTiles = GetAllSurroundingTiles(_GridPoint, false);

		// Detirmine the exterior tiles
		foreach(CTileInterface surroundingTile in surroundingTiles)
			DetirmineExternalTileTypes(surroundingTile);

		// Update the meta data
		if(m_Grid.GetTileInterface(tileInterface.m_GridPosition) != null)
			tileInterface.UpdateAllCurrentTileMetaData();

		foreach(CTileInterface surroundingTile in surroundingTiles)
			if(m_Grid.GetTileInterface(surroundingTile.m_GridPosition) != null)
				surroundingTile.UpdateAllCurrentTileMetaData();
	}

	private List<CTileInterface> GetAllSurroundingTiles(CGridPoint _GridPoint, bool _CreateNewTiles)
	{
		List<CTileInterface> surroundingTiles = new List<CTileInterface>(27);

		// Create tiles around in each direction as external walls
		foreach(CNeighbour neighbour in CTileInterface.s_PossibleNeighbours)
		{
			CGridPoint neighboutGridPoint = new CGridPoint(_GridPoint.ToVector + neighbour.m_GridPointOffset.ToVector);
			CTileInterface neighbourTileInterface = m_Grid.GetTileInterface(neighboutGridPoint);

			if(neighbourTileInterface == null && _CreateNewTiles)
				neighbourTileInterface = m_Grid.PlaceTile(neighboutGridPoint);

			if(neighbourTileInterface != null)
				surroundingTiles.Add(neighbourTileInterface);
		}

		// Get the upper tiles
		CGridPoint upperGridPoint = new CGridPoint(_GridPoint.ToVector + Vector3.up);
		CTileInterface upperTileInterface = m_Grid.GetTileInterface(upperGridPoint);

		if(upperTileInterface == null && _CreateNewTiles)
			upperTileInterface = m_Grid.PlaceTile(upperGridPoint);

		if(upperTileInterface != null)
			surroundingTiles.Add(upperTileInterface);
		
		// Create tiles around in each direction as external upper
		foreach(CNeighbour neighbour in CTileInterface.s_PossibleNeighbours)
		{
			CGridPoint neighboutGridPoint = new CGridPoint(upperGridPoint.ToVector + neighbour.m_GridPointOffset.ToVector);
			CTileInterface neighbourTileInterface = m_Grid.GetTileInterface(neighboutGridPoint);

			if(neighbourTileInterface == null && _CreateNewTiles)
				neighbourTileInterface = m_Grid.PlaceTile(neighboutGridPoint);

			if(neighbourTileInterface != null)
				surroundingTiles.Add(neighbourTileInterface);
		}
		
		// Check the lower tiles if they are the correct type
		CGridPoint lowerGridPoint = new CGridPoint(_GridPoint.ToVector - Vector3.up);
		CTileInterface lowerTileInterface = m_Grid.GetTileInterface(lowerGridPoint);

		if(lowerTileInterface == null && _CreateNewTiles)
			lowerTileInterface = m_Grid.PlaceTile(lowerGridPoint);

		if(lowerTileInterface != null)
			surroundingTiles.Add(lowerTileInterface);
		
		// Create tiles around in each direction as external walls
		foreach(CNeighbour neighbour in CTileInterface.s_PossibleNeighbours)
		{
			CGridPoint neighboutGridPoint = new CGridPoint(lowerGridPoint.ToVector + neighbour.m_GridPointOffset.ToVector);
			CTileInterface neighbourTileInterface = m_Grid.GetTileInterface(neighboutGridPoint);

			if(neighbourTileInterface == null && _CreateNewTiles)
				neighbourTileInterface = m_Grid.PlaceTile(neighboutGridPoint);
			
			if(neighbourTileInterface != null)
				surroundingTiles.Add(neighbourTileInterface);
		}

		return(surroundingTiles);
	}

	[AServerOnly]
	private void DetirmineExternalTileTypes(CTileInterface _TileInterface)
	{
		if(_TileInterface.GetTileTypeState(CTile.EType.Interior_Wall))
			return;

		List<CTile.EType> tileTypes = new List<CTile.EType>();



		tileTypes.Add(CTile.EType.Exterior_Wall);
		tileTypes.Add(CTile.EType.Exterior_Wall_Inverse_Corner);
		tileTypes.Add(CTile.EType.Exterior_Lower);
		tileTypes.Add(CTile.EType.Exterior_Lower_Inverse_Corner);
		tileTypes.Add(CTile.EType.Exterior_Upper);
		tileTypes.Add(CTile.EType.Exterior_Upper_Inverse_Corner);


//		bool internalWallNeighbourExists = _TileInterface.m_NeighbourHood.Exists(
//			n => n.m_TileInterface.GetTileTypeState(CTile.EType.Interior_Wall));
//
//		bool upperExists = m_Grid.GetTileInterface(new CGridPoint(_TileInterface.m_GridPosition.ToVector + Vector3.up)) != null;
//	
//		bool lowerExists = m_Grid.GetTileInterface(new CGridPoint(_TileInterface.m_GridPosition.ToVector - Vector3.up)) != null;
//
//		if(internalWallNeighbourExists || !upperExists || !lowerExists)
//		{
//			tileTypes.Add(CTile.EType.Exterior_Wall);
//			tileTypes.Add(CTile.EType.Exterior_Wall_Inverse_Corner);
//			tileTypes.Add(CTile.EType.Exterior_Lower);
//			tileTypes.Add(CTile.EType.Exterior_Lower_Inverse_Corner);
//			tileTypes.Add(CTile.EType.Exterior_Upper);
//			tileTypes.Add(CTile.EType.Exterior_Upper_Inverse_Corner);
//		}

//		CGridPoint lowerGridPoint = new CGridPoint(_TileInterface.m_GridPosition.ToVector - Vector3.up);
//		CTileInterface lowerTileInterface = m_Grid.GetTileInterface(lowerGridPoint);
//		
//		CGridPoint upperGridPoint = new CGridPoint(_TileInterface.m_GridPosition.ToVector + Vector3.up);
//		CTileInterface upperTileInterface = m_Grid.GetTileInterface(upperGridPoint);
//
//		bool upperContainsWall = upperTileInterface != null && 
//			(upperTileInterface.GetTileTypeState(CTile.EType.Exterior_Wall) || upperTileInterface.GetTileTypeState(CTile.EType.Interior_Wall));
//
//		bool upperContainsWallCap = upperTileInterface != null && 
//			(upperTileInterface.GetTileTypeState(CTile.EType.Exterior_Wall_Inverse_Corner));
//
//		bool lowerContainsWall = lowerTileInterface != null &&
//			(lowerTileInterface.GetTileTypeState(CTile.EType.Exterior_Wall) || lowerTileInterface.GetTileTypeState(CTile.EType.Interior_Wall));
//
//		bool lowerContainsWallCap = lowerTileInterface != null &&
//			(lowerTileInterface.GetTileTypeState(CTile.EType.Exterior_Wall_Inverse_Corner));
//
//		if(upperContainsWall)
//			tileTypes.Add(CTile.EType.Exterior_Lower);
//
//		if(upperContainsWallCap)
//			tileTypes.Add(CTile.EType.Exterior_Lower_Inverse_Corner);
//
//		if(lowerContainsWall)
//			tileTypes.Add(CTile.EType.Exterior_Upper);
//
//		if(lowerContainsWallCap)
//			tileTypes.Add(CTile.EType.Exterior_Upper_Inverse_Corner);

		if(tileTypes.Count != 0)
		{
			for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
				_TileInterface.SetTileTypeState((CTile.EType)i, tileTypes.Contains((CTile.EType)i));
		}
		else
		{
			m_Grid.RemoveTile(_TileInterface.m_GridPosition);
		}
	}

	[AServerOnly]
	private void ModifyWallModification(bool _State, CTile_InteriorWall.EModification _Modifier, CTileInterface _TileInterface1, CTileInterface _TileInterface2)
	{
		CTile wall1 = _TileInterface1.GetTile(CTile.EType.Interior_Wall);
		if(wall1 == null)
			wall1 = _TileInterface1.GetTile(CTile.EType.Exterior_Wall);

		CTile wall2 = _TileInterface2.GetTile(CTile.EType.Interior_Wall);
		if(wall2 == null)
			wall2 = _TileInterface2.GetTile(CTile.EType.Exterior_Wall);

		if(wall1 == null || wall2 == null)
			return;

		CNeighbour neighbour1 = _TileInterface1.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _TileInterface2);
		CNeighbour neighbour2 = _TileInterface2.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _TileInterface1);
		
		if(neighbour1 == null || neighbour2 == null)
			return;
		
		EDirection dir1 = neighbour1.m_Direction;
		EDirection dir2 = neighbour2.m_Direction;

		if(wall1.m_TileType == CTile.EType.Interior_Wall && wall2.m_TileType == CTile.EType.Interior_Wall)
		{
			if(!wall1.GetNeighbourExemptionState(dir1) || !wall2.GetNeighbourExemptionState(dir2))
			{
				wall1.SetTileModification((int)_Modifier, dir1, false);
				wall2.SetTileModification((int)_Modifier, dir2, false);
				return;
			}
		}

		wall1.SetTileModification((int)_Modifier, dir1, _State);
		wall2.SetTileModification((int)_Modifier, dir2, _State);
	}

	[AServerOnly]
	private void ModifyInteriorWall(bool _State, CTileInterface _TileInterface1, CTileInterface _TileInterface2)
	{
		if(!_State)
		{
			ModifyWallModification(false, CTile_InteriorWall.EModification.Door, _TileInterface1, _TileInterface2);
			ModifyWallModification(false, CTile_InteriorWall.EModification.Window, _TileInterface1, _TileInterface2);
		}

		if(!_TileInterface1.GetTileTypeState(CTile.EType.Interior_Wall) || !_TileInterface2.GetTileTypeState(CTile.EType.Interior_Wall))
			return;

		CNeighbour neighbour1 = _TileInterface1.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _TileInterface2);
		CNeighbour neighbour2 = _TileInterface2.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _TileInterface1);
		
		if(neighbour1 == null || neighbour2 == null)
			return;
		
		EDirection dir1 = neighbour1.m_Direction;
		EDirection dir2 = neighbour2.m_Direction;

		_TileInterface1.GetTile(CTile.EType.Interior_Wall).SetNeighbourExemptionState(dir1, _State);
		_TileInterface2.GetTile(CTile.EType.Interior_Wall).SetNeighbourExemptionState(dir2, _State);

		_TileInterface1.GetTile(CTile.EType.Interior_Wall).UpdateCurrentTileMetaData();
		_TileInterface2.GetTile(CTile.EType.Interior_Wall).UpdateCurrentTileMetaData();
	}

	[AServerOnly]
	private void ModifyInteriorFloorAndCeiling(bool _State, CTile _FloorTile1, CTile _FloorTile2, CTile _CeilingTile1, CTile _CeilingTile2)
	{
		CNeighbour neighbourFloor1 = _FloorTile1.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _FloorTile2.m_TileInterface);
		CNeighbour neighbourFloor2 = _FloorTile2.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _FloorTile1.m_TileInterface);
		
		if(neighbourFloor1 == null || neighbourFloor2 == null)
			return;
		
		EDirection dir1 = neighbourFloor1.m_Direction;
		EDirection dir2 = neighbourFloor2.m_Direction;
		
		_FloorTile1.SetNeighbourExemptionState(dir1, _State);
		_FloorTile2.SetNeighbourExemptionState(dir2, _State);

		CNeighbour neighbourCeiling1 = _CeilingTile1.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _CeilingTile2.m_TileInterface);
		CNeighbour neighbourCeiling2 = _CeilingTile2.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _CeilingTile1.m_TileInterface);
		
		if(neighbourCeiling1 == null || neighbourCeiling2 == null)
			return;
		
		dir1 = neighbourCeiling1.m_Direction;
		dir2 = neighbourCeiling2.m_Direction;
		
		_CeilingTile1.SetNeighbourExemptionState(dir1, _State);
		_CeilingTile2.SetNeighbourExemptionState(dir2, _State);
	}

	[AServerOnly]
	private void SelectMultipleTiles()
	{
		// Get the diagonal corner points
		CGridPoint point1 = m_MouseDownGridPoint;
		CGridPoint point2 = m_CurrentMouseGridPoint;
		
		// Determine the rect properties
		int left = point1.x < point2.x ? point1.x : point2.x;
		int bottom = point1.z < point2.z ? point1.z : point2.z;
		int width = Mathf.Abs(point1.x - point2.x) + 1;
		int height = Mathf.Abs(point1.z - point2.z) + 1;
		
		// Itterate through the positions
		for(int x = left; x < (left + width); ++x)
		{
			for(int z = bottom; z < (bottom + height); ++z)
			{
				SelectTile(new CGridPoint(x, point1.y, z));
			}
		}
	}

	private void OnTileInterfaceCreated(CTileInterface _Tile)
	{
		// Register events
		_Tile.EventTileGeometryChanged += OnTileGeometryChange;
	}

	private void OnTileGeometryChange(CTileInterface _Tile)
	{
		// Set the material for all children and self
		Renderer tileRenderer = _Tile.gameObject.GetComponent<Renderer>();
		if(tileRenderer != null)
		{
			Material[] sharedMaterials = tileRenderer.sharedMaterials;
			for(int i = 0; i < tileRenderer.sharedMaterials.Length; ++i)
			{
				tileRenderer.sharedMaterials[i] = m_TileMaterial;
			}
			tileRenderer.sharedMaterials = sharedMaterials;
		}

		foreach(Renderer childRenderer in _Tile.gameObject.GetComponentsInChildren<Renderer>())
		{
			Material[] sharedMaterials = childRenderer.sharedMaterials;
			for(int i = 0; i < childRenderer.sharedMaterials.Length; ++i)
			{
				sharedMaterials[i] = m_TileMaterial;
			}
			childRenderer.sharedMaterials = sharedMaterials;
			childRenderer.castShadows = false;
			childRenderer.receiveShadows = false;
		}
	}

//	[AServerOnly]
//	private void OnModuleCreate(CModuleInterface _Module, CFacilityInterface _FacilityParent)
//	{
//		GameObject newModulePort = (GameObject)GameObject.Instantiate(m_SmallModulePortPrefab);
//		Vector3 originalScale = newModulePort.transform.localScale;
//		newModulePort.transform.parent = m_Grid.TileContainer.transform;
//		newModulePort.transform.localScale = originalScale;
//		newModulePort.transform.localRotation = Quaternion.identity;
//		newModulePort.transform.localPosition =  _Module.transform.position - _FacilityParent.transform.position;
//	}

	[AServerOnly]
	public void ExportTilesToShip()
	{
		CShipFacilities shipFacility = CGameShips.Ship.GetComponent<CShipFacilities>();
		shipFacility.ImportNewGridTiles(m_Grid.TileInterfaces);
	}
}


