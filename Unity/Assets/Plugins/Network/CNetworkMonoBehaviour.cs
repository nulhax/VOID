//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CNetworkMonoBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public abstract class CNetworkMonoBehaviour : MonoBehaviour
{

// Member Types


// Member Functions

    // public:


    public abstract void InstanceNetworkVars();


    public void InvokeRpc(ulong _ulPlayerId, string _sMethodName, params object[] _caParameters)
    {
        NetworkView.InvokeRpc(_ulPlayerId, this, _sMethodName, _caParameters);
    }


    public void InvokeRpcAll(string _sMethodName, params object[] _caParameters)
    {
        NetworkView.InvokeRpc(0, this, _sMethodName, _caParameters);
    }


    public CNetworkView NetworkView
    {
        get { return (gameObject.GetComponent<CNetworkView>()); }
    }


	public void EnableOutboundSending()
	{

	}


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


};
