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


public class CDuiAirlockInternalBehaviour : MonoBehaviour
{

// Member Types


    public enum EButton
    {
        INVLAID,

        OpenAirlock,
        CloseAirlock,

        MAX
    };


// Member Delegates & Events


    public delegate void NotifyButtonPressed(EButton _eButton);

    public event NotifyButtonPressed EventOpenFacilityDoor;
    public event NotifyButtonPressed EventCloseFacilityDoor;

    public event NotifyButtonPressed EventOpenHullDoor;
    public event NotifyButtonPressed EventCloseHullDoor;


// Member Properties


// Member Methods


    public void SetStatusText(string _sString)
    {
        m_cStatusText.text = _sString;
    }


    public void OnClickOpenFacilityDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventOpenFacilityDoor != null) EventOpenFacilityDoor(EButton.CloseAirlock);
        }
    }


    public void OnClickCloseFacilityDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventCloseFacilityDoor != null) EventCloseFacilityDoor(EButton.CloseAirlock);
        }
    }


    public void OnClickOpenHullDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventOpenHullDoor != null) EventOpenHullDoor(EButton.OpenAirlock);
        }
    }


    public void OnClickCloseHullDoor()
    {
        if (CNetwork.IsServer)
        {
            if (EventCloseHullDoor != null) EventCloseHullDoor(EButton.CloseAirlock);
        }
    }


	void Start()
	{
        // Empty
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

    public UIButton m_cButtonOpenFacilityDoor = null;
    public UIButton m_cButtonCloseFacilityDoor = null;

    public UIButton m_cButtonOpenHullDoor = null;
    public UIButton m_cButtonCloseHullDoor = null;

    //public UIButton m_cButtonOpenAirlock = null;
    //public UIButton m_cButtonCloseAirlock = null;


};
