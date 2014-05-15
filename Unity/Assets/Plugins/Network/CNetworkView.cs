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
    public CNetworkViewRegistrar(Dictionary<byte, INetworkVar> _mNetworkVars, CNetworkView _cOwnerNetworkView, Dictionary<byte, CNetworkView.TRpcMethod> _mRpcMethods)
    {
        m_mNetworkVars = _mNetworkVars;
        m_cOwnerNetworkView = _cOwnerNetworkView;
        m_mRpcMethods = _mRpcMethods;
    }


    public CNetworkVar<TYPE> CreateReliableNetworkVar<TYPE>(CNetworkVar<TYPE>.SyncedHandler _nSyncCallback, TYPE _DefaultValue)
    {
        CNetworkVar<TYPE> cNewNetworkVar = new CNetworkVar<TYPE>(_nSyncCallback, _DefaultValue);

        RegisterNetworkVar(cNewNetworkVar, INetworkVar.EReliabilityType.Reliable_Ordered);

        return (cNewNetworkVar);
    }


    public CNetworkVar<TYPE> CreateReliableNetworkVar<TYPE>(CNetworkVar<TYPE>.SyncedHandler _nSyncCallback)
    {
        CNetworkVar<TYPE> cNewNetworkVar = new CNetworkVar<TYPE>(_nSyncCallback);

        RegisterNetworkVar(cNewNetworkVar, INetworkVar.EReliabilityType.Reliable_Ordered);

        return (cNewNetworkVar);
    }


    public CNetworkVar<TYPE> CreateUnreliableNetworkVar<TYPE>(CNetworkVar<TYPE>.SyncedHandler _nSyncCallback, float _fInterval, TYPE _DefaultValue)
    {
        CNetworkVar<TYPE> cNewNetworkVar = new CNetworkVar<TYPE>(_nSyncCallback, _DefaultValue);
        cNewNetworkVar.SetSendInterval(_fInterval);

        RegisterNetworkVar(cNewNetworkVar, INetworkVar.EReliabilityType.Unreliable_Sequenced);

        return (cNewNetworkVar);
    }


    public CNetworkVar<TYPE> CreateUnreliableNetworkVar<TYPE>(CNetworkVar<TYPE>.SyncedHandler _nSyncCallback, float _fInterval)
    {
        CNetworkVar<TYPE> cNewNetworkVar = new CNetworkVar<TYPE>(_nSyncCallback);
        cNewNetworkVar.SetSendInterval(_fInterval);

        RegisterNetworkVar(cNewNetworkVar, INetworkVar.EReliabilityType.Unreliable_Sequenced);

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


    void RegisterNetworkVar(INetworkVar _cNetworkVar, INetworkVar.EReliabilityType _eReliabilityType)
    {
        // Generate network var id, 1-byte.MAX
        byte bId = (byte)(m_mNetworkVars.Count + 1);

        // Store network var instance towards ids
        m_mNetworkVars.Add(bId, _cNetworkVar);

        // Set the network view owner of the network var
        _cNetworkVar.Initialise(m_cOwnerNetworkView, bId, _eReliabilityType);
    }

    CNetworkView m_cOwnerNetworkView;
    Dictionary<byte, INetworkVar> m_mNetworkVars;
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


	public delegate void NotiftyPreDestory(GameObject _cSender);
	public event NotiftyPreDestory EventPreDestory;


	public Dictionary<byte, CNetworkView> ChildrenNetworkViews
	{
		get { return (m_mChildrenNetworkViews); }
	}


    public bool Ready
    {
        get { return (m_bReady); }
    }


// Member Functions
    
    // public:


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        _cRegistrar.RegisterRpc(this, "RemoteSetPosition");
        _cRegistrar.RegisterRpc(this, "RemoteSetEuler");
        _cRegistrar.RegisterRpc(this, "RemoteSetLocalPosition");
        _cRegistrar.RegisterRpc(this, "RemoteSetLocalEuler");
        _cRegistrar.RegisterRpc(this, "RemoteSetScale");
        _cRegistrar.RegisterRpc(this, "RemoteSetRigidBodyMass");
        _cRegistrar.RegisterRpc(this, "RemoteSetParent");
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
        // Empty
	}


	public void NotifyPreDestory() // Call in CNetworkFactory
	{
		if (EventPreDestory != null)
		{
			EventPreDestory(gameObject);
		}

        foreach (INetworkVar cVar in m_mNetworkVars.Values)
        {
            cVar.SetSendInterval(0.0f);
        }

        foreach (CNetworkView cChild in m_mChildrenNetworkViews.Values)
        {
            cChild.NotifyPreDestory();
        }

        if (!ViewId.IsChildViewId)
        {
            s_cNetworkViews.Remove(ViewId.Id);
        }
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
        if (_ulPlayerId == 0 ||
            _ulPlayerId == ulong.MaxValue)
        {
            // Skip server
            if (_ulPlayerId != ulong.MaxValue)
            {
			    // Process on server straight away
			    CNetworkView.ProcessInboundStream(0, cRpcStream);
			    cRpcStream.SetReadOffset(0);
            }

			// Append rpc stream to connected non-host players
            foreach (KeyValuePair<ulong, CNetworkPlayer> tEntry in CNetwork.Server.GetNetworkPlayers())
            {
                // Server has already processed the RPC call
                if (!tEntry.Value.IsHost)
                {
                    // Append packet data
                    tEntry.Value.BufferedStream.Write(cRpcStream);	

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
                cNetworkPlayer.BufferedStream.Write(cRpcStream);
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

        // Compile list of network players to send to
        List<CNetworkPlayer> aNetworkPlayers = null;

        if (_ulPlayerId == 0)
        {
            aNetworkPlayers = new List<CNetworkPlayer>(CNetwork.Server.GetNetworkPlayers().Values);
        }
        else
        {
            aNetworkPlayers = new List<CNetworkPlayer>();
            aNetworkPlayers.Add(CNetwork.Server.FindNetworkPlayer(_ulPlayerId));
        }

        // Process on server straight away
        CNetworkView.ProcessInboundStream(0, cVarStream);
        cVarStream.SetReadOffset(0);

		// Append to target network players
        foreach (CNetworkPlayer cNetworkPlayer in aNetworkPlayers)
        {
            if (!cNetworkPlayer.IsHost)
            {
                switch (m_mNetworkVars[_bNetworkVarId].GetReliabilityType())
                {
                    case INetworkVar.EReliabilityType.Reliable_Ordered:
                        cNetworkPlayer.BufferedStream.Write(cVarStream);
                        break;

                    case INetworkVar.EReliabilityType.Unreliable_Sequenced:
                        cNetworkPlayer.UnbufferedStream.Write(cVarStream);
                        break;

                    default:
                        Debug.LogError("Unknown reliable type");
                        break;
                }
            }
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
		SetEuler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
	}


    public void SyncTransformLocalPosition()
    {
        SetLocalPosition(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }


    public void SyncTransformLocalEuler()
    {
        SetLocalEuler(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
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


    public void SyncParent()
    {
        // Ensure servers only sync RigidBodyMass
        Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot sync network object's parent!!!");

        SetParent(transform.parent.GetComponent<CNetworkView>().ViewId);
    }


	public void SetParent(TNetworkViewId _cParentViewId)
	{
        InvokeRpcAll("RemoteSetParent", _cParentViewId);
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


    public void SetLocalPosition(Vector3 _vPosition)
    {
        SetLocalPosition(_vPosition.x, _vPosition.y, _vPosition.z);
    }


    public void SetLocalPosition(float _fX, float _fY, float _fZ)
    {
        // Ensure servers only sync transforms
        Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's transform local position!!!");

        InvokeRpcAll("RemoteSetLocalPosition", _fX, _fY, _fZ);
    }


	public void SetRotation(Quaternion _Rotation)
	{
		SetEuler(_Rotation.eulerAngles);
	}


	public void SetEuler(Vector3 _vEulerAngles)
	{
		SetEuler(_vEulerAngles.x, _vEulerAngles.y, _vEulerAngles.z);
	}


	public void SetEuler(float _fX, float _fY, float _fZ)
	{
		// Ensure servers only set transforms
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's transform euler!!!");

        InvokeRpcAll("RemoteSetEuler", _fX, _fY, _fZ);
	}


    public void SetLocalEuler(Vector3 _vEulerAngles)
    {
        SetLocalEuler(_vEulerAngles.x, _vEulerAngles.y, _vEulerAngles.z);
    }


    public void SetLocalEuler(float _fX, float _fY, float _fZ)
    {
        // Ensure servers only set transforms
        Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot set network object's transform local euler rotation!!!");

        InvokeRpcAll("RemoteSetLocalEuler", _fX, _fY, _fZ);
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


	public TNetworkViewId ViewId
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


    static ushort m_uiNextDynamicId = k_usMaxStaticViewId + 1;


    public static TNetworkViewId GenerateDynamicViewId()
    {
		// Ensure servers only generate dynamic view ids
		Logger.WriteErrorOn(!CNetwork.IsServer, "Clients cannot generate network view ids!!!");

        //TNetworkViewId cViewId = new TNetworkViewId(m_uiNextDynamicId++, 0);
        //s_cNetworkViews.Add((ushort)(m_uiNextDynamicId - 1), null);
        
        TNetworkViewId cViewId = null;

        for (ushort i = k_usMaxStaticViewId; i < k_usMaxDynamicViewId; ++i)
        {
            // Check the dynamic view id is free
            if (!s_cNetworkViews.ContainsKey(i))
            {
                cViewId = new TNetworkViewId(i, 0);

				// Add id into list without owner so someone else does not claim the id
                s_cNetworkViews.Add(i, null);

                break;
            }
        }

		// Ensure id was generated
		Logger.WriteErrorOn(cViewId.Id == 0, "Oh shit, the network view id generator ran out of ids. The game is now broken. GG");

        return (cViewId);
    }


    public static CNetworkView FindUsingViewId(TNetworkViewId _cViewId)
    {
		CNetworkView cNetworkView = null;

		if (_cViewId != null)
		{
			if (!s_cNetworkViews.ContainsKey(_cViewId.Id))
			{
				//Logger.WriteError("Cannot find network view with id ({0})", _cViewId.Id);
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

                //Logger.WriteErrorOn(cNetworkView == null, "Could not find child network view. ViewId({0}) IdOwnerName() SubViewId({1})", _cViewId.Id, s_mViewIdOwnerNames[_cViewId.Id], _cViewId.ChildId);

				cNetworkView = cNetworkView.FindChildNetworkView(_cViewId.ChildId);

				
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
            TNetworkViewId cNetworkViewId = _cStream.Read<TNetworkViewId>();

            // Extract procedure type
            EProdecure eProcedure = (EProdecure)_cStream.Read<byte>();

            // Retrieve network view instance
            CNetworkView cNetworkView = CNetworkView.FindUsingViewId(cNetworkViewId);

            if (cNetworkView == null)
            {
                Debug.LogWarning(string.Format("Network view id invalid. NetworkViewId({0})", cNetworkViewId.Id));
                continue;
            }
			
            // Process network var sync procedure
            if (eProcedure == EProdecure.SyncNetworkVar)
            {
                // Extract network var identifier
                byte bNetworkVarIdentifier = _cStream.Read<byte>();

                if (!cNetworkView.m_mNetworkVars.ContainsKey(bNetworkVarIdentifier))
                {
                    Debug.LogError(string.Format("Network var not found. GameObject({0}) ViewId({1}) VarId({2})", cNetworkView.gameObject.name, cNetworkView.ViewId, bNetworkVarIdentifier));
                }

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
                byte bMethodIdentifier = _cStream.Read<byte>();

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


    void Awake()
    {
        // Run class initialisers
        InitialiseNetworkMonoBehaviours();

        // Since I have a parent on creation, I am a child network view 
        // and i need to register with the main network view which was
        // created through the network factory
        if (transform.parent != null)
        {
            Transform cParent = transform.parent;

            for (int i = 0; cParent.parent != null && i < 25; ++i)
            {
                cParent = cParent.parent;
            }

            // Register for sub network view id
            cParent.GetComponent<CNetworkView>().RegisterChildNetworkView(this);

            if (ViewId.ChildId == 0)
                Debug.LogError("I do not have a sub view id!");
        }
    }


    void Start()
    {
        if (m_cNetworkViewId == null)
        {
            // Generate static view id if server did not
            // provide one when this object was created
            if (transform.parent == null)
            {
                this.ViewId = GenerateStaticViewId();
            }
            else
            {
                Transform cParent = transform.parent;

                for (int i = 0; cParent.parent != null && i < 25; ++i)
                {
                    if ( cParent.GetComponent<CNetworkView>() != null &&
                        !cParent.GetComponent<CNetworkView>().ViewId.IsChildViewId)
                    {
                        break;
                    }
   
                    cParent = cParent.parent;
                }

                if (cParent.GetComponent<CNetworkView>() == null || 
                    cParent.GetComponent<CNetworkView>().ViewId.IsChildViewId)
                {
                    Debug.LogError("could not find parent!!!");
                }
                else
                {
                    cParent.GetComponent<CNetworkView>().RegisterChildNetworkView(this);
                }
            }
        }

        if (!ViewId.IsChildViewId &&
            !s_mViewIdOwnerNames.ContainsKey(ViewId.Id))
        {
            s_mViewIdOwnerNames.Add(ViewId.Id, gameObject.name);
        }
    }


    void OnDestroy()
    {

    }


    void Update()
    {
        // Empty
    }


    void InitialiseNetworkMonoBehaviours()
    {
        // Extract components from game object
        CNetworkMonoBehaviour[] aComponents = gameObject.GetComponents<CNetworkMonoBehaviour>();
        CNetworkViewRegistrar cRegistrar = new CNetworkViewRegistrar(m_mNetworkVars, this, m_mNetworkRpcs);

        foreach (CNetworkMonoBehaviour cNetworkMonoBehaviour in aComponents)
        {
            cNetworkMonoBehaviour.RegisterNetworkComponents(cRegistrar);
        }

        m_bReady = true;
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
	void RemoteSetEuler(float _fRotationX, float _fRotationY, float _fRotationZ)
	{
		transform.rotation = Quaternion.Euler(_fRotationX, _fRotationY, _fRotationZ);
	}


    [ANetworkRpc]
    void RemoteSetLocalPosition(float _fPositionX, float _fPositionY, float _fPositionZ)
    {
        transform.localPosition = new Vector3(_fPositionX, _fPositionY, _fPositionZ);
    }


    [ANetworkRpc]
    void RemoteSetLocalEuler(float _fRotationX, float _fRotationY, float _fRotationZ)
    {
        transform.localRotation = Quaternion.Euler(_fRotationX, _fRotationY, _fRotationZ);
    }


	[ANetworkRpc]
	void RemoteSetScale(float _fScaleX, float _fScaleY, float _fScaleZ)
	{
		transform.localScale = new Vector3(_fScaleX, _fScaleY, _fScaleZ);
	}


    [ANetworkRpc]
    void RemoteSetParent(TNetworkViewId _cParentViewId)
    {
        if (_cParentViewId == null)
        {
            transform.parent = null;
        }
        else
        {
            transform.parent = _cParentViewId.GameObject.transform;
        }
        
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

                if (ViewId == null)
                {
                    _cChildView.ViewId = new TNetworkViewId(0, i);
                }
                else
                {
                    _cChildView.ViewId = new TNetworkViewId(ViewId.Id, i);
                }

				//Debug.LogError(string.Format("Registered ({0}) sub newwork view with ViewId({1}) SubViewId({2})", _cSubView.gameObject.name, _cSubView.ViewId.Id, _cSubView.ViewId.ChildId));

				break;
			}
		}
	}


    static TNetworkViewId GenerateStaticViewId()
    {
        TNetworkViewId cViewId = null;

        for (ushort i = 5; i < k_usMaxStaticViewId; ++i)
        {
            // Check the static view id is free
            if (!s_cNetworkViews.ContainsKey(i))
            {
                cViewId = new TNetworkViewId(i, 0);

                // Add id into list without owner so someone else does not claim the id
                s_cNetworkViews.Add(i, null);

                break;
            }
        }

        // Ensure id was generated
        Logger.WriteErrorOn(cViewId == null, "Oh shit, the network view id generator ran out of ids. The game is now broken. GG");

        return (cViewId);
    }


// Member Variables
    
    // protected:


    // private:


    public TNetworkViewId m_cNetworkViewId = null;
    

    Dictionary<byte, INetworkVar> m_mNetworkVars = new Dictionary<byte, INetworkVar>();
    Dictionary<byte, TRpcMethod> m_mNetworkRpcs = new Dictionary<byte, TRpcMethod>();
	Dictionary<byte, CNetworkView> m_mChildrenNetworkViews = new Dictionary<byte, CNetworkView>();


    bool m_bReady = false;


	static Dictionary<ushort, CNetworkView> s_cNetworkViews = new Dictionary<ushort, CNetworkView>();
    static Dictionary<ushort, string> s_mViewIdOwnerNames = new Dictionary<ushort, string>();


};