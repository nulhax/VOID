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

    public event NotifyButtonPressed EventOpenAirlock;
    public event NotifyButtonPressed EventCloseAirlock;


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


    public void OnClickOpenAirlock()
    {
        if (CNetwork.IsServer)
        {
            if (EventOpenAirlock != null) EventOpenAirlock(EButton.OpenAirlock);
        }
    }


    public void OnClickCloseAirlock()
    {
        if (CNetwork.IsServer)
        {
            if (EventCloseAirlock != null) EventCloseAirlock(EButton.CloseAirlock);
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

    public UIButton m_cButtonOpenAirlock = null;
    public UIButton m_cButtonCloseAirlock = null;


    GameObject m_cDoorObject = null;


};
