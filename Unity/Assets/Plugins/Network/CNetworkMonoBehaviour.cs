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


	public CNetworkViewId ViewId
	{
		get { return(SelfNetworkView.ViewId); }
	}


// Member Functions

    // public:


    public abstract void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar);


    public void InvokeRpc(ulong _ulPlayerId, string _sMethodName, params object[] _caParameters)
    {
        SelfNetworkView.InvokeRpc(_ulPlayerId, this, _sMethodName, _caParameters);
    }


    public void InvokeRpcAll(string _sMethodName, params object[] _caParameters)
    {
        SelfNetworkView.InvokeRpc(0, this, _sMethodName, _caParameters);
    }


    public void InvokeRpcThisAll(params object[] _caParameters)
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
        System.Diagnostics.StackFrame sf = st.GetFrame(1);
        System.Reflection.MethodBase currentMethodName = sf.GetMethod();
        Debug.LogError("Method name: " + currentMethodName.Name);
    }


    public CNetworkView SelfNetworkView
    {
        get { return (gameObject.GetComponent<CNetworkView>()); }
    }


    public CNetworkViewId SelfNetworkViewId
    {
        get { return (SelfNetworkView.ViewId); }
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
