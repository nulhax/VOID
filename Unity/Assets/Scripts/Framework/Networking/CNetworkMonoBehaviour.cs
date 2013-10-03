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


    public abstract void InitialiseNetworkVars();


    public void InvokeRpc(uint _uiPlayerId, string _sMethodName, params object[] _caParameters)
    {
        NetworkView.InvokeRpc(_uiPlayerId, this, _sMethodName, _caParameters);
    }


    public void InvokeRpcAll(string _sMethodName, params object[] _caParameters)
    {
        NetworkView.InvokeRpcAll(this, _sMethodName, _caParameters);
    }


    public CNetworkView NetworkView
    {
        get { return (gameObject.GetComponent<CNetworkView>()); }
    }


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


};
