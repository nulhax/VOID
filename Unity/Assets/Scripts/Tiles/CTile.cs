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
using System.Linq;


/* Implementation */


public abstract class CTile : MonoBehaviour
{
	// Member Types
	public enum EType
	{
		INVALID = -1,
		
		Interior_Floor,
		Interior_Floor_Inverse_Corner,
		Interior_Wall,
		Interior_Wall_Inverse_Corner,
		Interior_Ceiling,
		Interior_Ceiling_Inverse_Corner,
		Exterior_Wall,
		Exterior_Wall_Inverse_Corner,
		Exterior_Upper,
		Exterior_Upper_Inverse_Corner,
		Exterior_Lower,
		Exterior_Lower_Inverse_Corner,

		MAX
	}

	[System.Serializable]
	public class CModification
	{
		public CModification(int _Modification, EDirection _WorldSide, EDirection _LocalSide)
		{
			m_Modification = _Modification;
			m_WorldSide = _WorldSide;
			m_LocalSide = _LocalSide;
		}

		public int m_Modification;
		public EDirection m_WorldSide;
		public EDirection m_LocalSide;
		
		public override bool Equals(System.Object obj)
		{
			// If parameter is null return false.
			if (obj == null)
				return false;
			
			// If parameter cannot be cast to Point return false.
			CModification p = (CModification)obj;
			if ((System.Object)p == null)
				return false;
			
			// Return true if the fields match:
			return ((m_Modification == p.m_Modification) &&
			        (m_WorldSide == p.m_WorldSide) &&
			        (m_LocalSide == p.m_LocalSide));
		}
		
		public bool Equals(CModification p)
		{
			// If parameter is null return false:
			if ((object)p == null)
				return false;
			
			// Return true if the fields match:
			return ((m_Modification == p.m_Modification) &&
			        (m_WorldSide == p.m_WorldSide) &&
			        (m_LocalSide == p.m_LocalSide));
		}
	}

	[System.Serializable]
	public class CMeta
	{
		public static CMeta Default
		{
			get { return(new CMeta(-1, 0)); }
		}

		public CMeta(int _TileMask, int _MetaType)
		{
			m_TileMask = _TileMask;
			m_MetaType = _MetaType;
			m_Rotations = 0;
			m_ModificationMask = 0;
		}

		public CMeta(CMeta _Other)
		{
			m_TileMask = _Other.m_TileMask;
			m_MetaType = _Other.m_MetaType;
			m_Rotations = _Other.m_Rotations;
			m_ModificationMask = _Other.m_ModificationMask;
		}

		public int m_TileMask;
		public int m_MetaType;
		public int m_Rotations;
		public int m_ModificationMask;

		public override bool Equals(System.Object obj)
		{
			// If parameter is null return false.
			if (obj == null)
				return false;
			
			// If parameter cannot be cast to Point return false.
			CMeta p = (CMeta)obj;
			if ((System.Object)p == null)
				return false;
			
			// Return true if the fields match:
			return ((m_TileMask == p.m_TileMask) &&
			        (m_MetaType == p.m_MetaType) &&
			        (m_Rotations == p.m_Rotations) &&
			        (m_ModificationMask == p.m_ModificationMask));
		}

		public bool Equals(CMeta p)
		{
			// If parameter is null return false:
			if ((object)p == null)
				return false;
			
			// Return true if the fields match:
			return ((m_TileMask == p.m_TileMask) && 
			        (m_MetaType == p.m_MetaType) &&
			        (m_Rotations == p.m_Rotations) &&
			        (m_ModificationMask == p.m_ModificationMask));
		}
	}

	// Member Delegates & Events
	public delegate void HandleTileEvent(CTile _Self);

	public event HandleTileEvent EventTileObjectChanged;

	
	// Member Fields
	public EType m_TileType = EType.INVALID;

	public CMeta m_ActiveTileMeta = CMeta.Default;
	public CMeta m_CurrentTileMeta = CMeta.Default;
	
	public CTileInterface m_TileInterface = null;
	public GameObject m_TileObject = null;

	public List<EDirection> m_NeighbourExemptions = new List<EDirection>();
	public List<CModification> m_Modifications = new List<CModification>();


	// Member Properties
	public CTileInterface TileInterface
	{
		set { m_TileInterface = value; }
	}

	public abstract Dictionary<int, CTile.CMeta> TileMetaDictionary
	{
		get;
	}

	public abstract List<EDirection> RelevantDirections
	{
		get;
	}


	// Member Methods
	protected void Update()
	{
		if(m_ActiveTileMeta.Equals(m_CurrentTileMeta))
			return;

		// Update the visible tile object
		UpdateTileObject();

		// Invoke network RPC to update current meta for all clients
		if(CNetwork.IsServer)
			m_TileInterface.InvokeTileCurrentMetaUpdate(this);
	}

	protected void OnDestroy()
	{
		if(m_TileObject != null)
			ReleaseTileObject();
	}

