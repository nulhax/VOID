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


    public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
    {
    }


    public bool CreateModule(CModuleInterface.EType _cType, Vector3 _vPosition, float _fRotationY)
    {
        bool bModuleBuilt = false;

        // Check ship has enough nanites to build module
        CGameRegistrator.ENetworkPrefab eModuleNetworkPrefab = CModuleInterface.GetPrefabType(_cType);

        GameObject cModulePRefab = Resources.Load(CNetwork.Factory.GetRegisteredPrefabFile(eModuleNetworkPrefab), typeof(GameObject)) as GameObject;

        if (CGameShips.Ship.GetComponent<CShipNaniteSystem>().NanaiteQuanity > cModulePRefab.GetComponent<CModuleInterface>().m_fNanitesCost)
        {
            GameObject cBuiltModule = CNetwork.Factory.CreateGameObject(eModuleNetworkPrefab);

            // Set module position
            cBuiltModule.GetComponent<CNetworkView>().SetPosition(_vPosition);

            bModuleBuilt = true;
        }

        return (bModuleBuilt);
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
