//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CAccessoryInterface.cs
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


public class CAccessoryInterface : MonoBehaviour
{

// Member Types


    public enum EType
    {
        INVALID,

        Alarm,
        DuiMonitor,

        MAX
    }


// Member Delegates & Events


// Member Properties


    public EType AccessoryType
    {
        get { return (m_eAccessoryType); }
    }


// Member Methods


    public static void RegisterPrefab(EType _eAccessoryType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_eAccessoryType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eAccessoryType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_eAccessoryType))
        {
            Debug.LogError(string.Format("Accessory type ({0}) has not been registered a prefab", _eAccessoryType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_eAccessoryType]);
    }


	void Start()
	{
        // Ensure a type of defined
        if (m_eAccessoryType == EType.INVALID)
        {
            Debug.LogError(string.Format("This accessory has not been given a accessory type. GameObjectName({0})", gameObject.name));
        }

        // Register self with parent facility
        Transform cParent = transform.parent;

        for (int i = 0; i < 20; ++i)
        {
            if (cParent != null)
            {
                if (cParent.GetComponent<CFacilityInterface>() != null)
                {
                    cParent.GetComponent<CFacilityInterface>().RegisterAccessory(this);
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
	}


	void Update()
	{
	}


// Member Fields


    public EType m_eAccessoryType = EType.INVALID;


    static Dictionary<EType, List<GameObject>> s_mAccessoryObjects = new Dictionary<EType, List<GameObject>>();
    static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


};
