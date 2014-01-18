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


public class CFacilityModules : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	public List<GameObject> FindModulesByType(CModuleInterface.EType _eComponentType)
	{
		if (!m_mComponents.ContainsKey(_eComponentType))
		{
			return (null);
		}

		return (m_mComponents[_eComponentType]);
	}


	public void RegisterModule(CModuleInterface _cComponentInterface)
	{
		// Add component it a list based on its component type
		if (!m_mComponents.ContainsKey(_cComponentInterface.ModuleType))
		{
			m_mComponents.Add(_cComponentInterface.ModuleType, new List<GameObject>());
		}

		m_mComponents[_cComponentInterface.ModuleType].Add(_cComponentInterface.gameObject);
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
			List<GameObject> aAlarmObjects = FindModulesByType(CModuleInterface.EType.Alarm);

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


	Dictionary<CModuleInterface.EType, List<GameObject>> m_mComponents = new Dictionary<CModuleInterface.EType, List<GameObject>>();


};
