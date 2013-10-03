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


    const ushort k_usMaxStaticViewId = 500;
    const ushort k_usMaxDynamicViewId = ushort.MaxValue;


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
        // Run class initialisers
        InitialiseNetworkVars();
		InitialiseNetworkRpcs();
    }


    public void Start()
    {
        // Generate static view id if server did not
        // provide one when this object was created
        if (m_usViewId == 0)
        {
            this.ViewId = GenerateStaticViewId();
        }
    }


    public void Update()
    {
		// Empty
    }


    public void InvokeRpcAll(Component _cComponent, string _sFunction, params object[] _caParameters)
    {
        // Invoke RPC on all players
        InvokeRpc(0, _cComponent, _sFunction, _caParameters);
    }


    public void InvokeRpc(uint _uiPlayerId, Component _cComponent, string _sFunction, params object[] _caParameterValues)
	{
        // Ensure sure only servers can invoke rpcs
        if (!CGame.IsServer())
        {
            Logger.WriteError("Only servers can invoke RPCs douche");
        }
        else
        {
            // Extract method from component using its name
            MethodInfo tMethodInfo = _cComponent.GetType().GetMethod(_sFunction, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Ensure the method exists
            if (tMethodInfo == null)
            {
                Logger.WriteError("Could not find method ({0}) in component ({1})", _sFunction, _cComponent.GetType().Name);
            }
            else
            {
                // Find network rpc index using the method info
                byte bNetworkRpcId = FindNetworkRpcIndexUsingMethodInfo(tMethodInfo);

                // Ensure the network rpc was found
                if (bNetworkRpcId == 0)
                {
                    Logger.WriteError("The network rpc method ({0}) in component ({1}) is not connected to this network view id ({2})", _sFunction, _cComponent.GetType().Name, this.ViewId);
                }
                else
                {
                    // Instance new packet stream
                    CPacketStream cRpcStream = new CPacketStream();

                    // View id
                    cRpcStream.Write(this.ViewId);

                    // Prodecure type
                    cRpcStream.Write((byte)EProdecure.InvokeNetworkRpc);

                    // Rpc identifier
                    cRpcStream.Write(bNetworkRpcId);

                    // Method parameter values
                    cRpcStream.Write(tMethodInfo, _caParameterValues);


                    // Send to all players
                    if (_uiPlayerId == 0)
                    {
                        foreach (KeyValuePair<uint, CNetworkPlayer> tEntry in CNetworkPlayer.FindAll())
                        {
                            // Make host execute RPC straight away
                            if (tEntry.Value.IsHost())
                            {
                                CNetworkView.ProcessInboundNetworkData(cRpcStream);
                                cRpcStream.SetReadOffset(0);
                            }
                            else
                            {
                                // Append packet data
                                tEntry.Value.PacketStream.Write(cRpcStream);

                                Logger.WriteError("Written {0} bytes into player ({1})", cRpcStream.GetSize(), tEntry.Value.PlayerId);
                            }
                        }

                        Logger.Write("Sent RPC call for ({0}) to all players", _sFunction);
                    }

                    // Send to individual player
                    else
                    {
                        // Retrieve player instance
                        CNetworkPlayer cNetworkPlayer = CNetworkPlayer.FindUsingPlayerId(_uiPlayerId);

                        // Make host execute RPC straight away
                        if (cNetworkPlayer.IsHost())
                        {
                            CNetworkView.ProcessInboundNetworkData(cRpcStream);
                            cRpcStream.SetReadOffset(0);
                        }
                        else
                        {
                            // Append packet data
                            cNetworkPlayer.PacketStream.Write(cRpcStream);
                        }

                        Logger.Write("Sent RPC call for ({0}) to player id ({1})", _sFunction, _uiPlayerId);
                    }
                }
            }
        }
	}


    public void SyncNetworkVarAll(byte _bNetworkVarId)
    {
        SyncNetworkVar(0, _bNetworkVarId);
    }


    public void SyncNetworkVar(uint _uiPlayerId, byte _bNetworkVarId)
    {
        if (!CGame.IsServer())
        {
            Logger.WriteError("Clients cannot sync network vars fool!");
            return;
        }

        // Instance new packet stream
        CPacketStream cVarStream = new CPacketStream();

        // View id
        cVarStream.Write(this.ViewId);

        // Prodecure type
        cVarStream.Write((byte)EProdecure.SyncNetworkVar);

        // Network var identifier
        cVarStream.Write(_bNetworkVarId);

        // Network var value
        cVarStream.Write(m_mNetworkVars[_bNetworkVarId].GetAsObject(), m_mNetworkVars[_bNetworkVarId].GetValueType());

        // Send to all players
        if (_uiPlayerId == 0)
        {
            foreach (KeyValuePair<uint, CNetworkPlayer> tEntry in CNetworkPlayer.FindAll())
            {
                // Make host execute sync straight away
                if (tEntry.Value.IsHost())
                {
                    CNetworkView.ProcessInboundNetworkData(cVarStream);
                    cVarStream.SetReadOffset(0);
                }
                else
                {
                    // Append packet data
                    tEntry.Value.PacketStream.Write(cVarStream);
                }
            }

            Logger.Write("Sent network var sync (id {0}) to all players", _bNetworkVarId);
        }

        // Send to individual player
        else
        {
            // Retieve player instance
            CNetworkPlayer cNetworkPlayer = CNetworkPlayer.FindUsingPlayerId(_uiPlayerId);

            // Make host execute sync straight away
            if (cNetworkPlayer.IsHost())
            {
                CNetworkView.ProcessInboundNetworkData(cVarStream);
                cVarStream.SetReadOffset(0);
            }
            else
            {
                // Append packet data
                cNetworkPlayer.PacketStream.Write(cVarStream);
            }

            Logger.Write("Sent network var sync id ({0}) to player id ({1})", _bNetworkVarId, _uiPlayerId);
        }
    }


    public void SyncPlayerNetworkVarValues(uint _uiPlayerId)
    {
        // Retieve player instance
        CNetworkPlayer cNetworkPlayer = CNetworkPlayer.FindUsingPlayerId(_uiPlayerId);

        // Send all current network variable values to player
        foreach (KeyValuePair<byte, INetworkVar> tEntry in m_mNetworkVars)
        {
            INetworkVar cNetworkVar = tEntry.Value;

            SyncNetworkVar(_uiPlayerId, tEntry.Key);
        }

        Logger.WriteError("Sent player id ({0}) all network var values from network view id ({1})", _uiPlayerId, this.ViewId);
    }
     

	public void OnNetworkVarChange(byte _bNetworkVarId)
	{
        SyncNetworkVarAll(_bNetworkVarId);
	}


    public ushort ViewId
    {
        set
        {
            // Ensure network view id cannot change once set
            if (m_usViewId != 0)
            {
                Logger.WriteError("The network view id cannot be changed once set. CurrentViewId({0}) TargetViewId({1})", m_usViewId, value);
            }
            else
            {
                // Check network view id already exists
                if (s_cNetworkViews.ContainsKey(value))
                {
                    // Ensure there is not network view attached to this view id yet
                    if (s_cNetworkViews[value] != null)
                    {
                        Logger.WriteError("Unable to assign GameObject ({0}) network view id ({1}) because its already in use!", gameObject.name, value);
                    }

                    // Take ownership of this network view id
                    else
                    {
                        s_cNetworkViews[value] = this;
                        m_usViewId = value;
                    }
                }

                // Ensure servers should never reach this part for dynamic view ids
                // because the keys without owners are created during GenerateDynamicViewId()
                else if (value < k_usMaxStaticViewId ||
                         !CGame.IsServer())
                {
                    s_cNetworkViews.Add(value, this);
                    m_usViewId = value;
                }
                else
                {
                    Logger.WriteError("Somethign went wrong when setting the network view id ({0})", value);
                }
            }
        }

        get { return (m_usViewId); }
    }


    public static ushort GenerateDynamicViewId()
    {
        ushort usViewId = 0;

        // Ensure servers only generate dynamic view ids
        if (!CGame.IsServer())
        {
            Logger.WriteError("Clients cannot generate network view ids!!!");
        }
        else
        {
            for (ushort i = k_usMaxStaticViewId; i < k_usMaxDynamicViewId; ++i)
            {
                // Check the dynamic view id is free
                if (!s_cNetworkViews.ContainsKey(i))
                {
                    usViewId = i;

                    // Add id into list without owner
                    s_cNetworkViews.Add(i, null);

                    break;
                }

                // Check reached maximum number of dynamic view ids
                if (i == ushort.MaxValue - 1)
                {
                    Logger.WriteError("Oh shit, the network view id generator ran out of ids. The game is now broken. GG");
                }
            }
        }


        return (usViewId);
    }


    public static ushort GenerateStaticViewId()
    {
        ushort usViewId = 0;

        for (ushort i = 5; i < k_usMaxStaticViewId; ++i)
        {
            // Check the static view id is free
            if (!s_cNetworkViews.ContainsKey(i))
            {
                usViewId = i;

                // Add id into list without owner
                s_cNetworkViews.Add(i, null);

                break;
            }

            // Check reached maximum number of static view ids
            if (i == k_usMaxStaticViewId - 1)
            {
                Logger.WriteError("Oh shit, the network view id generator ran out of ids. The game is now broken. GG");
            }
        }


        return (usViewId);
    }


    public static CNetworkView FindUsingViewId(ushort _usViewId)
    {
		CNetworkView cNetworkView = null;
		

		if (!s_cNetworkViews.ContainsKey(_usViewId))
		{
			Logger.WriteError("Cannot find network view with id ({0})", _usViewId);
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
        // Extract components from game object
        CNetworkMonoBehaviour[] aComponents = gameObject.GetComponents<CNetworkMonoBehaviour>();


        foreach (CNetworkMonoBehaviour cComponent in aComponents)
        {
            // Initialise the network vars within network component
            cComponent.InitialiseNetworkVars();

            // Extract fields from component
            FieldInfo[] aFieldInfos = cComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


            foreach (FieldInfo cFieldInfo in aFieldInfos)
            {
                // Check this is a network var
                if (cFieldInfo.FieldType.GetInterface(typeof(INetworkVar).Name, false) != null)
                {
                    // Extract network var instance
                    INetworkVar cNetworkVar = (INetworkVar)cFieldInfo.GetValue(cComponent);

                    // Generate network var id, 1-byte.MAX
                    byte bId = (byte)(m_mNetworkVars.Count + 1);

                    // Store network var instance towards ids
                    m_mNetworkVars.Add(bId, cNetworkVar);

                    // Set the network view owner of the network var
                    cNetworkVar.SetNetworkViewOwner(this, bId);

                    Logger.Write("Added network var to list");
                }
            }
        }
    }


	void InitialiseNetworkRpcs()
    {
        // Extract components from game object
        CNetworkMonoBehaviour[] aComponents = gameObject.GetComponents<CNetworkMonoBehaviour>();


        foreach (CNetworkMonoBehaviour cComponent in aComponents)
        {
            // Extract methods from component
            MethodInfo[] caMethodInfos = cComponent.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


            foreach (MethodInfo cMethodInfo in caMethodInfos)
            {
				foreach (ANetworkRpc cAttribute in cMethodInfo.GetCustomAttributes(typeof(ANetworkRpc), false))
				{
                    // Generate network rpc id, 1-byte.MAX
					byte bId = (byte)(m_mNetworkRpcs.Count + 1);

                    // Create rpc method info
                    TRpcMethod tRpcMethodInfo = new TRpcMethod();
                    tRpcMethodInfo.cUnityComponent = cComponent;
                    tRpcMethodInfo.cMethodInfo = cMethodInfo;
                    
                    // Store the rpc info towards the id
                    m_mNetworkRpcs.Add(bId, tRpcMethodInfo);

					Logger.Write("Added network rpc ({0}) to list. Method name ({1})", bId, cMethodInfo.Name);
				}
			}
		}
	}


	byte FindNetworkRpcIndexUsingMethodInfo(MethodInfo _tMethodInfo)
	{
		byte bNetworkRpcId = 0;

        // Iterate thorugh all the network rpcs in this network view
        foreach (KeyValuePair<byte, TRpcMethod> tEntry in m_mNetworkRpcs)
		{
            // Check if this is a match
			if (tEntry.Value.cMethodInfo == _tMethodInfo)
			{
				bNetworkRpcId = tEntry.Key;
				break;
			}
		}


		return (bNetworkRpcId);
	}


    void SendPlayerInvokeRpc(CNetworkPlayer _cTargetPlayer, CPacketStream _cData)
    {

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