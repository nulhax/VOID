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
		public TTileIdentifier(ETileType _TileType, ETileMetaType _TileMetaType, ETileVariant _TileVariant)
		{
			m_TileType = _TileType;
			m_TileMetaType = _TileMetaType;
			m_TileVariant = _TileVariant;
		}

		public ETileType m_TileType;
		public ETileMetaType m_TileMetaType;
		public ETileVariant m_TileVariant;
	}
	
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

	public List<ETileMetaType> m_WallIntCapTileTypes = new List<ETileMetaType>();
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

		if(m_WallExtTileTypes_Opening.Count != m_WallExtTilePrefabs_Opening.Count)
			Debug.LogError("Wall Ext Door tile type -> Wall Ext Door tile prefab mismatch.");

		// Fill floor tiles
		for(int i = 0; i < m_FloorTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(ETileType.Floor, m_FloorTileTypes[i], ETileVariant.Default);
			m_TilePrefabPairs[identifier] = m_FloorTilePrefabs[i];
		}

		// Fill Wall Ext tiles
		for(int i = 0; i < m_WallExtTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(ETileType.Wall_Ext, m_WallExtTileTypes[i], ETileVariant.Default);
			m_TilePrefabPairs[identifier] = m_WallExtTilePrefabs[i];
		}

		//  Fill Wall Ext Opening tiles
		for(int i = 0; i < m_WallExtTileTypes_Opening.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(ETileType.Wall_Ext, m_WallExtTileTypes[i], ETileVariant.Opening);
			m_TilePrefabPairs[identifier] = m_WallExtTilePrefabs_Opening[i];
		}

		// Fill Wall Int tiles
		for(int i = 0; i < m_WallIntTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(ETileType.Wall_Int, m_WallIntTileTypes[i], ETileVariant.Default);
			m_TilePrefabPairs[identifier] = m_WallIntTilePrefabs[i];
		}

		// Fill Ceiling tiles
		for(int i = 0; i < m_CeilingTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(ETileType.Ceiling, m_CeilingTileTypes[i], ETileVariant.Default);
			m_TilePrefabPairs[identifier] = m_CeilingTilePrefabs[i];
		}

		// Wall Ext Cap Ceiling tiles
		for(int i = 0; i < m_WallExtCapTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(ETileType.Wall_Ext_Cap, m_WallExtCapTileTypes[i], ETileVariant.Default);
			m_TilePrefabPairs[identifier] = m_WallExtCapTilePrefabs[i];
		}

		// Wall Int Cap Ceiling tiles
		for(int i = 0; i < m_WallIntCapTileTypes.Count; ++i)
		{
			TTileIdentifier identifier = new TTileIdentifier(ETileType.Wall_Int_Cap, m_WallIntCapTileTypes[i], ETileVariant.Default);
			m_TilePrefabPairs[identifier] = m_WallIntCapTilePrefabs[i];
		}
	}

	public GameObject InstanceNewTile(ETileType _TileType, ETileMetaType _TileMetaType, ETileVariant _TileVariant)
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