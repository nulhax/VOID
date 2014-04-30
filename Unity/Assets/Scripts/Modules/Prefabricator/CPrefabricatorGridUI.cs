
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
		ModifyTileVariants,
		PlaceModulePort,
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

	public CTile.EType m_CurrentlySelectedType = CTile.EType.INVALID;
	public List<CTileInterface> m_SelectedTiles = null;

	public Material m_TileMaterial = null;

	private List<RaycastHit> m_RaycastHits;
	private RaycastHit m_PlaneHit;

	private Quaternion m_DragRotateStart = Quaternion.identity;
	private Vector3 m_DragMovementStart = Vector3.zero;

	private bool m_FacilityTilesDirty = false;
	private List<List<CTileInterface>> m_FacilityTiles = new List<List<CTileInterface>>();


	// Member Properties
	public bool IsShiftKeyDown
	{
		get { return(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)); }
	}
	
	public bool IsCtrlKeyDown
	{
		get { return(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)); }
	}
	
	public bool AltKeyDown
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

		// Set a default rotation
		m_Grid.transform.localRotation = Quaternion.Euler(20.0f, 0.0f, 0.0f);
		
		// Update scale and clamp
		m_GridScale = Mathf.Clamp(m_GridScale, m_GridScaleLimits.x, m_GridScaleLimits.y);
		UpdateGridScale(m_GridScale);

		// Default enums
		m_CurrentMode = EToolMode.Nothing;
		m_CurrentPlaneInteraction = EPlaneInteraction.Nothing;
		m_CurrentlySelectedType = CTile.EType.InteriorFloor;

		// Instance new material
		m_TileMaterial = new Material(m_TileMaterial);

		// Import the tiles from the current ship
		ImportPreplacedTiles();

		// Register for when players join
		CGamePlayers.Instance.EventPlayerJoin += OnPlayerJoin;
	}

	private void ImportPreplacedTiles()
	{
		foreach(CTileInterface tile in m_Grid.ImportTileInformation(CGameShips.Ship.GetComponent<CShipFacilities>().m_ShipGrid.Tiles))
		{
			// Register events
			tile.EventTileGeometryChanged += OnTileGeometryChange;
		}
		m_FacilityTilesDirty = true;
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
		m_RaycastHits.RemoveAll(hit => CUtility.FindInParents<CGrid>(hit.collider.transform) == null);

		// Get the plane hit
		m_PlaneHit = m_RaycastHits.Find(hit => hit.collider.gameObject == m_GridPlane);

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
		Vector3 up = m_Grid.transform.up;
		Vector3 pos = m_Grid.transform.position;
		m_TileMaterial.SetVector("_PlaneNormal", new Vector4(up.x, up.y, up.z));
		m_TileMaterial.SetVector("_PlanePoint", new Vector4(pos.x, pos.y + 2.0f * m_GridScale, pos.z));
		m_TileMaterial.SetFloat("_GlowDist", 0.3f * m_GridScale);
	}

	private void LateUpdate()
	{
		// Update the facilities tiles
		if(m_FacilityTilesDirty)
		{
			UpdateFacilityTiles();
			m_FacilityTilesDirty = false;
		}
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
			m_CurrentMode = EToolMode.ModifyTileVariants;

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

			CTileInterface tile1 = m_Grid.GetTile(tilePos1);
			CTileInterface tile2 = m_Grid.GetTile(tilePos2);
			
			if(tile1 == null || tile2 == null)
				return;

			CTile tileInteriorWall1 = tile1.GetTile(CTile.EType.InteriorWall);
			CTile tileInteriorWall2 = tile2.GetTile(CTile.EType.InteriorWall);
			
			if(tileInteriorWall1 == null || tileInteriorWall2 == null)
				return;

			ModifyInteriorWall(!IsCtrlKeyDown, tileInteriorWall1, tileInteriorWall2);

			tile1.UpdateAllCurrentTileMetaData();
			tile2.UpdateAllCurrentTileMetaData();
		}

		if(m_CurrentMode == EToolMode.Paint_Interior_Floors)
		{
			CTileInterface tile = m_Grid.GetTile(m_CurrentMouseGridPoint);
			if(tile != null)
			{
				tile.SetTileTypeState(CTile.EType.InteriorFloor, !IsCtrlKeyDown);
			}
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
		if(m_CurrentMode == EToolMode.ModifyTileVariants) 
		{
			if(!IsCtrlKeyDown)
				m_SelectedTiles.Clear();

			SelectTile(m_CurrentMouseGridPoint);

			if(EventTileSelectionChange != null)
				EventTileSelectionChange();

			return;
		}

		if(m_CurrentMode == EToolMode.PlaceModulePort)
		{
			// Placeholder as fuck
//			Vector3 modulePosition = m_ModulePortCursors[CModuleInterface.ESize.Small].transform.localPosition;
//			CModuleInterface.EType moduleType = CUtility.FindInParents<CPrefabricatorBehaviour>(gameObject).PrefabricatorUI.SelectedModuleType;
//			CFacilityInterface facility = CGameShips.Ship.GetComponent<CShipFacilities>().FindFacilities(
//				CFacilityInterface.EType.Bridge).FirstOrDefault().GetComponent<CFacilityInterface>();
//			CModuleInterface.CreateNewModule(moduleType, facility, modulePosition);
//			return;
		}
	}

	private void HandleLeftClickUpMulti()
	{
		switch(m_CurrentMode) 
		{
			case EToolMode.ModifyTileVariants: 
			{
				if (!IsCtrlKeyDown)
					m_SelectedTiles.Clear();

				SelectMultipleTiles();
				
				if(EventTileSelectionChange != null)
					EventTileSelectionChange();

				break;
			}
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
		   m_CurrentMode == EToolMode.ModifyTileVariants)
		{
			m_GridCursor.renderer.enabled = true;
		}
		else
		{
			m_GridCursor.renderer.enabled = false;
		}

//		if(m_CurrentMode == EToolMode.PlaceModulePort)
//		{
//			m_ModulePortCursors[CModuleInterface.ESize.Small].renderer.enabled = true;
//		}
//		else
//		{
//			m_ModulePortCursors[CModuleInterface.ESize.Small].renderer.enabled = false;
//		}

//		if(m_CurrentMode == EToolMode.PlaceModulePort)
//		{
//			Vector3 centerPos = m_Grid.GetGridPosition(m_CurrentMouseHitPoint - (m_Grid.TileContainer.transform.rotation * m_TilesOffset));
//			centerPos.x = Mathf.Round(centerPos.x * 2.0f) / 2.0f; 
//			centerPos.z = Mathf.Round(centerPos.z * 2.0f) / 2.0f; 
//
//			centerPos = m_Grid.GetLocalPosition(centerPos) + m_TilesOffset;
//			centerPos.y += 0.25f;
//			m_ModulePortCursors[CModuleInterface.ESize.Small].transform.localScale = new Vector3(3.0f, 0.5f, 3.0f);
//			m_ModulePortCursors[CModuleInterface.ESize.Small].transform.localPosition = centerPos;
//			return;
//		}

		if(m_CurrentMode == EToolMode.Paint_Exterior)
		{
			if(IsShiftKeyDown)
			{
				Vector3 point1 = m_Grid.GetLocalPosition(m_CurrentMouseGridPoint.ToVector * m_GridScale) + m_TilesOffset;
				Vector3 point2 = m_Grid.GetLocalPosition(m_MouseDownGridPoint.ToVector * m_GridScale) + m_TilesOffset;

				Vector3 centerPos = (point1 + point2) * 0.5f;
				float width = Mathf.Abs(m_CurrentMouseGridPoint.x - m_MouseDownGridPoint.x) + 1.0f;
				float height = Mathf.Abs(m_CurrentMouseGridPoint.y - m_MouseDownGridPoint.y) + 1.0f;
				float depth = Mathf.Abs(m_CurrentMouseGridPoint.z - m_MouseDownGridPoint.z) + 1.0f;
				centerPos.y += m_Grid.m_TileSize * 0.5f;

				m_GridCursor.transform.localScale = new Vector3(width, height, depth) * m_Grid.m_TileSize;
				m_GridCursor.transform.localPosition = centerPos;
				return;
			}
			else
			{
				Vector3 centerPos = m_Grid.GetLocalPosition(m_CurrentMouseGridPoint.ToVector) + m_TilesOffset;
				centerPos.y += m_Grid.m_TileSize * 0.5f;
				
				m_GridCursor.transform.localScale = Vector3.one * m_Grid.m_TileSize;
				m_GridCursor.transform.localPosition = centerPos;
				return;
			}
		}

		if(m_CurrentMode == EToolMode.Paint_Interior_Walls)
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
		CTileInterface tile = m_Grid.GetTile(_GridPoint);

		if(tile != null)
			m_SelectedTiles.Add(tile);
	}

	[AServerOnly]
	private void CreateInternalTile(CGridPoint _GridPoint)
	{
		// Place the interior tile
		CTileInterface tileInterface = m_Grid.PlaceTile(_GridPoint);

		// Set the tile types
		List<CTile.EType> tileTypes = new CTile.EType[]{ CTile.EType.InteriorFloor, CTile.EType.InteriorWall, CTile.EType.InteriorWallCap, CTile.EType.InteriorCeiling }.ToList();
		for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
			tileInterface.SetTileTypeState((CTile.EType)i, tileTypes.Contains((CTile.EType)i));

		// Create tiles around in each direction as external walls
		foreach(CNeighbour neighbour in CTileInterface.s_PossibleNeighbours)
		{
			CGridPoint neighboutGridPoint = new CGridPoint(_GridPoint.ToVector + neighbour.m_GridPointOffset.ToVector);
			if(m_Grid.GetTile(neighboutGridPoint) == null)
			{
				CTileInterface neighbourTileInterface = m_Grid.PlaceTile(neighboutGridPoint);

				// Set the tile types
				tileTypes = new CTile.EType[]{ CTile.EType.ExteriorWall, CTile.EType.ExteriorWallCap }.ToList();
				for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
					neighbourTileInterface.SetTileTypeState((CTile.EType)i, tileTypes.Contains((CTile.EType)i));
			}
		}

		// Update the tiles type mask
		tileInterface.UpdateTileTypeMask();

		// Check for interior wall cap exemptions
		UpdateTileInternalWallCapExemptions(tileInterface);

		// Update tile meta data
		tileInterface.UpdateAllCurrentTileMetaData();
	}

	[AServerOnly]
	private void DestroyInternalTile(CGridPoint _GridPoint)
	{
		// Only internal tile can be erased
		CTileInterface tileInterface = m_Grid.GetTile(_GridPoint);
		if(tileInterface == null || !tileInterface.GetTileTypeState(CTile.EType.InteriorWall))
			return;

		// Get all the tiles within this tiles neighbourhood
		List<CGridPoint> tileToRemove = new List<CGridPoint>();
		foreach(CNeighbour neighbour in tileInterface.m_NeighbourHood)
		{
			// Remove any internal wall exemption states
			if(neighbour.m_TileInterface.GetTileTypeState(CTile.EType.InteriorWall))
				ModifyInteriorWall(false, tileInterface.GetTile(CTile.EType.InteriorWall), neighbour.m_TileInterface.GetTile(CTile.EType.InteriorWall));

			// Skip non exterior walls
			if(!neighbour.m_TileInterface.GetTileTypeState(CTile.EType.ExteriorWall))
				continue;

			// Get all neighbours of this tile which are internal
			List<CNeighbour> internalNeighbours = neighbour.m_TileInterface.m_NeighbourHood.FindAll(n => n.m_TileInterface.GetTileTypeState(CTile.EType.InteriorWall));

			// If this was the only neighbour, release it
			if(internalNeighbours.Count == 1)
				tileToRemove.Add(neighbour.m_TileInterface.m_GridPosition);
		}

		// Get all neighbours of this tile which are external
		List<CNeighbour> externalNeighbours = tileInterface.m_NeighbourHood.FindAll(n => n.m_TileInterface.GetTileTypeState(CTile.EType.ExteriorWall));

		// Remove the tiles that arent needed
		foreach(CGridPoint tilePos in tileToRemove)
			m_Grid.RemoveTile(tilePos);

		// If this tile only has external tiles surrounding it, release it
		if(externalNeighbours.Count != 8)
		{
			// Replace this tile with an external tile
			tileInterface = m_Grid.PlaceTile(_GridPoint);
			
			// Set the tile types
			List<CTile.EType> tileTypes = new CTile.EType[]{ CTile.EType.ExteriorWall, CTile.EType.ExteriorWallCap }.ToList();
			for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
				tileInterface.SetTileTypeState((CTile.EType)i, tileTypes.Contains((CTile.EType)i));

			// Update the tiles meta data
			tileInterface.UpdateAllCurrentTileMetaData();
		}
		else
		{
			m_Grid.RemoveTile(tileInterface.m_GridPosition);
		}
	}

	[AServerOnly]
	private void ModifyInteriorWall(bool _State, CTile _TileInteriorWall1, CTile _TileInteriorWall2)
	{
		CNeighbour neighbour1 = _TileInteriorWall1.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _TileInteriorWall2.m_TileInterface);
		CNeighbour neighbour2 = _TileInteriorWall2.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_TileInterface == _TileInteriorWall1.m_TileInterface);
		
		if(neighbour1 == null || neighbour2 == null)
			return;
		
		EDirection dir1 = neighbour1.m_Direction;
		EDirection dir2 = neighbour2.m_Direction;

		if(_TileInteriorWall1.GetNeighbourExemptionState(dir1) == _State &&
		   _TileInteriorWall2.GetNeighbourExemptionState(dir2) == _State)
			return;
		
		_TileInteriorWall1.SetNeighbourExemptionState(dir1, _State);
		_TileInteriorWall2.SetNeighbourExemptionState(dir2, _State);
		
		EDirection tile1LeftDir = CNeighbour.GetLeftDirectionNeighbour(dir1);
		EDirection tile1RightDir = CNeighbour.GetRightDirectionNeighbour(dir1);
		EDirection tile2LeftDir = CNeighbour.GetLeftDirectionNeighbour(dir2);
		EDirection tile2RightDir = CNeighbour.GetRightDirectionNeighbour(dir2);
		
		CNeighbour tile1NeighbourLeft = _TileInteriorWall1.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_Direction == tile1LeftDir);
		CNeighbour tile1NeighbourRight = _TileInteriorWall1.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_Direction == tile1RightDir);
		CNeighbour tile2NeighbourLeft = _TileInteriorWall2.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_Direction == tile2LeftDir);
		CNeighbour tile2NeighbourRight = _TileInteriorWall2.m_TileInterface.m_NeighbourHood.Find(neighbour => neighbour.m_Direction == tile2RightDir);

		if(tile1NeighbourLeft != null)
			UpdateTileInternalWallCapExemptions(tile1NeighbourLeft.m_TileInterface);

		if(tile1NeighbourRight != null)
			UpdateTileInternalWallCapExemptions(tile1NeighbourRight.m_TileInterface);

		if(tile2NeighbourLeft != null)
			UpdateTileInternalWallCapExemptions(tile2NeighbourLeft.m_TileInterface);

		if(tile2NeighbourRight != null)
			UpdateTileInternalWallCapExemptions(tile2NeighbourRight.m_TileInterface);
	}

	[AServerOnly]
	private void UpdateTileInternalWallCapExemptions(CTileInterface _TileInterface)
	{
		EDirection[] diagonalDirections = new EDirection[]{ EDirection.NorthEast, EDirection.NorthWest, EDirection.SouthEast, EDirection.SouthWest };
		CTile interiorWallCap = _TileInterface.GetTile(CTile.EType.InteriorWallCap);

		if(interiorWallCap == null)
			return;

		foreach(EDirection diagonalDir in diagonalDirections)
		{
			CNeighbour neighbour = _TileInterface.m_NeighbourHood.Find(n => n.m_Direction == diagonalDir);

			if(neighbour == null)
				continue;

			CTile neighbourInteriorWall = neighbour.m_TileInterface.GetTile(CTile.EType.InteriorWall);

			if(neighbourInteriorWall == null)
				continue;

			EDirection left = CNeighbour.GetLeftDirectionNeighbour(CNeighbour.GetOppositeDirection(diagonalDir));
			EDirection right = CNeighbour.GetRightDirectionNeighbour(CNeighbour.GetOppositeDirection(diagonalDir));

			bool state = neighbourInteriorWall.GetNeighbourExemptionState(left) || 
				neighbourInteriorWall.GetNeighbourExemptionState(right);

			interiorWallCap.SetNeighbourExemptionState(diagonalDir, state);
		}
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

		m_FacilityTilesDirty = true;
	}

	[AServerOnly]
	private void UpdateFacilityTiles()
	{
		// Clear the list of facility tiles
		m_FacilityTiles.Clear();

		// Find each of the tiles facility parent
		List<CTileInterface> interiorTiles = m_Grid.Tiles.FindAll(tile => tile.GetTileTypeState(CTile.EType.InteriorWall));
		foreach(CTileInterface tile in interiorTiles)
		{
			// If there are facilities then we need to check if this tile belongs in one of them
			List<List<CTileInterface>> tileFacilities = new List<List<CTileInterface>>();
			foreach(CNeighbour neighbour in tile.m_NeighbourHood)
			{
				// Get the center of the tile and direction to the neighbout
				Vector3 origin = tile.transform.position + m_Grid.transform.up * 0.5f * m_Grid.m_TileSize * m_GridScale;
				Vector3 dir = (neighbour.m_TileInterface.transform.position - tile.transform.position).normalized;

				// Raycast to check if the path is occluded
				Ray ray = new Ray(origin, dir);
				if(!Physics.Raycast(ray, m_Grid.m_TileSize * m_GridScale))
				{
					// If there is no occlusion, find the list in which this neighbour belongs to
					List<CTileInterface> facilityTileList = m_FacilityTiles.Find(list => list.Contains(neighbour.m_TileInterface));
					if(facilityTileList != null)
					{
						// Only add to the first facility found
						if(tileFacilities.Count == 0)
							facilityTileList.Add(tile);

						// Save the facilities this tile belongs to
						if(!tileFacilities.Contains(facilityTileList))
							tileFacilities.Add(facilityTileList);
					}

					Debug.DrawLine(origin, origin + (dir * m_Grid.m_TileSize * m_GridScale * 0.45f), Color.green, 1.0f);
				}
				else
					Debug.DrawLine(origin, origin + (dir * m_Grid.m_TileSize * m_GridScale * 0.45f), Color.red, 1.0f);
			}

			// If there was no facilities added add this to a new list
			if(tileFacilities.Count == 0)
			{
				m_FacilityTiles.Add(new CTileInterface[]{ tile }.ToList());
				continue;
			}

			// Check if this tile belongs to more than one list
			if(tileFacilities.Count > 1)
			{
				List<CTileInterface> newList = new List<CTileInterface>();

				// Remove these lists from the main list and add to new list
				foreach(List<CTileInterface> list in tileFacilities)
				{
					newList.AddRange(list);
					m_FacilityTiles.Remove(list);
				}

				// Add combined list to the main list
				m_FacilityTiles.Add(newList);
			}
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
		CGameShips.Ship.GetComponent<CShipFacilities>().ImportNewGridTiles(m_Grid.Tiles, m_FacilityTiles);
	}
}


