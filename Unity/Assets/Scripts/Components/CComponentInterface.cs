//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CComponentInterface.cs
//  Description :   --------------------------
//
//  Author  	:  Scott Emery & 
//  Mail    	:  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CComponentInterface : CNetworkMonoBehaviour
{

// Member Types


    public enum EType
    {
        INVALID,

		// Old Systems
        //CellSlot,
        //FuseBox,
        //CircuitBox,

		// Components
		LiquidComp,
		CalibratorComp,
		WiringComp,
		RatchetComp,

        MAX
    }


// Member Delegates & Events
	// create the delegates


	public delegate void NotifyComponentStateChange();

	public event NotifyComponentStateChange EventComponentBreak;
	public event NotifyComponentStateChange EventComponentFix;

	public delegate void NotifyHealthChange(float _fHealth);

	public event NotifyHealthChange EventHealthChange;

	public bool IsFunctional
	{
		get { return m_bIsFunctional.Get (); }

		[AServerOnly]
		set { m_bIsFunctional.Set (value); 	}
	}

// Member Properties


    public EType ComponentType
    {
        get { return (m_eComponentType); }
    }


// Member Methods
	public override void InstanceNetworkVars (CNetworkViewRegistrar _cRegistrar)
	{
		m_bIsFunctional = new CNetworkVar<bool>(OnNetworkVarSync, true);
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if(CNetwork.IsServer)
		{
			if(m_bIsFunctional.Get ())
			{
				if(EventComponentFix != null)
				{
					EventComponentFix();
				}	
			}
			else
			{
				if(EventComponentBreak != null)
				{
					EventComponentBreak();
				}
			}
		}

	}



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

		// Register self with parent module
		CModuleInterface mi = NGUITools.FindInParents<CModuleInterface>(gameObject);
		
		if(mi != null)
		{
			mi.RegisterAttachedComponent(this);
		}
		else
		{
			Debug.LogError("Could not find module to register to");
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

	CNetworkVar<bool>m_bIsFunctional;
};
