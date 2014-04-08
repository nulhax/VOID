
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
public class CGridUI : MonoBehaviour 
{
	// Member Types
	public enum EToolMode
	{
		INVALID,

		Nothing,
		Paint_Exterior,
		Paint_Interior_Walls,
		Paint_Interior_Floors,
		Select_Tiles,
	}

	public enum ETileInteraction
	{
		INVALID,

		Nothing,
		SingleSelection,
		MultipleSelection,
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

	private GameObject m_GridPlane = null;
	private GameObject m_GridSphere = null;
	private GameObject m_GridCursor = null;

	public float m_GridScale = 0.1f;
	public Vector2 m_GridScaleLimits = new Vector2(0.05f, 0.2f);
	public Vector3 m_TilesOffset = Vector3.zero;

	public EToolMode m_CurrentMode = EToolMode.INVALID;
	public ETileInteraction m_CurrentTileInteraction = ETileInteraction.INVALID;
	public EPlaneInteraction m_CurrentPlaneInteraction = EPlaneInteraction.INVALID;
	public int m_CurrentVerticalLayer = 0;
	
	public Vector3 m_CurrentMouseHitPoint = Vector3.zero;
	public TGridPoint m_CurrentMouseGridPoint;
	
	public Vector3 m_MouseDownHitPoint = Vector3.zero;
	public TGridPoint m_MouseDownGridPoint;

	public CTile.ETileType m_CurrentlySelectedType = CTile.ETileType.INVALID;
	public List<CTile> m_SelectedTiles = null;

	public Material m_TileMaterial = null;

	private List<RaycastHit> m_RaycastHits;
	private RaycastHit m_PlaneHit;

	private Quaternion m_DragRotateStart = Quaternion.identity;
	private Vector3 m_DragMovementStart = Vector3.zero;

	private static CTile.ETileType[] s_TT_F = new CTile.ETileType[] { CTile.ETileType.Floor };
	private static CTile.ETileType[] s_TT_FeW = new CTile.ETileType[] { CTile.ETileType.Floor, CTile.ETileType.Wall_Ext };
	private static CTile.ETileType[] s_TT_FeWC = new CTile.ETileType[] { CTile.ETileType.Floor, CTile.ETileType.Wall_Ext, CTile.ETileType.Ceiling };

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

		// Register tile creation/removal
		m_Grid.EventTileAdded += OnTileCreated;
		m_Grid.EventTileRemoved += OnTileRemoved;
	}
	
	private void Start() 
	{
		// Create the grid objects
		CreateGridUIObjects();

		// Default enums
		m_CurrentMode = EToolMode.Nothing;
		m_CurrentPlaneInteraction = EPlaneInteraction.Nothing;
		m_CurrentlySelectedType = CTile.ETileType.Floor;

		// Instance new material
		m_TileMaterial = new Material(m_TileMaterial);

		// Get the ship grid to register for when facilities are created
		CGameShips.Ship.GetComponent<CShipFacilities>().EventOnFaciltiyCreate += OnFacilityCreate;

		// Update the grid to be fully up to date
		foreach(GameObject facility in CGameShips.Ship.GetComponent<CShipFacilities>().Facilities)
		{
			CFacilityInterface fi = facility.GetComponent<CFacilityInterface>();
			OnFacilityCreate(fi);
		}
	}
	
	private void CreateGridUIObjects()
	{
		m_Grid.transform.localRotation = Quaternion.Euler(20.0f, 0.0f, 0.0f);

		m_GridPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		m_GridPlane.name = "Raycast Plane";
		m_GridPlane.renderer.material.shader = Shader.Find("Transparent/Diffuse");
		m_GridPlane.renderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
		m_GridPlane.collider.isTrigger = true;
		m_GridPlane.transform.parent = m_Grid.transform;
		m_GridPlane.transform.localPosition = Vector3.zero;
		m_GridPlane.transform.localRotation = Quaternion.identity;
		
		m_GridCursor = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_GridCursor.name = "Cursor";
		m_GridCursor.renderer.material.shader = Shader.Find("Transparent/Diffuse");
		m_GridCursor.renderer.material.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);
		Destroy(m_GridCursor.collider);
		m_GridCursor.transform.parent = m_Grid.transform;
		m_GridCursor.transform.localScale = Vector3.one * m_Grid.m_TileSize;
		m_GridCursor.transform.localPosition = Vector3.zero;
		m_GridCursor.transform.localRotation = Quaternion.identity;

