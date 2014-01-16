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
public class CComponentInterface : MonoBehaviour
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


	public EType ComponentType
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

		case EType.Alarm:
			ePrefabType = CGame.ENetworkRegisteredPrefab.Alarm;
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
			Debug.LogError("This component has not been given a compoent type");
		}

		// Register self with parent facility
		Transform cParent = transform.parent;

		for (int i = 0; i < 20; ++ i)
		{
			if (cParent.GetComponent<CFacilityComponents>() != null)
			{
				cParent.GetComponent<CFacilityComponents>().RegisterComponent(this);
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
		s_mComponentObjects[ComponentType].Remove(gameObject);
	}


	void Update()
	{
		// Empty
	}


// Member Fields


	public EType m_eComponentType = EType.INVALID;


	static Dictionary<CComponentInterface.EType, List<GameObject>> s_mComponentObjects = new Dictionary<EType, List<GameObject>>();


};
