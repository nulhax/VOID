//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerTurretBehaviour.cs
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


public class CPlayerTurretBehaviour : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        INVALID,

        TakeTurretControl,
        ReleaseTurretControl,
    }


// Member Delegates & Events


    public delegate void TakeTurretControlHandler(CPlayerTurretBehaviour _cSender, TNetworkViewId _cTurretViewId);
    public event TakeTurretControlHandler EventTakeTurretControl;


    public delegate void ReleaseTurretControlHandler(CPlayerTurretBehaviour _cSender);
    public event ReleaseTurretControlHandler EventReleaseTurretControl;


// Member Properties


    public GameObject ControlledTurret
    {
        get
        {
            if (!HasTurretControl)
                return (null);

            return (m_cTurretViewId.Value.GameObject);
        }
    }


    public CTurretInterface ControlledTurretInterface
    {
        get
        {
            if (!HasTurretControl)
                return (null);

            return (ControlledTurret.GetComponent<CTurretInterface>());
        }
    }


    public bool HasTurretControl
    {
        get { return (m_cTurretViewId.Value != null); }
    }


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_cTurretViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
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

        CPlayerTurretBehaviour cPlayerTurretBehaviour = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerTurretBehaviour>();

        while (_cStream.HasUnreadData)
        {
            // Extract network action
            ENetworkAction eNetworkAction = _cStream.Read<ENetworkAction>();

            // Process action
            switch (eNetworkAction)
            {
                case ENetworkAction.TakeTurretControl:
                    cPlayerTurretBehaviour.HandleAttemptTakeTurretControl(_cStream.Read<TNetworkViewId>());
                    break;

                case ENetworkAction.ReleaseTurretControl:
                    cPlayerTurretBehaviour.HandleReleaseTurretControl();
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
            CUserInput.SubscribeInputChange(CUserInput.EInput.Use, OnEventInput);

            CGameHUD.Hud2dInterface.TurretCockpitControlInterface.EventButtonTakeControlPressed += OnEventButtonTakeControlPressed;
        }

        if (CNetwork.IsServer)
        {
            GetComponent<CPlayerCockpitBehaviour>().EventLeaveCockpit += OnEventLeaveCockpit;
        }
	}


	void OnDestroy()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Use, OnEventInput);
        }
	}


	void Update()
	{
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_cTurretViewId)
        {
            HandleTurretViewIdSync();
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


    void OnEventButtonTakeControlPressed(CHudTurretCockpitInterface _cSender, TNetworkViewId _cTurretViewId)
    {
        s_cNetworkStream.Write(ENetworkAction.TakeTurretControl);
        s_cNetworkStream.Write(_cTurretViewId);
    }


    void OnEventButtonReleaseControlPressed(CHudTurretCockpitInterface _cSender)
    {
        s_cNetworkStream.Write(ENetworkAction.ReleaseTurretControl);
    }


    [AServerOnly]
    void OnEventLeaveCockpit(CPlayerCockpitBehaviour _cSender)
    {
        if (!HasTurretControl)
            return;

        CTurretInterface cTurretInterface = m_cTurretViewId.Value.GameObject.GetComponent<CTurretInterface>();

        cTurretInterface.ReleaseControl();
    }


    [AServerOnly]
    void OnEventTurretControlReleased(CTurretInterface _cSender, ulong _ulPlayerId)
    {
        if (GetComponent<CPlayerInterface>().PlayerId != _ulPlayerId)
            Debug.LogError("Player id mismatch, check event subscribing");

        m_cTurretViewId.Set(null);

        // Unsubscribe from event
        _cSender.EventControlReleased -= OnEventTurretControlReleased;
    }


    [ALocalOnly]
    void HandleUseInput(bool _bDown)
    {
        // Check key was down
        if (_bDown)
        {
            if (HasTurretControl)
            {
                CGameHUD.Hud2dInterface.ToggleHud(CHud2dInterface.EHud.TurretCockpitMenu);
            }
        }
    }


    [AServerOnly]
    void HandleAttemptTakeTurretControl(TNetworkViewId _cTurretViewId)
    {
        // Release previous turret control
        if (HasTurretControl)
        {
            HandleReleaseTurretControl();
        }

        CTurretInterface cTurretInterface = _cTurretViewId.GameObject.GetComponent<CTurretInterface>();

        bool bControlTaken = cTurretInterface.TakeControl(GetComponent<CPlayerInterface>().PlayerId);

        // Check take control attempt failed
        if (!bControlTaken)
            return;

        m_cTurretViewId.Set(_cTurretViewId);

        // Sign up for cockpit dismounting
        cTurretInterface.EventControlReleased += OnEventTurretControlReleased;
    }


    [AServerOnly]
    void HandleReleaseTurretControl()
    {
        if (!HasTurretControl)
            return;

        CTurretInterface cTurretInterface = m_cTurretViewId.Value.GameObject.GetComponent<CTurretInterface>();

        // Error check
        if (cTurretInterface.ControllerPlayerId != GetComponent<CPlayerInterface>().PlayerId)
            Debug.LogError("Controlling player id mismatch. This should have not happened");

        // Eject currently player - we are subscribed to the dismount event
        cTurretInterface.ReleaseControl();
    }


    void HandleTurretViewIdSync()
    {
        if (m_cTurretViewId.Value != null)
        {
            if (GetComponent<CPlayerInterface>().IsOwnedByMe)
            {
                CGameHUD.Hud2dInterface.HideHud(CHud2dInterface.EHud.TurretCockpitMenu);
            }

            if (EventTakeTurretControl != null)
                EventTakeTurretControl(this, m_cTurretViewId.Value);
        }
        else
        {
            if (EventReleaseTurretControl != null)
                EventReleaseTurretControl(this);   
        }
    }


// Member Fields


    CNetworkVar<TNetworkViewId> m_cTurretViewId = null;


    static CNetworkStream s_cNetworkStream = new CNetworkStream();


};
