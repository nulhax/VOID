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


	public void RegisterComponent(CFacilityComponentInterface _cComponentInterface)
	{
		if (!m_mComponents.ContainsKey(_cComponentInterface.FacilityComponentType))
		{
			m_mComponents.Add(_cComponentInterface.FacilityComponentType, new List<GameObject>());
		}

		m_mComponents[_cComponentInterface.FacilityComponentType].Add(_cComponentInterface.gameObject);
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


	Dictionary<CFacilityComponentInterface.EType, List<GameObject>> m_mComponents = new Dictionary<CFacilityComponentInterface.EType, List<GameObject>>();


};
