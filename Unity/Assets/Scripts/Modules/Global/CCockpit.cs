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
public class CCockpit : CNetworkMonoBehaviour
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
		get { return (m_cMountedPlayerId.Get()); }
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
				return (CNetwork.Factory.FindObject(CGamePlayers.GetPlayerActorViewId(MountedPlayerId)));
			}
		}
	}


	public bool IsMounted
	{
		get { return (MountedPlayerId != 0); }
	}


	public GameObject SeatObject
	{
		get { return (m_cSeat); }
	}

	
// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_cMountedPlayerId = _cRegistrar.CreateReliableNetworkVar<ulong>(OnNetworkVarSync, 0);
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

            // Parent the player to the cockpit seat
            cPlayerActor.GetComponent<CNetworkView>().SetParent(m_cSeat.GetComponent<CNetworkView>().ViewId);

            // Set the player kinematic
            cPlayerActor.rigidbody.isKinematic = true;

            m_cMountedPlayerId.Set(_ulPlayerId);

            // Notify observers
            if (EventMounted != null) EventMounted(m_cMountedPlayerId.Get());
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

                // Turn of kinematic
                cPlayerActor.rigidbody.isKinematic = false;
            }

            m_cMountedPlayerId.Set(0);
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

            GameObject cCockpitObject = CNetwork.Factory.FindObject(cCockpitObjectViewId);

            CCockpit cCockpit = cCockpitObject.GetComponent<CCockpit>();

            switch (eAction)
            {
                case ENetworkAction.EnterCockpit:
                    cCockpit.EnterCockpit(_cNetworkPlayer.PlayerId);
                    break;

                case ENetworkAction.LeaveCockpit:
                    {
                        if (_cNetworkPlayer.PlayerId == cCockpit.m_cMountedPlayerId.Value)
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
        if (_cSynedVar == m_cMountedPlayerId)
        {
            // Check player dismounted the cockpit
            if (m_cMountedPlayerId.Value == 0)
            {
                // Notify player left
                if (EventDismounted != null)
                    EventDismounted(m_cMountedPlayerId.PreviousValue);
            }

            // Player entered the cockput
            else
            {
                // Notify player enter
                if (EventMounted != null)
                    EventMounted(m_cMountedPlayerId.Value);
            }

            // Check entering player was myself
            if (m_cMountedPlayerId.Value == CNetwork.PlayerId)
            {
                // Disable inputs
                CGamePlayers.SelfActor.GetComponent<CPlayerMotor>().DisableInput(this);
                CGamePlayers.SelfActor.GetComponent<CPlayerHead>().DisableInput(this);


                m_vLocalEnterPosition = CGamePlayers.SelfActor.transform.position;
                m_qLocalEnterRotation = CGamePlayers.SelfActor.transform.rotation;
                m_qLocalHeadEnterRotation = CGamePlayers.SelfActor.GetComponent<CPlayerHead>().Head.transform.localRotation;


                CGamePlayers.SelfActor.transform.position = m_cSeat.transform.position;
                CGamePlayers.SelfActor.transform.rotation = m_cSeat.transform.rotation;

                CGamePlayers.SelfActor.GetComponent<CPlayerHead>().Head.transform.LookAt(m_cLookAt.position);
            }

            // Check exiting player was myself
            if (m_cMountedPlayerId.PreviousValue == CNetwork.PlayerId &&
                CGamePlayers.SelfActor != null)
            {
                // Reenable intputs
                CGamePlayers.SelfActor.GetComponent<CPlayerMotor>().EnableInput(this);
                CGamePlayers.SelfActor.GetComponent<CPlayerHead>().EnableInput(this);

                // Move player back to positions when entered
                CGamePlayers.SelfActor.transform.position = m_vLocalEnterPosition;
                CGamePlayers.SelfActor.transform.rotation = m_qLocalEnterRotation;
                CGamePlayers.SelfActor.GetComponent<CPlayerHead>().Head.transform.localRotation = m_qLocalHeadEnterRotation;
            }
        }
    }


// Member Fields


	public GameObject m_cSeat = null;
	public Transform m_cLookAt = null;


	CNetworkVar<ulong> m_cMountedPlayerId = null;


    CModuleInterface m_cModuleInterface = null;


    Vector3 m_vLocalEnterPosition = Vector3.zero;
    Quaternion m_qLocalEnterRotation = Quaternion.identity;
    Quaternion m_qLocalHeadEnterRotation = Quaternion.identity;


    public CComponentInterface[] m_Components;

	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
