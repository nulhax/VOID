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


    public bool IsMounted
    {
        get { return (m_tMountedCockpitViewId.Value != null); }
    }


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_tMountedCockpitViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync);
    }


    [AServerOnly]
    public void SetMountedCockpitViewId(TNetworkViewId _tCockpitViewId)
    {
        m_tMountedCockpitViewId.Set(_tCockpitViewId);
    }


	void Start()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            m_cHudTurretCockpitControlInterface = CGameHUD.Hud2dInterface.TurretCockpitControlInterface;
        }
	}


	void OnDestroy()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.TurretMenu_ToggleDisplay, OnEventInput);
        }
	}


	void Update()
	{

	}


    void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
    {
        if (!_bDown)
            return;
        
        switch (_eInput)
        {
            case CUserInput.EInput.TurretMenu_ToggleDisplay:
                break;

            default:
                Debug.LogError("Unknown input: " + _eInput);
                break;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_tMountedCockpitViewId)
        {
            if (m_tMountedCockpitViewId.Value != null)
            {
                m_cActiveCockpitInterface = m_tMountedCockpitViewId.Value.GameObject.GetComponent<CCockpitInterface>();

                if (GetComponent<CPlayerInterface>().IsOwnedByMe)
                {
                    CUserInput.SubscribeInputChange(CUserInput.EInput.TurretMenu_ToggleDisplay, OnEventInput);

                    CGameHUD.Hud2dInterface.OpenHud(CHud2dInterface.EHud.TurretCockpitMenu);
                }

                if (EventEnterCockpit != null)
                    EventEnterCockpit(this, m_tMountedCockpitViewId.Value);
            }
            else
            {
                m_cActiveCockpitInterface = null;

                if (GetComponent<CPlayerInterface>().IsOwnedByMe)
                {
                    CUserInput.UnsubscribeInputChange(CUserInput.EInput.TurretMenu_ToggleDisplay, OnEventInput);

                    CGameHUD.Hud2dInterface.CloseHud(CHud2dInterface.EHud.TurretCockpitMenu);
                }

                if (EventLeaveCockpit != null)
                    EventLeaveCockpit(this);
            }
        }
    }


// Member Fields


    CNetworkVar<TNetworkViewId> m_tMountedCockpitViewId = null;

    CCockpitInterface m_cActiveCockpitInterface = null;
    CHudTurretCockpitControlInterface m_cHudTurretCockpitControlInterface = null;


};
