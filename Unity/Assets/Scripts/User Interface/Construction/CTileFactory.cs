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
	public List<TTileMeta.EType> m_FloorTileTypes = new List<TTileMeta.EType>();
	public List<GameObject> m_FloorTilePrefabs = new List<GameObject>();

	public List<TTileMeta.EType> m_WallExtTileTypes = new List<TTileMeta.EType>();
	public List<GameObject> m_WallExtTilePrefabs = new List<GameObject>();

	public List<TTileMeta.EType> m_WallIntTileTypes = new List<TTileMeta.EType>();
	public List<GameObject> m_WallIntTilePrefabs = new List<GameObject>();

	public List<TTileMeta.EType> m_CeilingTileTypes = new List<TTileMeta.EType>();
	public List<GameObject> m_CeilingTilePrefabs = new List<GameObject>();

	private Dictionary<CTile.ETileType, Dictionary<TTileMeta.EType, GameObject>> m_TilePrefabPairs = 
		new Dictionary<CTile.ETileType, Dictionary<TTileMeta.EType, GameObject>>();

	private Dictionary<CTile.ETileType, Dictionary<TTileMeta.EType, List<GameObject>>> m_TileInstances = 
		new Dictionary<CTile.ETileType, Dictionary<TTileMeta.EType, List<GameObject>>>();


	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		// Create the dictionaries
		m_TilePrefabPairs[CTile.ETileType.Floor] = new Dictionary<TTileMeta.EType, GameObject>();
		m_TilePrefabPairs[CTile.ETileType.Wall_Ext] = new Dictionary<TTileMeta.EType, GameObject>();
		m_TilePrefabPairs[CTile.ETileType.Wall_Int] = new Dictionary<TTileMeta.EType, GameObject>();
		m_TilePrefabPairs[CTile.ETileType.Ceiling] = new Dictionary<TTileMeta.EType, GameObject>();
		m_TileInstances[CTile.ETileType.Floor] = new Dictionary<TTileMeta.EType, List<GameObject>>();
		m_TileInstances[CTile.ETileType.Wall_Ext] = new Dictionary<TTileMeta.EType, List<GameObject>>();
		m_TileInstances[CTile.ETileType.Wall_Int] = new Dictionary<TTileMeta.EType, List<GameObject>>();
		m_TileInstances[CTile.ETileType.Ceiling] = new Dictionary<TTileMeta.EType, List<GameObject>>();

		// Check miss matches
		if(m_FloorTileTypes.Count != m_FloorTilePrefabs.Count)
			Debug.LogError("Floor tile type -> Floor tile prefab mismatch.");

		if(m_WallExtTileTypes.Count != m_WallExtTilePrefabs.Count)
			Debug.LogError("Wall Ext tile type -> Wall Ext tile prefab mismatch.");

		if(m_WallIntTileTypes.Count != m_WallIntTilePrefabs.Count)
			Debug.LogError("Wall Int tile type -> Wall Int tile prefab mismatch.");

		if(m_WallIntTileTypes.Count != m_WallIntTilePrefabs.Count)
			Debug.LogError("Ceiling tile type -> Ceiling tile prefab mismatch.");

		// Fill floor tiles
		for(int i = 0; i < m_FloorTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[CTile.ETileType.Floor].Add(m_FloorTileTypes[i], m_FloorTilePrefabs[i]);
		}

		// Fill wall exterior tiles
		for(int i = 0; i < m_WallExtTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[CTile.ETileType.Wall_Ext].Add(m_WallExtTileTypes[i], m_WallExtTilePrefabs[i]);
		}

		// Fill wall Interior tiles
		for(int i = 0; i < m_WallIntTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[CTile.ETileType.Wall_Int].Add(m_WallIntTileTypes[i], m_WallIntTilePrefabs[i]);
		}

		// Fill Ceiling tiles
		for(int i = 0; i < m_CeilingTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[CTile.ETileType.Ceiling].Add(m_CeilingTileTypes[i], m_CeilingTilePrefabs[i]);
		}
	}

	public GameObject NewTile(CTile.ETileType _TileType, TTileMeta.EType _MetaType)
	{
		// Create new list if it doesnt exist yet
		if(!m_TileInstances[_TileType].ContainsKey(_MetaType))
			m_TileInstances[_TileType][_MetaType] = new List<GameObject>();

		// Get the first instance of any free tile of this type
		GameObject tileObject = (from item in m_TileInstances[_TileType][_MetaType]
		                         where item != null && !item.activeInHierarchy
		                         select item).FirstOrDefault();

		// If not found create a new instance
		if(tileObject == null)
		{
			tileObject = CreateTileInstance(_TileType, _MetaType);
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

	private GameObject CreateTileInstance(CTile.ETileType _TileType, TTileMeta.EType _MetaType)
	{
		if(!m_TilePrefabPairs.ContainsKey(_TileType))
			Debug.LogError("Tile Prefab type was not found! :" + _TileType);

		if(!m_TilePrefabPairs[_TileType].ContainsKey(_MetaType))
			Debug.LogError("Tile Prefab meta type prefab was not found! :" + _MetaType + " From: " + _TileType);

		GameObject newObject = (GameObject)GameObject.Instantiate(m_TilePrefabPairs[_TileType][_MetaType]);
		newObject.transform.parent = transform;
		newObject.gameObject.SetActive(false);
		
		// Add it to list for later use
		m_TileInstances[_TileType][_MetaType].Add(newObject);

		return(newObject);
	}
}