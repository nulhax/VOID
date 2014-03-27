//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTile.cs
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


/* Implementation */


[System.Serializable]
public class CTile : CGridObject 
{
	// Member Types
	public enum ETileType 
	{ 
		INVALID = -1,

		None, 
		FloorMid, 
		WallCorner, 
		WallStraignt, 
		WallInverse,
		Hallway, 
		Deadend, 
		Cell,

		MAX
	}

	private struct TTileMetaInfo
	{
		public TTileMetaInfo(ETileType _Type, int _Identifier, Quaternion _Rotation)
		{
			m_TileIdentifier = _Identifier;
			m_TileType = _Type;
			m_TileRotation = _Rotation;
		}

		public int m_TileIdentifier;
		public ETileType m_TileType;
		public Quaternion m_TileRotation;
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	public ETileType m_TileType;
	public GameObject m_Tile;
	public List<CNeighbour> m_NeighbourHood;

	public int m_TileMetaIdentifier = 0;
	private GameObject m_TileObject;
	
	static Dictionary<int, TTileMetaInfo> s_TileMetaInfo = new Dictionary<int, TTileMetaInfo>();


	// Member Properties
	
	
	// Member Methods
	public CTile (int x, int y, int z)
		: base(x, y, z)
	{
		// If the dictionary is empty we need to fill it
		if(s_TileMetaInfo.Count == 0)
		{
			FillTileMetaData();
		}
	}

	private static void FillTileMetaData()
	{
		// Add every combination of possible tile layout

		AddMetaDataEntry(ETileType.Cell, 				0, 			Quaternion.identity);

		AddMetaDataEntry(ETileType.FloorMid, 			255, 		Quaternion.identity);

		AddMetaDataEntry(ETileType.WallCorner, 			112, 		Quaternion.identity);
		AddMetaDataEntry(ETileType.WallCorner, 			248, 		Quaternion.identity);

		AddMetaDataEntry(ETileType.WallCorner, 			193, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallCorner, 			227, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));

		AddMetaDataEntry(ETileType.WallCorner, 			7, 			Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallCorner, 			143, 		Quaternion.Euler(0.0f, 180.0f, 0.0f));

		AddMetaDataEntry(ETileType.WallCorner, 			28, 		Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallCorner, 			62, 		Quaternion.Euler(0.0f, 270.0f, 0.0f));

		AddMetaDataEntry(ETileType.WallStraignt, 		124, 		Quaternion.identity);
		AddMetaDataEntry(ETileType.WallStraignt, 		241, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallStraignt, 		199, 		Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallStraignt, 		31, 		Quaternion.Euler(0.0f, 270.0f, 0.0f));

		AddMetaDataEntry(ETileType.WallStraignt, 		126, 		Quaternion.identity);
		AddMetaDataEntry(ETileType.WallStraignt, 		252, 		Quaternion.identity);

		AddMetaDataEntry(ETileType.WallStraignt, 		243, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallStraignt, 		249, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));

		AddMetaDataEntry(ETileType.WallStraignt, 		231, 		Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallStraignt, 		207, 		Quaternion.Euler(0.0f, 180.0f, 0.0f));

		AddMetaDataEntry(ETileType.WallStraignt, 		63, 		Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallStraignt, 		159, 		Quaternion.Euler(0.0f, 270.0f, 0.0f));

		AddMetaDataEntry(ETileType.WallInverse, 		253, 		Quaternion.identity);
		AddMetaDataEntry(ETileType.WallInverse, 		247, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallInverse, 		223, 		Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddMetaDataEntry(ETileType.WallInverse, 		127, 		Quaternion.Euler(0.0f, 270.0f, 0.0f));

		AddMetaDataEntry(ETileType.Hallway, 			68, 		Quaternion.identity);
		AddMetaDataEntry(ETileType.Hallway, 			228, 		Quaternion.identity);
		AddMetaDataEntry(ETileType.Hallway, 			78, 		Quaternion.identity);

		AddMetaDataEntry(ETileType.Hallway, 			17, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddMetaDataEntry(ETileType.Hallway, 			147, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddMetaDataEntry(ETileType.Hallway, 			57, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));

		AddMetaDataEntry(ETileType.Deadend, 			4, 			Quaternion.identity);
		AddMetaDataEntry(ETileType.Deadend, 			14, 			Quaternion.identity);

		AddMetaDataEntry(ETileType.Deadend, 			16, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));
		AddMetaDataEntry(ETileType.Deadend, 			56, 		Quaternion.Euler(0.0f, 90.0f, 0.0f));

		AddMetaDataEntry(ETileType.Deadend, 			64, 		Quaternion.Euler(0.0f, 180.0f, 0.0f));
		AddMetaDataEntry(ETileType.Deadend, 			224, 		Quaternion.Euler(0.0f, 180.0f, 0.0f));

		AddMetaDataEntry(ETileType.Deadend, 			1, 			Quaternion.Euler(0.0f, 270.0f, 0.0f));
		AddMetaDataEntry(ETileType.Deadend, 			131, 		Quaternion.Euler(0.0f, 270.0f, 0.0f));
	}