		// Update scale and clamp
		m_GridScale = Mathf.Clamp(m_GridScale, m_GridScaleLimits.x, m_GridScaleLimits.y);
		UpdateGridScale(m_GridScale);
	}
	
	private void Update() 
	{
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
			m_CurrentMouseGridPoint = m_Grid.GetGridPoint(m_CurrentMouseHitPoint - (m_Grid.TileContainer.rotation * m_TilesOffset * m_GridScale));
		}
		
		// Update cursor
		UpdateCursor();

		// Update default input
		UpdateInput();

		// Update the material variables
		Vector3 up = m_Grid.transform.up;
		Vector3 pos = m_Grid.transform.position;
		m_TileMaterial.SetVector("_PlaneNormal", new Vector4(up.x, up.y, up.z));
		m_TileMaterial.SetVector("_PlanePoint", new Vector4(pos.x, pos.y + 2.0f * m_GridScale, pos.z));
		m_TileMaterial.SetFloat("_GlowDist", 0.3f * m_GridScale);
	}

	private void UpdateInput()
	{
		// Toggle modes
		if(Input.GetKeyDown(KeyCode.Alpha1))
			m_CurrentMode = EToolMode.Paint_Exterior;

		else if(Input.GetKeyDown(KeyCode.Alpha2))
			m_CurrentMode = EToolMode.Paint_Interior_Walls;

		else if(Input.GetKeyDown(KeyCode.Alpha3))
			m_CurrentMode = EToolMode.Select_Tiles;

		if(m_CurrentMode == EToolMode.Select_Tiles)
		{
			UpdateTileSelectInput();
		}

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

	private void UpdateTileSelectInput()
	{
		if(m_SelectedTiles.Count == 0)
			return;

		// Toggling tile type
		if(Input.GetKeyDown(KeyCode.Q))
			m_CurrentlySelectedType = CTile.ETileType.Floor;
		
		else if(Input.GetKeyDown(KeyCode.W))
			m_CurrentlySelectedType = CTile.ETileType.Wall_Ext;
		
		else if(Input.GetKeyDown(KeyCode.E))
			m_CurrentlySelectedType = CTile.ETileType.Wall_Int;

		else if(Input.GetKeyDown(KeyCode.R))
			m_CurrentlySelectedType = CTile.ETileType.Ceiling;

		// Enabling/Disabling tile type
		if(Input.GetKeyDown(KeyCode.Insert))
		{
			// Update the type state for given tiles
			foreach(CTile tile in m_SelectedTiles)
			{
				tile.SetTileTypeState(m_CurrentlySelectedType, true);
				tile.UpdateTileMetaData();
			}
		}
		else if(Input.GetKeyDown(KeyCode.Delete))
		{
			// Update the type state for given tiles
			foreach(CTile tile in m_SelectedTiles)
			{
				tile.SetTileTypeState(m_CurrentlySelectedType, false);
				tile.UpdateTileMetaData();
			}
		}

		// Toggling Variants of neighbout exemptsions
		if(Input.GetKeyDown(KeyCode.Keypad8))
		{
			foreach(CTile tile in m_SelectedTiles)
			{
				EDirection localNorth = tile.GetTileTypeLocalNorth(m_CurrentlySelectedType);
				EDirection final = CNeighbour.GetLocalDirection(localNorth, EDirection.North);
				bool state = tile.GetTileNeighbourExemptionState(m_CurrentlySelectedType, final);
				tile.SetTileNeighbourExemptionState(m_CurrentlySelectedType, final, !state);
				tile.UpdateTileMetaData();
			}
		}
		else if(Input.GetKeyDown(KeyCode.Keypad2))
		{
			foreach(CTile tile in m_SelectedTiles)
			{
				EDirection localNorth = tile.GetTileTypeLocalNorth(m_CurrentlySelectedType);
				EDirection final = CNeighbour.GetLocalDirection(localNorth, EDirection.South);
				bool state = tile.GetTileNeighbourExemptionState(m_CurrentlySelectedType, final);
				tile.SetTileNeighbourExemptionState(m_CurrentlySelectedType, final, !state);
				tile.UpdateTileMetaData();
			}
		}
		else if(Input.GetKeyDown(KeyCode.Keypad4))
		{
			foreach(CTile tile in m_SelectedTiles)
			{
				EDirection localNorth = tile.GetTileTypeLocalNorth(m_CurrentlySelectedType);
				EDirection final = CNeighbour.GetLocalDirection(localNorth, EDirection.West);
				bool state = tile.GetTileNeighbourExemptionState(m_CurrentlySelectedType, final);
				tile.SetTileNeighbourExemptionState(m_CurrentlySelectedType, final, !state);
				tile.UpdateTileMetaData();
			}
		}
		else if(Input.GetKeyDown(KeyCode.Keypad6))
		{
			foreach(CTile tile in m_SelectedTiles)
			{
				EDirection localNorth = tile.GetTileTypeLocalNorth(m_CurrentlySelectedType);
				EDirection final = CNeighbour.GetLocalDirection(localNorth, EDirection.East);
				bool state = tile.GetTileNeighbourExemptionState(m_CurrentlySelectedType, final);
				tile.SetTileNeighbourExemptionState(m_CurrentlySelectedType, final, !state);
				tile.UpdateTileMetaData();
			}
		}
	}

	private void HandleLeftClickDown()
	{
		if(m_PlaneHit.collider == null)
			return;

		m_MouseDownHitPoint = m_CurrentMouseHitPoint;
		m_MouseDownGridPoint = m_CurrentMouseGridPoint;
		
		if(IsShiftKeyDown)
			m_CurrentTileInteraction = ETileInteraction.MultipleSelection;
		else
			m_CurrentTileInteraction = ETileInteraction.SingleSelection;
	}
	
	private void HandleLeftClickHold()
	{
		if(m_PlaneHit.collider == null)
			return;

		if(m_CurrentTileInteraction != ETileInteraction.SingleSelection)
			return;

		if(m_CurrentMode == EToolMode.Paint_Exterior)
		{
			if(!IsCtrlKeyDown)
				m_Grid.AddNewTile(m_CurrentMouseGridPoint, s_TT_FeWC);
			else
				m_Grid.ReleaseTile(m_CurrentMouseGridPoint);
			return;
		}
		
		if(m_CurrentMode == EToolMode.Paint_Interior_Walls)
		{
			CTile tile = m_Grid.GetTile(m_CurrentMouseGridPoint);
			if(tile != null)
			{
				tile.SetTileTypeState(CTile.ETileType.Wall_Int, !IsCtrlKeyDown);
				tile.UpdateTileMetaData();
			}
			return;
		}

		if(m_CurrentMode == EToolMode.Paint_Interior_Floors)
		{
			CTile tile = m_Grid.GetTile(m_CurrentMouseGridPoint);
			if(tile != null)
			{
				tile.SetTileTypeState(CTile.ETileType.Floor, !IsCtrlKeyDown);
				tile.UpdateTileMetaData();
			}
			return;
		}
	}
	
	private void HandleLeftClickUp()
	{
		if (m_PlaneHit.collider == null)
			return;

		bool single = m_CurrentTileInteraction == ETileInteraction.SingleSelection;
		bool multi = m_CurrentTileInteraction == ETileInteraction.MultipleSelection;

		if (single)
			HandleLeftClickUpSingle();

		if (multi)
			HandleLeftClickUpMulti();
	}

	private void HandleLeftClickUpSingle()
	{
		if(m_CurrentMode == EToolMode.Select_Tiles) 
		{
			if(!IsCtrlKeyDown)
				m_SelectedTiles.Clear();

			SelectTile(m_CurrentMouseGridPoint);

			if(EventTileSelectionChange != null)
				EventTileSelectionChange();
		}
	}

	private void HandleLeftClickUpMulti()
	{
		switch(m_CurrentMode) 
		{
			case EToolMode.Select_Tiles: 
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
		m_CurrentTileInteraction = ETileInteraction.SingleSelection;
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

	private void UpdateGridScale(float _GridScale)
	{
		m_GridScale = _GridScale;

		// Update the grid root scale
		m_Grid.transform.localScale = Vector3.one * _GridScale;
		
		// Update the raycast plane to be the same
		m_GridPlane.transform.localScale = Vector3.one * 0.25f / _GridScale;
	}

	private void ChangeVerticalLayer(int _Direction)
	{
		m_CurrentVerticalLayer += _Direction;

		// Update the tiles offset
		m_TilesOffset -= Vector3.up * (float)_Direction * m_Grid.m_TileSize;
		m_Grid.TileContainer.transform.localPosition = m_TilesOffset;
	}
	
	private void UpdateCursor()
	{
		if(m_CurrentPlaneInteraction != EPlaneInteraction.Nothing)
			return;

		if(m_CurrentMode == EToolMode.Nothing)
		{
			m_GridCursor.renderer.enabled = false;
			return;
		}
		else if(m_GridCursor.renderer.enabled == false)
			m_GridCursor.renderer.enabled = true;
		

		if(m_CurrentTileInteraction == ETileInteraction.MultipleSelection)
		{
			Vector3 point1 = m_Grid.GetLocalPosition(m_CurrentMouseGridPoint) + m_TilesOffset;
			Vector3 point2 = m_Grid.GetLocalPosition(m_MouseDownGridPoint) + m_TilesOffset;

			Vector3 centerPos = (point1 + point2) * 0.5f;
			float width = Mathf.Abs(m_CurrentMouseGridPoint.x - m_MouseDownGridPoint.x) + 1.0f;
			float height = Mathf.Abs(m_CurrentMouseGridPoint.y - m_MouseDownGridPoint.y) + 1.0f;
			float depth = Mathf.Abs(m_CurrentMouseGridPoint.z - m_MouseDownGridPoint.z) + 1.0f;
			centerPos.y += m_Grid.m_TileSize * 0.5f;

			m_GridCursor.transform.localScale = new Vector3(width, height, depth) * m_Grid.m_TileSize;
			m_GridCursor.transform.localPosition = centerPos;
		}
		else
		{
			Vector3 centerPos = m_Grid.GetLocalPosition(m_CurrentMouseGridPoint) + m_TilesOffset;
			centerPos.y += m_Grid.m_TileSize * 0.5f;

			m_GridCursor.transform.localScale = Vector3.one * m_Grid.m_TileSize;
			m_GridCursor.transform.localPosition = centerPos;
		}
	}

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
	
	private void SelectionManipulateTiles(bool _Remove)
	{
		// Get the diagonal corner points
		TGridPoint point1 = m_MouseDownGridPoint;
		TGridPoint point2 = m_CurrentMouseGridPoint;
		
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
					if(_Remove) m_Grid.ReleaseTile(new TGridPoint(x, y, z));
					else m_Grid.AddNewTile(new TGridPoint(x, y, z), s_TT_FeWC);
				}
			}
		}	
	}

	private void SelectTile(TGridPoint _GridPoint)
	{
		CTile tile = m_Grid.GetTile(_GridPoint);

		if(tile != null)
			m_SelectedTiles.Add(tile);
	}

	private void SelectMultipleTiles()
	{
		// Get the diagonal corner points
		TGridPoint point1 = m_MouseDownGridPoint;
		TGridPoint point2 = m_CurrentMouseGridPoint;
		
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
				SelectTile(new TGridPoint(x, point1.y, z));
			}
		}
	}

	private void OnTileCreated(CTile _Tile)
	{
		// Check the tiles lower/upper neighbours
		CNeighbour upper = _Tile.m_NeighbourHood.Find(neighbour => neighbour.m_WorldDirection == EDirection.Upper);
		CNeighbour lower = _Tile.m_NeighbourHood.Find(neighbour => neighbour.m_WorldDirection == EDirection.Lower);

		// If upper exists, remove my ceiling
		if(upper != null)
		{
			_Tile.SetTileTypeState(CTile.ETileType.Ceiling, false);
		}

		// If lower exists
		if(lower != null)
		{
			// Remove their ceiling
			lower.m_Tile.SetTileTypeState(CTile.ETileType.Ceiling, false);

			// Remove floor
			_Tile.SetTileTypeState(CTile.ETileType.Floor, false);

//			bool wallext = lower.m_Tile.GetMetaData(CTile.ETileType.Wall_Ext).m_Type != TTileMeta.EType.None;
//			bool wallint = lower.m_Tile.GetTileTypeState(CTile.ETileType.Wall_Int);
//
//			if(!wallext && !wallint)
//				
		}

		// Register tile events change
		_Tile.EventTileAppearanceChanged += OnTileAppearanceChange;
		_Tile.EventTileTypeStateChange += OnTileTypeStateChange;
	}

	private void OnTileRemoved(CTile _Tile)
	{
		// Check the tiles lower neighbours for ceiling check
		CNeighbour lower = _Tile.m_NeighbourHood.Find(neighbour => neighbour.m_WorldDirection == EDirection.Lower);

		// If lower exists, re-enable ceiling and floor
		if(lower != null)
		{
			lower.m_Tile.SetTileTypeState(CTile.ETileType.Ceiling, true);
		}

		// Unregister tile events 
		_Tile.EventTileAppearanceChanged -= OnTileAppearanceChange;
		_Tile.EventTileTypeStateChange -= OnTileTypeStateChange;
	}

	private void OnTileTypeStateChange(CTile _Tile)
	{
		// Check if there is a internal wall without a floor
		if(_Tile.GetTileTypeState(CTile.ETileType.Wall_Int) && !_Tile.GetTileTypeState(CTile.ETileType.Floor))
		{
			_Tile.SetTileTypeState(CTile.ETileType.Wall_Int, false);
		}
	}

	private void OnTileAppearanceChange(CTile _Tile)
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
		}
	}

	private void OnFacilityCreate(CFacilityInterface _Facility)
	{
		// Update the state of the UI grid
		m_Grid.ImportTileInformation(CGameShips.Ship.GetComponent<CShipFacilities>().m_ShipGrid.Tiles.ToArray());
	}
}


