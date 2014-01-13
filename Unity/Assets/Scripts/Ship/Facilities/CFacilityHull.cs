
//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityHull.cs
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


public class CFacilityHull : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events
	

	public delegate void NotifyBreached();
	public event NotifyBreached EventBreached;


    public delegate void NotifyBreachFixed();
    public event NotifyBreachFixed EventBreachFixed;


// Member Properties
	

	public bool IsBreached
	{
        get { return (m_bBreached.Get()); }
	}


// Member Methods


    public override void InstanceNetworkVars()
    {
        m_bBreached = new CNetworkVar<bool>(OnNetworkVarSync, false);
    }


	void Start()
	{
		if(EventBreached != null)
		{
			EventBreached();
		}
	}


	void OnDestroy()
	{
        // Empty
	}


	void Update()
	{
        // Empty

        // Debug
        if (CNetwork.IsServer &&
            Input.GetKeyDown(KeyCode.P))
        {
            if (IsBreached)
            {
                m_bBreached.Set(false);
            }
            else
            {
                m_bBreached.Set(true);
            }
        }
	}
	

	void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_bBreached)
        {
            if (m_bBreached.Get())
            {
                if (EventBreached != null) EventBreached();
            }
            else
            {
                if (EventBreachFixed != null) EventBreachFixed();
            }
        }
    }


// Member Fields


    CNetworkVar<bool> m_bBreached = null;


};