	[ContextMenu("Update Current Tile Meta Data")]
	[AServerOnly]
	public void UpdateCurrentTileMetaData()
	{
		int tileMask = DetirmineTileMask();
		
		// Update the tile meta info
		bool metaChanged = RetrieveTileMetaEntry(tileMask);
		
		// Return if the meta data hasn't changed
		if(!metaChanged)
			return;

		// Invoke neighbours to update their tile meta data
		foreach(CNeighbour neighbour in m_TileInterface.m_NeighbourHood)
		{
			//neighbour.m_TileInterface.UpdateAllCurrentTileMetaData();
		}
	}

	protected abstract int DetirmineTileMask();

	[AServerOnly]
	protected bool RetrieveTileMetaEntry(int _TileMask)
	{	
		bool currentMetaChanged = false;

		// Find the tile mask out of possible 4 rotations
		for(int i = 0; i < 4; ++i)
		{
			int tileMask = 0;
			for(int j = 0; j < (int)EDirection.MAX; ++j)
			{
				// Define a new mask based on neighbours that exist in the mask
				if((_TileMask & (1 << j)) != 0)
				{
					// "Rotate" the mask by 90*, 2 bits ac for each rotation
					int dirMask = j - (2 * i);
					if(dirMask < 0)
						dirMask += 8;
					
					tileMask |= 1 << dirMask;
				}
			}
			
			// Continue if it was not a result
			if(!TileMetaDictionary.ContainsKey(tileMask))
				continue;

			// Get the meta entry for the result with a correct rotation
			CTile.CMeta newTileMeta = new CMeta(TileMetaDictionary[tileMask].m_TileMask, TileMetaDictionary[tileMask].m_MetaType);
			newTileMeta.m_Rotations = i;
			newTileMeta.m_ModificationMask = m_CurrentTileMeta.m_ModificationMask;
			
			// Update the current tile meta data
			currentMetaChanged = !m_CurrentTileMeta.Equals(newTileMeta);
			m_CurrentTileMeta = newTileMeta;

			return(currentMetaChanged);
		}

		return(false);

		Debug.LogWarning("Tile Meta data wasn't found for: " + m_TileType + " Mask: " + _TileMask);
	}

	[ContextMenu("Update Tile Object")]
	public void UpdateTileObject()
	{
		// Release the tile object
		ReleaseTileObject();
		
		// Create the new tile type object as long as it is not marked as none ("0")
		if(m_CurrentTileMeta.m_MetaType != 0)
		{
			m_TileObject = m_TileInterface.m_Grid.TileFactory.InstanceNewTile(m_TileType, m_CurrentTileMeta.m_MetaType, 0);
			m_TileObject.transform.parent = transform;
			m_TileObject.transform.localPosition = Vector3.zero;
			m_TileObject.transform.localScale = Vector3.one;
			m_TileObject.transform.localRotation = Quaternion.Euler(0.0f, m_CurrentTileMeta.m_Rotations * 90.0f, 0.0f);

			UpdateTileModifications();

			if(EventTileObjectChanged != null)
				EventTileObjectChanged(this);
		}
		
		// Update the tiles active meta data
		m_ActiveTileMeta = new CTile.CMeta(m_CurrentTileMeta);
	}

	protected void UpdateTileModifications()
	{
		Type classType = GetTileClassType(m_TileType);
		Type enumModType = classType.GetNestedType("EModification");

		if(enumModType == null)
			return;
		
		var enumModValues = Enum.GetValues(enumModType);
		m_Modifications = GetCurrentModificationsFromMask(enumModValues);

		List<EDirection> defaultSides = new List<EDirection>(RelevantDirections);
		foreach(CModification mod in m_Modifications)
			defaultSides.Remove(mod.m_WorldSide);

		foreach(EDirection side in RelevantDirections)
		{
			GameObject child = GetModificationObject(0, GetUnrotatedDirection(side));
			
			if(child != null)
			{
				if(defaultSides.Contains(side))
					child.gameObject.SetActive(true);
				else
					child.gameObject.SetActive(false);
			}
			
			foreach(var modType in enumModValues)
			{
				if((int)modType == 0)
					continue;

				child = GetModificationObject((int)modType, GetUnrotatedDirection(side));
				
				if(child != null)
				{
					if(m_Modifications.Exists(m => m.m_WorldSide == side && m.m_Modification == (int)modType))
						child.gameObject.SetActive(true);
					else
						child.gameObject.SetActive(false);
				}
			}
		}
	}

	protected List<CModification> GetCurrentModificationsFromMask(Array _ModEnumValues)
	{
		List<CModification> modifications = new List<CModification>();
		
		foreach(var modType in _ModEnumValues)
		{
			for(int i = (int)EDirection.INVALID + 1; i < (int)EDirection.MAX; ++i)
			{
				int value = ((int)modType * (int)EDirection.MAX) + i;
				if(CUtility.GetMaskState(value, m_CurrentTileMeta.m_ModificationMask))
				{
					modifications.Add(new CModification((int)modType, (EDirection)i, GetUnrotatedDirection((EDirection)i)));
				}
			}
		}
		
		return(modifications);
	}
	
