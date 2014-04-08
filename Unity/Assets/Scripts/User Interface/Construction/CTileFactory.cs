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
	public List<ETileMetaType> m_FloorTileTypes = new List<ETileMetaType>();
	public List<GameObject> m_FloorTilePrefabs = new List<GameObject>();

	public List<ETileMetaType> m_WallExtTileTypes = new List<ETileMetaType>();
	public List<GameObject> m_WallExtTilePrefabs = new List<GameObject>();

	public List<ETileMetaType> m_WallExtTileTypes_Opening = new List<ETileMetaType>();
	public List<GameObject> m_WallExtTilePrefabs_Opening = new List<GameObject>();

	public List<ETileMetaType> m_WallIntTileTypes = new List<ETileMetaType>();
	public List<GameObject> m_WallIntTilePrefabs = new List<GameObject>();

	public List<ETileMetaType> m_CeilingTileTypes = new List<ETileMetaType>();
	public List<GameObject> m_CeilingTilePrefabs = new List<GameObject>();

	public List<ETileMetaType> m_WallExtCapTileTypes = new List<ETileMetaType>();
	public List<GameObject> m_WallExtCapTilePrefabs = new List<GameObject>();

	private Dictionary<ETileType, Dictionary<ETileMetaType, Dictionary<ETileVariant, GameObject>>> m_TilePrefabPairs = 
		new Dictionary<ETileType, Dictionary<ETileMetaType, Dictionary<ETileVariant, GameObject>>>();

	private Dictionary<ETileType, Dictionary<ETileMetaType, Dictionary<ETileVariant, List<GameObject>>>> m_TileInstances = 
		new Dictionary<ETileType, Dictionary<ETileMetaType, Dictionary<ETileVariant, List<GameObject>>>>();


	// Member Properties
	
	
	// Member Methods
	public void Awake()
	{
		for(int i = (int)ETileType.INVALID; i < (int)ETileType.MAX; ++i)
		{
			m_TilePrefabPairs[(ETileType)i] = new Dictionary<ETileMetaType, Dictionary<ETileVariant, GameObject>>();
			m_TileInstances[(ETileType)i] = new Dictionary<ETileMetaType, Dictionary<ETileVariant, List<GameObject>>>();
		}

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

		if(m_WallExtTileTypes_Opening.Count != m_WallExtTilePrefabs_Opening.Count)
			Debug.LogError("Wall Ext Door tile type -> Wall Ext Door tile prefab mismatch.");

		// Fill floor tiles
		for(int i = 0; i < m_FloorTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[ETileType.Floor].Add(m_FloorTileTypes[i], new Dictionary<ETileVariant, GameObject>());
			m_TilePrefabPairs[ETileType.Floor][m_FloorTileTypes[i]].Add(ETileVariant.Default, m_FloorTilePrefabs[i]);
		}

		// Fill Wall Ext tiles
		for(int i = 0; i < m_WallExtTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[ETileType.Wall_Ext].Add(m_WallExtTileTypes[i], new Dictionary<ETileVariant, GameObject>());
			m_TilePrefabPairs[ETileType.Wall_Ext][m_WallExtTileTypes[i]].Add(ETileVariant.Default, m_WallExtTilePrefabs[i]);
		}

		//  Fill Wall Ext Opening tiles
		for(int i = 0; i < m_WallExtTileTypes_Opening.Count; ++i)
		{
			m_TilePrefabPairs[ETileType.Wall_Ext][m_WallExtTileTypes_Opening[i]].Add(ETileVariant.Opening, m_WallExtTilePrefabs_Opening[i]);
		}

		// Fill Wall Int tiles
		for(int i = 0; i < m_WallIntTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[ETileType.Wall_Int].Add(m_WallIntTileTypes[i], new Dictionary<ETileVariant, GameObject>());
			m_TilePrefabPairs[ETileType.Wall_Int][m_WallIntTileTypes[i]].Add(ETileVariant.Default, m_WallIntTilePrefabs[i]);
		}

		// Fill Ceiling tiles
		for(int i = 0; i < m_CeilingTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[ETileType.Ceiling].Add(m_CeilingTileTypes[i], new Dictionary<ETileVariant, GameObject>());
			m_TilePrefabPairs[ETileType.Ceiling][m_CeilingTileTypes[i]].Add(ETileVariant.Default, m_CeilingTilePrefabs[i]);
		}

		// Wall Ext Cap Ceiling tiles
		for(int i = 0; i < m_WallExtCapTileTypes.Count; ++i)
		{
			m_TilePrefabPairs[ETileType.Wall_Ext_Cap].Add(m_WallExtCapTileTypes[i], new Dictionary<ETileVariant, GameObject>());
			m_TilePrefabPairs[ETileType.Wall_Ext_Cap][m_WallExtCapTileTypes[i]].Add(ETileVariant.Default, m_WallExtCapTilePrefabs[i]);
		}
	}

	public GameObject InstanceNewTile(ETileType _TileType, ETileMetaType _TileMetaType, ETileVariant _TileVariant)
	{
		// Create new dictionary for meta type if it doesnt exist yet
		if(!m_TileInstances[_TileType].ContainsKey(_TileMetaType))
			m_TileInstances[_TileType][_TileMetaType] = new Dictionary<ETileVariant, List<GameObject>>();

		// Create new list for variant if it doesnt exist yet
		if(!m_TileInstances[_TileType][_TileMetaType].ContainsKey(_TileVariant))
			m_TileInstances[_TileType][_TileMetaType][_TileVariant] = new List<GameObject>();

		// Get the first instance of any free tile of this type
		GameObject tileObject = (from item in m_TileInstances[_TileType][_TileMetaType][_TileVariant]
		                         where item != null && !item.activeInHierarchy
		                         select item).FirstOrDefault();

		// If not found create a new instance
		if(tileObject == null)
		{
			tileObject = CreateTileInstance(_TileType, _TileMetaType, _TileVariant);
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

	private GameObject CreateTileInstance(ETileType _TileType, ETileMetaType _TileMetaType, ETileVariant _TileVariant)
	{
		if(!m_TilePrefabPairs.ContainsKey(_TileType))
		{
			Debug.LogError("Tile Prefab type was not found! :" + _TileType);
			return null;
		}

		if(!m_TilePrefabPairs[_TileType].ContainsKey(_TileMetaType))
		{
			Debug.LogError("Tile Prefab meta type was not found! :" + _TileMetaType + " From: " + _TileType);
			return null;
		}

		if(!m_TilePrefabPairs[_TileType][_TileMetaType].ContainsKey(_TileVariant))
		{
			Debug.LogError("Tile Prefab meta type variant was not found! :" + _TileVariant + " From: " + _TileMetaType + ", " + _TileType);
			return null;
		}

		GameObject newObject = (GameObject)GameObject.Instantiate(m_TilePrefabPairs[_TileType][_TileMetaType][_TileVariant]);
		newObject.transform.parent = transform;
		newObject.gameObject.SetActive(false);
		
		// Add it to list for later use
		m_TileInstances[_TileType][_TileMetaType][_TileVariant].Add(newObject);

		return(newObject);
	}
}