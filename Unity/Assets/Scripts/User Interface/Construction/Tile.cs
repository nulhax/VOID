using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Tile : GridObject 
{
	Transform hullSegment;
	public Transform hullcontainer;

	public enum TileType { None, FloorMid, WallCorner, WallStraignt, Hallway, Deadend, Cell}
	public TileType tileType;

	public Tile (int x, int y, int z)
		: base(x, y, z)
	{
		//Passable = true;
	}
	
	public List<Neighbour> neighbourHood;

	public void FindNeighbours ()
	{
		neighbourHood = new List<Neighbour>();

		List <Neighbour> neighbours = new List<Neighbour> ();
		
		List <Neighbour> possibleNeighbours = allNeighbours;
		
		foreach (Neighbour pn in possibleNeighbours) {
			Tile possibleNeightbour = new Tile (X + pn.gridPositionOffset.X, 
			                                    Y + pn.gridPositionOffset.Y, 
			                                    Z + pn.gridPositionOffset.Z);
			
			if (GridManager.I.m_GridBoard.ContainsKey(possibleNeightbour.ToString()))
			{
				Neighbour newNeightbour = new Neighbour(pn.gridPositionOffset, pn.direction);
				newNeightbour.tile = GridManager.I.m_GridBoard[possibleNeightbour.ToString()].tile;
				neighbours.Add (newNeightbour);
			}
		}
		
		if (neighbours.Count > 0)
			neighbourHood = neighbours;

		UpdateTileType();
	}
	
	//change of coordinates when moving in any direction
	public List<Neighbour> allNeighbours {
		get {
			return new List<Neighbour>
			{
				new Neighbour (new Point(0, 0, 1), Neighbour.Direction.North),
				new Neighbour (new Point(1, 0, 1), Neighbour.Direction.Northeast),
				new Neighbour (new Point(1, 0, 0), Neighbour.Direction.East),
				new Neighbour (new Point(1, 0, -1), Neighbour.Direction.Southeast),
				new Neighbour (new Point(0, 0, -1), Neighbour.Direction.South),
				new Neighbour (new Point(-1, 0, 1), Neighbour.Direction.Northwest),
				new Neighbour (new Point(-1, 0, 0), Neighbour.Direction.West),
				new Neighbour (new Point(-1, 0, -1), Neighbour.Direction.Southwest),
			};
		}
	}

	public void UpdateNeighbours()
	{
		foreach (Neighbour neighbour in neighbourHood)
		{
			neighbour.tile.FindNeighbours();
		}
	}

	public void UpdateTileType ()
	{
		bool n = false, ne = false, e = false, se = false, s = false, sw = false, w = false, nw = false;

		foreach (Neighbour neighbour in neighbourHood)
		{
			switch (neighbour.direction)
			{
			case Neighbour.Direction.North:
				n = true;
				break;
			case Neighbour.Direction.Northeast:
				ne = true;
				break;
			case Neighbour.Direction.East:
				e = true;
				break;
			case Neighbour.Direction.Southeast:
				se = true;
				break;
			case Neighbour.Direction.South:
				s = true;
				break;
			case Neighbour.Direction.Northwest:
				nw = true;
				break;
			case Neighbour.Direction.West:
				w = true;
				break;
			case Neighbour.Direction.Southwest:
				sw = true;
				break;
			}
		}

		if (n)
		{
			if (s)
			{
				if (e)
				{
					if (w)
					{
						if (tileType != TileType.FloorMid)
						{
							RemoveHull();
							tileType = TileType.FloorMid;
							hullSegment = GridManager.I.floorTiles.nextFree.transform;
							hullSegment.parent = hullcontainer;
							hullSegment.transform.localRotation = Quaternion.identity;
						}
					}
					else
					{
						if (tileType != TileType.WallStraignt)
						{
							RemoveHull();
							tileType = TileType.WallStraignt;
							hullSegment = GridManager.I.wallStraightTiles.nextFree.transform;
							hullSegment.parent = hullcontainer;
							hullSegment.localRotation = Quaternion.Euler(new Vector3(0,-90,0));
						}
					}
				}
				else if (w)
				{
					if (tileType != TileType.WallStraignt)
					{
						RemoveHull();
						tileType = TileType.WallStraignt;
						hullSegment = GridManager.I.wallStraightTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.Euler(new Vector3(0,90,0));
					}
				}
				else
				{
					if (tileType != TileType.Hallway)
					{
						RemoveHull();
						tileType = TileType.Hallway;
						hullSegment = GridManager.I.HallwayTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.Euler(new Vector3(0,90,0));
					}

				}
			}
			else if (e)
			{
				if (w)
				{
					if (tileType != TileType.WallStraignt)
					{
						RemoveHull();
						tileType = TileType.WallStraignt;
						hullSegment = GridManager.I.wallStraightTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.Euler(new Vector3(0,180,0));
					}
				}
				else
				{
					if (tileType != TileType.WallCorner)
					{
						RemoveHull();
						tileType = TileType.WallCorner;
						hullSegment = GridManager.I.wallCornerTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.Euler(new Vector3(0,180,0));
					}
				}
			}
			else if (w)
			{
				if (e)
				{
					if (tileType != TileType.WallStraignt)
					{
						RemoveHull();
						tileType = TileType.WallStraignt;
						hullSegment = GridManager.I.wallStraightTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.Euler(new Vector3(0,180,0));
					}
				}
				else
				{
					if (tileType != TileType.WallCorner)
					{
						RemoveHull();
						tileType = TileType.WallCorner;
						hullSegment = GridManager.I.wallCornerTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.Euler(new Vector3(0,90,0));
					}
				}

			}
			else
			{
				if (tileType != TileType.Deadend)
				{
					RemoveHull();
					tileType = TileType.Deadend;
					hullSegment = GridManager.I.DeadEndTiles.nextFree.transform;
					hullSegment.parent = hullcontainer;
					hullSegment.localRotation = Quaternion.Euler(new Vector3(0,-90,0));
				}
			}
		}
		else if (s)
		{
			if (e)
			{
				if (w)
				{
					RemoveHull();
					tileType = TileType.WallStraignt;
					hullSegment = GridManager.I.wallStraightTiles.nextFree.transform;
					hullSegment.parent = hullcontainer;
					hullSegment.localRotation = Quaternion.identity;
				}
				else
				{
					if (tileType != TileType.WallCorner)
					{
						RemoveHull();
						tileType = TileType.WallCorner;
						hullSegment = GridManager.I.wallCornerTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.Euler(new Vector3(0,-90,0));
					}
				}
			}
			else if (w)
			{
				if (e)
				{
					if (tileType != TileType.WallStraignt)
					{
						RemoveHull();
						tileType = TileType.WallStraignt;
						hullSegment = GridManager.I.wallStraightTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.identity;
					}
				}
				else
				{
					if (tileType != TileType.WallCorner)
					{
						RemoveHull();
						tileType = TileType.WallCorner;
						hullSegment = GridManager.I.wallCornerTiles.nextFree.transform;
						hullSegment.parent = hullcontainer;
						hullSegment.localRotation = Quaternion.identity;
					}
				}
			}
			else
			{
				if (tileType != TileType.Deadend)
				{
					RemoveHull();
					tileType = TileType.Deadend;
					hullSegment = GridManager.I.DeadEndTiles.nextFree.transform;
					hullSegment.parent = hullcontainer;
					hullSegment.localRotation = Quaternion.Euler(new Vector3(0,90,0));
				}
			}
		}
		else if (e)
		{
			if (w)
			{
				RemoveHull();
				tileType = TileType.Hallway;
				hullSegment = GridManager.I.HallwayTiles.nextFree.transform;
				hullSegment.parent = hullcontainer;
				hullSegment.localRotation = Quaternion.identity;
			}
			else
			{
				if (tileType != TileType.Deadend)
				{
					RemoveHull();
					tileType = TileType.Deadend;
					hullSegment = GridManager.I.DeadEndTiles.nextFree.transform;
					hullSegment.parent = hullcontainer;
					hullSegment.localRotation = Quaternion.identity;
				}
			}
		}
		else if (w)
		{
			if (tileType != TileType.Deadend)
			{
				RemoveHull();
				tileType = TileType.Deadend;
				hullSegment = GridManager.I.DeadEndTiles.nextFree.transform;
				hullSegment.parent = hullcontainer;
				hullSegment.localRotation = Quaternion.Euler(new Vector3(0,-180,0));
			}
		}
		else
		{
			if (tileType != TileType.Cell)
			{
				RemoveHull();
				tileType = TileType.Cell;
				hullSegment = GridManager.I.cellTiles.nextFree.transform;
				hullSegment.parent = hullcontainer;
				hullSegment.localRotation = Quaternion.identity;
			}
		}

		if (hullSegment != null)
		{
			// Reset the position and scale
			hullSegment.localScale = Vector3.one;
			hullSegment.localPosition = Vector3.zero;
		}
	}

	public void RemoveHull ()
	{
		if (hullSegment !=null)
		{
			hullSegment.parent = null;
			switch (tileType)
			{
			case TileType.Deadend:
					GridManager.I.DeadEndTiles.freeObject (hullSegment.gameObject);
				break;
			case TileType.FloorMid:
					GridManager.I.floorTiles.freeObject (hullSegment.gameObject);
				break;
			case TileType.Hallway:
					GridManager.I.HallwayTiles.freeObject (hullSegment.gameObject);
				break;
			case TileType.WallCorner:
					GridManager.I.wallCornerTiles.freeObject (hullSegment.gameObject);
				break;
			case TileType.WallStraignt:
					GridManager.I.wallStraightTiles.freeObject (hullSegment.gameObject);
				break;
			case TileType.Cell:
					GridManager.I.cellTiles.freeObject (hullSegment.gameObject);
				break;
			}
			
			tileType = TileType.None;
		}
	}
}


[System.Serializable]
public class Neighbour
{
	public Neighbour (Point gridoffset, Direction newdirection)
	{
		direction = newdirection;
		gridPositionOffset = gridoffset;
	}

	public enum Direction { North, Northeast, East, Southeast, South, Southwest, West, Northwest }
	public Direction direction;

	public Point gridPositionOffset;

	public Tile tile;
}
