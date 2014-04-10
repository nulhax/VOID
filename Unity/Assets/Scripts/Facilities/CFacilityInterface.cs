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
	
	public delegate void HandleModuleEvent(CModuleInterface _Module, CFacilityInterface _FacilityParent);
	
	public event HandleModuleEvent EventModuleCreated;
	public event HandleModuleEvent EventModuleDestroyed;

// Member Fields

	public EType m_Type = EType.INVALID;
	public Mesh m_CombinedMesh = null;

	private CNetworkVar<uint> m_FacilityId = null;
	private List<CTile> m_Tiles = new List<CTile>();

	
	private Dictionary<CAccessoryInterface.EType, List<GameObject>> m_Accessories = new Dictionary<CAccessoryInterface.EType, List<GameObject>>();
	private Dictionary<CModuleInterface.EType, List<GameObject>> m_Modules = new Dictionary<CModuleInterface.EType, List<GameObject>>();
	
	
	static Dictionary<EType, List<GameObject>> s_FacilityObjects = new Dictionary<EType, List<GameObject>>();
	static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_RegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();
	static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_RegisteredMiniaturePrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();



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
        get { return (m_Type); }
	}


	public List<CTile> FacilityTiles
	{
		get { return(m_Tiles); }
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


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
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


    public static void RegisterPrefab(EType _eFacilityType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_RegisteredPrefabs.Add(_eFacilityType, _ePrefab);
    }

	public static void RegistMiniaturePrefab(EType _eFacilityType, CGameRegistrator.ENetworkPrefab _ePrefab)
	{
		s_RegisteredMiniaturePrefabs.Add(_eFacilityType, _ePrefab);
	}

    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eFacilityType)
    {
        if (!s_RegisteredPrefabs.ContainsKey(_eFacilityType))
        {
            Debug.LogError(string.Format("Facility type ({0}) has not been registered a prefab", _eFacilityType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_RegisteredPrefabs[_eFacilityType]);
    }


    public static Dictionary<EType, List<GameObject>> GetAllFacilities()
    {
        return (s_FacilityObjects);
    }


    void Awake()
    {
        if (!s_FacilityObjects.ContainsKey(m_Type))
        {
            s_FacilityObjects.Add(m_Type, new List<GameObject>());
        }

        s_FacilityObjects[m_Type].Add(gameObject);
    }


	void Start()
	{
		// Find all of the tiles contained within this facility
		m_Tiles = new List<CTile>(gameObject.GetComponentsInChildren<CTile>());
      
	    // Register facility
	    CGameShips.Ship.GetComponent<CShipFacilities>().RegisterFacility(this);

		// Delete all the preset tiles
		foreach(CTile tile in m_Tiles)
		{
			Destroy(tile.gameObject);
		}

	    // Listen to event to unregister facility
	    NetworkView.EventPreDestory += () =>
	    {
			CGameShips.Ship.GetComponent<CShipFacilities>().UnregisterFacility(this);
		};

		// Create facility triggers
		ConfigureFacility();
	}


    void OnDestroy()
    {
        s_FacilityObjects[m_Type].Remove(gameObject);
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


	void ConfigureFacility()
	{
		if(m_CombinedMesh == null)
			Debug.LogError("Facility " + gameObject.name + " is missing its CombinedMesh instance. Ensure this is connected or the facility will be broken.");

		MeshCollider mc = null;

		// Create the triggers/colliders
		GameObject internalTrigger = new GameObject("_InteriorTrigger");
		GameObject collider = new GameObject("_Collider");
		GameObject exitTrigger = new GameObject("_ExitTrigger");
		GameObject entryTrigger = new GameObject("_ExitTrigger");
         internalTrigger.tag = "GalaxyShip";
         collider.tag      = "GalaxyShip";
         exitTrigger.tag = "GalaxyShip";
         entryTrigger.tag = "GalaxyShip";

		// Create the exterior version of the facility
		GameObject extFacility = new GameObject("_" + gameObject.name + "Ext");

		// Child the exit trigger and interior trigger to the facility
		exitTrigger.transform.parent = transform;
		exitTrigger.transform.localPosition = Vector3.zero;
		exitTrigger.transform.localRotation = Quaternion.identity;
		internalTrigger.transform.parent = transform;
		internalTrigger.transform.localPosition = Vector3.zero;
		internalTrigger.transform.localRotation = Quaternion.identity;

		// Child the entry trigger and collider to the exterior facility
		entryTrigger.transform.parent = extFacility.transform;
		entryTrigger.transform.localPosition = Vector3.zero;
		entryTrigger.transform.localRotation = Quaternion.identity;
		collider.transform.parent = extFacility.transform;
		collider.transform.localPosition = Vector3.zero;
		collider.transform.localRotation = Quaternion.identity;

		// Set the exterior facility on the galaxy layer
		CUtility.SetLayerRecursively(extFacility, LayerMask.NameToLayer("Galaxy"));

		// Configure the internal trigger
		internalTrigger.AddComponent<CInteriorTrigger>();
		mc = internalTrigger.AddComponent<MeshCollider>();
		mc.sharedMesh = m_CombinedMesh;
		mc.convex = true;
		mc.isTrigger = true;

		// Configure the exit trigger
		exitTrigger.transform.localScale = Vector3.one * 1.02f;
		exitTrigger.AddComponent<CExitTrigger>();
		mc = exitTrigger.AddComponent<MeshCollider>();
		mc.sharedMesh = m_CombinedMesh;
		mc.convex = true;
		mc.isTrigger = true;

		// Configure the entry trigger
		entryTrigger.transform.localScale = Vector3.one * 1.02f;
		entryTrigger.AddComponent<CEntryTrigger>();
		mc = entryTrigger.AddComponent<MeshCollider>();
		mc.sharedMesh = m_CombinedMesh;
		mc.convex = true;
		mc.isTrigger = true;

		// Configure the collider trigger
		mc = collider.AddComponent<MeshCollider>();
		mc.sharedMesh = m_CombinedMesh;
		mc.convex = true;

		// Attach the exterior to the facility to the galaxy ship
		CGalaxyShipFacilities galaxyShipCollider = CGameShips.GalaxyShip.GetComponent<CGalaxyShipFacilities>();
		galaxyShipCollider.AttachNewFacility(extFacility, transform.localPosition, transform.localRotation);
	}

};
