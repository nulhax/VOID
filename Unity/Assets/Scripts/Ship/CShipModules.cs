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


public class CShipModules : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
    }


	[AServerOnly]
    public CModuleInterface CreateModule(CModuleInterface.EType _cType, Vector3 _Position, Quaternion _Rotation)
    {
		CModuleInterface moduleInterface = null;

        CGameRegistrator.ENetworkPrefab eModuleNetworkPrefab = CModuleInterface.GetPrefabType(_cType);
        GameObject cModulePrefab = Resources.Load(CNetwork.Factory.GetRegisteredPrefabFile(eModuleNetworkPrefab), typeof(GameObject)) as GameObject;

		// Create the module
		moduleInterface = CNetwork.Factory.CreateGameObject(eModuleNetworkPrefab).GetComponent<CModuleInterface>();

    	// Set module position and rotation
		moduleInterface.GetComponent<CNetworkView>().SetParent(CGameShips.ShipViewId);
		moduleInterface.GetComponent<CNetworkView>().SetPosition(_Position);
		moduleInterface.GetComponent<CNetworkView>().SetRotation(_Rotation);

		return(moduleInterface);
    }


    public void RegisterModule(GameObject _cModule)
    {
        CModuleInterface cModuleInterface = _cModule.GetComponent<CModuleInterface>();

        // Create list for module type
        if (!m_mModulesByType.ContainsKey(cModuleInterface.ModuleType))
        {
            m_mModulesByType.Add(cModuleInterface.ModuleType, new List<GameObject>());
        }

        // Add to directory
        m_mModulesByType[cModuleInterface.ModuleType].Add(_cModule);

        // Remove from directory on destroy
        _cModule.GetComponent<CNetworkView>().EventPreDestory += ((GameObject _cSender) =>
        {
            m_mModulesByType[cModuleInterface.ModuleType].Remove(_cModule);
        });
    }


    public List<GameObject> FindModulesByType(CModuleInterface.EType _eModuleType)
    {
        if (!m_mModulesByType.ContainsKey(_eModuleType))
        {
            return (new List<GameObject>());
        }

        return (m_mModulesByType[_eModuleType]);
    }


	void Start()
	{
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void OnNetworkSyncVar(INetworkVar _cSyncedVar)
    {

    }


// Member Fields


    Dictionary<CModuleInterface.EType, List<GameObject>> m_mModulesByType = new Dictionary<CModuleInterface.EType, List<GameObject>>();


};
