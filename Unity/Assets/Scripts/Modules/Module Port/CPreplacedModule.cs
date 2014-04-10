//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModulePortInterface.cs
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


public class CPreplacedModule : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Fields
	

	public CModuleInterface.EType m_PreplacedModuleType = CModuleInterface.EType.INVALID;
	public bool m_PreplacedModuleBuilt = false;


// Member Properties



// Member Methods
	

    public GameObject CreateModule(CModuleInterface.EType _eType)
    {
		GameObject moduleObject = CNetwork.Factory.CreateObject(CModuleInterface.GetPrefabType(_eType));
        moduleObject.GetComponent<CNetworkView>().SetPosition(transform.position);
        moduleObject.GetComponent<CNetworkView>().SetEulerAngles(transform.rotation.eulerAngles);
		moduleObject.GetComponent<CNetworkView>().SetParent(CUtility.FindInParents<CFacilityInterface>(transform).transform);

        return(moduleObject);
    }

	void Start()
	{
        if(m_PreplacedModuleType != CModuleInterface.EType.INVALID && CNetwork.IsServer)
        {
            GameObject module = CreateModule(m_PreplacedModuleType);

			// Make the module fully built already
            if(m_PreplacedModuleBuilt)
				module.GetComponent<CModuleInterface>().IncrementBuiltRatio(1.0f);
        }
	}
};
