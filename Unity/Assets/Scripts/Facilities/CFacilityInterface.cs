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
[RequireComponent(typeof(CFacilityGravity))]
[RequireComponent(typeof(CFacilityHull))]
[RequireComponent(typeof(CFacilityLighting))]
[RequireComponent(typeof(CFacilityOnboardActors))]
[RequireComponent(typeof(CFacilityPower))]
[RequireComponent(typeof(CNetworkView))]
public class CFacilityInterface : CNetworkMonoBehaviour
{

// Member Types


    public enum EType
	{
		INVALID = -1,
		
		Bridge,
		HallwayStraight,
		HallwayCorner,
		HallwayTSection,
		HallwayXSection,
        Airlock,
        Test,

		MAX,
	}


// Member Delegates & Events



// Member Fields
	
	
	public EType m_eType = EType.INVALID;
	public CDUIConsole m_FacilityControlPanel = null;
	
	CNetworkVar<uint> m_FacilityId = null;
	
	
	Dictionary<CAccessoryInterface.EType, List<GameObject>> m_mAccessories = new Dictionary<CAccessoryInterface.EType, List<GameObject>>();
	Dictionary<CModuleInterface.EType, List<GameObject>> m_mModules = new Dictionary<CModuleInterface.EType, List<GameObject>>();
	
	
	static Dictionary<EType, List<GameObject>> s_mFacilityObjects = new Dictionary<EType, List<GameObject>>();
	static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();
	static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredMiniaturePrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();



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
        get { return (m_eType); }
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_FacilityId = _cRegistrar.CreateNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
	}


    public List<GameObject> FindAccessoriesByType(CAccessoryInterface.EType _eAccessoryType)
    {
        if (!m_mAccessories.ContainsKey(_eAccessoryType))
        {
            return (new List<GameObject>());
        }

        return (m_mAccessories[_eAccessoryType]);
    }


    public List<GameObject> FindModulesByType(CModuleInterface.EType _eModuleType)
    {
        if (!m_mModules.ContainsKey(_eModuleType))
        {
            return (new List<GameObject>());
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

	public static void RegistMiniaturePrefab(EType _eFacilityType, CGameRegistrator.ENetworkPrefab _ePrefab)
	{
		s_mRegisteredMiniaturePrefabs.Add(_eFacilityType, _ePrefab);
	}

    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eFacilityType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_eFacilityType))
        {
            Debug.LogError(string.Format("Facility type ({0}) has not been registered a prefab", _eFacilityType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_eFacilityType]);
    }

	public static CGameRegistrator.ENetworkPrefab GetMiniaturePrefabType(EType _eFacilityType)
	{
		if (!s_mRegisteredMiniaturePrefabs.ContainsKey(_eFacilityType))
		{
			Debug.LogError(string.Format("Facility type miniature ({0}) has not been registered a prefab", _eFacilityType));
			
			return (CGameRegistrator.ENetworkPrefab.INVALID);
		}
		
		return (s_mRegisteredMiniaturePrefabs[_eFacilityType]);
	}


    public static Dictionary<EType, List<GameObject>> GetAllFacilities()
    {
        return (s_mFacilityObjects);
    }


    void Awake()
    {
        if (!s_mFacilityObjects.ContainsKey(m_eType))
        {
            s_mFacilityObjects.Add(m_eType, new List<GameObject>());
        }

        s_mFacilityObjects[m_eType].Add(gameObject);
    }


	void Start()
	{
        if (CNetwork.IsServer)
        {
            // Register facility
            CGameShips.Ship.GetComponent<CShipFacilities>().RegisterFacility(gameObject);

            // Unregister facility
            SelfNetworkView.EventPreDestory += () =>
            {
                CGameShips.Ship.GetComponent<CShipFacilities>().UnregisterFacility(gameObject);
            };

            // Parent self to ship
            SelfNetworkView.SetParent(CGameShips.Ship.GetComponent<CNetworkView>().ViewId);
        }

		// Attach the collider for the facility to the galaxy ship
		CGalaxyShipCollider galaxyShipCollider = CGameShips.GalaxyShip.GetComponent<CGalaxyShipCollider>();
		galaxyShipCollider.AttachNewCollider("Prefabs/" + CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetPrefabType(FacilityType)) + "Ext", transform.localPosition, transform.localRotation);
	
		// Add self to the ship facilities
        if (!CNetwork.IsServer)
        {
            CGameShips.Ship.GetComponent<CShipFacilities>().AddNewlyCreatedFacility(gameObject, FacilityId, FacilityType);
        }
	}


    void OnDestroy()
    {
        s_mFacilityObjects[m_eType].Remove(gameObject);
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
        // Empty
    }


};
