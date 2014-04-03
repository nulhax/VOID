//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDuiFacilityDoorBehaviour.cs
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


public class CDuiDoorControlBehaviour : CNetworkMonoBehaviour
{

// Member Types


    public enum EButton
    {
        INVLAID,

        OpenDoor,
        CloseDoor,

        MAX
    };


    public enum EPanel
    {
        INVALID,

        OpenDoor,
        CloseDoor,
        DecompressionClosed,
    }


// Member Delegates & Events


    public delegate void NotifyButtonPressed(EButton _eButton);

    public event NotifyButtonPressed EventClickOpenDoor;
    public event NotifyButtonPressed EventClickCloseDoor;


// Member Properties


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_ePanel = _cRegistrar.CreateReliableNetworkVar<EPanel>(OnNetworkVarSync, EPanel.INVALID);
    }


    public void OnClickOpen()
    {
        if (CNetwork.IsServer)
        {
            if (EventClickOpenDoor != null) EventClickOpenDoor(EButton.OpenDoor);
        }
    }


    public void OnClickClose()
    {
        if (CNetwork.IsServer)
        {
            if (EventClickCloseDoor != null) EventClickCloseDoor(EButton.CloseDoor);
        }
    }


    [AServerOnly]
    public void SetPanel(EPanel _ePanel)
    {
        m_ePanel.Set(_ePanel);
    }


    void Start()
    {
        if (CNetwork.IsServer)
        {
            SetPanel(EPanel.OpenDoor);
        }
    }


    void OnDestroy()
    {
    }


    void Update()
    {
    }


    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        if (_cSynedVar == m_ePanel)
        {
            switch (m_ePanel.Get())
            {
                case EPanel.OpenDoor:
                    m_cOpenPanel.gameObject.SetActive(true);
                    m_cClosePanel.gameObject.SetActive(false);
                    break;

                case EPanel.CloseDoor:
                    m_cOpenPanel.gameObject.SetActive(false);
                    m_cClosePanel.gameObject.SetActive(true);
                    break;

                case EPanel.DecompressionClosed:
                    break;

                default:
                    Debug.LogError("Unknown panel: " + m_ePanel.Get());
                    break;
            }
        }
    }


// Member Fields


    public UIPanel m_cOpenPanel = null;
    public UIPanel m_cClosePanel = null;


    CNetworkVar<EPanel> m_ePanel = null;


};
