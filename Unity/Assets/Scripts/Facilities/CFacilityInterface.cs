//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityInfo.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


[RequireComponent(typeof(CFacilityAtmosphere))]
[RequireComponent(typeof(CFacilityGravity))]
[RequireComponent(typeof(CFacilityHull))]
[RequireComponent(typeof(CFacilityLighting))]
[RequireComponent(typeof(CFacilityOnboardActors))]
[RequireComponent(typeof(CFacilityPower))]
[RequireComponent(typeof(CFacilityTiles))]
[RequireComponent(typeof(CFacilityWarningSystem))]
[RequireComponent(typeof(CNetworkView))]
public class CFacilityInterface : CNetworkMonoBehaviour
{
	// Member Types


	// Member Delegates & Events
	public delegate void HandleModuleEvent(CModuleInterface _Module, CFacilityInterface _FacilityParent);
	
	public event HandleModuleEvent EventModuleCreated;
	public event HandleModuleEvent EventModuleDestroyed;


	// Member Fields
	private CNetworkVar<uint> m_FacilityId = null;
	
	private Dictionary<CAccessoryInterface.EType, List<GameObject>> m_Accessories = new Dictionary<CAccessoryInterface.EType, List<GameObject>>();
	private Dictionary<CModuleInterface.EType, List<GameObject>> m_Modules = new Dictionary<CModuleInterface.EType, List<GameObject>>();

	
	// Member Properties
	public uint FacilityId 
	{
		get{return(m_FacilityId.Get());}			

		[AServerOnly]
		set
		{
			if(m_FacilityId.Get() == uint.MaxValue)
			{
				m_FacilityId.Set(value);
			}
			else
			{
				Debug.LogError("Cannot set facility ID value twice!");
			}
		}			
	}

	public List<GameObject> FacilityModules
	{
		get
		{
			List<GameObject> modules = new List<GameObject>();
			foreach(List<GameObject> moduleLists in m_Modules.Values)
			{
				modules.AddRange(moduleLists);
			}
			return(modules);
		}
	}


	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_FacilityId = _cRegistrar.CreateReliableNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
	}
	
    public List<GameObject> FindAccessoriesByType(CAccessoryInterface.EType _eAccessoryType)
    {
        if (!m_Accessories.ContainsKey(_eAccessoryType))
        {
            return (new List<GameObject>());
        }

        return (m_Accessories[_eAccessoryType]);
    }
	
    public List<GameObject> FindModulesByType(CModuleInterface.EType _eModuleType)
    {
        if (!m_Modules.ContainsKey(_eModuleType))
        {
            return (new List<GameObject>());
        }

        return (m_Modules[_eModuleType]);
    }
	
    public void RegisterAccessory(CAccessoryInterface _cAccessoryInterface)
    {
        if (!m_Accessories.ContainsKey(_cAccessoryInterface.AccessoryType))
        {
            m_Accessories.Add(_cAccessoryInterface.AccessoryType, new List<GameObject>());
        }

        m_Accessories[_cAccessoryInterface.AccessoryType].Add(_cAccessoryInterface.gameObject);
    }
	
    public void RegisterModule(CModuleInterface _ModuleInterface)
    {
        if (!m_Modules.ContainsKey(_ModuleInterface.ModuleType))
        {
            m_Modules.Add(_ModuleInterface.ModuleType, new List<GameObject>());
        }

        m_Modules[_ModuleInterface.ModuleType].Add(_ModuleInterface.gameObject);

		// Notify observers
		if (EventModuleCreated != null) 
			EventModuleCreated(_ModuleInterface, this);
    }

    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Empty
    }
};
