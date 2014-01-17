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


	public List<GameObject> FindFacilityComponents(CComponentInterface.EType _eComponentType)
	{
		if (!m_mComponents.ContainsKey(_eComponentType))
		{
			return (null);
		}

		return (m_mComponents[_eComponentType]);
	}


	public void RegisterComponent(CComponentInterface _cComponentInterface)
	{
		// Add component it a list based on its component type
		if (!m_mComponents.ContainsKey(_cComponentInterface.ComponentType))
		{
			m_mComponents.Add(_cComponentInterface.ComponentType, new List<GameObject>());
		}

		m_mComponents[_cComponentInterface.ComponentType].Add(_cComponentInterface.gameObject);
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

		// Debug
		if (CNetwork.IsServer &&
		    Input.GetKeyDown(KeyCode.M))
		{
			List<GameObject> aAlarmObjects = FindFacilityComponents(CComponentInterface.EType.Alarm);

			if (aAlarmObjects != null &&
			    aAlarmObjects.Count > 0)
			{
				bool bToggle = aAlarmObjects[0].GetComponent<CAlarmBehaviour>().IsActive;

				foreach (GameObject cAlarmObject in aAlarmObjects)
				{
					cAlarmObject.GetComponent<CAlarmBehaviour>().SetAlarmActive(!bToggle);
				}
			}
		}
	}


// Member Fields


	Dictionary<CComponentInterface.EType, List<GameObject>> m_mComponents = new Dictionary<CComponentInterface.EType, List<GameObject>>();


};
