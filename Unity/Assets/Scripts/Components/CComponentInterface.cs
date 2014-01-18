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


public class CComponentInterface : MonoBehaviour
{

// Member Types


    public enum EType
    {
        INVALID,

        CellSlot,
        FuseBox,
        CircuitBox,

        MAX
    }


// Member Delegates & Events


// Member Properties


    public EType ComponentType
    {
        get { return (m_eComponentType); }
    }


// Member Methods


    public static void RegisterPrefab(EType _eComponentType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_eComponentType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eModuleType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_eModuleType))
        {
            Debug.LogError(string.Format("Component type ({0}) has not been registered a prefab", _eModuleType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_eModuleType]);
    }


	void Start()
	{
        // Ensure a type of defined 
        if (m_eComponentType == EType.INVALID)
        {
            Debug.LogError(string.Format("This component has not been given a component type. GameObjectName({0})", gameObject.name));
        }

        // Register self with parent facility
        Transform cParent = transform.parent;

        for (int i = 0; i < 20; ++i)
        {
            if (cParent != null)
            {
                if (cParent.GetComponent<CModuleInterface>() != null)
                {
                    cParent.GetComponent<CModuleInterface>().RegisterAttachedComponent(this);
                    break;
                }

                cParent = cParent.parent;
            }

            if (i == 19)
            {
                Debug.LogError("Could not find module to register to");
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


    public EType m_eComponentType = EType.INVALID;


    static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


};
