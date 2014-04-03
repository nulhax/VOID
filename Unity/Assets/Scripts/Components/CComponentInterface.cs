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
		MechanicalComp,

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
		m_bIsFunctional = _cRegistrar.CreateReliableNetworkVar(OnNetworkVarSync, true);
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
		GetComponent<CActorHealth>().EventOnSetHealth += OnHealthChange;
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
			Debug.LogError(name + " has CComponentInterface, but could not find CModuleInterface in parent GameObjects.");
		}

        if (EventHealthChange != null)
        {
            EventHealthChange(this, GetComponent<CActorHealth>());
        }
	}


	private void OnHealthChange(/*GameObject _Sender, */float _PreviousHealth, float _CurrentHealth)
	{
        if (EventHealthChange != null)
        {
            EventHealthChange(this, GetComponent<CActorHealth>());
        }

		if(CNetwork.IsServer) 
		{
			if(_CurrentHealth == 0.0f)
			{
				m_bIsFunctional.Set(false);
			}
			else if(_CurrentHealth == /*_Sender.*/GetComponent<CActorHealth>().health_max)
			{
				m_bIsFunctional.Set(true);
			}
		}
	}


    // Public trigger function for use by CHazardSystem
    [AServerOnly]
    public void TriggerMalfunction()
    {
        // TODO: Data-drive this variable
        float fExplosionRadius = 2.0f;

        // NOTES:
        //      Consider randomising the chance for an explosion.
        //      Consider randomising explosion radius.
        //      Consider other potential effects beyond explosions.
        //      Consider damaging both components and fire nodes instead of instant destruction
        //      Consider different actions based on component type. 
        
        // Set component health to 0
        GetComponent<CActorHealth>().health = 0;

        // Get all of the fire hazard nodes on the ship
        CFireHazard[] ArrayFires = CGameShips.Ship.GetComponentsInChildren<CFireHazard>();

        // For each fire hazard node
        foreach (CFireHazard FireNode in ArrayFires)
        {
            // If the distance between the fire node and the current component
            // is less than or equal to the explosion radius
            if ((transform.position - FireNode.transform.position).magnitude <= fExplosionRadius)
            {
                // Set the node's health to 0
                // Sets the node on fire
                FireNode.GetComponent<CActorHealth>().health = 0;
            }
        }

        // Trigger an 'explosion' centred around the local transform
        // Note: Final values will need to be adjusted. Specifically the impulse.
        CGameShips.GalaxyShip.GetComponent<CShipDamageOnCollision>().ApplyExplosiveDamage(transform.position, fExplosionRadius, 100000.0f);
    }

	void Update() { }


// Member Fields
    public EType m_eComponentType = EType.INVALID;

    static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();

	CNetworkVar<bool> m_bIsFunctional;
};
