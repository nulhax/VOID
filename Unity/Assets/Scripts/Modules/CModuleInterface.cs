//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CComponentInterface.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CModuleInterface : MonoBehaviour
{

// Member Types


	public enum EType
	{
		INVALID,

		FuseBox,
		PlayerSpawner,
		TurretCockpit,
		PilotCockpit,
		Alarm,
	}


// Member Delegates & Events


// Member Properties


	public EType ModuleType
	{
		get { return (m_eComponentType); }
	}


// Member Methods



	public static List<GameObject> FindComponentsByType(EType _eFacilityComponentType)
	{
		if (!s_mComponentObjects.ContainsKey(_eFacilityComponentType))
		{
			return (null);
		}

		return (s_mComponentObjects[_eFacilityComponentType]);
	}


	public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eFacilityComponentType)
	{
		CGameRegistrator.ENetworkPrefab ePrefabType = CGameRegistrator.ENetworkPrefab.INVALID;
		
		switch (_eFacilityComponentType)
		{
		case EType.FuseBox:
			ePrefabType = CGameRegistrator.ENetworkPrefab.PanelFuseBox;
			break;

		case EType.PlayerSpawner:
			ePrefabType = CGameRegistrator.ENetworkPrefab.PlayerSpawner;
			break;

		case EType.TurretCockpit:
			ePrefabType = CGameRegistrator.ENetworkPrefab.TurretCockpit;
			break;

		case EType.PilotCockpit:
			ePrefabType = CGameRegistrator.ENetworkPrefab.BridgeCockpit;
			break;

		case EType.Alarm:
			ePrefabType = CGameRegistrator.ENetworkPrefab.Alarm;
			break;

		default:
			Debug.LogError(string.Format("Unknown component type. Type({0})", _eFacilityComponentType));
			break;
		}

		return (ePrefabType);
	}


	void Awake()
	{
		// Add self to the global list of components
		if (!s_mComponentObjects.ContainsKey(m_eComponentType))
		{
			s_mComponentObjects.Add(m_eComponentType, new List<GameObject>());
		}
	
		s_mComponentObjects[m_eComponentType].Add(gameObject);
	}


	void Start()
	{
		// Ensure a type of defined for component
		if (m_eComponentType == EType.INVALID)
		{
			Debug.LogError("This component has not been given a component type");
		}

		// Register self with parent facility
		Transform cParent = transform.parent;

		for (int i = 0; i < 20; ++ i)
		{
			if (cParent.GetComponent<CFacilityModules>() != null)
			{
				cParent.GetComponent<CFacilityModules>().RegisterModule(this);
				break;
			}

			cParent = cParent.parent;

			if (i == 19)
			{
				Debug.LogError("Could not find facility to register to");
			}
		}
	}


	void OnDestroy()
	{
		// Remove self from global list of components
		s_mComponentObjects[ModuleType].Remove(gameObject);
	}


	void Update()
	{
		// Empty
	}


// Member Fields


	public EType m_eComponentType = EType.INVALID;


	static Dictionary<CModuleInterface.EType, List<GameObject>> s_mComponentObjects = new Dictionary<EType, List<GameObject>>();


};
