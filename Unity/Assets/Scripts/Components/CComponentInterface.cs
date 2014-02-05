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
		FluidComp,
		CalibratorComp,
		CircuitryComp,
		RatchetComp,

        MAX
    }


// Member Delegates & Events
	// create the delegates


	public delegate void NotifyComponentStateChange(CComponentInterface _Sender);

	public event NotifyComponentStateChange EventComponentBreak;
	public event NotifyComponentStateChange EventComponentFix;

	public delegate void NotifyHealthChange(CComponentInterface _Sender, CActorHealth _SenderHealth);

	public event NotifyHealthChange EventHealthChange;

	public bool IsFunctional
	{
		get { return m_bIsFunctional.Get(); }
	}


// Member Properties


    public EType ComponentType
    {
        get { return (m_eComponentType); }
    }


// Member Methods
	public override void InstanceNetworkVars (CNetworkViewRegistrar _cRegistrar)
	{
		m_bIsFunctional = _cRegistrar.CreateNetworkVar(OnNetworkVarSync, true);
	}


	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if(_cSyncedNetworkVar == m_bIsFunctional)
		{
			if(IsFunctional)
			{
				if(EventComponentFix != null)
				{
					EventComponentFix(this);
				}	
			}
			else
			{
				if(EventComponentBreak != null)
				{
					EventComponentBreak(this);
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


	void Awake()
	{
		// Register health change from the CActorHealth
		gameObject.GetComponent<CActorHealth>().EventOnSetHealth += OnHealthChange;
	}


	void Start()
	{
        // Ensure a type of defined 
        if (m_eComponentType == EType.INVALID)
        {
            Debug.LogError(string.Format("This component has not been given a component type. GameObjectName({0})", gameObject.name));
        }

		// Register self with parent module
		CModuleInterface mi = CUtility.FindInParents<CModuleInterface>(gameObject);
		
		if(mi != null)
		{
			mi.RegisterAttachedComponent(this);
		}
		else
		{
			Debug.LogError("Could not find module to register to");
		}

        if (EventHealthChange != null)
        {
            EventHealthChange(this, GetComponent<CActorHealth>());
        }
	}


	private void OnHealthChange(GameObject _Sender, float _PreviousHealth, float _CurrentHealth)
	{
        if (EventHealthChange != null)
        {
            EventHealthChange(this, GetComponent<CActorHealth>());
        }

		if(CNetwork.IsServer && _CurrentHealth == 0.0f)
		{
			m_bIsFunctional.Set(false);
		}
	}


	void Update() { }


// Member Fields


    public EType m_eComponentType = EType.INVALID;


    static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();

	CNetworkVar<bool> m_bIsFunctional;
};
