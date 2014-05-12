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


public class CDuiAirlockDoorBehaviour : MonoBehaviour
{

// Member Types


    public enum EButton
    {
        INVLAID,

        EnableFacilityDoorAutoOpen,
        DisableFacilityDoorAutoOpen,

        OpenExternalDoor,
        CloseExternalDoor,

        LockExternalDoor,
        UnlockExternalDoor,

        LockFacilityDoor,
        UnlockFacilityDoor,

        MAX
    };


// Member Delegates & Events


    public delegate void NotifyButtonPressed(EButton _eButton);

    public event NotifyButtonPressed EventEnableFacilityDoorAutoOpen;
    public event NotifyButtonPressed EventDisableFacilityDoorAutoClose;

    public event NotifyButtonPressed EventOpenExternalDoor;
    public event NotifyButtonPressed EventCloseExternalDoor;
   
    public event NotifyButtonPressed EventLockExternalDoor;
    public event NotifyButtonPressed EventUnlockExternalDoor;

    public event NotifyButtonPressed EventLockFacilityDoor;
    public event NotifyButtonPressed EventUnlockFacilityDoor;


// Member Properties


// Member Methods


    public void SetStatusText(string _sString)
    {
        m_cStatusText.text = _sString;
    }


    public void SetDoorObject(GameObject _cDoorObject)
    {
        m_cDoorObject = _cDoorObject;
    }


    public void OnClickEnableFacilityDoorAutoOpen()
    {
        if (CNetwork.IsServer)
        {
            if (EventEnableFacilityDoorAutoOpen != null) EventEnableFacilityDoorAutoOpen(EButton.EnableFacilityDoorAutoOpen);
        }
    }


    public void OnClickDisableFacilityDoorAutoOpen()
    {
        if (CNetwork.IsServer)
        {
            if (EventEnableFacilityDoorAutoOpen != null) EventEnableFacilityDoorAutoOpen(EButton.EnableFacilityDoorAutoOpen);
        }
    }


    public void OnClickOpenExternalDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventOpenExternalDoor != null) EventOpenExternalDoor(EButton.EnableFacilityDoorAutoOpen);
        }
    }


    public void OnClickCloseExternalDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventCloseExternalDoor != null) EventCloseExternalDoor(EButton.EnableFacilityDoorAutoOpen);
        }
    }


    public void OnClickLockExternalDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventLockExternalDoor != null) EventLockExternalDoor(EButton.EnableFacilityDoorAutoOpen);
        }
    }


    public void OnClickUnlockExternalDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventUnlockExternalDoor != null) EventUnlockExternalDoor(EButton.EnableFacilityDoorAutoOpen);
        }
    }


    public void OnClickLockFacilityDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventLockFacilityDoor != null) EventLockFacilityDoor(EButton.EnableFacilityDoorAutoOpen);
        }
    }


    public void OnClickUnlockFacilityDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventUnlockFacilityDoor != null) EventUnlockFacilityDoor(EButton.EnableFacilityDoorAutoOpen);
        }
    }


	void Start()
	{
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


// Member Fields


    public UILabel m_cStatusText = null;

    public UIProgressBar m_cPressureProgressBar = null;
    
    public UIButton m_cButtonEnableFacilityDoorAutoOpen = null;
    public UIButton m_cButtonDisableFacilityDoorAutoOpen = null;

    public UIButton m_cButtonOpenExternalDoor = null;
    public UIButton m_cButtonCloseExternalDoor = null;

    public UIButton m_cButtonLockExternalDoor = null;
    public UIButton m_cButtonUnlockExternalDoor = null;

    public UIButton m_cButtonLockFacilityDoor = null;
    public UIButton m_cButtonUnlockFacilityDoor = null;


    GameObject m_cDoorObject = null;


};
