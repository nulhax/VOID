//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkView.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System;


/* Implementation */


public class CNetworkView : MonoBehaviour
{

// Member Types


    public enum EProdecure : byte
    {
        InvokeNetworkRpc,
        SyncNetworkVar
    }


    public struct TRpcMethod
    {
        public Component cUnityComponent;
        public MethodInfo cMethodInfo;
    }


// Member Functions
    
    // public:


    public void Awake()
    {
        InitialiseNetworkVars();
		InitialiseNetworkRpcs();
    }


    public void Start()
    {
        // Generate static view id if server did not provide one
        if (m_usViewId == 0)
        {
            this.ViewId = GenerateStaticViewId();
        }
    }


    public void Update()
    {
		// Empty
    }


    public void InvokeRpc(Component _cComponent, string _sFunction, params object[] _caParameters)
    {
        InvokeRpc(0, _cComponent, _sFunction, _caParameters);
    }


    public void InvokeRpc(uint _uiPlayerId, Component _cComponent, string _sFunction, params object[] _caParameterValues)
	{
        // Make sure only servers can invoke rpcs
        if (!CGame.IsServer())
        {
            Debug.LogError("Only servers can invoke RPCs  douche");
        }
        else
        {
            // Extract method from component using its name
            MethodInfo tMethodInfo = _cComponent.GetType().GetMethod(_sFunction, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Ensure the method exists
            if (tMethodInfo == null)
            {
                Debug.LogError(string.Format("Could not find method ({0}) in component ({1})", _sFunction, _cComponent.GetType().Name));
            }
            else
            {
                // Find network rpc index using the method info
                byte bNetworkRpcId = FindNetworkRpcIndexUsingMethodInfo(tMethodInfo);

                // Ensure the network rpc is found
                if (bNetworkRpcId == 0)
                {
                    Debug.LogError(string.Format("The network rpc method ({0}) in component ({1}) is not connected to this network view id({2})", _sFunction, _cComponent.GetType().Name, m_usViewId));
                }
                else
                {
                    // Send to all players
                    if (_uiPlayerId == 0)
                    {
                        foreach (KeyValuePair<uint, CNetworkPlayer> tEntry in CNetworkPlayer.FindAll())
                        {
                            // Network player instance
                            CNetworkPlayer cNetworkPlayer = tEntry.Value;

                            SendPlayerInvokeRpc(cNetworkPlayer.PlayerId, bNetworkRpcId, _caParameterValues);
                        }

                        //Debug.Log(string.Format("Sent RPC call for ({0}) to all players", _sFunction));
                    }

                    // Send to individual player
                    else
                    {
                        SendPlayerInvokeRpc(_uiPlayerId, bNetworkRpcId, _caParameterValues);

                        Debug.Log(string.Format("Sent RPC call for ({0}) to player id ({1})", _sFunction, _uiPlayerId));
                    }
                }
            }
        }
	}


    public void SendPlayerAllNetworkVarValues(uint _uiPlayerId)
    {
        // Retieve player instance
        CNetworkPlayer cNetworkPlayer = CNetworkPlayer.FindUsingPlayerId(_uiPlayerId);

        // Send all current network variable values to player
        foreach (KeyValuePair<byte, INetworkVar> tEntry in m_mNetworkVars)
        {
            INetworkVar cNetworkVar = tEntry.Value;
 
            SendPlayerVarSync(_uiPlayerId, tEntry.Key);
        }

        //Debug.LogError(string.Format("Sent player id ({0}) all network var values from network view id ({1})", _uiPlayerId, this.ViewId));
    }
     

	public void OnNetworkVarChange(byte _bNetworkVarId)
	{
        foreach (KeyValuePair<uint, CNetworkPlayer> tEntry in CNetworkPlayer.FindAll())
        {
            SendPlayerVarSync(tEntry.Key, _bNetworkVarId);
        }

        //Debug.Log(string.Format("Sent network var id ({0}) change to all players", _bNetworkVarId));
	}


    public ushort ViewId
    {
        set
        {
            if (m_usViewId != 0)
            {
                Debug.LogError(string.Format("The network view id cannot be changed once set. CurrentViewId({0}) TargetViewId({1})", m_usViewId, value));
            }
            else
            {
                if (s_cNetworkViews.ContainsKey(value))
                {
                    if (s_cNetworkViews[value] != null)
                    {
                        Debug.LogError(string.Format("Unable to assign GameObject ({0}) network view id ({1}) because its already in use!", gameObject.name, value));
                    }
                    else
                    {
                        s_cNetworkViews[value] = this;
                        m_usViewId = value;
                    }
                }
                else
                {
                    s_cNetworkViews.Add(value, this);
                    m_usViewId = value;
                }
            }
        }

        get { return (m_usViewId); }
    }


    public static ushort GenerateDynamicViewId()
    {
        ushort usViewId = 0;


        if (!CGame.IsServer())
        {
            Debug.LogError(string.Format("Clients cannot generate network view ids!!!"));
        }
        else
        {
            for (ushort i = 500; i < ushort.MaxValue; ++ i)
            {
                if (!s_cNetworkViews.ContainsKey(i))
                {
                    usViewId = i;
                    s_cNetworkViews.Add(i, null);
                    break;
                }


                if (i == ushort.MaxValue - 1)
                {
                    Debug.LogError(string.Format("Oh shit, the network view id generator ran out of ids. The game is now broken. GG"));
                }
            }
        }


        return (usViewId);
    }


    public static ushort GenerateStaticViewId()
    {
        ushort usViewId = 0;


        for (ushort i = 5; i < ushort.MaxValue; ++i)
        {
            if (!s_cNetworkViews.ContainsKey(i))
            {
                usViewId = i;
                s_cNetworkViews.Add(i, null);
                break;
            }


            if (i == ushort.MaxValue - 1)
            {
                Debug.LogError(string.Format("Oh shit, the network view id generator ran out of ids. The game is now broken. GG"));
            }
        }


        return (usViewId);
    }


    public static CNetworkView FindUsingViewId(ushort _usViewId)
    {
		CNetworkView cNetworkView = null;
		

		if (!s_cNetworkViews.ContainsKey(_usViewId))
		{
			Debug.LogError(string.Format("Cannot find network view with id ({0})", _usViewId));
		}
		else
		{
			cNetworkView = s_cNetworkViews[_usViewId];
		}


        return (cNetworkView);
    }


    public static void ProcessInboundNetworkData(CPacketStream _cStream)
    {
        while (_cStream.HasUnreadData())
        {
            // Extract owner network view id
            ushort usNetworkViewId = _cStream.ReadUShort();

            // Extract procedure type
            EProdecure eProcedure = (EProdecure)_cStream.ReadByte();

            // Retrieve network view instance
            CNetworkView cNetworkView = CNetworkView.FindUsingViewId(usNetworkViewId);


            // Process network var sync procedure
            if (eProcedure == EProdecure.SyncNetworkVar)
            {
                // Extract network var identifier
                byte bNetworkVarIdentifier = _cStream.ReadByte();

                // Retrieve network var instance
                INetworkVar cNetworkVar = cNetworkView.m_mNetworkVars[bNetworkVarIdentifier];

                // Retrieve network var type
                Type cVarType = cNetworkVar.GetValueType();

                // Extract value serialized
                byte[] baVarValue = _cStream.ReadType(cVarType);

                // Convert serialized data to object
                object cNewVarValue = Converter.ToObject(baVarValue, cVarType);

                // Sync with new value
                cNetworkVar.Sync(cNewVarValue);
            }

            // Process network rpc procedure
            else if (eProcedure == EProdecure.InvokeNetworkRpc)
            {
                // Extract rpc method identifier
                byte bMethodIdentifier = _cStream.ReadByte();

                // Retrieve method owner instance
                Component cParentComponent = cNetworkView.m_mNetworkRpcs[bMethodIdentifier].cUnityComponent;

                // Retrieve method info
                MethodInfo cMethodInfo = cNetworkView.m_mNetworkRpcs[bMethodIdentifier].cMethodInfo;

                // Extract method parameters
                object[] caParameterValues = _cStream.ReadMethodParameters(cMethodInfo);

                // Invoke the rpc method
                cMethodInfo.Invoke(cParentComponent, caParameterValues);
            }
        }
    }


    // protected:


    // private:


    void InitialiseNetworkVars()
    {
        MonoBehaviour[] aComponents = gameObject.GetComponents<MonoBehaviour>();


        foreach (MonoBehaviour cComponent in aComponents)
        {
            if (cComponent == this)
				continue;

            INetworkComponent cNetworkComponent = cComponent as INetworkComponent;


            if (cNetworkComponent != null)
            {
                cNetworkComponent.InitialiseNetworkVars();
                FieldInfo[] aFieldInfos = cComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


                foreach (FieldInfo cFieldInfo in aFieldInfos)
                {
                    if (cFieldInfo.FieldType.GetInterface(typeof(INetworkVar).Name, false) != null)
                    {
                        INetworkVar cNetworkVar = (INetworkVar)cFieldInfo.GetValue(cComponent);
                        byte bId = (byte)(m_mNetworkVars.Count + 1);


                        m_mNetworkVars.Add(bId, cNetworkVar);
                        cNetworkVar.SetNetworkViewOwner(this, bId);


                        //Debug.Log("Added network var to list");
                    }
                }
            }
        }
    }


	void InitialiseNetworkRpcs()
	{
        MonoBehaviour[] aComponents = gameObject.GetComponents<MonoBehaviour>();


        foreach (MonoBehaviour cComponent in aComponents)
        {
			if (cComponent == this)
				continue;


            MethodInfo[] caMethodInfos = cComponent.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


            foreach (MethodInfo cMethodInfo in caMethodInfos)
            {
				foreach (ANetworkRpc cAttribute in cMethodInfo.GetCustomAttributes(typeof(ANetworkRpc), false))
				{
					byte bId = (byte)(m_mNetworkRpcs.Count + 1);


                    TRpcMethod tRpcMethodInfo = new TRpcMethod();
                    tRpcMethodInfo.cUnityComponent = cComponent;
                    tRpcMethodInfo.cMethodInfo = cMethodInfo;
                    
                    
                    m_mNetworkRpcs.Add(bId, tRpcMethodInfo);


					//Debug.Log(string.Format("Added network rpc ({0}) to list. Method name ({1})", bId, cMethodInfo.Name));
				}
			}
		}
	}


	byte FindNetworkRpcIndexUsingMethodInfo(MethodInfo _tMethodInfo)
	{
		byte bNetworkRpcId = 0;


        foreach (KeyValuePair<byte, TRpcMethod> tEntry in m_mNetworkRpcs)
		{
			if (tEntry.Value.cMethodInfo == _tMethodInfo)
			{
				bNetworkRpcId = tEntry.Key;
				break;
			}
		}


		return (bNetworkRpcId);
	}


    void SendPlayerInvokeRpc(uint _uiPlayerId, byte _bRpcIdentifier, object[] _acParameterValues)
    {
        // Retrieve player instance
        CNetworkPlayer cNetworkPlayer = CNetworkPlayer.FindUsingPlayerId(_uiPlayerId);

        // View id
        cNetworkPlayer.PacketStream.Write(this.ViewId);

        // Prodecure type
        cNetworkPlayer.PacketStream.Write((byte)EProdecure.InvokeNetworkRpc);

        // Rpc identifier
        cNetworkPlayer.PacketStream.Write(_bRpcIdentifier);

        // Method parameters
        cNetworkPlayer.PacketStream.Write(m_mNetworkRpcs[_bRpcIdentifier].cMethodInfo, _acParameterValues);
    }


    void SendPlayerVarSync(uint _uiPlayerId, byte _bVarIdentifier)
    {
        INetworkVar cNetworkVar = m_mNetworkVars[_bVarIdentifier];

        // Retrieve player instance
        CNetworkPlayer cNetworkPlayer = CNetworkPlayer.FindUsingPlayerId(_uiPlayerId);

        // View id
        cNetworkPlayer.PacketStream.Write(this.ViewId);

        // Prodecure type
        cNetworkPlayer.PacketStream.Write((byte)EProdecure.SyncNetworkVar);

        // Var identifier
        cNetworkPlayer.PacketStream.Write(_bVarIdentifier);

        // Value
        cNetworkPlayer.PacketStream.Write(cNetworkVar.GetAsObject(), cNetworkVar.GetValueType());
    }


// Member Variables
    
    // protected:


    // private:


    ushort m_usViewId = 0;

    
    Dictionary<byte, INetworkVar> m_mNetworkVars = new Dictionary<byte, INetworkVar>();
    Dictionary<byte, TRpcMethod> m_mNetworkRpcs = new Dictionary<byte, TRpcMethod>();


    static Dictionary<ushort, CNetworkView> s_cNetworkViews = new Dictionary<ushort, CNetworkView>();


};