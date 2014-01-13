//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CFacilityComponentInterface : MonoBehaviour
{

// Member Types


	public enum EType
	{
		INVALID,

		FuseBox,
		PlayerSpawner,
		TurretCockpit,
		PilotCockpit
	}


// Member Delegates & Events


// Member Properties


	public EType FacilityComponentType
	{
		get { return (m_eComponentType); }
	}


// Member Methods



	public static List<GameObject> FindFacilityComponents(EType _eFacilityComponentType)
	{
		if (!s_mComponentObjects.ContainsKey(_eFacilityComponentType))
		{
			return (null);
		}

		return (s_mComponentObjects[_eFacilityComponentType]);
	}


	public static CGame.ENetworkRegisteredPrefab GetPrefabType(EType _eFacilityComponentType)
	{
		CGame.ENetworkRegisteredPrefab ePrefabType = CGame.ENetworkRegisteredPrefab.INVALID;
		
		switch (_eFacilityComponentType)
		{
		case EType.FuseBox:
			ePrefabType = CGame.ENetworkRegisteredPrefab.PanelFuseBox;
			break;

		case EType.PlayerSpawner:
			ePrefabType = CGame.ENetworkRegisteredPrefab.PlayerSpawner;
			break;

		case EType.TurretCockpit:
			ePrefabType = CGame.ENetworkRegisteredPrefab.TurretCockpit;
			break;

		case EType.PilotCockpit:
			ePrefabType = CGame.ENetworkRegisteredPrefab.BridgeCockpit;
			break;

		default:
			Debug.LogError(string.Format("Unknown component type. Type({0})", _eFacilityComponentType));
			break;
		}

		return (ePrefabType);
	}


	void Awake()
	{
		if (!s_mComponentObjects.ContainsKey(m_eComponentType))
		{
			s_mComponentObjects.Add(m_eComponentType, new List<GameObject>());
		}
	
		s_mComponentObjects[m_eComponentType].Add(gameObject);
	}


	void Start()
	{
		// Empty
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		// Empty
	}


// Member Fields


	public EType m_eComponentType = EType.INVALID;


	static Dictionary<CFacilityComponentInterface.EType, List<GameObject>> s_mComponentObjects = new Dictionary<EType, List<GameObject>>();


};
