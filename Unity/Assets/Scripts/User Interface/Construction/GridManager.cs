using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour {
	
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public GameObject m_GridPlane = null;

	private Vector3 m_GridExtents = Vector3.zero;

	private GameObject m_TileContainer = null;
	public float m_TileSize = 4.0f;
	public float m_TileScale = 1.0f;

	public Vector3 m_CurrentMousePoint = Vector3.zero;
	public Vector3 m_CurrentGridPoistion = Vector3.zero;
	public Vector3 m_MouseDownPoint = Vector3.zero;
	private bool m_FinishedDragOnThisFrame;

	public List<TileBehaviour> m_TilesOnScreen = new List<TileBehaviour>();
	public List<TileBehaviour> m_TilesInDrag = new List<TileBehaviour>();
	public List<TileBehaviour> m_CurrentlySelectedTiles = new List<TileBehaviour>();

	public GUIStyle MouseDragSkin;

	public bool m_UserIsDragging;
	private float m_TimeLimitBeforeDeclareDrag = 1f;
	private float m_TimeLeftBeforeDeclareDrag;
	private Vector2 m_MouseDragStart;

	private float m_ClickDragzone = 1.3f;

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

	#region GUI
	float boxWidth;
	float boxHeight;
	
	float boxLeft;
	float boxTop;
	Vector2 boxStart;
	Vector2 boxFinish;
	#endregion
	
	
	// Member Properties
	
	
	// Member Methods
	void Awake()
	{
		I = this;
	}

	void Start() 
	{
		m_GridExtents = m_GridPlane.collider.bounds.extents;

		// Create the tile conatiner
		m_TileContainer = new GameObject("Tiles Parent");
		m_TileContainer.transform.parent = transform;
		m_TileContainer.transform.localScale = Vector3.one * m_TileScale;
		m_TileContainer.transform.localPosition = Vector3.zero;
		m_TileContainer.transform.localRotation = Quaternion.identity;

		// Create the recycler container
		GameObject recyclerContainer = new GameObject("Recycler Container");
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
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;

		if(Physics.Raycast(ray, out hit, Mathf.Infinity))
		{
			m_CurrentMousePoint = hit.point;
			m_CurrentGridPoistion = GridPosition(hit.point);

			//Store point at mouse button down
			if (Input.GetMouseButtonDown(0))
			{
				m_MouseDownPoint = hit.point;
				m_TimeLeftBeforeDeclareDrag = m_TimeLimitBeforeDeclareDrag;
				m_MouseDragStart = Input.mousePosition;
			}

			else if (Input.GetMouseButton(0))
			{
				//if dragging trigger tests
				if (!m_UserIsDragging)
				{
					m_TimeLeftBeforeDeclareDrag -=Time.deltaTime;

					if (m_TimeLeftBeforeDeclareDrag <= 0f || UserDraggingByPosition(m_MouseDragStart, Input.mousePosition))
						m_UserIsDragging = true;
				}
			}

			else if (Input.GetMouseButtonUp(0))
			{
				if (m_UserIsDragging)
					m_FinishedDragOnThisFrame = true;
				else
				{
					if (CtrlKeyDown())
					{
						RemoveTile (GridPosition(m_CurrentMousePoint));
					}
					else
						CreateTile(GridPosition(m_CurrentMousePoint));
				}
				m_UserIsDragging = false;
			}
		}

		if(m_UserIsDragging)
		{
			//GUI variables
			boxWidth = Camera.main.WorldToScreenPoint(m_MouseDownPoint).x - Camera.main.WorldToScreenPoint(m_CurrentMousePoint).x;
			boxHeight = Camera.main.WorldToScreenPoint(m_MouseDownPoint).y - Camera.main.WorldToScreenPoint(m_CurrentMousePoint).y;
			
			boxLeft = Input.mousePosition.x;
			boxTop = (Screen.height - Input.mousePosition.y) - boxHeight;
		}

		if (boxWidth > 0f && boxHeight < 0f)
		{
			boxStart = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		}
		else if (boxWidth > 0f && boxHeight > 0f)
		{
			boxStart = new Vector2 (Input.mousePosition.x, Input.mousePosition.y + boxHeight);
		}
		else if (boxWidth < 0f && boxWidth < 0f)
		{
			boxStart = new Vector2 (Input.mousePosition.x + boxWidth, Input.mousePosition.y);
		}
		else if (boxWidth < 0f && boxWidth > 0f)
		{
			boxStart = new Vector2 (Input.mousePosition.x +boxWidth, Input.mousePosition.y +boxHeight);
		}

		boxFinish = new Vector2 (
			boxStart.x + Mathf.Abs(boxWidth),
			boxStart.y + Mathf.Abs(boxHeight)
			);

		m_TileContainer.transform.rotation = m_GridPlane.transform.rotation;
	}

	void LateUpdate()
	{
		m_TilesInDrag.Clear();

		//if user is dragging. or finished on this frame and Tiles on screen
		if ((m_UserIsDragging || m_FinishedDragOnThisFrame) && m_TilesOnScreen.Count > 0)
		{
			for (int i=0; i < m_TilesOnScreen.Count; i++)
			{
				if (TilesInsideDrag(m_TilesOnScreen[i].screenPos))
				{
					m_TilesInDrag.Add(m_TilesOnScreen[i]);
				}
			}
		}

		if (m_FinishedDragOnThisFrame)
		{
			m_FinishedDragOnThisFrame = false;
			PutDraggedTilesInSelectedTiles();
			if (CtrlKeyDown())
			{
				DragManipulateTiles(m_MouseDownPoint, m_CurrentMousePoint,true);
			}
			else
				DragManipulateTiles(m_MouseDownPoint, m_CurrentMousePoint,false);
		}

	}

	void OnGUI()
	{
		if (m_UserIsDragging)
		{
			GUI.Box (new Rect (boxLeft,
			                   boxTop,
			                   boxWidth,
			                   boxHeight), "", MouseDragSkin);
		}
	}

	public bool ShiftKeyDown ()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			return true; else return false;
	}

	public bool CtrlKeyDown ()
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			return true; else return false;
	}

	//is the user dragging, relative to the mouse drag start point
	public bool UserDraggingByPosition (Vector2 DragStartPoint, Vector2 NewPoint)
	{
		if(
			(NewPoint.x > DragStartPoint.x + m_ClickDragzone || NewPoint.x < DragStartPoint.x - m_ClickDragzone) ||
			(NewPoint.y > DragStartPoint.y + m_ClickDragzone || NewPoint.y < DragStartPoint.y - m_ClickDragzone)
			)
			return true; else return false;
	}

	public bool DidiUSerClickLeftMouse (Vector3 hitPoint)
	{
		if (
			(m_MouseDownPoint.x < hitPoint.x + m_ClickDragzone && m_MouseDownPoint.x > hitPoint.x - m_ClickDragzone) &&
			(m_MouseDownPoint.y < hitPoint.y + m_ClickDragzone && m_MouseDownPoint.y > hitPoint.y - m_ClickDragzone) &&
			(m_MouseDownPoint.z < hitPoint.z + m_ClickDragzone && m_MouseDownPoint.z > hitPoint.z - m_ClickDragzone)
			)
			return true; else return false;
	}

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

	public bool TilesInsideDrag (Vector2 ScreenPosition)
	{
		if (
			(ScreenPosition.x > boxStart.x && ScreenPosition.y < boxStart.y) &&
			(ScreenPosition.x < boxFinish.x && ScreenPosition.y > boxFinish.y)
			) return true; else return false;
	}

	//take all nodes in nodes in drag into currently selectedTiles
	public void PutDraggedTilesInSelectedTiles()
	{
		if (m_TilesInDrag.Count > 0)
		{
			for (int i = 0; i < m_TilesInDrag.Count; i++)
			{
				m_CurrentlySelectedTiles.Add(m_TilesInDrag[i]);
			}
		}
		m_TilesInDrag.Clear();
	}

	public Vector3 GridPosition(Vector3 worldPosition)
	{
		// Convert the world position to grid space
		Vector3 gridpos = worldPosition - m_TileContainer.transform.position;
		gridpos = gridpos / m_TileSize / m_TileScale;
		gridpos.x = Mathf.Round(gridpos.x);
		gridpos.y = Mathf.Round(gridpos.y);
		gridpos.z = Mathf.Round(gridpos.z);
		return gridpos;
	}

	public void DragManipulateTiles(Vector3 mouseDownPoint, Vector3 currentPoint, bool Remove)
	{
		Vector3 point1 = GridPosition(mouseDownPoint);
		Vector3 point2 = GridPosition(currentPoint);


		if (point1.x < point2.x)
		{
		//for (float y = gridY.x; y < gridY.y; y++)
		//{
			if (point1.z < point2.z)
			{
			for (float x = point1.x; x < point2.x; x++)
				{

				for (float z = point1.z; z < point2.z; z++)
					{
						if (Remove)
						{
							RemoveTile (new Vector3 (x,0,z));
						}
						else
							CreateTile (new Vector3 (x,0,z));
					}
				}
			}
			else
			{
				for (float x = point1.x; x < point2.x; x++)
				{
					
					for (float z = point2.z; z < point1.z; z++)
					{
						if (Remove)
						{
							RemoveTile (new Vector3 (x,0,z));
						}
						else
							CreateTile (new Vector3 (x,0,z));
					}
				}
			}
		//}
		}
		else
		{
			if (point1.z < point2.z)
			{
				for (float x = point2.x; x < point1.x; x++)
				{
					
					for (float z = point1.z; z < point2.z; z++)
					{
						if (Remove)
						{
							RemoveTile (new Vector3 (x,0,z));
						}
						else
							CreateTile (new Vector3 (x,0,z));
					}
				}
			}
			else
			{
				for (float x = point2.x; x < point1.x; x++)
				{
					
					for (float z = point2.z; z < point1.z; z++)
					{
						if (Remove)
						{
							RemoveTile (new Vector3 (x,0,z));
						}
						else
							CreateTile (new Vector3 (x,0,z));
					}
				}
			}
		}

	}


	public void CreateTile (Vector3 gridPosition)
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

	public void RemoveTile (Vector3 gridPosition)
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


