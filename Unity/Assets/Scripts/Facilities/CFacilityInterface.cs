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
[RequireComponent(typeof(CFacilityExpansion))]
[RequireComponent(typeof(CFacilityGeneral))]
[RequireComponent(typeof(CFacilityGravity))]
[RequireComponent(typeof(CFacilityHull))]
[RequireComponent(typeof(CFacilityOnboardActors))]
[RequireComponent(typeof(CFacilityPower))]
[RequireComponent(typeof(CFacilityTurrets))]
[RequireComponent(typeof(CNetworkView))]
public class CFacilityInterface : CNetworkMonoBehaviour
{

// Member Types


    public enum EType
	{
		INVALID = -1,
		
		Bridge,
		Engine,
		Factory,
		GravityGenerator,
		LifeSupportDome,
		Replicator,
		Scanner,
		//ShieldGenerator,
		HallwayStraight,
		HallwayCorner,
		HallwayTSection,
		HallwayXSection,

		MAX,
	}


// Member Delegates & Events


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
	
	
	public EType FacilityType 
	{
		get { return(m_FacilityType.Get()); }

		[AServerOnly]
		set
		{
			if(m_FacilityType.Get() == EType.INVALID)
			{
				m_FacilityType.Set(value);
			}
			else
			{
				Debug.LogError("Cannot set facility type value twice!");
			}
		}
	}


// Member Methods


	public override void InstanceNetworkVars()
	{
		m_FacilityId = new CNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
		m_FacilityType = new CNetworkVar<EType>(OnNetworkVarSync, EType.INVALID);
	}


    public List<GameObject> FindAccessoriesByType(CAccessoryInterface.EType _eAccessoryType)
    {
        if (!m_mAccessories.ContainsKey(_eAccessoryType))
        {
            return (null);
        }

        return (m_mAccessories[_eAccessoryType]);
    }


    public List<GameObject> FindModulesByType(CModuleInterface.EType _eModuleType)
    {
        if (!m_mModules.ContainsKey(_eModuleType))
        {
            return (null);
        }

        return (m_mModules[_eModuleType]);
    }


    public void RegisterAccessory(CAccessoryInterface _cAccessoryInterface)
    {
        if (!m_mAccessories.ContainsKey(_cAccessoryInterface.AccessoryType))
        {
            m_mAccessories.Add(_cAccessoryInterface.AccessoryType, new List<GameObject>());
        }

        m_mAccessories[_cAccessoryInterface.AccessoryType].Add(_cAccessoryInterface.gameObject);
    }


    public void RegisterModule(CModuleInterface _cModuleInterface)
    {
        if (!m_mModules.ContainsKey(_cModuleInterface.ModuleType))
        {
            m_mModules.Add(_cModuleInterface.ModuleType, new List<GameObject>());
        }

        m_mModules[_cModuleInterface.ModuleType].Add(_cModuleInterface.gameObject);
    }


    public static void RegisterPrefab(EType _eFacilityType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_eFacilityType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eFacilityType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_eFacilityType))
        {
            Debug.LogError(string.Format("Module type ({0}) has not been registered a prefab", _eFacilityType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_eFacilityType]);
    }


	void Start()
	{
		// Attach the collider for the facility to the galaxy ship
		CGalaxyShipCollider galaxyShipCollider = CGameShips.GalaxyShip.GetComponent<CGalaxyShipCollider>();
		galaxyShipCollider.AttachNewCollider("Prefabs/" + CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetPrefabType(FacilityType)) + "Ext", transform.localPosition, transform.localRotation);
	
		// Add self to the ship facilities
        if (!CNetwork.IsServer)
        {
            CGameShips.Ship.GetComponent<CShipFacilities>().AddNewlyCreatedFacility(gameObject, FacilityId, FacilityType);
        }
	}


    void Update()
    {
        if (CNetwork.IsServer &&
            Input.GetKeyDown(KeyCode.M))
        {
            List<GameObject> aAlarmObjects = FindAccessoriesByType(CAccessoryInterface.EType.Alarm);

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


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {

    }


    // Member Fields


    CNetworkVar<uint> m_FacilityId = null;
    CNetworkVar<EType> m_FacilityType = null;


    Dictionary<CAccessoryInterface.EType, List<GameObject>> m_mAccessories = new Dictionary<CAccessoryInterface.EType, List<GameObject>>();
    Dictionary<CModuleInterface.EType, List<GameObject>> m_mModules = new Dictionary<CModuleInterface.EType, List<GameObject>>();


    static Dictionary<EType, List<GameObject>> s_mModuleObjects = new Dictionary<EType, List<GameObject>>();
    static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


};