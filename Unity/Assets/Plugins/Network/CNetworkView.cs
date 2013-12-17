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


public class CNetworkView : CNetworkMonoBehaviour
{

// Member Types


	public const ushort k_usMaxStaticViewId = 500;
	public const ushort k_usMaxDynamicViewId = ushort.MaxValue;


    public enum EProdecure : byte
    {
        InvokeNetworkRpc,
        SyncNetworkVar,
		SyncNetworkVarDefault,
    }


    public struct TRpcMethod
    {
        public Component cUnityComponent;
        public MethodInfo cMethodInfo;
    }


	public delegate void NotiftyPreDestory();
	public event NotiftyPreDestory EventPreDestory;


// Member Functions
    
    // public:


	public override void InstanceNetworkVars()
	{
		// Empty
	}


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


	public void OnPreDestory() // Call in CNetworkFactory
	{
		if (EventPreDestory != null)
		{
			EventPreDestory();
		}

        s_cNetworkViews.Remove(ViewId);
	}


    public void OnDestroy()
    {
        
    }


    public void Update()
    {
		// Empty
    }


    public void InvokeRpc(ulong _ulPlayerId, Component _cComponent, string _sFunction, params object[] _caParameterValues)
	{
        // Ensure sure only servers can invoke rpcs
        Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers can invoke RPCs douche");

        // Extract method from component using its name
        MethodInfo tMethodInfo = _cComponent.GetType().GetMethod(_sFunction, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Ensure the method exists
        Logger.WriteErrorOn(tMethodInfo == null, "Could not find method ({0}) in component ({1})", _sFunction, _cComponent.GetType().Name);
 
        // Find network rpc index using the method info
        byte bNetworkRpcId = FindNetworkRpcIndexUsingMethodInfo(tMethodInfo);

        // Ensure the network rpc was found
        Logger.WriteErrorOn(bNetworkRpcId == 0, "The network rpc method ({0}) in component ({1}) is not connected to this network view id ({2})", _sFunction, _cComponent.GetType().Name, this.ViewId);

        // Instance new packet stream
        CNetworkStream cRpcStream = new CNetworkStream();

        // View id
        cRpcStream.Write(this.ViewId);

        // Prodecure type
        cRpcStream.Write((byte)EProdecure.InvokeNetworkRpc);

        // Rpc identifier
        cRpcStream.Write(bNetworkRpcId);

        // Method parameter values
        cRpcStream.Write(tMethodInfo, _caParameterValues);

        // Send to all players
        if (_ulPlayerId == 0)
        {
			// Process on server straight away
			CNetworkView.ProcessInboundStream(0, cRpcStream);
			cRpcStream.SetReadOffset(0);

			// Append rpc stream to connected non-host players
            foreach (KeyValuePair<ulong, CNetworkPlayer> tEntry in CNetwork.Server.FindNetworkPlayers())
            {
                // Make host execute RPC straight away
                if (!tEntry.Value.IsHost)
                {
                    // Append packet data
                    tEntry.Value.NetworkViewStream.Write(cRpcStream);	

                    //Logger.WriteError("Written {0} bytes into player ({1})", cRpcStream.Size, tEntry.Value.PlayerId);
                }
            }

			// Notice
            Logger.Write("Sent RPC call for ({0}) to all players", _sFunction);
        }

        // Send to individual player
        else
        {
            // Retrieve player instance
            CNetworkPlayer cNetworkPlayer = CNetwork.Server.FindNetworkPlayer(_ulPlayerId);

            // Make host execute RPC straight away
            if (cNetworkPlayer.IsHost)
            {
                CNetworkView.ProcessInboundStream(0, cRpcStream);
                cRpcStream.SetReadOffset(0);
            }
            else
            {
                // Append packet data
                cNetworkPlayer.NetworkViewStream.Write(cRpcStream);
            }

            Logger.Write("Sent RPC call for ({0}) to player id ({1})", _sFunction, _ulPlayerId);
        }
	}


    public void SyncNetworkVar(ulong _ulPlayerId, byte _bNetworkVarId)
    {
        // Ensure only servers can sync network vars
        Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot sync network vars fool!");

        // Instance new packet stream
        CNetworkStream cVarStream = new CNetworkStream();

        // View id
        cVarStream.Write(this.ViewId);

        // Prodecure type
        cVarStream.Write((byte)EProdecure.SyncNetworkVar);

        // Network var identifier
        cVarStream.Write(_bNetworkVarId);

        // Network var value
        cVarStream.Write(m_mNetworkVars[_bNetworkVarId].GetValueObject(), m_mNetworkVars[_bNetworkVarId].GetValueType());

        // Send to all players
        if (_ulPlayerId == 0)
        {
			// Process on server straight away
			CNetworkView.ProcessInboundStream(0, cVarStream);
			cVarStream.SetReadOffset(0);

			// Append network var sync stream to connected non-host players
            foreach (KeyValuePair<ulong, CNetworkPlayer> tEntry in CNetwork.Server.FindNetworkPlayers())
            {
                // Make host execute sync straight away
                if (!tEntry.Value.IsHost)
                {
                    // Append packet data
                    tEntry.Value.NetworkViewStream.Write(cVarStream);
                }
            }

			// Notice
            //Logger.Write("Sent network var sync (id {0}) to all players", _bNetworkVarId);
        }

        // Send to individual player
        else
        {
            // Retieve player instance
            CNetworkPlayer cNetworkPlayer = CNetwork.Server.FindNetworkPlayer(_ulPlayerId);

            // Make host execute sync straight away
            if (cNetworkPlayer.IsHost)
            {
                CNetworkView.ProcessInboundStream(0, cVarStream);
                cVarStream.SetReadOffset(0);
            }
            else
            {
                // Append packet data
                cNetworkPlayer.NetworkViewStream.Write(cVarStream);
            }

			// Notice
            //Logger.Write("Sent network var sync id ({0}) to player id ({1})", _bNetworkVarId, _ulPlayerId);
        }
    }


    public void SyncNetworkVarsWithPlayer(ulong _ulPlayerId)
    {
        // Send all current network variable values to player
        foreach (KeyValuePair<byte, INetworkVar> tEntry in m_mNetworkVars)
        {
            INetworkVar cNetworkVar = tEntry.Value;

			if (!cNetworkVar.IsDefault())
			{
				SyncNetworkVar(_ulPlayerId, tEntry.Key);
			}
        }

        Logger.Write("Sent player id ({0}) all network var values from network view id ({1})", _ulPlayerId, this.ViewId);
    }


	public void SyncTransformPosition()
	{
		SetPosition(transform.position.x, transform.position.y, transform.position.z);
	}


	public void SyncTransformRotation()
	{
		SetRotation(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
	}


	public void SyncTransformScale()
	{
		// Ensure servers only sync parents
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot sync network object's transform scale!!!");

		InvokeRpcAll("RemoteSetScale", transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}


    public void SyncRigidBodyMass()
    {
        // Ensure servers only sync parents
        Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot sync network object's rigid body mass!!!");

		InvokeRpcAll("RemoteSetRigidBodyMass", rigidbody.mass);
    }


	public void SyncParent()
	{
		if (transform.parent != null)
		{
			SetParent(transform.parent.GetComponent<CNetworkView>().ViewId);
		}
		else
		{
			SetParent(0);
		}
	}


	public void SetParent(ushort _sParentViewId)
	{
		// Ensure servers only sync parents
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's parents!!!");

		// Ensure transform has a network view for parent
		Logger.WriteErrorOn(_sParentViewId != 0 && FindUsingViewId(_sParentViewId).GetComponent<CNetworkView>() == null, "Syncing to a parent requires a parent with a network view!!!");

		InvokeRpcAll("RemoteSetParent", _sParentViewId);
	}


	public void SetPosition(Vector3 _vPosition)
	{
		SetPosition(_vPosition.x, _vPosition.y, _vPosition.z);
	}


	public void SetPosition(float _fX, float _fY, float _fZ)
	{
		// Ensure servers only sync transforms
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's transform position!!!");

		InvokeRpcAll("RemoteSetPosition", _fX, _fY, _fZ);
	}


	public void SetRotation(Vector3 _vEulerAngles)
	{
		SetRotation(_vEulerAngles.x, _vEulerAngles.y, _vEulerAngles.z);
	}


	public void SetRotation(float _fX, float _fY, float _fZ)
	{
		// Ensure servers only set transforms
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's transform rotation!!!");

		InvokeRpcAll("RemoteSetRotation", _fX, _fY, _fZ);
	}


	public ushort ViewId
	{
		set
		{
			// Ensure network view id cannot change once set
			if (CNetwork.IsServer &&
				m_usViewId != 0)
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
						 !CNetwork.IsServer)
				{
					s_cNetworkViews.Add(value, this);
					m_usViewId = value;
				}
				else
				{
					Logger.WriteError("Somethign went wrong when setting the network view id ({0})", value);
				}
			}
			lol = m_usViewId;
		}

		get { return (m_usViewId); }
	}


    public static ushort GenerateDynamicViewId()
    {
		// Ensure servers only generate dynamic view ids
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot generate network view ids!!!");

        ushort usViewId = 0;

        for (ushort i = k_usMaxStaticViewId; i < k_usMaxDynamicViewId; ++i)
        {
            // Check the dynamic view id is free
            if (!s_cNetworkViews.ContainsKey(i))
            {
                usViewId = i;

				// Add id into list without owner so someone else does not claim the id
                s_cNetworkViews.Add(i, null);

                break;
            }
        }

		// Ensure id was generated
		Logger.WriteErrorOn(usViewId == 0, "Oh shit, the network view id generator ran out of ids. The game is now broken. GG");

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


    public static Dictionary<ushort, CNetworkView> FindAll()
    {
        return (s_cNetworkViews);
    }


    public static void ProcessInboundStream(ulong _uiLatency, CNetworkStream _cStream)
    {
		List<INetworkVar> cSyncedNetworkVars = new List<INetworkVar>();
		float fSyncedTick = 0.0f;

		if (CNetwork.IsServer)
		{
			fSyncedTick  = ((float)RakNet.RakNet.GetTimeMS()) / 1000.0f;
			fSyncedTick -= Mathf.Floor(fSyncedTick);
			fSyncedTick *= CNetworkServer.k_fSendRate;
		}
		else
		{
			fSyncedTick = CNetwork.Connection.Tick - ((float)_uiLatency / 1000.0f);

			if (fSyncedTick < 0.0f)
			{
				fSyncedTick = CNetworkServer.k_fSendRate - fSyncedTick;
				Debug.Log(fSyncedTick);
			}
		}

        while (_cStream.HasUnreadData)
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
				cNetworkVar.SyncValue(cNewVarValue, fSyncedTick);

				// Add to callback list
				cSyncedNetworkVars.Add(cNetworkVar);
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

		// Invoke callbacks for synced network vars
		foreach (INetworkVar cSyncedNetworkVar in cSyncedNetworkVars)
		{
			cSyncedNetworkVar.InvokeSyncCallback();
		}
    }


    // protected:


	protected void OnNetworkVarChange(byte _bNetworkVarId)
	{
		SyncNetworkVar(0, _bNetworkVarId);
	}


	protected static ushort GenerateStaticViewId()
	{
		ushort usViewId = 0;

		for (ushort i = 5; i < k_usMaxStaticViewId; ++i)
		{
			// Check the static view id is free
			if (!s_cNetworkViews.ContainsKey(i))
			{
				usViewId = i;

				// Add id into list without owner so someone else does not claim the id
				s_cNetworkViews.Add(i, null);

				break;
			}
		}

		// Ensure id was generated
		Logger.WriteErrorOn(usViewId == 0, "Oh shit, the network view id generator ran out of ids. The game is now broken. GG");

		return (usViewId);
	}


    // private:


    void InitialiseNetworkVars()
    {
        // Extract components from game object
        CNetworkMonoBehaviour[] aComponents = gameObject.GetComponents<CNetworkMonoBehaviour>();


        foreach (CNetworkMonoBehaviour cComponent in aComponents)
        {
            // Initialise the network vars within network component
            cComponent.InstanceNetworkVars();

            // Extract fields from component
            FieldInfo[] aFieldInfos = cComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


            foreach (FieldInfo cFieldInfo in aFieldInfos)
            {
                // Check this is a network var
				if (cFieldInfo.FieldType.IsSubclassOf(typeof(INetworkVar)))
				{
					// Extract network var instance
					INetworkVar cNetworkVar = (INetworkVar)cFieldInfo.GetValue(cComponent);
					
					
					ReigserNetworkVar(cNetworkVar);
				}
				
                // Check this is a network var
				else if (cFieldInfo.FieldType.IsArray &&
						 cFieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(INetworkVar)))
				{
					INetworkVar[] acItems = cFieldInfo.GetValue(cComponent) as INetworkVar[];

					
		            foreach (INetworkVar cItem in acItems)
		            {
						ReigserNetworkVar(cItem);
					}
				}
            }
        }
    }
	
	
	void ReigserNetworkVar(INetworkVar _cNetworkVar)
	{
		// Generate network var id, 1-byte.MAX
		byte bId = (byte)(m_mNetworkVars.Count + 1);

		// Store network var instance towards ids
		m_mNetworkVars.Add(bId, _cNetworkVar);

		// Set the network view owner of the network var
		_cNetworkVar.SetNetworkViewOwner(bId, OnNetworkVarChange);
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
				if (cMethodInfo.GetCustomAttributes(typeof(ANetworkRpc), false).Length == 1)
				{
                    // Generate network rpc id, 1-byte.MAX
					byte bId = (byte)(m_mNetworkRpcs.Count + 1);

                    // Create rpc method info
                    TRpcMethod tRpcMethodInfo = new TRpcMethod();
                    tRpcMethodInfo.cUnityComponent = cComponent;
                    tRpcMethodInfo.cMethodInfo = cMethodInfo;
                    
                    // Store the rpc info towards the id
                    m_mNetworkRpcs.Add(bId, tRpcMethodInfo);

					//Logger.Write("Added network rpc ({0}) to list. Method name ({1})", bId, cMethodInfo.Name);
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


	[ANetworkRpc]
	void RemoteSetPosition(float _fPositionX, float _fPositionY, float _fPositionZ)
	{
		transform.position = new Vector3(_fPositionX, _fPositionY, _fPositionZ);
	}


	[ANetworkRpc]
	void RemoteSetRotation(float _fRotationX, float _fRotationY, float _fRotationZ)
	{
		transform.rotation = Quaternion.Euler(_fRotationX, _fRotationY, _fRotationZ);
	}


	[ANetworkRpc]
	void RemoteSetScale(float _fScaleX, float _fScaleY, float _fScaleZ)
	{
		transform.localScale = new Vector3(_fScaleX, _fScaleY, _fScaleZ);
	}


    [ANetworkRpc]
    void RemoteSetRigidBodyMass(float _fMass)
    {
        rigidbody.mass = _fMass;
    }


	[ANetworkRpc]
	void RemoteSetParent(ushort _usParentViewId)
	{
		if (_usParentViewId != 0)
		{
			transform.parent = CNetwork.Factory.FindObject(_usParentViewId).transform;
		}
		else
		{
			transform.parent = null;
		}
	}


// Member Variables
    
    // protected:


    // private:


    public ushort m_usViewId = 0;
	public int lol = 0;
    
    Dictionary<byte, INetworkVar> m_mNetworkVars = new Dictionary<byte, INetworkVar>();
    Dictionary<byte, TRpcMethod> m_mNetworkRpcs = new Dictionary<byte, TRpcMethod>();


    static Dictionary<ushort, CNetworkView> s_cNetworkViews = new Dictionary<ushort, CNetworkView>();


};