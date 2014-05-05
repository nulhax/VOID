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
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
    }


// Member Fields


    CNetworkVar<TNetworkViewId> m_cMountedCockpitViewId = null;


};
