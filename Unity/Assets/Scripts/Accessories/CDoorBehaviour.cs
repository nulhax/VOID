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


public class CDoorBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


    public bool IsOpened
    {
        get { return (m_cOpened.Get()); }
    }


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_cOpened = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
    }


    [AServerOnly]
    public void SetOpened(bool _bOpened)
    {
        m_cOpened.Set(_bOpened);
    }


	void Start()
	{
        //m_cDuiObject.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiAirlockBehaviour>().SetDoorObject(gameObject);

        m_vClosedPosition = transform.position;
        m_vOpenedPosition = m_vClosedPosition + new Vector3(0.0f, 2.5f, 0.0f);
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        if (m_fMotorTimer < m_fOpenCloseInterval)
        {
            m_fMotorTimer += Time.deltaTime;

            if (IsOpened)
            {
                transform.position = Vector3.Lerp(m_vClosedPosition, m_vOpenedPosition, m_fMotorTimer);
            }
            else
            {
                transform.position = Vector3.Lerp(m_vOpenedPosition, m_vClosedPosition, m_fMotorTimer);
            }
        }
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (m_cOpened == _cSyncedVar)
        {
            m_fMotorTimer = 0.0f;
        }
    }


// Member Fields


    public GameObject m_cDuiObject = null;


    CNetworkVar<bool> m_cOpened = null;


    Vector3 m_vClosedPosition;
    Vector3 m_vOpenedPosition;


    float m_fMotorTimer         = 0.0f;
    float m_fOpenCloseInterval  = 1.0f;


};
