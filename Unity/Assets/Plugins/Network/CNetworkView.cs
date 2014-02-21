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


public class CNetworkViewRegistrar
{
    public CNetworkViewRegistrar(Dictionary<byte, INetworkVar> _mNetworkVars, CNetworkVar<object>.OnSetCallback _nSetCallback,  Dictionary<byte, CNetworkView.TRpcMethod> _mRpcMethods)
    {
        m_mNetworkVars = _mNetworkVars;
        m_nSetCallback = _nSetCallback;
        m_mRpcMethods = _mRpcMethods;
    }

    public CNetworkVar<TYPE> CreateNetworkVar<TYPE>(CNetworkVar<TYPE>.OnSyncCallback _nSyncCallback, TYPE _DefaultValue)
    {
        CNetworkVar<TYPE> cNewNetworkVar = new CNetworkVar<TYPE>(_nSyncCallback, _DefaultValue);

        RegisterNetworkVar(cNewNetworkVar);

        return (cNewNetworkVar);
    }

    public CNetworkVar<TYPE> CreateNetworkVar<TYPE>(CNetworkVar<TYPE>.OnSyncCallback _nSyncCallback)
    {
        CNetworkVar<TYPE> cNewNetworkVar = new CNetworkVar<TYPE>(_nSyncCallback);

        RegisterNetworkVar(cNewNetworkVar);

        return (cNewNetworkVar);
    }


    public void RegisterRpc(CNetworkMonoBehaviour _cComponent, string _sMethodName)
    {
        // Generate network rpc id, 1-byte.MAX
        byte bId = (byte)(m_mRpcMethods.Count + 1);

        // Create rpc method info
        CNetworkView.TRpcMethod tRpcMethodInfo = new CNetworkView.TRpcMethod();
        tRpcMethodInfo.cUnityComponent = _cComponent;
        tRpcMethodInfo.cMethodInfo = _cComponent.GetType().GetMethod(_sMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Store the rpc info towards the id
        m_mRpcMethods.Add(bId, tRpcMethodInfo);

        //Debug.LogError("Regisered method: " + _sMethodName + " : " + bId);

        //Logger.Write("Added network rpc ({0}) to list. Method name ({1})", bId, cMethodInfo.Name);
    }


    void RegisterNetworkVar(INetworkVar _cNetworkVar)
    {
        // Generate network var id, 1-byte.MAX
        byte bId = (byte)(m_mNetworkVars.Count + 1);

        // Store network var instance towards ids
        m_mNetworkVars.Add(bId, _cNetworkVar);

        // Set the network view owner of the network var
        _cNetworkVar.SetNetworkViewOwner(bId, m_nSetCallback);
    }

    Dictionary<byte, INetworkVar> m_mNetworkVars;
    CNetworkVar<object>.OnSetCallback m_nSetCallback;
    Dictionary<byte, CNetworkView.TRpcMethod> m_mRpcMethods;
}


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


	public Dictionary<byte, CNetworkView> ChildrenNetworkViews
	{
		get { return (m_mChildrenNetworkViews); }
	}


// Member Functions
    
    // public:


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_cParentViewId = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
        
