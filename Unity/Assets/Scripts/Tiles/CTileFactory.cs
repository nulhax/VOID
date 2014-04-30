//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTileFactory.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/* Implementation */


public class CTileFactory : MonoBehaviour
{
	// Member Types
	public struct TTileIdentifier
	{
		public TTileIdentifier(CTile.EType _TileType, int _TileMetaType, int _TileVariant)
		{
			m_TileType = _TileType;
			m_TileMetaType = _TileMetaType;
			m_TileVariant = _TileVariant;
		}

		public CTile.EType m_TileType;
		public int m_TileMetaType;
		public int m_TileVariant;
	}
	
	// Member Delegates & Events

	
	// Member Fields
	public List<CTile_InteriorFloor.EType> m_FloorTileTypes = new List<CTile_InteriorFloor.EType>();
	public List<GameObject> m_FloorTilePrefabs = new List<GameObject>();

	public List<CTile_ExteriorWall.EType> m_WallExtTileTypes = new List<CTile_ExteriorWall.EType>();
	public List<GameObject> m_WallExtTilePrefabs = new List<GameObject>();

	public List<CTile_InteriorWall.EType> m_WallIntTileTypes = new List<CTile_InteriorWall.EType>();
	public List<GameObject> m_WallIntTilePrefabs = new List<GameObject>();

	public List<CTile_InteriorCeiling.EType> m_CeilingTileTypes = new List<CTile_InteriorCeiling.EType>();
	public List<GameObject> m_CeilingTilePrefabs = new List<GameObject>();

	public List<CTile_ExternalWallCap.EType> m_WallExtCapTileTypes = new List<CTile_ExternalWallCap.EType>();
	public List<GameObject> m_WallExtCapTilePrefabs = new List<GameObject>();

	public List<CTile_InteriorWallCap.EType> m_WallIntCapTileTypes = new List<CTile_InteriorWallCap.EType>();
	public List<GameObject> m_WallIntCapTilePrefabs = new List<GameObject>();

	private Dictionary<TTileIdentifier, GameObject> m_TilePrefabPairs = new Dictionary<TTileIdentifier, GameObject>();
	private Dictionary<TTileIdentifier, List<GameObject>> m_TileInstances = new Dictionary<TTileIdentifier, List<GameObject>>();


	// Member Properties
	
	
	// Member Methods
	public void Awake()
	{
		// Check miss matches
		if(m_FloorTileTypes.Count != m_FloorTilePrefabs.Count)
			Debug.LogError("Floor tile type -> Floor tile prefab mismatch.");

		if(m_WallExtTileTypes.Count != m_WallExtTilePrefabs.Count)
			Debug.LogError("Wall Ext tile type -> Wall Ext tile prefab mismatch.");

		if(m_WallIntTileTypes.Count != m_WallIntTilePrefabs.Count)
			Debug.LogError("Wall Int tile type -> Wall Int tile prefab mismatch.");

		if(m_CeilingTileTypes.Count != m_CeilingTilePrefabs.Count)
			Debug.LogError("Ceiling tile type -> Ceiling tile prefab mismatch.");

		if(m_WallExtCapTileTypes.Count != m_WallExtCapTilePrefabs.Count)
			Debug.LogError("Wall Ext Cap tile type -> Wall Ext Cap tile prefab mismatch.");

		if(m_WallIntCapTileTypes.Count != m_WallIntCapTilePrefabs.Count)
			Debug.LogError("Wall Int Cap tile type -> Wall Int Cap tile prefab mismatch.");

		// Fill floor tiles
		for(int i = 0; i < m_FloorTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(CTile.EType.InteriorFloor, (int)m_FloorTileTypes[i], 0);
			m_TilePrefabPairs[identifier] = m_FloorTilePrefabs[i];
		}

		// Fill Wall Ext tiles
		for(int i = 0; i < m_WallExtTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(CTile.EType.ExteriorWall, (int)m_WallExtTileTypes[i], 0);
			m_TilePrefabPairs[identifier] = m_WallExtTilePrefabs[i];
		}

		// Fill Wall Int tiles
		for(int i = 0; i < m_WallIntTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(CTile.EType.InteriorWall, (int)m_WallIntTileTypes[i], 0);
			m_TilePrefabPairs[identifier] = m_WallIntTilePrefabs[i];
		}

		// Fill Ceiling tiles
		for(int i = 0; i < m_CeilingTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(CTile.EType.InteriorCeiling, (int)m_CeilingTileTypes[i], 0);
			m_TilePrefabPairs[identifier] = m_CeilingTilePrefabs[i];
		}

		// Wall Ext Cap Ceiling tiles
		for(int i = 0; i < m_WallExtCapTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(CTile.EType.ExteriorWallCap, (int)m_WallExtCapTileTypes[i], 0);
			m_TilePrefabPairs[identifier] = m_WallExtCapTilePrefabs[i];
		}

		// Wall Int Cap Ceiling tiles
		for(int i = 0; i < m_WallIntCapTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(CTile.EType.InteriorWallCap, (int)m_WallIntCapTileTypes[i], 0);
			m_TilePrefabPairs[identifier] = m_WallIntCapTilePrefabs[i];
		}
	}

	public GameObject InstanceNewTile(CTile.EType _TileType, int _TileMetaType, int _TileVariant)
	{
		TTileIdentifier identifier = new TTileIdentifier(_TileType, _TileMetaType, _TileVariant);

		// Create new list for variant if it doesnt exist yet
		if(!m_TileInstances.ContainsKey(identifier))
			m_TileInstances[identifier] = new List<GameObject>();

		// Get the first instance of any free tile of this type
		GameObject tileObject = (from item in m_TileInstances[identifier]
		                         where item != null && !item.activeInHierarchy
		                         select item).FirstOrDefault();

		// If not found create a new instance
		if(tileObject == null)
		{
			tileObject = CreateTileInstance(identifier);
		}
		
		tileObject.SetActive(true);
		
		return(tileObject);
	}
	
	public void ReleaseTileObject(GameObject _TileToRelease)
	{
		_TileToRelease.transform.parent = transform;
		_TileToRelease.transform.localPosition = Vector3.zero;
		_TileToRelease.transform.localRotation = Quaternion.identity;
		_TileToRelease.transform.localScale = Vector3.one;
		_TileToRelease.gameObject.SetActive(false);
	}

	private GameObject CreateTileInstance(TTileIdentifier _TileIdentifier)
	{
		if(!m_TilePrefabPairs.ContainsKey(_TileIdentifier))
		{
			Debug.LogError("Tile identifier type was not found! :" + _TileIdentifier.m_TileType + ", " + _TileIdentifier.m_TileMetaType + ", " + _TileIdentifier.m_TileVariant);
			return null;
		}

		GameObject newObject = (GameObject)GameObject.Instantiate(m_TilePrefabPairs[_TileIdentifier]);
		newObject.transform.parent = transform;
		newObject.gameObject.SetActive(false);
		
		// Add it to list for later use
		m_TileInstances[_TileIdentifier].Add(newObject);

		return(newObject);
	}
}