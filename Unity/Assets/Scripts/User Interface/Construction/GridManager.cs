using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour {
	
	// Member Types
	public enum EInteraction
	{
		INVALID,

		Nothing,
		CursorPaint,
		DragSelection,
		DragRotation,
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	private GameObject m_GridRoot = null;
	private GameObject m_RaycastPlane = null;
	private GameObject m_GridCursor = null;
	private GameObject m_TileContainer = null;

	public float m_TileSize = 4.0f;

	public float m_GridScale = 0.0f;
	public Vector2 m_GridScaleLimits = new Vector2(0.05f, 0.2f);

	public EInteraction m_CurrentInteraction = EInteraction.INVALID;

	public Vector3 m_CurrentMousePoint = Vector3.zero;
	public Vector3 m_CurrentMouseGridPoistion = Vector3.zero;
	public Vector3 m_CurrentMousePosition = Vector3.zero;

	public Vector3 m_MouseDownPoint = Vector3.zero;
	public Vector3 m_MouseDownGridPoistion = Vector3.zero;
	public Vector3 m_MouseDownPosition = Vector3.zero;

	private Quaternion m_DragRotateStart = Quaternion.identity;

	public List<TileBehaviour> m_TilesOnScreen = new List<TileBehaviour>();
	public List<TileBehaviour> m_TilesInDrag = new List<TileBehaviour>();
	public List<TileBehaviour> m_CurrentlySelectedTiles = new List<TileBehaviour>();

	//public GUIStyle MouseDragSkin;

	//public bool m_UserIsDragging;
	//private float m_TimeLimitBeforeDeclareDrag = 1f;
	//private float m_TimeLeftBeforeDeclareDrag;

	//private float m_ClickDragzone = 1.3f;

	public Transform tile;
	public Transform floorTile;
	public Transform wallStraight;
	public Transform wallCorner;
	public Transform Hallway;
	public Transform DeadEnd;
	public Transform cell;
	public ObjectRecycler DeadEndTiles;
	public ObjectRecycler HallwayTiles;
	public ObjectRecycler wallStraightTiles;
	public ObjectRecycler wallCornerTiles;
	public ObjectRecycler floorTiles;
	public ObjectRecycler tiles;
	public ObjectRecycler cellTiles;

	public Dictionary<string, TileBehaviour> m_GridBoard = new Dictionary<string, TileBehaviour>();

	public static GridManager I = null;
//
//	#region GUI
//	float boxWidth;
//	float boxHeight;
//	
//	float boxLeft;
//	float boxTop;
//	Vector2 boxStart;
//	Vector2 boxFinish;
//	#endregion
	
	
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
	void Awake()
	{
		I = this;
	}

	void Start() 
	{
		// Create the grid objects
		CreateGridObjects();

		// Create the tile recyclers
		CreateRecyclers();
	}

	void CreateGridObjects()
	{
		m_GridRoot = new GameObject("GridRoot");
		m_GridRoot.transform.parent = transform;
		m_GridRoot.transform.localPosition = Vector3.zero;
		m_GridRoot.transform.localRotation = Quaternion.identity;

		m_TileContainer = new GameObject("Tile Container");
		m_TileContainer.transform.parent = m_GridRoot.transform;
		m_TileContainer.transform.localScale = Vector3.one;
		m_TileContainer.transform.localPosition = Vector3.zero;
		m_TileContainer.transform.localRotation = Quaternion.identity;

		m_RaycastPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		m_RaycastPlane.name = "Raycast Plane";
		m_RaycastPlane.renderer.material.shader = Shader.Find("Transparent/Diffuse");
		m_RaycastPlane.renderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
		m_RaycastPlane.collider.isTrigger = true;
		m_RaycastPlane.transform.parent = m_GridRoot.transform;
		m_RaycastPlane.transform.localPosition = Vector3.zero;
		m_RaycastPlane.transform.localRotation = Quaternion.identity;

		m_GridCursor = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_GridCursor.name = "Cursor";
		m_GridCursor.renderer.material.shader = Shader.Find("Transparent/Diffuse");
		m_GridCursor.renderer.material.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);
		Destroy(m_GridCursor.collider);
		m_GridCursor.transform.parent = m_GridRoot.transform;
		m_GridCursor.transform.localScale = Vector3.one * m_TileSize;
		m_GridCursor.transform.localPosition = Vector3.zero;
		m_GridCursor.transform.localRotation = Quaternion.identity;

		// Use the average of scale limits
		m_GridScale = (m_GridScaleLimits.x + m_GridScaleLimits.y) * 0.5f;
		UpdateGridScale();
	}

	void CreateRecyclers()
	{
		// Create the recycler container
		GameObject recyclerContainer = new GameObject("Recycler");
		recyclerContainer.transform.parent = transform;
		recyclerContainer.transform.localScale = Vector3.one;
		recyclerContainer.transform.localPosition = Vector3.zero;
		recyclerContainer.transform.localRotation = Quaternion.identity;

		// Create the tile reyclers
		tiles = new ObjectRecycler(tile.gameObject, recyclerContainer, 3);
		floorTiles = new ObjectRecycler(floorTile.gameObject, recyclerContainer, 1);
		wallStraightTiles = new ObjectRecycler(wallStraight.gameObject, recyclerContainer, 1);
		wallCornerTiles = new ObjectRecycler(wallCorner.gameObject, recyclerContainer, 1);
		HallwayTiles = new ObjectRecycler(Hallway.gameObject, recyclerContainer, 1);
		DeadEndTiles = new ObjectRecycler(DeadEnd.gameObject, recyclerContainer, 1);
		cellTiles = new ObjectRecycler(cell.gameObject, recyclerContainer, 1);
	}

	void Update() 
	{
		// Update the input
		UpdateInput();
	}

	void UpdateInput()
	{
		m_CurrentMousePosition = Input.mousePosition;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

		foreach(RaycastHit hit in hits)
		{
			// If a grid plane collision
			if(hit.collider.gameObject == m_RaycastPlane)
			{
				m_CurrentMousePoint = hit.point;
				m_CurrentMouseGridPoistion = GetGridPosition(hit.point);

				// Left Click Down
				if(Input.GetMouseButtonDown(0))
				{
					m_MouseDownPoint = hit.point;
					m_MouseDownGridPoistion = m_CurrentMouseGridPoistion;
					m_MouseDownPosition = Input.mousePosition;

					if(IsShiftKeyDown)
						m_CurrentInteraction = EInteraction.DragSelection;
					else
						m_CurrentInteraction = EInteraction.CursorPaint;
				}

				// Left Click Hold
				if(Input.GetMouseButton(0))
				{
					if(m_CurrentInteraction == EInteraction.CursorPaint)
					{
						if(IsCtrlKeyDown)
							RemoveTile(GetGridPosition(m_CurrentMousePoint));
						else
							CreateTile(GetGridPosition(m_CurrentMousePoint));
					}
				}

				// Left Click Up
				if(Input.GetMouseButtonUp(0))
				{
					if(m_CurrentInteraction == EInteraction.DragSelection)
					{
						if (IsCtrlKeyDown)
							SelectionManipulateTiles(true);
						else
							SelectionManipulateTiles(false);
					}

					m_CurrentInteraction = EInteraction.Nothing;
				}

				// Configure the cursor
				UpdateCursor();
			}

			// If a sphere collision
			else if(hit.collider.gameObject == gameObject)
			{
				m_CurrentMousePoint = hit.point;

				// Right Click Down
				if(Input.GetMouseButtonDown(1))
				{
					m_MouseDownPoint = hit.point;
					m_MouseDownPosition = Input.mousePosition;
					m_DragRotateStart = m_GridRoot.transform.rotation;

					m_CurrentInteraction = EInteraction.DragRotation;
				}

				// Mouse Scroll
				float sw = Input.GetAxis("Mouse ScrollWheel");
				if(sw != 0.0f)
				{
					m_GridScale = Mathf.Clamp(m_GridScale + sw * 0.1f, m_GridScaleLimits.x, m_GridScaleLimits.y);
					UpdateGridScale();
				}
			}
		}

		// Right Click Hold
		if(Input.GetMouseButton(1) && m_CurrentInteraction == EInteraction.DragRotation)
		{
			DragRotateGrid();
		}

		// Right Click Up
		else if(Input.GetMouseButtonUp(1) && m_CurrentInteraction == EInteraction.DragRotation)
		{
			m_CurrentInteraction = EInteraction.Nothing;
		}
	}

	void UpdateGridScale()
	{
		// Update the grid root scale
		m_GridRoot.transform.localScale = Vector3.one * m_GridScale;

		// Update the raycast plane to be the same
		m_RaycastPlane.transform.localScale = Vector3.one * 0.5f / m_GridScale;

	}

	void UpdateCursor()
	{
		if(m_CurrentInteraction == EInteraction.DragSelection)
		{
			Vector3 centerPos = (GetLocalPosition(m_CurrentMouseGridPoistion) + GetLocalPosition(m_MouseDownGridPoistion)) * 0.5f;
			float width = Mathf.Abs(m_CurrentMouseGridPoistion.x - m_MouseDownGridPoistion.x) + 1.0f;
			float depth = Mathf.Abs(m_CurrentMouseGridPoistion.z - m_MouseDownGridPoistion.z) + 1.0f;
			
			m_GridCursor.transform.localScale = new Vector3(width, 1.0f, depth) * m_TileSize;
			m_GridCursor.transform.localPosition = centerPos + Vector3.up * m_TileSize * 0.5f;
		}
		else if(m_CurrentInteraction != EInteraction.DragRotation)
		{
			m_GridCursor.transform.localScale = Vector3.one * m_TileSize;
			m_GridCursor.transform.localPosition = GetLocalPosition(m_CurrentMouseGridPoistion) + Vector3.up * m_TileSize * 0.5f;
		}
	}

//			//Store point at mouse button down
//			if(Input.GetMouseButtonDown(0))
//			{
//				m_MouseDownPoint = hit.point;
//				m_TimeLeftBeforeDeclareDrag = m_TimeLimitBeforeDeclareDrag;
//				m_MouseDragStart = Input.mousePosition;
//			}
//			
//			else if(Input.GetMouseButton(0))
//			{
//				//if dragging trigger tests
//				if(!m_UserIsDragging)
//				{
//					m_TimeLeftBeforeDeclareDrag -= Time.deltaTime;
//					
//					if (m_TimeLeftBeforeDeclareDrag <= 0f || UserDraggingByPosition(m_MouseDragStart, Input.mousePosition))
//						m_UserIsDragging = true;
//				}
//				else
//				{
//					if (CtrlKeyDown())
//						RemoveTile(GridPosition(m_CurrentMousePoint));
//					else
//						CreateTile(GridPosition(m_CurrentMousePoint));
//				}
//			}
//			
//			else if (Input.GetMouseButtonUp(0))
//			{
//				if (m_UserIsDragging)
//					m_FinishedDragOnThisFrame = true;
//				else
//				{
//					if (CtrlKeyDown())
//						RemoveTile(GridPosition(m_CurrentMousePoint));
//					else
//						CreateTile(GridPosition(m_CurrentMousePoint));
//				}
//				m_UserIsDragging = false;
//			}
//		}
//		
//		if(m_UserIsDragging)
//		{
//			//GUI variables
//			boxWidth = Camera.main.WorldToScreenPoint(m_MouseDownPoint).x - Camera.main.WorldToScreenPoint(m_CurrentMousePoint).x;
//			boxHeight = Camera.main.WorldToScreenPoint(m_MouseDownPoint).y - Camera.main.WorldToScreenPoint(m_CurrentMousePoint).y;
//			
//			boxLeft = Input.mousePosition.x;
//			boxTop = (Screen.height - Input.mousePosition.y) - boxHeight;
//		}
//		
//		if (boxWidth > 0f && boxHeight < 0f)
//		{
//			boxStart = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
//		}
//		else if (boxWidth > 0f && boxHeight > 0f)
//		{
//			boxStart = new Vector2 (Input.mousePosition.x, Input.mousePosition.y + boxHeight);
//		}
//		else if (boxWidth < 0f && boxWidth < 0f)
//		{
//			boxStart = new Vector2 (Input.mousePosition.x + boxWidth, Input.mousePosition.y);
//		}
//		else if (boxWidth < 0f && boxWidth > 0f)
//		{
//			boxStart = new Vector2 (Input.mousePosition.x +boxWidth, Input.mousePosition.y +boxHeight);
//		}
//		
//		boxFinish = new Vector2 (
//			boxStart.x + Mathf.Abs(boxWidth),
//			boxStart.y + Mathf.Abs(boxHeight)
//			);
//	}
//
//	void LateUpdate()
//	{
//		m_TilesInDrag.Clear();
//
//		//if user is dragging. or finished on this frame and Tiles on screen
//		if ((m_UserIsDragging || m_FinishedDragOnThisFrame) && m_TilesOnScreen.Count > 0)
//		{
//			for (int i = 0; i < m_TilesOnScreen.Count; i++)
//			{
//				if (TilesInsideDrag(m_TilesOnScreen[i].screenPos))
//				{
//					m_TilesInDrag.Add(m_TilesOnScreen[i]);
//				}
//			}
//		}
//
//		if (m_FinishedDragOnThisFrame)
//		{
//			m_FinishedDragOnThisFrame = false;
//			PutDraggedTilesInSelectedTiles();
//
//			if (CtrlKeyDown())
//				DragManipulateTiles(m_MouseDownPoint, m_CurrentMousePoint,true);
//			else
//				DragManipulateTiles(m_MouseDownPoint, m_CurrentMousePoint,false);
//		}
//	}
//
//	void OnGUI()
//	{
//		if (m_UserIsDragging)
//		{
//			GUI.Box (new Rect (boxLeft,
//			                   boxTop,
//			                   boxWidth,
//			                   boxHeight), "", MouseDragSkin);
//		}
//	}
//
//	//is the user dragging, relative to the mouse drag start point
//	public bool UserDraggingByPosition (Vector2 DragStartPoint, Vector2 NewPoint)
//	{
//		if(
//			(NewPoint.x > DragStartPoint.x + m_ClickDragzone || NewPoint.x < DragStartPoint.x - m_ClickDragzone) ||
//			(NewPoint.y > DragStartPoint.y + m_ClickDragzone || NewPoint.y < DragStartPoint.y - m_ClickDragzone)
//			)
//			return true; else return false;
//	}
//
//	public bool DidUserClickLeftMouse (Vector3 hitPoint)
//	{
//		if (
//			(m_MouseDownPoint.x < hitPoint.x + m_ClickDragzone && m_MouseDownPoint.x > hitPoint.x - m_ClickDragzone) &&
//			(m_MouseDownPoint.y < hitPoint.y + m_ClickDragzone && m_MouseDownPoint.y > hitPoint.y - m_ClickDragzone) &&
//			(m_MouseDownPoint.z < hitPoint.z + m_ClickDragzone && m_MouseDownPoint.z > hitPoint.z - m_ClickDragzone)
//			)
//			return true; else return false;
//	}

	//Check if a node is within the screen space to deal with mouse drag selecting
	public bool NodeWithinScreenSpace(Vector2 NodeScreenPos)
	{
		if (
			(NodeScreenPos.x < Screen.width && NodeScreenPos.y < Screen.height) &&
			(NodeScreenPos.x > 0f && NodeScreenPos.y > 0f)
			)
			return true; else return false;
	}

	//Remove note from the screen from NodesOnScreen List
	public void RemoveFromOnScreenUnts (TileBehaviour Node)
	{
		for (int i = 0; i < m_TilesOnScreen.Count; i++)
		{
			TileBehaviour tb = m_TilesOnScreen[i];
			if (Node == tb)
			{
				m_TilesOnScreen.RemoveAt(i);
				tb.onScreen = false;
				return;
			}
		}

		return;
	}

//	public bool TilesInsideDrag (Vector2 ScreenPosition)
//	{
//		if (
//			(ScreenPosition.x > boxStart.x && ScreenPosition.y < boxStart.y) &&
//			(ScreenPosition.x < boxFinish.x && ScreenPosition.y > boxFinish.y)
//			) return true; else return false;
//	}
//
//	//take all nodes in nodes in drag into currently selectedTiles
//	public void PutDraggedTilesInSelectedTiles()
//	{
//		if (m_TilesInDrag.Count > 0)
//		{
//			for (int i = 0; i < m_TilesInDrag.Count; i++)
//			{
//				m_CurrentlySelectedTiles.Add(m_TilesInDrag[i]);
//			}
//		}
//		m_TilesInDrag.Clear();
//	}

	public Vector3 GetGridPosition(Vector3 worldPosition)
	{
		// Convert the world space to grid space
		Vector3 gridpos = Quaternion.Inverse(m_GridRoot.transform.rotation) * (worldPosition - m_GridRoot.transform.position);

		// Scale the position to tilesize and scale
		gridpos = gridpos / m_TileSize / m_GridScale;

		// Round each position to be an integer number
		gridpos.x = Mathf.Round(gridpos.x);
		gridpos.y = Mathf.Round(gridpos.y);
		gridpos.z = Mathf.Round(gridpos.z);

		return gridpos;
	}

	public Vector3 GetLocalPosition(Vector3 gridPosition)
	{
		// Convert from grid space to local space
		return(gridPosition * m_TileSize);
	}

	public void DragRotateGrid()
	{
		// Get the screen mouse positions
		Vector3 point1 = m_MouseDownPosition;
		Vector3 point2 = m_CurrentMousePosition;

		// Get the difference of the two
		Vector3 diff = (point1 - point2);

		// Rotate plane using the axis of camera to rotate pitch and yaw
		Quaternion rotPitch = Quaternion.AngleAxis(-diff.y * 0.1f, Camera.main.transform.right);
		Quaternion rotYaw = Quaternion.AngleAxis(diff.x * 0.2f, transform.up);

		// Lerp to the final rotation
		m_GridRoot.transform.rotation = rotPitch * m_DragRotateStart * rotYaw;
	}

	public void SelectionManipulateTiles(bool Remove)
	{
		// Get the diagonal corner points
		Vector3 point1 = m_MouseDownGridPoistion;
		Vector3 point2 = m_CurrentMouseGridPoistion;

		// Determine the rect properties
		float left = point1.x < point2.x ? point1.x : point2.x;
		float bottom = point1.z < point2.z ? point1.z : point2.z;
		float width = Mathf.Abs(point1.x - point2.x) + 1.0f;
		float height = Mathf.Abs(point1.z - point2.z) + 1.0f;

		// Itterate through the positions
		for(float x = left; x < (left + width); ++x)
		{
			for(float z = bottom; z < (bottom + height); ++z)
			{
				if(Remove) RemoveTile (new Vector3 (x, point1.y, z));
				else CreateTile (new Vector3 (x, point1.y, z));
			}
		}
	}


	public void CreateTile(Vector3 gridPosition)
	{
		Tile newtile = new Tile((int)gridPosition.x, (int)gridPosition.y, (int)gridPosition.z);
		if(!m_GridBoard.ContainsKey(newtile.ToString()))
		{
			GameObject tile = tiles.nextFree;
			tile.transform.parent = m_TileContainer.transform;
			tile.transform.localScale = Vector3.one;
			tile.transform.localRotation = Quaternion.identity;
			tile.transform.localPosition = gridPosition * m_TileSize;

			TileBehaviour tb = tile.GetComponent<TileBehaviour>();
			tb.tile = newtile;
			tb.tile.hullcontainer = tb.transform;
			m_GridBoard.Add(tb.tile.ToString(), tb);
			tb.tile.FindNeighbours();
			tb.tile.UpdateNeighbours();
		}
	}

	public void RemoveTile(Vector3 gridPosition)
	{
		Tile newtile = new Tile ((int)gridPosition.x, (int)gridPosition.y, (int)gridPosition.z);
		if (m_GridBoard.ContainsKey(newtile.ToString()))
		{
			TileBehaviour tb = m_GridBoard[newtile.ToString()];
			tb.tile.RemoveHull();
			tiles.freeObject(tb.transform.gameObject);
			m_GridBoard.Remove(newtile.ToString());
			tb.tile.UpdateNeighbours();
		}
		
	}

	public class ObjectRecycler
	{
		public delegate void ObjectRecyclerChangedEventHandler ( int avaliable, int total);
		public event ObjectRecyclerChangedEventHandler onObjectRecyclerChanged;
		
		private List<GameObject> objectList;
		private GameObject objectToRecycle;
		private GameObject objectContainer;

		public ObjectRecycler(GameObject go, GameObject container, int totalObjectsAtStart)
		{
			objectList = new List<GameObject>(totalObjectsAtStart);
			objectToRecycle = go;
			objectContainer = container;
			
			for ( int i = 0; i < totalObjectsAtStart; i++)
			{
				//Create a new instance and set ourselfs as the recycleBin
				GameObject newObject = UnityEngine.Object.Instantiate(go) as GameObject;
				newObject.transform.parent = objectContainer.transform;
				newObject.gameObject.SetActive(false);
				
				//add it to object store for later use
				objectList.Add(newObject);
			}
		}
		
		private void fireRecyledEvent()
		{
			if ( onObjectRecyclerChanged !=null)
			{
				var allFree = from item in objectList
					where !item.activeInHierarchy
						select item;
				
				onObjectRecyclerChanged( allFree.Count(), objectList.Count );
			}
		}
		
		// Gets the next avaliable free object or null
		public GameObject nextFree
		{
			get
			{
				GameObject freeObject = (from item in objectList
				                         where !item.activeInHierarchy
				                         select item).FirstOrDefault();
				
				if (freeObject == null)
				{
					freeObject = UnityEngine.Object.Instantiate(objectToRecycle) as GameObject;
					freeObject.transform.parent = objectContainer.transform;
					objectList.Add ( freeObject );
				}
				
				freeObject.SetActive(true);
				fireRecyledEvent();
				
				return freeObject;
			}
		}
		
		public void freeObject(GameObject objectToFree)
		{
			objectToFree.transform.parent = objectContainer.transform;
			objectToFree.gameObject.SetActive(false);
			objectToFree.transform.rotation = Quaternion.identity;
			fireRecyledEvent();
		}
		
		public List<GameObject> returnObjectList ()
		{
			return objectList;
		}
		
		public int objectCount ()
		{
			return objectList.Count;
		}
		
		//Create an object recycler to manager
		//recycler = new ObjectRecycler ( selectionRing.transform.gameObject, 1);
		
		//Get next objext from recycler
		//var go = recycler.nextFree;
	}
}