	public GameObject GetModificationObject(int _ModificationType, EDirection _LocalSide)
	{
		Type classType = GetTileClassType(m_TileType);
		Type enumModType = classType.GetNestedType("EModification");
		Type enumTileMetaTypes = classType.GetNestedType("EType");

		if(enumModType == null)
			return(null);

		if(m_TileObject == null)
			return(null);

		string modName =  Enum.GetName(enumTileMetaTypes, m_CurrentTileMeta.m_MetaType) + "_" + Enum.GetName(enumModType, _ModificationType) + "_" + _LocalSide;
		Transform child = m_TileObject.transform.FindChild(modName);
		
		if(child != null)
			return(child.gameObject);
		else
			return(null);
	}

	[AServerOnly]
	public void SetTileModification(int _ModificationType, EDirection _WorldSide, bool _State)
	{
		if(_State)
		{
			m_Modifications.RemoveAll(m => m.m_WorldSide == _WorldSide);
			m_Modifications.Add(new CModification(_ModificationType, _WorldSide, GetUnrotatedDirection(_WorldSide)));
		}
		else
		{
			m_Modifications.RemoveAll(m => m.m_Modification == _ModificationType && m.m_WorldSide == _WorldSide);
		}

		m_CurrentTileMeta.m_ModificationMask = CalculateModificationsMask();
	}

	[AServerOnly]
	protected int CalculateModificationsMask()
	{
		int modMask = 0;
		foreach(CModification mod in m_Modifications)
		{
			modMask |= 1 << (mod.m_Modification * (int)EDirection.MAX) + (int)mod.m_WorldSide;
		}
		return(modMask);
	}

	[AServerOnly]
	public void SetNeighbourExemptionState(EDirection _Direction, bool _State)
	{
		if(m_NeighbourExemptions.Contains(_Direction) && _State)
			return;
		
		if(!m_NeighbourExemptions.Contains(_Direction) && !_State)
			return;

		if(_State)
		{
			m_NeighbourExemptions.Add(_Direction);
		}
		else
		{
			m_NeighbourExemptions.Remove(_Direction);
		}
	}

	[AServerOnly]
	public bool GetNeighbourExemptionState(EDirection _Direction)
	{
		return(m_NeighbourExemptions.Contains(_Direction));
	}

	public EDirection GetUnrotatedDirection(EDirection _WorldDirection)
	{
		for(int i = m_CurrentTileMeta.m_Rotations; i > 0; --i)
		{
			_WorldDirection = CNeighbour.GetLeftDirectionNeighbour(CNeighbour.GetLeftDirectionNeighbour(_WorldDirection));
		}
		
		return(_WorldDirection);
	}

	public void ReleaseTileObject()
	{
		if(m_TileObject == null)
			return;

		if(m_TileInterface.m_Grid != null)
			m_TileInterface.m_Grid.TileFactory.ReleaseTileObject(m_TileObject);

		m_TileObject = null;
	}
	
	protected static CMeta CreateMetaEntry(int _MetaType, EDirection[] _Neighbours)
	{
		// Define the neighbours into a mask
		int mask = 0;
		foreach(EDirection direction in _Neighbours)
		{
			mask |= 1 << (int)direction;
		}
		
		return(new CMeta(mask, (int)_MetaType));
	}

	public static Type GetTileClassType(EType _TileType)
	{
		Type classType = null;
		
		switch(_TileType)
		{
		case EType.Interior_Floor: 						classType = typeof(CTile_InteriorFloor); break;
		case EType.Interior_Floor_Inverse_Corner: 		classType = typeof(CTile_InteriorFloorCap); break;
		case EType.Interior_Wall: 						classType = typeof(CTile_InteriorWall); break;
		case EType.Interior_Wall_Inverse_Corner: 		classType = typeof(CTile_InteriorWallCap); break;
		case EType.Interior_Ceiling: 					classType = typeof(CTile_InteriorCeiling); break;
		case EType.Interior_Ceiling_Inverse_Corner: 	classType = typeof(CTile_InteriorCeilingCap); break;
		case EType.Exterior_Wall: 						classType = typeof(CTile_ExteriorWall); break;
		case EType.Exterior_Wall_Inverse_Corner: 		classType = typeof(CTile_ExteriorWallCap); break;
		case EType.Exterior_Upper: 						classType = typeof(CTile_ExteriorUpper); break;
		case EType.Exterior_Upper_Inverse_Corner: 		classType = typeof(CTile_ExteriorUpperCap); break;
		case EType.Exterior_Lower: 						classType = typeof(CTile_ExteriorLower); break;
		case EType.Exterior_Lower_Inverse_Corner: 		classType = typeof(CTile_ExteriorLowerCap); break;
		}
		
		return(classType);
	}
}