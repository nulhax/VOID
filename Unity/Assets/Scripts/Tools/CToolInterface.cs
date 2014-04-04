//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CToolInterface.cs
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

//This Script contains all the tool info, such as:
//		an enum for what type of tool the tool is
//		an ammo counter?
//		slots taken up
//		
//This Script can:
//		Get the type of tool
//		Shoot the Tool
//		Reload the Tool


[RequireComponent(typeof(CActorInteractable))]
[RequireComponent(typeof(CActorBoardable))]
[RequireComponent(typeof(CActorGravity))]
public class CToolInterface : CNetworkMonoBehaviour
{

// Member Types


	public enum EType
	{
		INVALID,

		Ratchet,
		CircuitryKit,
		Calibrator,
		Fluidizer,
		ModuleCreator,
		FireExtinguisher,
		Norbert,
		HealingKit,
		AK47,
		MiningDrill,

		MAX
	}


    [ABitSize(4)]
	public enum ENetworkAction : byte
	{
        INVALID,

		PickUp,

        MAX
	}


// Member Delegates & Events


    [ALocalOnly]
    public delegate void NotifyPrimaryActiveChange(bool _bActive);
    public event NotifyPrimaryActiveChange EventPrimaryActiveChange;

    [ALocalOnly]
    public delegate void NotifySecondaryActiveChange(bool _bActive);
	public event NotifySecondaryActiveChange EventSecondaryActiveChange;

    [ALocalOnly]
    public delegate void NotifyEquippedChange(bool _bEquipped);
    public event NotifyEquippedChange EventEquippedChange;

    public delegate void NotifyToolEvent();
    public event NotifyToolEvent EventUse;
	public event NotifyToolEvent EventReload;
    public event NotifyToolEvent EventPickedUp;
    public event NotifyToolEvent EventDropped;


// Member Properties
	
	
	public EType ToolType
    {
        get { return (m_eToolType); }
    }
    

    public GameObject OwnerPlayerActor
    {
		get
		{
            if (!IsOwned)
            {
                return (null);
            }

			return (CGamePlayers.GetPlayerActor(OwnerPlayerId)); 
		}
    }


	public ulong OwnerPlayerId
	{
		get { return (m_ulOwnerPlayerId.Get()); }
	}


    public bool IsOwned
    {
		get { return (m_ulOwnerPlayerId.Get() != 0); }
    }


    [ALocalOnly]
    public bool IsEquiped
    {
        get { return (m_bEquipped); }
    }


    [ALocalOnly]
    public bool IsPrimaryActive
    {
        get { return (m_bPrimaryActive); }
    }


