//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


public class CAirlockFacilityBehaviour : CNetworkMonoBehaviour
{

// Member Types


    enum EStates
    {
        INVALID,

        HullBreach_Facility,
        HullBreach_External,
        DoorLocked_Facility,
        DoorLocked_External,
        Decompressing,
        Compressing,

        MAX
    }   


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_bStates = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, 0);
    }


	void Start()
	{
        //m_cHullExpansionPortBehaviour.DoorBehaviour.SetOpened(false);
        //m_cFacilityExpansionPortBehaviour.DoorBehaviour.SetOpened(true);

        // Open airlock
        if (CNetwork.IsServer)
        {
            m_cDuiInternal.GetComponent<CDUIConsole>().DUIRoot.GetComponent<CDuiAirlockInternalBehaviour>().EventOpenFacilityDoor += (CDuiAirlockInternalBehaviour.EButton _eButton) =>
            {
                m_cFacilityExpansionPortBehaviour.DoorBehaviour.SetOpened(true);
            };

            // Close airlock
            m_cDuiInternal.GetComponent<CDUIConsole>().DUIRoot.GetComponent<CDuiAirlockInternalBehaviour>().EventCloseFacilityDoor += (CDuiAirlockInternalBehaviour.EButton _eButton) =>
            {
                m_cFacilityExpansionPortBehaviour.DoorBehaviour.SetOpened(false);
            };

            // Open hull airlock
            m_cDuiInternal.GetComponent<CDUIConsole>().DUIRoot.GetComponent<CDuiAirlockInternalBehaviour>().EventOpenHullDoor += (CDuiAirlockInternalBehaviour.EButton _eButton) =>
            {
                m_cHullExpansionPortBehaviour.DoorBehaviour.SetOpened(true);
            };

            // Close hull airlock
            m_cDuiInternal.GetComponent<CDUIConsole>().DUIRoot.GetComponent<CDuiAirlockInternalBehaviour>().EventCloseHullDoor += (CDuiAirlockInternalBehaviour.EButton _eButton) =>
            {
                m_cHullExpansionPortBehaviour.DoorBehaviour.SetOpened(false);
            };
        }

        //m_cDuiFacility.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>();
        //m_cDuiExternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>();
	}


	void OnDestroy()
	{
        // Empty
	}


	void Update()
	{
        // Empty
	}


    void ProcessNewStates()
    {
        byte bPreviousStates = m_bStates.GetPrevious();
        byte bCurrentStates = m_bStates.Get();

        for (EStates eState = 0; eState < EStates.MAX; ++eState)
        {
            // State was turned on
            if ((bPreviousStates & 1 << (int)eState) == 0 &&
                (bCurrentStates  & 1 << (int)eState) > 0)
            {
                ProcessStateTurnedOn(eState);
            }

            // State was turn off
            else if ((bPreviousStates & (int)1 << (int)eState) > 0 &&
                     (bCurrentStates  & (int)1 << (int)eState) == 0)
            {
                ProcessStateTurnedOff(eState);
            }
        }
    }


    void ProcessStateTurnedOn(EStates _eState)
    {
        switch (_eState)
        {
            case EStates.DoorLocked_External:
                break;

            case EStates.DoorLocked_Facility:
                break;

            case EStates.HullBreach_External:
                break;

            case EStates.HullBreach_Facility:
                break;

            case EStates.Compressing:
                break;

            case EStates.Decompressing:
                {
                    foreach (GameObject cSpayer in m_caOxygenParticalSprayers)
                    {
                        cSpayer.GetComponent<ParticleSystem>().Play();
                    }

                    m_cDuiInternal.GetComponent<CDUIConsole>().DUIRoot.GetComponent<CDuiAirlockInternalBehaviour>().SetStatusText("Decompressing");
                }
                break;

            default:
                Debug.LogError("Unknown state");
                break;
        }
    }


    void ProcessStateTurnedOff(EStates _eState)
    {
        switch (_eState)
        {
            case EStates.DoorLocked_External:
                break;

            case EStates.DoorLocked_Facility:
                break;

            case EStates.HullBreach_External:
                break;

            case EStates.HullBreach_Facility:
                break;

            case EStates.Compressing:
                break;

            case EStates.Decompressing:
                {
                    foreach (GameObject cSpayer in m_caOxygenParticalSprayers)
                    {
                        cSpayer.GetComponent<ParticleSystem>().Stop();
                    }

                    m_cDuiInternal.GetComponent<CDUIConsole>().DUIRoot.GetComponent<CDuiAirlockInternalBehaviour>().SetStatusText("Decompressed");
                }
                break;

            default:
                Debug.LogError("Unknown state");
                break;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_bStates)
        {
            ProcessNewStates();
        }
    }


// Member Fields

    public CExpansionPortBehaviour m_cFacilityExpansionPortBehaviour = null;
    public CExpansionPortBehaviour m_cHullExpansionPortBehaviour = null;

    public GameObject m_cDuiInternal = null;
    public GameObject m_cDuiFacility = null;
    public GameObject m_cDuiExternal = null;

    public GameObject[] m_caOxygenParticalSprayers = null;

    CNetworkVar<byte> m_bStates = null;


};
