//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerAirMotor.cs
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


[RequireComponent(typeof(CPlayerSuit))]
public class CPlayerAirMotor : CNetworkMonoBehaviour
{

// Member Types


	public enum EMoveState
	{
        INVALID     = -1,

		Forward	    = 1 << 0,
		Backward	= 1 << 1,
		Up		    = 1 << 2,
		Down		= 1 << 3,
		StrafeLeft	= 1 << 4,
		StrafeRight	= 1 << 5,
		RollLeft	= 1 << 6,
		RollRight	= 1 << 7,
		Turbo		= 1 << 8,
	}


// Member Delegates & Events


// Member Properties





// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
    }


	[ALocalOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{

	}
	
	
	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{

	}



    void Update()
    {
    }







    void OnNetworkVarSync(INetworkVar _SyncedVar)
    {

    }


// Member Fields





	CNetworkVar<bool> m_bActive = null;


    float m_fMouseDeltaX = 0.0f;
    float m_fMouseDeltaY = 0.0f;
	

	uint m_uiMovementStates = 0;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();







};
