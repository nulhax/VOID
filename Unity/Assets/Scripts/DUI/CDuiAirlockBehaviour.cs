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


public class CDuiAirlockBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


    public void SetDoorObject(GameObject _cDoorObject)
    {
        m_cDoorObject = _cDoorObject;
    }


    public void OnEventOpenButtonClick()
    {
        if (CNetwork.IsServer)
        {
            m_cDoorObject.GetComponent<CDoorBehaviour>().SetOpened(true);
        }
    }


    public void OnEventCloseButtonClick()
    {
        if (CNetwork.IsServer)
        {
            m_cDoorObject.GetComponent<CDoorBehaviour>().SetOpened(false);
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


    public UIProgressBar m_PropulsionBar = null;
    public UILabel m_cLabelWarning = null;


    GameObject m_cDoorObject = null;


};
