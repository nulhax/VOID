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

		Fluid,
		Calibrator,
		Circuitry,
		Mechanical,

        MAX
    }


// Member Delegates & Events


	public delegate void BrokenStateChangeHandler(CComponentInterface _cSender, bool _bBroken);
	public event BrokenStateChangeHandler EventBreakStateChange;


// Member Properties


    public EType ComponentType
    {
        get { return (m_eComponentType); }
    }


    public bool IsBroken
    {
        get { return (m_bBroken.Value); }
    }


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_bBroken = _cRegistrar.CreateReliableNetworkVar(OnNetworkVarSync, true);
	}


    [AServerOnly]
    public void TriggerMalfunction()
    {
        // Explosion GameObject
        GameObject Explosion = GameObject.Instantiate(Resources.Load("Prefabs/Accessories/Explosions/Small explosion")) as GameObject;

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

        // Set up the explosion game object
        Explosion.particleSystem.transform.parent = gameObject.transform;
        Explosion.particleSystem.transform.localPosition = transform.localPosition;
        Explosion.particleSystem.transform.localRotation = transform.localRotation;
        Explosion.particleSystem.transform.localScale = transform.localScale;

        // Explode!
        Explosion.particleEmitter.emit = true;
        Destroy(Explosion, 3.0f);

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
                FireNode.health.health = 0;
            }
        }

        // Trigger an 'explosion' centred around the local transform
        // Note: Final values will need to be adjusted. Specifically the impulse.
        CGameShips.GalaxyShip.GetComponent<CShipDamageOnCollision>().CreateExplosion(transform.position, fExplosionRadius, 100000.0f);
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
        // Register self with parent module
        CModuleInterface cModuleInterface = CUtility.FindInParents<CModuleInterface>(gameObject);

        if (cModuleInterface == null)
            Debug.LogError(name + " has CComponentInterface, but could not find CModuleInterface in parent GameObjects.");

        cModuleInterface.RegisterAttachedComponent(this);
    }


	void Start()
	{
        // Ensure a type of defined 
        if (m_eComponentType == EType.INVALID)
            Debug.LogError(string.Format("This component has not been given a component type. GameObjectName({0})", gameObject.name));


        // Register health change from the CActorHealth
        GetComponent<CActorHealth>().EventOnSetHealth += OnEventHealthChange;

        EventBreakStateChange += OnEventBreakStateChange;
	}


    void OnEventHealthChange(CActorHealth _cSender, float _fPreviousHealth, float _fNewHealth)
    {
        if (CNetwork.IsServer)
        {
            m_bBroken.Value = (_fNewHealth == 0.0f);
        }

        transform.FindChild("Model").renderer.material.color = Color.Lerp(Color.red, Color.magenta, _fNewHealth / _cSender.health_initial);
    }


    // Do the functionality in the on break. This will start when the eventcomponentbreak is triggered
    void OnEventBreakStateChange(CComponentInterface _cSender, bool _bBroken)
    {
        if (_bBroken)
        {
            //TODO swap between broken to fixed
            if (gameObject.GetComponent<CAudioCue>() != null)
                gameObject.GetComponent<CAudioCue>().StopAllSound();
        }
        else
        {
            // TODO: swap between fixed to broken
            if (gameObject.GetComponent<CAudioCue>() != null)
                gameObject.GetComponent<CAudioCue>().Play(0.3f, true, 0);
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    {
        if (_cSyncedNetworkVar == m_bBroken)
        {
            if (EventBreakStateChange != null)
                EventBreakStateChange(this, m_bBroken.Value);
        }
    }


// Member Fields


    public EType m_eComponentType = EType.INVALID;


	CNetworkVar<bool> m_bBroken = null;


    static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


};
