//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomPowerGenerator.cs
//  Description :   Class script for the power generator facility
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Notes:

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Implementation
public class CRoomPowerGenerator : CNetworkMonoBehaviour
{
    // Member Data
    CNetworkVar<float> m_fHealth;
    CNetworkVar<float> m_fPowerCost;
    CNetworkVar<float> m_fOutput;
    CNetworkVar<float> m_fRadiationLevel;
    CNetworkVar<float> m_fRadiationRadius;

    // Member Properties
    float Health          { get { return (m_fHealth.Get()); } }
    float PowerCost       { get { return (m_fPowerCost.Get()); } }
    float Output          { get { return (m_fOutput.Get()); } }
    float RadiationLevel  { get { return (m_fRadiationLevel.Get()); } }
    float RadiationRadius { get { return (m_fRadiationRadius.Get()); } }
	
    // Member Functions
    public override void InstanceNetworkVars()
    {
        m_fHealth          = new CNetworkVar<float>(OnNetworkVarSync);
        m_fPowerCost       = new CNetworkVar<float>(OnNetworkVarSync);
        m_fOutput          = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRadiationLevel  = new CNetworkVar<float>(OnNetworkVarSync);
        m_fRadiationRadius = new CNetworkVar<float>(OnNetworkVarSync);
    }

    public void Update()
    {
    
	}
    

    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        // Empty
    }

    public void Start() {}
    public void OnDestroy() {}
};