        _cRegistrar.RegisterRpc(this, "RemoteSetPosition");
        _cRegistrar.RegisterRpc(this, "RemoteSetRotation");
        _cRegistrar.RegisterRpc(this, "RemoteSetScale");
        _cRegistrar.RegisterRpc(this, "RemoteSetRigidBodyMass");
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cParentViewId)
		{
			if (m_cParentViewId.Get() == null)
			{
				transform.parent = null;
			}
			else
			{
				transform.parent = m_cParentViewId.Get().GameObject.transform;
			}
		}
	}


    public void Awake()
    {
        // Run class initialisers
        InitialiseNetworkVars();
		InitialiseNetworkRpcs();

		// Since I have a parent on creation, I am a child network view 
		// and i need to register with the main network view which was
		// created through the network factory
		if (transform.parent != null)
		{
			Transform cParent = transform.parent;
			
			for (int i = 0; cParent.parent != null && i < 25; ++ i)
			{
				cParent = cParent.parent;

					//Logger.WriteError("Could not find parent to register for sub view id");
					//break;
				//}
			}

			// Register for sub network view id
			cParent.GetComponent<CNetworkView>().RegisterChildNetworkView(this);
			
			if (ViewId.ChildId == 0)
				Debug.LogError("I do not have a sub view id!");
		}
    }


    public void Start()
    {
		if (m_cNetworkViewId == null)
		{
			// Generate static view id if server did not
			// provide one when this object was created
			if (transform.parent == null)
			{
				this.ViewId = GenerateStaticViewId();
			}

			if (ViewId.Id == 0)
				Debug.LogError("I do not have a view id!");
		}
    }


	public void OnPreDestory() // Call in CNetworkFactory
	{
		if (EventPreDestory != null)
		{
			EventPreDestory();
		}

		s_cNetworkViews.Remove(ViewId.Id);
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
		SetEulerAngles(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
	}


	public void SyncTransformScale()
	{
		// Ensure servers only sync parents
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot sync network object's transform scale!!!");

		InvokeRpcAll("RemoteSetScale", transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}


    public void SyncRigidBodyMass()
    {
		// Ensure servers only sync RigidBodyMass
        Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot sync network object's rigid body mass!!!");

		InvokeRpcAll("RemoteSetRigidBodyMass", rigidbody.mass);
	}

	public void SetParent(CNetworkViewId _cParentViewId)
	{
		m_cParentViewId.Set(_cParentViewId);
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


	public void SetRotation(Quaternion _Rotation)
	{
		SetEulerAngles(_Rotation.eulerAngles);
	}


	public void SetEulerAngles(Vector3 _vEulerAngles)
	{
		SetEulerAngles(_vEulerAngles.x, _vEulerAngles.y, _vEulerAngles.z);
	}


	public void SetEulerAngles(float _fX, float _fY, float _fZ)
	{
		// Ensure servers only set transforms
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's transform rotation!!!");

		InvokeRpcAll("RemoteSetRotation", _fX, _fY, _fZ);
	}


	public void SetScale(float _fX, float _fY, float _fZ)
	{
		// Ensure servers only set transforms
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's transform scale!!!");
		
		InvokeRpcAll("RemoteSetScale", _fX, _fY, _fZ);
	}


	public void SetScale(Vector3 _Scale)
	{
		SetScale(_Scale.x, _Scale.y, _Scale.z);
	}


	public CNetworkViewId ViewId
	{
		set
		{
			if (value.IsChildViewId)
			{
				m_cNetworkViewId = value;
			}
			else
			{
				// Ensure network view id cannot change once set
				if (CNetwork.IsServer &&
					m_cNetworkViewId != null)
				{
					Logger.WriteError("The network view id cannot be changed once set. CurrentViewId({0}) TargetViewId({1})", m_cNetworkViewId, value.Id);
				}
				else
				{
					// Check network view id already exists
					if (s_cNetworkViews.ContainsKey(value.Id))
					{
						// Ensure there is not network view attached to this view id yet
						if (s_cNetworkViews[value.Id] != null)
						{
							Logger.WriteError("Unable to assign GameObject ({0}) network view id ({1}) because its already in use!", gameObject.name, value.Id);
						}

						// Take ownership of this network view id
						else
						{
							s_cNetworkViews[value.Id] = this;
							m_cNetworkViewId = value;
						}
					}

					// Ensure servers should never reach this part for dynamic view ids
					// because the keys without owners are created during GenerateDynamicViewId()
					else if (value.Id < k_usMaxStaticViewId ||
							 !CNetwork.IsServer)
					{
						s_cNetworkViews.Add(value.Id, this);
						m_cNetworkViewId = value;
					}
					else
					{
						Logger.WriteError("Somethign went wrong when setting the network view id ({0})", value);
					}
				}

				if ( ViewId != null &&
				    !ViewId.IsChildViewId)
				{
					// Update children
					foreach (KeyValuePair<byte, CNetworkView> tEntity in ChildrenNetworkViews)
					{
						tEntity.Value.ViewId.Id = ViewId.Id;
					}
				}
			}
		}

		get { return (m_cNetworkViewId); }
	}


    public static CNetworkViewId GenerateDynamicViewId()
    {
		// Ensure servers only generate dynamic view ids
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot generate network view ids!!!");

        CNetworkViewId cViewId = new CNetworkViewId();

        for (ushort i = k_usMaxStaticViewId; i < k_usMaxDynamicViewId; ++i)
        {
            // Check the dynamic view id is free
            if (!s_cNetworkViews.ContainsKey(i))
            {
                cViewId = new CNetworkViewId(i, 0);

				// Add id into list without owner so someone else does not claim the id
                s_cNetworkViews.Add(i, null);

                break;
            }
        }

		// Ensure id was generated
		Logger.WriteErrorOn(cViewId.Id == 0, "Oh shit, the network view id generator ran out of ids. The game is now broken. GG");

        return (cViewId);
    }


    public static CNetworkView FindUsingViewId(CNetworkViewId _cViewId)
    {
		CNetworkView cNetworkView = null;

		if (_cViewId != null)
		{
			if (!s_cNetworkViews.ContainsKey(_cViewId.Id))
			{
				Logger.WriteError("Cannot find network view with id ({0})", _cViewId.Id);
			}
			else
			{
				cNetworkView = s_cNetworkViews[_cViewId.Id];
			}

			if (_cViewId != null &&
			    _cViewId.IsChildViewId)
			{
				/*
				foreach (KeyValuePair<byte, CNetworkView> Entry in cNetworkView.m_SubNetworkViews)
				{
					Debug.LogError(string.Format("MyViewId({0}) ChildId({1}) ChildViewId({2}) ChildSubViewId({3}) ",
					                             cNetworkView.ViewId.Id, Entry.Key, Entry.Value.ViewId.Id, Entry.Value.ViewId.SubId));
				}
				*/

				cNetworkView = cNetworkView.FindChildNetworkView(_cViewId.ChildId);

				Logger.WriteErrorOn(cNetworkView == null, "Could not find child network view. ViewId({0}) SubViewId({1})", _cViewId.Id, _cViewId.ChildId);
			}
		}

        return (cNetworkView);
    }


	public CNetworkView FindChildNetworkView(byte _bSubViewId)
	{
		if (!m_mChildrenNetworkViews.ContainsKey(_bSubViewId))
		{
			return (null);
		}

		return (m_mChildrenNetworkViews[_bSubViewId]);
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
				//Debug.Log(fSyncedTick);
			}
		}

        while (_cStream.HasUnreadData)
        {
            // Extract owner network view id
            CNetworkViewId cNetworkViewId = _cStream.ReadNetworkViewId();

            // Extract procedure type
            EProdecure eProcedure = (EProdecure)_cStream.ReadByte();

            // Retrieve network view instance
            CNetworkView cNetworkView = CNetworkView.FindUsingViewId(cNetworkViewId);
			
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
                object cNewVarValue = _cStream.ReadType(cVarType);

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


	protected static CNetworkViewId GenerateStaticViewId()
	{
		CNetworkViewId cViewId = null;

		for (ushort i = 5; i < k_usMaxStaticViewId; ++i)
		{
			// Check the static view id is free
			if (!s_cNetworkViews.ContainsKey(i))
			{
				cViewId = new CNetworkViewId(i, 0);

				// Add id into list without owner so someone else does not claim the id
				s_cNetworkViews.Add(i, null);

				break;
			}
		}

		// Ensure id was generated
		Logger.WriteErrorOn(cViewId == null, "Oh shit, the network view id generator ran out of ids. The game is now broken. GG");

		return (cViewId);
	}


    // private:


    void InitialiseNetworkVars()
    {
        // Extract components from game object
        CNetworkMonoBehaviour[] aComponents = gameObject.GetComponents<CNetworkMonoBehaviour>();
        CNetworkViewRegistrar cRegistrar = new CNetworkViewRegistrar(m_mNetworkVars, OnNetworkVarChange, m_mNetworkRpcs);

        foreach (CNetworkMonoBehaviour cNetworkMonoBehaviour in aComponents)
        {
            cNetworkMonoBehaviour.InstanceNetworkVars(cRegistrar);
        }

        /*
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
       */
    }


	void InitialiseNetworkRpcs()
    {
        /*
        CNetworkMonoBehaviour[] aComponents = gameObject.GetComponents<CNetworkMonoBehaviour>();
        CNetworkViewRegistrar cRegistrar = new CNetworkViewRegistrar(m_mNetworkVars, OnNetworkVarChange);

        foreach (CNetworkMonoBehaviour cNetworkMonoBehaviour in aComponents)
        {
            cNetworkMonoBehaviour.InstanceNetworkVars(cRegistrar);
        }
         * */


        /*
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
         * */
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


	void RegisterChildNetworkView(CNetworkView _cChildView)
	{
		Logger.WriteErrorOn(_cChildView.ViewId != null, "Child network view has already been registered a network view id");
		
		for (byte i = 1; i < byte.MaxValue; ++ i)
		{
			if (!m_mChildrenNetworkViews.ContainsKey(i))
			{
				m_mChildrenNetworkViews.Add(i, _cChildView);
				_cChildView.ViewId = new CNetworkViewId(0, i);

				//Debug.LogError(string.Format("Registered ({0}) sub newwork view with ViewId({1}) SubViewId({2})", _cSubView.gameObject.name, _cSubView.ViewId.Id, _cSubView.ViewId.ChildId));

				break;
			}
		}
	}


// Member Variables
    
    // protected:


    // private:


    public CNetworkViewId m_cNetworkViewId = null;


	CNetworkVar<CNetworkViewId> m_cParentViewId = null;
    

    Dictionary<byte, INetworkVar> m_mNetworkVars = new Dictionary<byte, INetworkVar>();
    Dictionary<byte, TRpcMethod> m_mNetworkRpcs = new Dictionary<byte, TRpcMethod>();
	Dictionary<byte, CNetworkView> m_mChildrenNetworkViews = new Dictionary<byte, CNetworkView>();


	static Dictionary<ushort, CNetworkView> s_cNetworkViews = new Dictionary<ushort, CNetworkView>();


};