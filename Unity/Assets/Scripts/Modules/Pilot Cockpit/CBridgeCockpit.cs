//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CBridgeCockpit.cs
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


public class CBridgeCockpit : CNetworkMonoBehaviour
{

// Member Types


    public enum ENetworkAction
    {
        MoveForward,
        MoveBackward,
        MoveUp,
        MoveDown,
        StrafeLeft,
        StrafeRight,
        RotateLeft,
        RotateRight,
        PitchUp,
        PitchDown
    }


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


    public static void SerializeCockpitInteractions(CNetworkStream _cStream)
    {
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    public static void UnserializeCockpitInteractions(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
    }


	void Start()
	{
        m_cCockpitBehaviour = GetComponent<CCockpit>();
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        if (CNetwork.IsServer &&
            m_cCockpitBehaviour.IsMounted)
        {
            UpdateInput();
        }
	}


    [AServerOnly]
    void UpdateInput()
    {
        
    }


// Member Fields


    CCockpit m_cCockpitBehaviour = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
