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
	public List<CTile.ETileType> m_TileTypes = new List<CTile.ETileType>();
	public List<GameObject> m_TilePrefabs = new List<GameObject>();

	private Dictionary<CTile.ETileType, GameObject> m_TilePairs = new Dictionary<CTile.ETileType, GameObject>();
	private Dictionary<CTile.ETileType, List<GameObject>> m_TileLists = new Dictionary<CTile.ETileType, List<GameObject>>();

	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		if(m_TileTypes.Count != m_TilePrefabs.Count)
			Debug.LogError("Tile type -> Tile prefab mismatch.");

		for(int i = 0; i < m_TileTypes.Count; ++i)
		{
			m_TilePairs.Add(m_TileTypes[i], m_TilePrefabs[i]);
		}
	}

	public GameObject GetNextFreeTile(CTile.ETileType _TileType)
	{
		if(!m_TileLists.ContainsKey(_TileType))
			m_TileLists[_TileType] = new List<GameObject>();

		GameObject tileObject = (from item in m_TileLists[_TileType]
		                         where !item.activeInHierarchy
		                         select item).FirstOrDefault();

		// If not found create a new instance
		if(tileObject == null)
		{
			tileObject = CreateTileInstance(_TileType);
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

	private GameObject CreateTileInstance(CTile.ETileType _TileType)
	{
		GameObject newObject = (GameObject)GameObject.Instantiate(m_TilePairs[_TileType]);
		newObject.transform.parent = transform;
		newObject.gameObject.SetActive(false);
		
		// Add it to list for later use
		m_TileLists[_TileType].Add(newObject);

		return(newObject);
	}
}