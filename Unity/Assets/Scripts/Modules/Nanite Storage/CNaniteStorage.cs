//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNaniteStorageBehaviour.cs
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


[RequireComponent(typeof(CModuleInterface))]
public class CNaniteStorage : CNetworkMonoBehaviour 
{
	
// Member Types
	
	
// Member Delegates & Events


// Member Properties


	public float Capacity
	{ 
		get { return (m_fCapacity.Get()); }
	}


// Member Functions


	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
        m_fCapacity = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0);
	}


    [AServerOnly]
    public void SetCapacity(float _fNewCapacity)
    {
        m_fCapacity.Value = _fNewCapacity;
    }


    void Start()
    {
        if (CNetwork.IsServer)
        {
            GetComponent<CModuleInterface>().EventBuilt += OnEventBuilt;
        }
    }


    [AServerOnly]
    void OnEventBuilt(CModuleInterface _cSender)
    {
        // Add nanite capacity to ship
        CGameShips.Ship.GetComponent<CShipNaniteSystem>().ChangeCapacity(m_fInitialCapacity);
    }


    void OnNetworkVarSync(INetworkVar _VarInstance)
    {
        if (_VarInstance == m_fCapacity)
        {
            // Empty
        }
    }

 
// Member Fields


    public float m_fInitialCapacity = 0.0f;


    CNetworkVar<float> m_fCapacity = null;


}
