//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityComponents.cs
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


public class CFacilityComponents : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public List<GameObject> FindFacilityComponents(CFacilityComponentInterface.EType _eComponentType)
	{
		if (!m_mComponents.ContainsKey(_eComponentType))
		{
			return (null);
		}

		return (m_mComponents[_eComponentType]);
	}


	void Start()
	{
		if (CNetwork.IsServer)
		{
			CFacilityComponentNode[] cComponentNodes = transform.GetComponentsInChildren<CFacilityComponentNode>();
		
			foreach (var cComponetNode in cComponentNodes)
			{
				GameObject cChild = cComponetNode.gameObject;

				if (cComponetNode != null)
				{
					CFacilityComponentInterface.EType eComponentType = cComponetNode.GetComponentType();

					if (!m_mComponents.ContainsKey(eComponentType))
					{
						m_mComponents.Add(eComponentType, new List<GameObject>());
					}

					// Create facility component prefab
					CGame.ENetworkRegisteredPrefab eNetworkPrefabType = CFacilityComponentInterface.GetPrefabType(eComponentType);

					GameObject cFacilityComponentObject = CNetwork.Factory.CreateObject(eNetworkPrefabType);
					cFacilityComponentObject.GetComponent<CNetworkView>().SetPosition(cChild.transform.position);
					cFacilityComponentObject.GetComponent<CNetworkView>().SetRotation(cChild.transform.rotation.eulerAngles);
					cFacilityComponentObject.GetComponent<CNetworkView>().SetParent(GetComponent<CNetworkView>().ViewId);

					m_mComponents[eComponentType].Add(cFacilityComponentObject);
				}
			}
		}
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


	Dictionary<CFacilityComponentInterface.EType, List<GameObject>> m_mComponents = new Dictionary<CFacilityComponentInterface.EType, List<GameObject>>();


};
