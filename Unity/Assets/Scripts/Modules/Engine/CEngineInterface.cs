//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPropulsionGeneratorSystem.cs
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
public class CEngineInterface : CNetworkMonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events
			

// Member Properties


	public float PropulsionForce
	{
        get { return (m_fPropulsion.Value); }
	}

	public float Propulsion
	{
        get { return (m_fPropulsion.Value); }
	}

	
// Member Functions
	

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fPropulsion = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}


    [AServerOnly]
    public void SetPropulsion(float _fValue)
    {
        m_fPropulsion.Value = _fValue;

        CGameShips.Ship.GetComponent<CShipPropulsionSystem>().ChangePropulsion(m_fPropulsion.Value - m_fPropulsion.PreviousValue);
    }
	

	void Start()
	{
		if(!CNetwork.IsServer)
			return;

        CGameShips.Ship.GetComponent<CShipPropulsionSystem>().ChangeMaxPropolsion(m_fInitialPropulsion);

        SetPropulsion(m_fInitialPropulsion);
	}


    void OnNetworkVarSync(INetworkVar _VarInstance)
    {
        if (m_fPropulsion == _VarInstance)
        {
            // Empty
        }
    }


// Member Fields


    public float m_fInitialPropulsion = 0.0f;


    CNetworkVar<float> m_fPropulsion = null;


}
