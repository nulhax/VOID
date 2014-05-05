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



// Member Properties


// Member Functions

    // public:


    public abstract void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar);


    public void InvokeRpc(ulong _ulPlayerId, string _sMethodName, params object[] _caParameters)
    {
        NetworkView.InvokeRpc(_ulPlayerId, this, _sMethodName, _caParameters);
    }


    public void InvokeRpcAll(string _sMethodName, params object[] _caParameters)
    {
        NetworkView.InvokeRpc(0, this, _sMethodName, _caParameters);
    }


    public void InvokeRpcThisAll(params object[] _caParameters)
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
        System.Diagnostics.StackFrame sf = st.GetFrame(1);
        System.Reflection.MethodBase currentMethodName = sf.GetMethod();
        Debug.LogError("Method name: " + currentMethodName.Name);
    }


    public CNetworkView NetworkView
    {
        get { return (gameObject.GetComponent<CNetworkView>()); }
    }


    public TNetworkViewId NetworkViewId
    {
        get { return (NetworkView.ViewId); }
    }


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


};
