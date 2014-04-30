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
using System;


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
	public GameObject m_TilesPrefab = null;

	private Dictionary<TTileIdentifier, GameObject> m_TilePrefabPairs = new Dictionary<TTileIdentifier, GameObject>();
	private Dictionary<TTileIdentifier, List<GameObject>> m_TileInstances = new Dictionary<TTileIdentifier, List<GameObject>>();


	// Member Properties
	
	
	// Member Methods
	public void Awake()
	{
		// Gather all tile types
		List<CTile.EType> tileTypes = new List<CTile.EType>();
		for(int i = (int)CTile.EType.INVALID + 1; i < (int)CTile.EType.MAX; ++i)
			tileTypes.Add((CTile.EType)i);

		// Iterate all tile types to get prefabs from container
		foreach(CTile.EType tileType in tileTypes)
		{
			Type tileClassType = CTile.GetTileClassType(tileType);
			Type tileMetaTypes = tileClassType.GetNestedType("EType");

			// Fill floor tiles
			foreach(var value in Enum.GetValues(tileMetaTypes))
			{
				if((int)value == 0)
					continue;

				Transform tile = m_TilesPrefab.transform.FindChild(tileType + "_" + value);
				if(tile == null)
				{
					Debug.LogError("Tile was not found! Type: " + tileType + " MetaType: " + value);
					continue;
				}

				TTileIdentifier identifier = new TTileIdentifier(tileType, (int)value, 0);
				m_TilePrefabPairs[identifier] = tile.gameObject;
			}
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