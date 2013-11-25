//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkFactory.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


/* Implementation */


public class CNetworkFactory : CNetworkMonoBehaviour
{

// Member Types


    const string ksPrefabDir = "Prefabs/";


    public struct TObjectInfo
    {
		public TObjectInfo(ushort _usPrefab, GameObject _cGameObject)
        {
            usPrefab = _usPrefab;
			cGameObject = _cGameObject;
        }

        public ushort usPrefab;
		public GameObject cGameObject;
    }


// Member Functions

    // public:


    public override void InstanceNetworkVars()
    {
        // Empty
    }


    public void Start()
    {
        CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnServerShutdown);
        CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnConnectionDisconnect);
    }


	public void RegisterPrefab(object _cPrefabId, string _sPrefabFilename)
	{
		m_mPrefabs.Add((ushort)_cPrefabId, _sPrefabFilename);
	}
	
	
	public string GetRegisteredPrefabFile(object _cPrefabId)
	{
		// Ensure the prefab file exists
		Logger.WriteErrorOn(!m_mPrefabs.ContainsKey((ushort)_cPrefabId), "The requested prefab has not been registered yet!!!");
			
		return (m_mPrefabs[(ushort)_cPrefabId]);
	}


	public GameObject CreateObject(object _cPrefabId)
	{
		// Ensure only servers call this function
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only the server can create objects");

		// Generate dynamic network view id for object
		ushort usObjectViewId = CNetworkView.GenerateDynamicViewId();

		// Invoke create local object call on all connected players
		InvokeRpcAll("CreateLocalObject", (ushort)_cPrefabId, usObjectViewId);

        return (CNetworkView.FindUsingViewId(usObjectViewId).gameObject);
    }


    public void DestoryObject(ushort _usObjectNetworkViewId)
    {
		// Ensure only servers call this function
		Logger.WriteErrorOn(!CNetwork.IsServer, "On the server can destroy objects");

        // Tell player to instantiate already created object
        InvokeRpcAll("DestroyLocalObject", _usObjectNetworkViewId);
    }


    public void SyncPlayer(CNetworkPlayer _cNetworkPlayer)
    {
		// Ensure only servers call this function
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only the server can sync players");

        // Sync new player with all the current game objects are their network var values
        if (!_cNetworkPlayer.IsHost)
        {
            Logger.Write("A Player joined. Sending them the objects and states");

            foreach (KeyValuePair<ushort, TObjectInfo> tEntry in m_mCreatedObjects)
            {
                // Tell player to instantiate already created object
                InvokeRpc(_cNetworkPlayer.PlayerId, "CreateLocalObject", tEntry.Value.usPrefab, tEntry.Key);
            }
			
			// Sync parents for each transform
			foreach (KeyValuePair<ushort, TObjectInfo> tEntry in m_mCreatedObjects)
			{
				// Extract the network view from this object
				CNetworkView cSelfView = tEntry.Value.cGameObject.GetComponent<CNetworkView>();
				
                // Invoke set parent rpc
                cSelfView.SyncParent();

				// Only sync if position is not default
				if (tEntry.Value.cGameObject.transform.position != Vector3.zero)
				{
					cSelfView.SyncTransformPosition();
				}

				// Sync if rotation is not default
				if (tEntry.Value.cGameObject.transform.eulerAngles != Vector3.zero)
				{
					cSelfView.SyncTransformRotation();
				}
				
				// Sync if scale is not default
				if (tEntry.Value.cGameObject.transform.localScale != Vector3.zero)
				{
					// Sync object's scale
					cSelfView.SyncTransformScale();
				}
			}

			// Sync network vars last
			foreach (KeyValuePair<ushort, TObjectInfo> tEntry in m_mCreatedObjects)
			{
				// Extract the network view from this object
				CNetworkView cNetworkView = tEntry.Value.cGameObject.GetComponent<CNetworkView>();

				// Tell object to sync all their network vars with the player
				cNetworkView.SyncNetworkVarsWithPlayer(_cNetworkPlayer.PlayerId);
			}
        }
    }


	public GameObject FindObject(ushort _usNetworkViewId)
	{
		GameObject cGameObject = null;

		// Dynamic object
		if (_usNetworkViewId >= CNetworkView.k_usMaxStaticViewId)
		{
			if(m_mCreatedObjects.ContainsKey(_usNetworkViewId))
			{
				cGameObject = m_mCreatedObjects[_usNetworkViewId].cGameObject;
			}
		}
		
		// Static object
		else
		{
			cGameObject = CNetworkView.FindUsingViewId(_usNetworkViewId).gameObject;
		}

		Logger.WriteErrorOn(cGameObject == null, "Could not find network object with view id ({0})", _usNetworkViewId);

		return (cGameObject);
	}


    // protected:


    protected void OnConnectionDisconnect()
    {
		// Ensure we are not the server
        if (!CNetwork.IsServer)
        {
            DestoryAllCreatedObjects();
        }
    }


    protected void OnServerShutdown()
    {
		DestoryAllCreatedObjects();
    }


	protected void DestoryAllCreatedObjects()
	{
		foreach (KeyValuePair<ushort, TObjectInfo> tEntry in m_mCreatedObjects)
		{
			GameObject.Destroy(tEntry.Value.cGameObject);
		}

		m_mCreatedObjects.Clear();
	}


    // private:


    [ANetworkRpc]
    void CreateLocalObject(ushort _usPrefabId, ushort _usNetworkViewId)
    {
		//((APrefabInfo)typeof(EPrefab).GetField(_ePrefab.ToString()).GetCustomAttributes(typeof(APrefabInfo), true)[0]).GetResourceName()
        // Extract prefab resource name
        string sPrefabName = m_mPrefabs[_usPrefabId];

        // Create the game object
        GameObject cNewgameObject = GameObject.Instantiate(Resources.Load(ksPrefabDir + sPrefabName, typeof(GameObject))) as GameObject;

		// Extract network view component from created object
        CNetworkView cNetworkView = cNewgameObject.GetComponent<CNetworkView>();

		// Ensure the created object has a network view component
		Logger.WriteErrorOn(cNetworkView == null, "The created prefab ({0}), name ({1}) does not have a network view!!!", _usNetworkViewId, cNewgameObject.name);

        cNetworkView.ViewId = _usNetworkViewId;
		m_mCreatedObjects.Add(_usNetworkViewId, new TObjectInfo(_usPrefabId, cNewgameObject));

		// Notice
        Logger.Write("Created new game object with prefab ({0}), name ({1}) and network view id ({2})", _usPrefabId, cNewgameObject.name, _usNetworkViewId);
    }


    [ANetworkRpc]
    void DestroyLocalObject(ushort _usObjectNetworkViewId)
    {
        TObjectInfo tObject = m_mCreatedObjects[_usObjectNetworkViewId];


		tObject.cGameObject.GetComponent<CNetworkView>().OnPreDestory();
        GameObject.Destroy(tObject.cGameObject);


        m_mCreatedObjects.Remove(_usObjectNetworkViewId);
    }


// Member Variables

    // protected:


    // private:


	Dictionary<ushort, string> m_mPrefabs = new Dictionary<ushort, string>();
    Dictionary<ushort, TObjectInfo> m_mCreatedObjects = new Dictionary<ushort, TObjectInfo>();


};
