//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerCockpitBehaviour.cs
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


public class CPlayerCockpitBehaviour : CNetworkMonoBehaviour
{

// Member Types


    [ABitSize(4)]
    public enum ENetworkAction
    {
        INVALID,
    
        MountCockpit,
        UnmountCockpit,
    }


// Member Delegates & Events


    public delegate void EnterCockpitHandler(CPlayerCockpitBehaviour _cSender, TNetworkViewId _tCockpitViewId);
    public event EnterCockpitHandler EventEnterCockpit;


    public delegate void LeaveCockpitHandler(CPlayerCockpitBehaviour _cSender);
    public event LeaveCockpitHandler EventLeaveCockpit;


// Member Properties


    public GameObject MountedCockpit
    {
        get 
        {
            if (!IsMounted)
                return (null);

            return (m_tMountedCockpitViewId.Value.GameObject); 
        }
    }


    public CCockpitInterface MountedCockpitInterface
    {
        get
        {
            if (!IsMounted)
                return (null);

            return (MountedCockpit.GetComponent<CCockpitInterface>());
        }
    }


    public bool IsMounted
    {
        get { return (m_tMountedCockpitViewId.Value != null); }
    }


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_tMountedCockpitViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync);
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        if (CGamePlayers.SelfActor == null)
            return;

        _cStream.Write(s_cNetworkStream);
        s_cNetworkStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        if (CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId) == null)
            return;

        CPlayerCockpitBehaviour cPlayerCockpitBehaviour = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerCockpitBehaviour>();

        while (_cStream.HasUnreadData)
        {
            // Extract network action
            ENetworkAction eNetworkAction = _cStream.Read<ENetworkAction>();

            // Process action
            switch (eNetworkAction)
            {
                case ENetworkAction.MountCockpit:
                    cPlayerCockpitBehaviour.HandleAttemptEnterCockpit(_cStream.Read<TNetworkViewId>());
                    break;

                case ENetworkAction.UnmountCockpit:
                    cPlayerCockpitBehaviour.HandleLeaveCockpit();
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eNetworkAction);
                    break;
            }
        }
    }


	void Start()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            m_cHudTurretCockpitControlInterface = CGameHUD.Hud2dInterface.TurretCockpitControlInterface;

            m_cHudTurretCockpitControlInterface.EventButtonEjectClicked += OnEventButtonEjectClicked;

            CUserInput.SubscribeInputChange(CUserInput.EInput.TurretMenu_ToggleDisplay, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Use, OnEventInput);
            
            GetComponent<CPlayerTurretBehaviour>().EventTakeTurretControl += OnEventTakeTurretControl;
            GetComponent<CPlayerTurretBehaviour>().EventReleaseTurretControl += OnEventReleaseTurretControl;
        }

        if (CNetwork.IsServer)
        {
            GetComponent<CPlayerHealth>().m_EventHealthStateChanged += OnEventHealthStateChange;
        }
	}


    void OnEventHealthStateChange(GameObject _cSourcePlayer, CPlayerHealth.HealthState _eNewState, CPlayerHealth.HealthState _ePreviousState)
    {
        if (_eNewState == CPlayerHealth.HealthState.DOWNED)
        {
            if (IsMounted)
            {
                // Eject currently player - we are subscribed to the dismount event
                MountedCockpitInterface.EjectPlayer();
            }
        }
    }


	void OnDestroy()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.TurretMenu_ToggleDisplay, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Use, OnEventInput);
        }

        if (CNetwork.IsServer)
        {
            // Unmount from cockpit
            if (IsMounted)
            {
                // Eject currently player - we are subscribed to the dismount event
                MountedCockpitInterface.EjectPlayer();
            }
        }
	}


	void Update()
	{
        // Empty
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_tMountedCockpitViewId)
        {
            HandleVarSyncCockpitViewId();
        }
    }


    [ALocalOnly]
    void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
    {
        switch (_eInput)
        {
            case CUserInput.EInput.Use:
                HandleUseInput(_bDown);
                break;

            case CUserInput.EInput.TurretMenu_ToggleDisplay:
                break;

            default:
                Debug.LogError("Unknown input: " + _eInput);
                break;
        }
    }


    [AServerOnly]
    void OnEventCockpitDismount(CCockpitInterface _cSender, ulong _ulPlayerId)
    {
        if (GetComponent<CPlayerInterface>().PlayerId != _ulPlayerId)
            Debug.LogError("Player id mismatch, check event subscribing");

        m_tMountedCockpitViewId.Set(null);

        // Unsubscribe from event
        _cSender.EventDismounted -= OnEventCockpitDismount;
    }


    public void OnEventTakeTurretControl(CPlayerTurretBehaviour _cSender, TNetworkViewId _cTurretViewId)
    {
        
    }

    public void OnEventReleaseTurretControl(CPlayerTurretBehaviour _cSender)
    {

    }


    [ALocalOnly]
    public void OnEventButtonEjectClicked(CHudTurretCockpitInterface _cSender)
    {
        if (!IsMounted)
            return;

        s_cNetworkStream.Write(ENetworkAction.UnmountCockpit);
    }


    [ALocalOnly]
    void HandleUseInput(bool _bDown)
    {
        // Check key was down
        if (_bDown)
        {
            if (!IsMounted &&
                GetComponent<CPlayerHealth>().CurrentHealthState == CPlayerHealth.HealthState.ALIVE)
            {
                GameObject cTarget = GetComponent<CPlayerInteractor>().TargetActorObject;

                // Check has target
                if (cTarget == null)
                    return;

                // Check target is a cockpit
                CCockpitInterface cCockpitInterface = cTarget.GetComponent<CCockpitInterface>();

                if (cCockpitInterface == null)
                    return;

                // Check cockpit is already mounted
                if (cCockpitInterface.IsMounted)
                    return;

                s_cNetworkStream.Write(ENetworkAction.MountCockpit);
                s_cNetworkStream.Write(cCockpitInterface.NetworkViewId);
            }
        }
    }


    [AServerOnly]
    void HandleAttemptEnterCockpit(TNetworkViewId _tCockpitViewId)
    {
        CCockpitInterface cCockpitInterface = _tCockpitViewId.GameObject.GetComponent<CCockpitInterface>();

        bool bMounted = cCockpitInterface.MountPlayer(GetComponent<CPlayerInterface>().PlayerId);

        // Check mount attempt failed
        if (!bMounted)
            return;

        m_tMountedCockpitViewId.Set(_tCockpitViewId);

        // Sign up for cockpit dismounting
        cCockpitInterface.EventDismounted += OnEventCockpitDismount;
    }


    [AServerOnly]
    void HandleLeaveCockpit()
    {
        if (!IsMounted)
            return;

        CCockpitInterface cCockpitInterface = m_tMountedCockpitViewId.Value.GameObject.GetComponent<CCockpitInterface>();

        // Eject currently player - we are subscribed to the dismount event
        cCockpitInterface.EjectPlayer();
    }


    void HandleVarSyncCockpitViewId()
    {
        if (m_tMountedCockpitViewId.Value != null)
        {
            // Lock movement and rotations
            GetComponent<CPlayerMotor>().DisableInput(this);
            GetComponent<CPlayerHead>().DisableInput(this);

            m_cActiveCockpitInterface = m_tMountedCockpitViewId.Value.GameObject.GetComponent<CCockpitInterface>();

            if (GetComponent<CPlayerInterface>().IsOwnedByMe &&
                m_cActiveCockpitInterface.CockpitType == CCockpitInterface.EType.Turret)
            {
                CGameHUD.Hud2dInterface.ShowHud(CHud2dInterface.EHud.TurretCockpitMenu);
            }

            if (EventEnterCockpit != null)
                EventEnterCockpit(this, m_tMountedCockpitViewId.Value);
        }
        else
        {
            // Unlock movement and rotations
            GetComponent<CPlayerMotor>().EnableInput(this);
            GetComponent<CPlayerHead>().EnableInput(this);

            m_cActiveCockpitInterface = null;

            if (GetComponent<CPlayerInterface>().IsOwnedByMe)
            {
                CGameHUD.Hud2dInterface.HideHud(CHud2dInterface.EHud.TurretCockpitMenu);
            }

            if (EventLeaveCockpit != null)
                EventLeaveCockpit(this);
        }
    }


// Member Fields


    CNetworkVar<TNetworkViewId> m_tMountedCockpitViewId = null;

    CCockpitInterface m_cActiveCockpitInterface = null;
    CHudTurretCockpitInterface m_cHudTurretCockpitControlInterface = null;


    static CNetworkStream s_cNetworkStream = new CNetworkStream();


};
