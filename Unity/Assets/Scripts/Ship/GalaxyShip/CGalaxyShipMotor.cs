//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGalaxyShipMotor.cs
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


public class CGalaxyShipMotor : CNetworkMonoBehaviour
{

// Member Types


    public enum EThrusters
    {
        INVALID = -1,

        Forward,
        Backward,
        StrafeLeft,
        StrafeRight,
        Up,
        Down,
        RollLeft,
        RollRight,
        PitchUp,
        PitchDown,

        MAX
    }


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        for (int i = 0; i < (int)EThrusters.MAX; ++ i)
        {
            m_baThustersEnabled[i] = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
        }
    }


    public void SetThrusterEnabled(EThrusters _eThusters, bool _bEnabled)
    {
        m_baThustersEnabled[(int)_eThusters].Set(_bEnabled);
    }


    void Start()
    {
    }


    void OnDestroy()
    {
    }


    void Update()
    {
        if (CNetwork.IsServer)
        {
            UpdateThusters();
        }
    }


    void UpdateThusters()
    {
        
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
    }


// Member Fields


    CNetworkVar<bool>[] m_baThustersEnabled = new CNetworkVar<bool>[(int)EThrusters.MAX];
    

// Server Members Fields


};
