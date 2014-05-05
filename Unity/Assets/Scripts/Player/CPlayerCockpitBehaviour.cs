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


// Member Properties


    public GameObject MountedCockpit
    {
        get 
        {
            if (!IsMounted)
                return (null);

            return (m_cMountedCockpitViewId.Value.GameObject); 
        }
    }


    public bool IsMounted
    {
        get { return (m_cMountedCockpitViewId.Value != null); }
    }


// Member Methods


    public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
    {
        m_cMountedCockpitViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync);
    }


    [AServerOnly]
    public void SetMountedCockpitViewId(TNetworkViewId _tCockpitViewId)
    {
        m_cMountedCockpitViewId.Set(_tCockpitViewId);
    }


	void Start()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.SubscribeInputChange(CUserInput.EInput.ModuleMenu_ToggleDisplay, OnEventInput);


            m_cTurretSelectMenu = GameObject.Instantiate(m_cTurretSelectMenu) as GameObject;
            m_cTurretSelectMenu.SetActive(false);

            //m_cTurretSelectMenu.GetComponent<CHudModuleMenu>().EventCreateModule += OnEventCreateModule;
        }
	}


	void OnDestroy()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.ModuleMenu_ToggleDisplay, OnEventInput);

            Destroy(m_cTurretSelectMenu);
        }
	}


	void Update()
	{
        if (m_bMenuOpen)
        {
        }
	}


    void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
    {
        if (_bDown)
        {
            switch (_eInput)
            {
                case CUserInput.EInput.TurretMenu_ToggleDisplay:
                    break;

                default:
                    Debug.LogError("Unknown input: " + _eInput);
                    break;
            }
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
    }


// Member Fields


    public GameObject m_cTurretSelectMenu = null;


    CNetworkVar<TNetworkViewId> m_cMountedCockpitViewId = null;


    bool m_bMenuOpen = false;


};
