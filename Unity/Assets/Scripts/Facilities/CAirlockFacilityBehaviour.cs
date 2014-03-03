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
        m_cDoorExternal.GetComponent<CDoorBehaviour>().SetOpened(false);
        m_cDoorFacility.GetComponent<CDoorBehaviour>().SetOpened(true);

        // Open airlock
        if (CNetwork.IsServer)
        {
            m_cDuiInternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiAirlockInternalBehaviour>().EventOpenAirlock += (CDuiAirlockInternalBehaviour.EButton _eButton) =>
            {
                m_bStates.Set((byte)(m_bStates.Get() | (1 << (byte)EStates.Decompressing)));
                m_cDoorExternal.GetComponent<CDoorBehaviour>().SetOpened(false);
                m_cDoorFacility.GetComponent<CDoorBehaviour>().SetOpened(false);

                foreach (GameObject cAlarmObject in gameObject.GetComponent<CFacilityInterface>().FindAccessoriesByType(CAccessoryInterface.EType.Alarm_Warning))
                {
                    cAlarmObject.GetComponent<CAlarmBehaviour>().SetAlarmActive(true);
                }
            };

            // Close airlock
            m_cDuiInternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiAirlockInternalBehaviour>().EventCloseAirlock += (CDuiAirlockInternalBehaviour.EButton _eButton) =>
            {
                m_bStates.Set((byte)(m_bStates.Get() & ~(1 << (byte)EStates.Decompressing)));
                m_cDoorExternal.GetComponent<CDoorBehaviour>().SetOpened(false);
                m_cDoorFacility.GetComponent<CDoorBehaviour>().SetOpened(true);

                foreach (GameObject cAlarmObject in gameObject.GetComponent<CFacilityInterface>().FindAccessoriesByType(CAccessoryInterface.EType.Alarm_Warning))
                {
                    cAlarmObject.GetComponent<CAlarmBehaviour>().SetAlarmActive(false);
                }

                //gameObject.GetComponent<CFacilityAtmosphere>().
            };
        }

        //m_cDuiFacility.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>();
        //m_cDuiExternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiFacilityDoorBehaviour>();
	}


	void OnDestroy()
	{
	}


	void Update()
	{
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

                    m_cDuiInternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiAirlockInternalBehaviour>().SetStatusText("Decompressing");
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

                    m_cDuiInternal.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiAirlockInternalBehaviour>().SetStatusText("Decompressed");
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


    public GameObject m_cDoorExternal = null;
    public GameObject m_cDoorFacility = null;

    public GameObject m_cDuiInternal = null;
    public GameObject m_cDuiFacility = null;
    public GameObject m_cDuiExternal = null;

    public GameObject[] m_caOxygenParticalSprayers = null;


    CNetworkVar<byte> m_bStates = null;


};
