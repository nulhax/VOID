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
	
	
	// Member Delegates & Events

	
	// Member Fields
	public List<CTile.TFloorTileMeta.EType> m_FloorTileTypes = new List<CTile.TFloorTileMeta.EType>();
	public List<GameObject> m_FloorTilePrefabs = new List<GameObject>();

	public List<CTile.TWallTileMeta.EType> m_WallTileTypes = new List<CTile.TWallTileMeta.EType>();
	public List<GameObject> m_WallTilePrefabs = new List<GameObject>();

	private Dictionary<CTile.TFloorTileMeta.EType, GameObject> m_FloorTilePairs = new Dictionary<CTile.TFloorTileMeta.EType, GameObject>();
	private Dictionary<CTile.TFloorTileMeta.EType, List<GameObject>> m_FloorTileLists = new Dictionary<CTile.TFloorTileMeta.EType, List<GameObject>>();

	private Dictionary<CTile.TWallTileMeta.EType, GameObject> m_WallTilePairs = new Dictionary<CTile.TWallTileMeta.EType, GameObject>();
	private Dictionary<CTile.TWallTileMeta.EType, List<GameObject>> m_WallTileLists = new Dictionary<CTile.TWallTileMeta.EType, List<GameObject>>();
	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		if(m_FloorTileTypes.Count != m_FloorTilePrefabs.Count)
			Debug.LogError("Floor tile type -> Floor tile prefab mismatch.");

		for(int i = 0; i < m_FloorTileTypes.Count; ++i)
		{
			m_FloorTilePairs.Add(m_FloorTileTypes[i], m_FloorTilePrefabs[i]);
		}

		if(m_WallTileTypes.Count != m_WallTilePrefabs.Count)
			Debug.LogError("Wall tile type -> Wall tile prefab mismatch.");

		for(int i = 0; i < m_WallTileTypes.Count; ++i)
		{
			m_WallTilePairs.Add(m_WallTileTypes[i], m_WallTilePrefabs[i]);
		}
	}

	public GameObject NewFloorTile(CTile.TFloorTileMeta.EType _FloorTileType)
	{
		if(!m_FloorTileLists.ContainsKey(_FloorTileType))
			m_FloorTileLists[_FloorTileType] = new List<GameObject>();

		GameObject tileObject = (from item in m_FloorTileLists[_FloorTileType]
		                         where item != null && !item.activeInHierarchy
		                         select item).FirstOrDefault();

		// If not found create a new instance
		if(tileObject == null)
		{
			tileObject = CreateFloorTileInstance(_FloorTileType);
		}
		
		tileObject.SetActive(true);
		
		return(tileObject);
	}

	public GameObject NewWallTile(CTile.TWallTileMeta.EType _WallTileType)
	{
		if(!m_WallTileLists.ContainsKey(_WallTileType))
			m_WallTileLists[_WallTileType] = new List<GameObject>();
		
		GameObject tileObject = (from item in m_WallTileLists[_WallTileType]
		                         where item != null && !item.activeInHierarchy
		                         select item).FirstOrDefault();
		
		// If not found create a new instance
		if(tileObject == null)
		{
			tileObject = CreateWallTileInstance(_WallTileType);
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

	private GameObject CreateFloorTileInstance(CTile.TFloorTileMeta.EType _FloorTileType)
	{
		GameObject newObject = (GameObject)GameObject.Instantiate(m_FloorTilePairs[_FloorTileType]);
		newObject.transform.parent = transform;
		newObject.gameObject.SetActive(false);
		
		// Add it to list for later use
		m_FloorTileLists[_FloorTileType].Add(newObject);

		return(newObject);
	}

	private GameObject CreateWallTileInstance(CTile.TWallTileMeta.EType _WallTileType)
	{
		GameObject newObject = (GameObject)GameObject.Instantiate(m_WallTilePairs[_WallTileType]);
		newObject.transform.parent = transform;
		newObject.gameObject.SetActive(false);
		
		// Add it to list for later use
		m_WallTileLists[_WallTileType].Add(newObject);
		
		return(newObject);
	}
}