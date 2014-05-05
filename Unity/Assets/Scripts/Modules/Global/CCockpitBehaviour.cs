//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCockpit.cs
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

[RequireComponent(typeof(CActorInteractable))]
public class CCockpitBehaviour : CNetworkMonoBehaviour
{

// Member Types


    [ABitSize(4)]
	public enum ENetworkAction
	{
        INVALID,

		EnterCockpit,
		LeaveCockpit
	}


// Member Delegates & Events

	
	public delegate void HandlePlayerEnter(ulong _ulPlayerId);
	public event HandlePlayerEnter EventMounted;

	
	public delegate void HandlePlayerLeave(ulong _ulPlayerId);
	public event HandlePlayerLeave EventDismounted;


// Member Properties


	public ulong MountedPlayerId
	{
		get { return (m_ulMountedPlayerId.Value); }
	}


	public GameObject MountedPlayerActor
	{
		get
		{
			if (!IsMounted)
			{
				return (null);
			}
			else
			{
				return (CNetwork.Factory.FindGameObject(CGamePlayers.GetPlayerActorViewId(MountedPlayerId)));
			}
		}
	}


	public bool IsMounted
	{
		get { return (MountedPlayerId != 0); }
	}


	public Transform Seat
	{
		get { return (m_cSeat); }
	}

	
// Member Methods


	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		m_ulMountedPlayerId = _cRegistrar.CreateReliableNetworkVar<ulong>(OnNetworkVarSync, 0);
	}


    [AServerOnly]
    void EnterCockpit(ulong _ulPlayerId)
    {
        GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_ulPlayerId);

        if ( cPlayerActor != null && 
            !IsMounted &&
             m_cModuleInterface.IsBuilt)
        {
            TNetworkViewId cPlayerActorViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

            // Save the position, rotation and head rotation
            m_vRemoteEnterPosition = cPlayerActor.transform.position;
            m_vRemoteEnterEuler = cPlayerActor.transform.eulerAngles;
            m_vRemoteHeadEuler = new Vector3(cPlayerActor.GetComponent<CPlayerHead>().RemoteHeadEulerX,
                                             cPlayerActor.GetComponent<CPlayerHead>().RemoteHeadEulerY,
                                             0.0f);

            // Move player to seat location and rotation
            cPlayerActor.GetComponent<CNetworkView>().SetParent(m_cSeat.GetComponent<CNetworkView>().ViewId);
            cPlayerActor.GetComponent<CNetworkView>().SetLocalPosition(0.0f, 0.0f, 0.0f);
            cPlayerActor.GetComponent<CNetworkView>().SetLocalEuler(0.0f, 0.0f, 0.0f);
            cPlayerActor.GetComponent<CPlayerHead>().SetLookDirection(0.0f, 0.0f);
            
            // Set the player kinematic
            cPlayerActor.rigidbody.isKinematic = true;

            m_ulMountedPlayerId.Set(_ulPlayerId);
        }
    }


    [ALocalOnly]
    public void EjectPlayer()
    {
        // Allow player to leave cockpit
        if (MountedPlayerId != 0)
        {
            // Teleport player back to entered position
            GameObject cPlayerActor = CGamePlayers.GetPlayerActor(MountedPlayerId);

            if (cPlayerActor != null)
            {
                // UnParent the player to the cockpit seat
                cPlayerActor.GetComponent<CNetworkView>().SetParent(null);

                // Move player back to positions when entered
                cPlayerActor.GetComponent<CNetworkView>().SetPosition(m_vRemoteEnterPosition);
                cPlayerActor.GetComponent<CNetworkView>().SetEuler(m_vRemoteEnterEuler);
                cPlayerActor.GetComponent<CNetworkView>().GetComponent<CPlayerHead>().SetLookDirection(m_vRemoteHeadEuler.x, m_vRemoteHeadEuler.y);

                // Turn of kinematic
                cPlayerActor.rigidbody.isKinematic = false;
            }

            m_ulMountedPlayerId.Set(0);
        }
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            TNetworkViewId cCockpitObjectViewId = _cStream.Read<TNetworkViewId>();
            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            GameObject cCockpitObject = CNetwork.Factory.FindGameObject(cCockpitObjectViewId);

            CCockpitBehaviour cCockpit = cCockpitObject.GetComponent<CCockpitBehaviour>();

            switch (eAction)
            {
                case ENetworkAction.EnterCockpit:
                    cCockpit.EnterCockpit(_cNetworkPlayer.PlayerId);
                    break;

                case ENetworkAction.LeaveCockpit:
                    {
                        if (_cNetworkPlayer.PlayerId == cCockpit.m_ulMountedPlayerId.Value)
                        {
                            cCockpit.EjectPlayer();
                        }
                    }
                    break;

                default:
                    Debug.LogError(string.Format("Unknown network action ({0})"));
                    break;
            }
        }
    }


    void Awake()
    {
        m_cModuleInterface = GetComponent<CModuleInterface>();
    }


	void Start()
	{
		// Sign up for event
		gameObject.GetComponent<CActorInteractable>().EventUse += OnEventInteractionUse;
		CNetwork.Server.EventPlayerDisconnect += OnPlayerDisconnect;

        CUserInput.SubscribeInputChange(CUserInput.EInput.Use, OnInputUse);

        if (CNetwork.IsServer)
        {
            
        }
	}


	void OnDestroy()
	{
        gameObject.GetComponent<CActorInteractable>().EventUse -= OnEventInteractionUse;
	}


	void Update()
	{
        // Empty
	}


	[ALocalOnly]
    void OnInputUse(CUserInput.EInput _eInput, bool _bDown)
	{
		if (_bDown &&
		    MountedPlayerId == CNetwork.PlayerId)
		{
            // Request to be unmounted from the cockpit
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write(ENetworkAction.LeaveCockpit);
		}
	}

	
	[ALocalOnly]
    void OnEventInteractionUse(RaycastHit _tRayHit, TNetworkViewId _cPlayerActorViewId, bool _bDown)	
	{
		// Check there is no one in the cockpit locally
		if ( _bDown &&
            !IsMounted)
		{
			// Request to enter the cockpit
			s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
			s_cSerializeStream.Write(ENetworkAction.EnterCockpit);
		}
	}


	[AServerOnly]
	void OnPlayerDisconnect(CNetworkPlayer _cNetworkPlayer)
	{
		if (MountedPlayerId == _cNetworkPlayer.PlayerId)
		{
            EjectPlayer();
		}
	}


    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        if (_cSynedVar == m_ulMountedPlayerId)
        {
            HandleVarSyncMountedPlayer();
        }
    }


    void HandleVarSyncMountedPlayer()
    {
        // Check player dismounted the cockpit
        if (m_ulMountedPlayerId.Value == 0)
        {
            GameObject cPlayerActor = CGamePlayers.GetPlayerActor(m_ulMountedPlayerId.PreviousValue);
            cPlayerActor.GetComponent<CPlayerMotor>().EnableInput(this);
            cPlayerActor.GetComponent<CPlayerHead>().EnableInput(this);

            // Notify obersvers
            if (EventDismounted != null)
                EventDismounted(m_ulMountedPlayerId.PreviousValue);

            if (CNetwork.IsServer &&
                m_ulMountedPlayerId.PreviousValue != 0)
            {
                CGamePlayers.GetPlayerActor(m_ulMountedPlayerId.PreviousValue).GetComponent<CPlayerCockpitBehaviour>().SetMountedCockpitViewId(null);
            }
        }

        // Player entered the cockput
        else
        {
            GameObject cPlayerActor = CGamePlayers.GetPlayerActor(m_ulMountedPlayerId.Value);
            cPlayerActor.GetComponent<CPlayerMotor>().DisableInput(this);
            cPlayerActor.GetComponent<CPlayerHead>().DisableInput(this);

            // Notify observers
            if (EventMounted != null)
                EventMounted(m_ulMountedPlayerId.Value);

            if (CNetwork.IsServer &&
                m_ulMountedPlayerId.PreviousValue == CNetwork.PlayerId)
            {
                CGamePlayers.SelfActor.GetComponent<CPlayerCockpitBehaviour>();
            }

            if (CNetwork.IsServer)
            {
                CGamePlayers.GetPlayerActor(m_ulMountedPlayerId.Value).GetComponent<CPlayerCockpitBehaviour>().SetMountedCockpitViewId(NetworkViewId);
            }
        }
    }


// Member Fields


    public Transform m_cSeat = null;
    public CComponentInterface[] m_Components;


	CNetworkVar<ulong> m_ulMountedPlayerId = null;


    CModuleInterface m_cModuleInterface = null;


    Vector3 m_vRemoteEnterPosition = Vector3.zero;
    Vector3 m_vRemoteEnterEuler = Vector3.zero;
    Vector3 m_vRemoteHeadEuler = Vector3.zero;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
