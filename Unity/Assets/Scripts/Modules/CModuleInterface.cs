//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CComponentInterface.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CModuleInterface : MonoBehaviour
{

// Member Types


	public enum EType
	{
		INVALID,

        PlayerSpawner,
        TurretCockpit,
        PilotCockpit,
        Turret,
        LifeSupport1,
        Power1,

        MAX
	}


// Member Delegates & Events


// Member Properties


	public EType ModuleType
	{
		get { return (m_eModuleType); }
	}


    public GameObject ParentFacility
    {
        get { return (m_cParentFacility); }
    }


// Member Methods


    public List<GameObject> FindAttachedComponentsByType(CComponentInterface.EType _eAccessoryType)
    {
        if (!m_mAttachedComponents.ContainsKey(_eAccessoryType))
        {
            return (null);
        }

        return (m_mAttachedComponents[_eAccessoryType]);
    }


    public void RegisterAttachedComponent(CComponentInterface _cComponentInterface)
    {
        if (!m_mAttachedComponents.ContainsKey(_cComponentInterface.ComponentType))
        {
            m_mAttachedComponents.Add(_cComponentInterface.ComponentType, new List<GameObject>());
        }

        m_mAttachedComponents[_cComponentInterface.ComponentType].Add(_cComponentInterface.gameObject);
    }


	public static List<GameObject> FindModulesByType(EType _eComponentType)
	{
		if (!s_mModuleObjects.ContainsKey(_eComponentType))
		{
			return (null);
		}

		return (s_mModuleObjects[_eComponentType]);
	}


    public static void RegisterPrefab(EType _eModuleType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_eModuleType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eModuleType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_eModuleType))
        {
            Debug.LogError(string.Format("Module type ({0}) has not been registered a prefab", _eModuleType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_eModuleType]);
    }


	void Awake()
	{
		// Add self to the global list of components
		if (!s_mModuleObjects.ContainsKey(m_eModuleType))
		{
			s_mModuleObjects.Add(m_eModuleType, new List<GameObject>());
		}
	
		s_mModuleObjects[m_eModuleType].Add(gameObject);
	}


	void Start()
	{
		// Ensure a type of defined 
		if (m_eModuleType == EType.INVALID)
		{
            Debug.LogError(string.Format("This module has not been given a module type. GameObjectName({0})", gameObject.name));
		}

		// Register self with parent facility
		Transform cParent = transform.parent;

		for (int i = 0; i < 20; ++ i)
		{
            if (cParent != null)
            {
                if (cParent.GetComponent<CFacilityInterface>() != null)
                {
                    cParent.GetComponent<CFacilityInterface>().RegisterModule(this);
                    m_cParentFacility = cParent.gameObject;
                    break;
                }

                cParent = cParent.parent;
            }

			if (i == 19)
			{
				Debug.LogError("Could not find facility to register to");
			}
		}
	}


	void OnDestroy()
	{
		// Remove self from global list of components
		s_mModuleObjects[ModuleType].Remove(gameObject);
	}


	void Update()
	{
		// Empty
	}


// Member Fields


	public EType m_eModuleType = EType.INVALID;


    GameObject m_cParentFacility = null;


    Dictionary<CComponentInterface.EType, List<GameObject>> m_mAttachedComponents = new Dictionary<CComponentInterface.EType, List<GameObject>>();


    static Dictionary<EType, List<GameObject>> s_mModuleObjects = new Dictionary<EType, List<GameObject>>();
    static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


};