    [ALocalOnly]
    public bool IsSeconaryActive
    {
        get { return (m_bSecondaryActive); }
    }


// Member Functions


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_ulOwnerPlayerId = _cRegistrar.CreateReliableNetworkVar<ulong>(OnNetworkVarSync, 0);
    }


	public void Awake()
    {
		// Empty
	}


	public void Start()
	{
		if (m_eToolType == EType.INVALID)
        {
            Debug.LogError(string.Format("This tool has not been given a tool type. GameObjectName({0})", gameObject.name));
        }

        EventDropped += () =>
        {
            if (m_bPrimaryActive)
            {
                SetPrimaryActive(false);
            }

            if (m_bSecondaryActive)
            {
                SetSecondaryActive(false);
            }
        };
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		if (IsOwned)
        { 
            Transform ActorHead = OwnerPlayerActor.GetComponent<CPlayerHead>().Head.transform;
            gameObject.transform.rotation = ActorHead.rotation;
        }
	}


    [ALocalOnly]
    public void SetEquipped(bool _bEquipped)
    {
        m_bEquipped = _bEquipped;

        if (EventEquippedChange != null) EventEquippedChange(m_bEquipped);
    }


    [ALocalOnly]
    public void SetPrimaryActive(bool _bActive)
    {
        m_bPrimaryActive = _bActive;

        if (EventPrimaryActiveChange != null) EventPrimaryActiveChange(_bActive);
    }


    [ALocalOnly]
    public void SetSecondaryActive(bool _bActive)
    {
        m_bSecondaryActive = _bActive;

        if (EventSecondaryActiveChange != null) EventSecondaryActiveChange(_bActive);
    }


    [AServerOnly]
    public void Reload()
    {
        Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

        // Check currently held
        if (IsOwned)
        {
            m_bReloading.Set(true);
        }
    }


	[AServerOnly]
	public void SetOwner(ulong _ulPlayerId)
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		if (!IsOwned)
		{
            // Set owner player
			m_ulOwnerPlayerId.Set(_ulPlayerId);
		}
	}


	[AServerOnly]
	public void SetDropped()
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		// Check currently held
		if (IsOwned)
		{
            // Set owning object view id
            m_ulOwnerPlayerId.Set(0);
		}
	}


    [AServerOnly]
    public void SetVisible(bool _bVisible)
    {
        if (_bVisible)
        {
            foreach (Renderer cRenderer in transform.GetComponentsInChildren<Renderer>())
            {
                cRenderer.enabled = true;
            }
        }
        else
        {
            foreach (Renderer cRenderer in transform.GetComponentsInChildren<Renderer>())
            {
                cRenderer.enabled = false;
            }
        }
    }


    public static void RegisterPrefab(EType _ToolType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_ToolType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _ToolType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_ToolType))
        {
            Debug.LogError(string.Format("Tool type ({0}) has not been registered a prefab", _ToolType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_ToolType]);
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_ulOwnerPlayerId)
        {
            if (IsOwned)
            {
                if (!OwnerPlayerActor.GetComponent<CPlayerInterface>().IsOwnedByMe)
                {
                    GameObject cOwnerPlayerActor = OwnerPlayerActor;

                    Transform[] children = OwnerPlayerActor.GetComponentsInChildren<Transform>();
                    foreach (Transform child in children)
                    {
                        if (child.name == "RightHandIndex2")
                        {
                            gameObject.transform.parent = child;
                        }
                    }

                    if (gameObject.transform.parent.gameObject == null)
                    {
                        Debug.LogError("Could not find right hand transform of player model!");
                    }

                    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                }
                else
                {
                }

                // Turn off dynamic physics
                if (CNetwork.IsServer)
                {
                    rigidbody.isKinematic = true;
                    rigidbody.detectCollisions = false;
                }

                // Stop receiving synchronizations
                GetComponent<CActorNetworkSyncronized>().m_SyncPosition = false;
                GetComponent<CActorNetworkSyncronized>().m_SyncRotation = false;

                // Notify observers
                if (EventPickedUp != null) EventPickedUp();
            }
            else
            {
                // Turn on dynamic physics
                if (CNetwork.IsServer)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.detectCollisions = true;
                }

                rigidbody.AddForce(transform.forward * 5.0f, ForceMode.VelocityChange);
                rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.VelocityChange);

                // Receive synchronizations
                GetComponent<CActorNetworkSyncronized>().m_SyncPosition = true;
                GetComponent<CActorNetworkSyncronized>().m_SyncRotation = true;

                if (m_bPrimaryActive)
                {
                    SetPrimaryActive(false);
                }

                if (m_bSecondaryActive)
                {
                    SetSecondaryActive(false);
                }

                // Notify observers
                if (EventDropped != null) EventDropped();
            }
        }
        else if (_cVarInstance == m_bReloading)
        {
            if (m_bReloading.Get())
            {
                // Notify observers
                if (EventReload != null) EventReload();
            }
        }
    }


// Member Fields

	public EType m_eToolType = EType.INVALID;


    CNetworkVar<ulong> m_ulOwnerPlayerId = null;
    CNetworkVar<bool> m_bReloading = null;


    bool m_bEquipped = false;
    bool m_bPrimaryActive = false;
    bool m_bSecondaryActive = false;


	static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


};
