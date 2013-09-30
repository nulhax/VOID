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


#pragma warning disable 0649


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


    public struct TRpc
    {
        public byte bMethodId;
        public object[] caParameterData;
    }


    public struct TRpcMethod
    {
        public Component cUnityComponent;
        public MethodInfo cMethodInfo;
    }


// Member Functions
    
    // public:


    public void Start()
    {
        // Generate static view id if server did not provide one
        if (m_usViewId == 0)
        {
            SetViewId(GenerateStaticViewId());
        }
        

        InitialiseNetworkVars();
		InitialiseNetworkRpcs();
    }
    
    
    public void OnDestroy()
    {
		// Empty
    }


    public void Update()
    {
		// Empty
    }


	public void InvokeRpc(Component _cComponent, string _sFunction, params object[] _caParameters)
	{
        if (!CGame.IsServer())
        {
            Debug.LogError("Only servers can invoke RPCs  douche");
        }
        else
        {
            MethodInfo tMethodInfo = _cComponent.GetType().GetMethod(_sFunction, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


            if (tMethodInfo == null)
            {
                Debug.LogError(string.Format("Could not find method ({0}) in component ({1})", _sFunction, _cComponent.GetType().Name));
            }
            else
            {
                byte bNetworkRpcId = GetNetworkRpcIndexByMethodInfo(tMethodInfo);


                if (bNetworkRpcId == 0)
                {
                    Debug.LogError(string.Format("The network rpc method ({0}) in component ({1}) is not connected to this network view id({2})", _sFunction, _cComponent.GetType().Name, m_usViewId));
                }
                else
                {
                    TRpc tRpc = new TRpc();
                    tRpc.bMethodId = bNetworkRpcId;
                    tRpc.caParameterData = _caParameters;


                    m_aRemoteProcedureCalls.Add(tRpc);
                    SetDirty();


                    //Debug.Log("Sent RPC call for " + _sFunction);
                }
            }
        }
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
     

	public void OnNetworkVarChange(byte _bNetworkVarId)
	{
		if (!m_aDirtyNetworkVariableIds.Contains(_bNetworkVarId))
		{
			m_aDirtyNetworkVariableIds.Add(_bNetworkVarId);
            SetDirty();


			//Debug.Log(string.Format("Added network variable id ({0}) to network view id's ({1}) dirty container", _bNetworkVarId, m_usViewId));
		}
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


	public static void CompileOutboundData(CPacketStream _cTransmission)
	{
		// Loop through all network views
		foreach(KeyValuePair<ushort, CNetworkView> tEntry in s_cNetworkViews)
		{
            // Skip undefined entries
            if (tEntry.Value == null)
                continue;


            // Extract the network view instance
			CNetworkView cNetworkView = tEntry.Value;


			// Check is dirty
            if (cNetworkView.m_bDirty)
			{
				// Write in network view id - To identify on clients
				_cTransmission.Write(cNetworkView.GetViewId()); // 2 Bytes

                // Write in the number of dirty variables
                _cTransmission.Write((byte)cNetworkView.m_aDirtyNetworkVariableIds.Count); // 1 Byte

                // Write the number of RPC's
                _cTransmission.Write((byte)cNetworkView.m_aRemoteProcedureCalls.Count); // 1 Byte


                // Write dirty variables data
				foreach (byte bDirtyNetworkVarId in cNetworkView.m_aDirtyNetworkVariableIds)
				{
                    // Extract dirty variable
					INetworkVar cNetworkVar = cNetworkView.m_cNetworkVariables[bDirtyNetworkVarId];

					// Write in network var identifier
                    _cTransmission.Write(bDirtyNetworkVarId); // 1 Byte

                    // Get network var vlaue serialized
                    byte[] baVarSerialized = cNetworkVar.GetSerialized();

                    // String types will need to insert their length as their size is undefined
                    if (cNetworkVar.GetVarType() == ENetworkVarType.String)
                    {
                        _cTransmission.Write((byte)baVarSerialized.Length); // 1 Bytes
                    }

                    // Finally write the value into the transmission
                    _cTransmission.Write(baVarSerialized); // Unknown number of bytes
                   
                    //Debug.LogError(string.Format("Size ({0}) Data({1})", cNetworkVar.GetSize(), cNetworkVar.GetSerialized()));
				}


                // Loop through Romote Procedure Calls
                foreach (TRpc tRpc in cNetworkView.m_aRemoteProcedureCalls)
                {
                    // Write the RPC identifier
                    _cTransmission.Write(tRpc.bMethodId);

                    // Get method parameters
                    ParameterInfo[] caParameters = cNetworkView.m_cNetworkRpcs[tRpc.bMethodId].cMethodInfo.GetParameters();

                    // Write RPC parameters
                    for (int j = 0; j < caParameters.Length; ++ j)
                    {
                        // Serialize the parameter value
                        byte[] baValueSerialized = Converter.ToByteArray(tRpc.caParameterData[j], caParameters[j].ParameterType);

                        if (caParameters[j].ParameterType == typeof(string))
                        {
                            _cTransmission.Write((byte)baValueSerialized.Length);
                        }

                        // Write parameter value
                        _cTransmission.Write(baValueSerialized);
                        //Debug.LogError(string.Format("Write: Index ({0}) Type ({1}) Size ({2})", j, caParameters[j].ParameterType, baValueSerialized.Length));
                    }
                }


                // Clear queued RPC & dirty network variable lists
                cNetworkView.m_aRemoteProcedureCalls.Clear();
				cNetworkView.m_aDirtyNetworkVariableIds.Clear();
			}
		}


		s_bHasOutboundData = false;
	}


    public static void HandleInboundData(CPacketStream _cTransmission)
    {
		// Loop until all the data has been read
		while (_cTransmission.HasUnreadData())
		{
			// Rewad owner network view id for this segment
			ushort usNetworkViewId = _cTransmission.ReadUShort();
			
			// Read the number of variable changes in this segment
			byte bNumNetworkVarChanges = _cTransmission.ReadByte();

            // Read the number of RPCs in this segment
            byte bRpcCount = _cTransmission.ReadByte();

            // Retrieve network view instance based on the network view id
            CNetworkView cNetworkView = FindUsingViewId(usNetworkViewId);
            

			// Process the variable changes
            for (byte i = 0; i < bNumNetworkVarChanges; ++i)
			{
				// Retrieve the target network var id
				byte bNetworkVarId = _cTransmission.ReadByte();

                // Extract network var instance base on network var id
                INetworkVar cNetworkVariable = cNetworkView.m_cNetworkVariables[bNetworkVarId];

                // Get network var value size so we know much to read from the transmission
                int iNetworkVarValueSize = cNetworkVariable.GetSize();

                // Strings have their size stored inside the transmission as a byte
                if (cNetworkVariable.GetVarType() == ENetworkVarType.String)
                {
                    iNetworkVarValueSize = _cTransmission.ReadByte();
                }

				// Read out the network var value data
                byte[] baNewValue = _cTransmission.ReadBytes(iNetworkVarValueSize);

				// Sync the new value for the network var
                cNetworkVariable.SyncSerialized(baNewValue);
			}


            // Process RPCs
            for (byte i = 0; i < bRpcCount; ++ i)
            {
                // Read the RPC method identifier
                byte bRpcMethodId = _cTransmission.ReadByte();

                // Retrieve the RPC method info base on the RPC method identifier
                TRpcMethod tRpcMethodInfo = cNetworkView.m_cNetworkRpcs[bRpcMethodId];

                // Extract the parameters from the method
                ParameterInfo[] caParameters = tRpcMethodInfo.cMethodInfo.GetParameters();

                // Instance a array of parameter objects
                object[] caParameterValues = new object[caParameters.Length];


                // Read parameter values from the transmission
                for (int j = 0; j < caParameters.Length; ++ j)
                {
                    // Read the size of the parameter type so we know how much to read from the transmission
                    int iParameterTypeSize = Converter.GetSizeOf(caParameters[j].ParameterType);


                    if (caParameters[j].ParameterType == typeof(string))
                    {
                        iParameterTypeSize = _cTransmission.ReadByte();
                    }

                    if (iParameterTypeSize > 0)
                    {
                        // Read the parameter value
                        byte[] baParameterValueSerialized = _cTransmission.ReadBytes(iParameterTypeSize);

                        caParameterValues[j] = Converter.ToType(baParameterValueSerialized, caParameters[j].ParameterType);
                        //Debug.LogError(string.Format("Index ({0}) Type ({1}) Size ({2})", j, caParameters[j].ParameterType, iParameterTypeSize));
                    }
                }


                // Finally invoke the function on the client with the extracted parameter values
                tRpcMethodInfo.cMethodInfo.Invoke(tRpcMethodInfo.cUnityComponent, caParameterValues);
            }
		}
    }


	public static bool HasOutboundData()
	{
		return (s_bHasOutboundData);
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


    // protected:


    // private:


    void InitialiseNetworkVars()
    {
        MonoBehaviour[] aComponents = gameObject.GetComponents<MonoBehaviour>();


        foreach (MonoBehaviour cComponent in aComponents)
        {
            if (cComponent == this)
				continue;


            FieldInfo[] aFieldInfos = cComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		
            foreach (FieldInfo cFieldInfo in aFieldInfos)
            {
				if (cFieldInfo.FieldType.GetInterface(typeof(INetworkVar).Name, false) != null)
				{
					INetworkVar cNetworkVar = (INetworkVar)cFieldInfo.GetValue(cComponent);
					byte bId = (byte)(m_cNetworkVariables.Count + 1);


					m_cNetworkVariables.Add(bId, cNetworkVar);
					cNetworkVar.SetNetworkViewOwner(this, bId);


					//Debug.Log("Added network var to list");
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
					byte bId = (byte)(m_cNetworkRpcs.Count + 1);


                    TRpcMethod tRpcMethodInfo = new TRpcMethod();
                    tRpcMethodInfo.cUnityComponent = cComponent;
                    tRpcMethodInfo.cMethodInfo = cMethodInfo;
                    
                    
                    m_cNetworkRpcs.Add(bId, tRpcMethodInfo);


					//Debug.Log(string.Format("Added network rpc ({0}) to list. Method name ({1})", bId, cMethodInfo.Name));
				}
			}
		}
	}


	byte GetNetworkRpcIndexByMethodInfo(MethodInfo _tMethodInfo)
	{
		byte bNetworkRpcId = 0;


        foreach (KeyValuePair<byte, TRpcMethod> tEntry in m_cNetworkRpcs)
		{
			if (tEntry.Value.cMethodInfo == _tMethodInfo)
			{
				bNetworkRpcId = tEntry.Key;
				break;
			}
		}


		return (bNetworkRpcId);
	}


    void SetDirty()
    {
        m_bDirty = true;
        s_bHasOutboundData = true;
    }


// Member Variables
    
    // public:


    // private:


	List<byte> m_aDirtyNetworkVariableIds = new List<byte>();
    List<TRpc> m_aRemoteProcedureCalls = new List<TRpc>();
    Dictionary<byte, INetworkVar> m_cNetworkVariables = new Dictionary<byte, INetworkVar>();
    Dictionary<byte, TRpcMethod> m_cNetworkRpcs = new Dictionary<byte, TRpcMethod>();


    ushort m_usViewId = 0;


    bool m_bDirty;


    static Dictionary<ushort, CNetworkView> s_cNetworkViews = new Dictionary<ushort, CNetworkView>();
	static bool s_bHasOutboundData = false;


};


/*
void RegisterComponent(MonoBehaviour _cComponent)
{
    MemberInfo[] aMethodInfos = _cComponent.GetType().GetMethods();


    foreach (MemberInfo cMethodInfo in aMethodInfos)
    {
        NetworkRpc[] cAttribrutes = cMethodInfo.GetCustomAttributes(typeof(NetworkRpc), false) as NetworkRpc[];


        foreach (NetworkRpc cAttribute in cAttribrutes)
        {
            TRpcMethod cRpcMethod = new TRpcMethod();
            cRpcMethod.cComponent = _cComponent;
            cRpcMethod.sName = cMethodInfo.Name;


            m_mRpcMethods.Add(cAttribute.GetUniqueId(), cRpcMethod);
        }
    }
}
 * */