	private static void AddMetaDataEntry(ETileType _Type, int _Identifier, Quaternion _Rotation)
	{
		s_TileMetaInfo.Add(_Identifier, new TTileMetaInfo(_Type, _Identifier, _Rotation));
	}

	public void FindNeighbours()
	{
		m_NeighbourHood = new List<CNeighbour>();

		List <CNeighbour> neighbours = new List<CNeighbour> ();
		
		List <CNeighbour> possibleNeighbours = allNeighbours;
		
		foreach (CNeighbour pn in possibleNeighbours) {
			CTile possibleNeightbour = new CTile (X + pn.gridPositionOffset.X, 
			                                    Y + pn.gridPositionOffset.Y, 
			                                    Z + pn.gridPositionOffset.Z);
			
			if (CGrid.I.m_GridBoard.ContainsKey(possibleNeightbour.ToString()))
			{
				CNeighbour newNeightbour = new CNeighbour(pn.gridPositionOffset, pn.direction);
				newNeightbour.tile = CGrid.I.m_GridBoard[possibleNeightbour.ToString()].tile;
				neighbours.Add (newNeightbour);
			}
		}
		
		if (neighbours.Count > 0)
			m_NeighbourHood = neighbours;

		UpdateTileType();
	}
	
	//change of coordinates when moving in any direction
	public List<CNeighbour> allNeighbours {
		get {
			return new List<CNeighbour>
			{
				new CNeighbour (new Point(0, 0, 1), CNeighbour.Direction.North),
				new CNeighbour (new Point(1, 0, 1), CNeighbour.Direction.Northeast),
				new CNeighbour (new Point(1, 0, 0), CNeighbour.Direction.East),
				new CNeighbour (new Point(1, 0, -1), CNeighbour.Direction.Southeast),
				new CNeighbour (new Point(0, 0, -1), CNeighbour.Direction.South),
				new CNeighbour (new Point(-1, 0, 1), CNeighbour.Direction.Northwest),
				new CNeighbour (new Point(-1, 0, 0), CNeighbour.Direction.West),
				new CNeighbour (new Point(-1, 0, -1), CNeighbour.Direction.Southwest),
			};
		}
	}

	public void UpdateNeighbours()
	{
		foreach (CNeighbour neighbour in m_NeighbourHood)
		{
			neighbour.tile.FindNeighbours();
		}
	}

	public void UpdateTileObject()
	{
		RemoveExistingTileObject();

		Quaternion rotation = Quaternion.identity;
		ETileType type = ETileType.FloorMid;

//		// Find the meta data for this tile
//		if(s_TileMetaInfo.ContainsKey(m_TileMetaIdentifier))
//		{
//			type = s_TileMetaInfo[m_TileMetaIdentifier].m_TileType;
//			rotation = s_TileMetaInfo[m_TileMetaIdentifier].m_TileRotation;
//		}
//		else
//		{
//			// if it wasn't found there was no meta data found for it
//			Debug.LogWarning("Tile Meta wasn't found for this tile within its neighbourhood. Reverted to floor mid piece instead.");
//		}

		m_TileType = type;
		m_TileObject = CGrid.I.m_TileFactory.GetNextFreeTile(m_TileType);
		m_TileObject.transform.parent = m_Tile.transform;
		m_TileObject.transform.localPosition = Vector3.zero;
		m_TileObject.transform.localScale = Vector3.one;
		m_TileObject.transform.localRotation = rotation;
	}

	public void UpdateTileType()
	{
		m_TileMetaIdentifier = 0;
		foreach (CNeighbour neighbour in m_NeighbourHood)
		{
			m_TileMetaIdentifier |= 1 << (int)neighbour.direction;
		}

		UpdateTileObject();
	}

	public void RemoveExistingTileObject ()
	{
		if (m_TileObject != null)
		{
			CGrid.I.m_TileFactory.ReleaseTileObject(m_TileObject.gameObject);
			m_TileType = ETileType.None;
		}
	}
}
