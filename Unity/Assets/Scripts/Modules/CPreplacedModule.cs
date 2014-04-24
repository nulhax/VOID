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
    public GameObject CreateModule(GameObject _FacilityParent)
    {
		GameObject moduleObject = CNetwork.Factory.CreateObject(CModuleInterface.GetPrefabType(m_PreplacedModuleType));
        moduleObject.GetComponent<CNetworkView>().SetPosition(transform.position);
        moduleObject.GetComponent<CNetworkView>().SetRotation(transform.rotation);
		moduleObject.GetComponent<CNetworkView>().SetParent(_FacilityParent.GetComponent<CNetworkView>().ViewId);

		if(m_PreplacedModuleBuilt)
			moduleObject.GetComponent<CModuleInterface>().Build(1.0f);

        return(moduleObject);
    }
};
