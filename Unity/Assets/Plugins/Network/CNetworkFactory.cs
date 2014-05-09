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


    const string k_sPrefabDir = "Prefabs/";


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


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "RemoteCreateLocalObject");
        _cRegistrar.RegisterRpc(this, "RemoteDestroyLocalObject");
    }


	public void RegisterPrefab(object _cPrefabId, string _sPrefabFilename)
	{
		m_mPrefabs.Add((ushort)_cPrefabId, _sPrefabFilename);
	}
	
	
	public string GetRegisteredPrefabFile(object _cPrefabId)
	{
		// Ensure the prefab file exists
		//Logger.WriteErrorOn(!m_mPrefabs.ContainsKey((ushort)_cPrefabId), "The requested prefab has not been registered yet!!! PrefabId({0})({1})", _cPrefabId, (ushort)_cPrefabId);

        if (!m_mPrefabs.ContainsKey((ushort)_cPrefabId))
        {
            return (null);
        }
			
		return ("Prefabs/" + m_mPrefabs[(ushort)_cPrefabId]);
	}


    public GameObject LoadPrefab(object _cPrefabId)
    {
        string sToolPrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(_cPrefabId);

        if (sToolPrefabFile == null)
            return (null);

        return (Resources.Load(sToolPrefabFile, typeof(GameObject)) as GameObject);
    }


    [AServerOnly]
	public GameObject CreateGameObject(object _cPrefabId)
	{
		// Ensure only servers call this function
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only the server can create objects");

		// Generate dynamic network view id for object
		TNetworkViewId cObjectViewId = CNetworkView.GenerateDynamicViewId();

		// Invoke create local object call on all connected players
		InvokeRpcAll("RemoteCreateLocalObject", (ushort)_cPrefabId, cObjectViewId);

        return (CNetworkView.FindUsingViewId(cObjectViewId).gameObject);
    }


    [AServerOnly]
	public void DestoryGameObject(GameObject _cObject)
	{
        if (_cObject != null)
        {
            if (_cObject.GetComponent<CNetworkView>() == null)
            {
                Debug.Log(string.Format("A game object with no network view cnanot be destroyed thorugh the network factory. GameObjectName({0})", _cObject.name));
            }

            DestoryGameObject(_cObject.GetComponent<CNetworkView>().ViewId);
        }
	}


    [AServerOnly]
    public void DestoryGameObject(TNetworkViewId _cObjectNetworkViewId)
    {
		// Ensure only servers call this function
		Logger.WriteErrorOn(!CNetwork.IsServer, "On the server can destroy objects");

        // Tell player to instantiate already created object
		InvokeRpcAll("RemoteDestroyLocalObject", _cObjectNetworkViewId);
	}


    [AServerOnly]
	public void SyncPlayer(CNetworkPlayer _cNetworkPlayer)
    {
		// Ensure only servers call this function
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only the server can sync players");

        // Sync new player with all the current game objects are their network var values
        if (!_cNetworkPlayer.IsHost)
        {
            Logger.Write("A Player joined. Sending them the objects and states");

            foreach (KeyValuePair<TNetworkViewId, TObjectInfo> tEntry in m_mCreatedObjects)
            {
                // Tell player to instantiate already created object
                InvokeRpc(_cNetworkPlayer.PlayerId, "RemoteCreateLocalObject", tEntry.Value.usPrefab, tEntry.Key);
            }
			
			// Sync parents for each transform
			foreach (KeyValuePair<TNetworkViewId, TObjectInfo> tEntry in m_mCreatedObjects)
			{
				if (tEntry.Value.cGameObject == null)
				{
					Debug.LogError(string.Format("Gameobject({0}) is null. PrefabId({1})", tEntry.Value.cGameObject, tEntry.Value.usPrefab));
				}


				if (tEntry.Value.cGameObject.GetComponent<CNetworkView>() == null)
				{
					Debug.LogError(string.Format("Gameobject({0}) does not have a networkview. PrefabId({1})", tEntry.Value.cGameObject, tEntry.Value.usPrefab));
				}

				// Extract the network view from this object
				CNetworkView cSelfView = tEntry.Value.cGameObject.GetComponent<CNetworkView>();

                // Check has parent
                if (tEntry.Value.cGameObject.transform.parent != null)
                {
                    cSelfView.SyncParent();

                    // Only sync if position is not default
                    if (tEntry.Value.cGameObject.transform.position != Vector3.zero)
                    {
                        cSelfView.SyncTransformLocalPosition();
                    }

                    // Sync if rotation is not default
                    if (tEntry.Value.cGameObject.transform.eulerAngles != Vector3.zero)
                    {
                        cSelfView.SyncTransformLocalEuler();
                    }
                }
                else
                {
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
                }

                // Sync if scale is not default
                if (tEntry.Value.cGameObject.transform.localScale != Vector3.one)
                {
                    // Sync object's scale
                    cSelfView.SyncTransformScale();
                }
			}

			// Sync network vars last
			foreach (KeyValuePair<TNetworkViewId, TObjectInfo> tEntry in m_mCreatedObjects)
			{
				// Extract the network view from this object
				CNetworkView cNetworkView = tEntry.Value.cGameObject.GetComponent<CNetworkView>();

				// Tell object to sync all their network vars with the player
				cNetworkView.SyncNetworkVarsWithPlayer(_cNetworkPlayer.PlayerId);

				// Have children network views to sync their values
				foreach (KeyValuePair<byte, CNetworkView> tEntity in cNetworkView.ChildrenNetworkViews)
				{
					tEntity.Value.SyncNetworkVarsWithPlayer(_cNetworkPlayer.PlayerId);
				}
			}
        }
    }


	public GameObject FindGameObject(TNetworkViewId _cNetworkViewId)
	{
		CNetworkView cObjectNetworkView = CNetworkView.FindUsingViewId(_cNetworkViewId);
		GameObject cGameObject = null;

		if (cObjectNetworkView != null)
		{
			cGameObject = cObjectNetworkView.gameObject;
		}
		else
		{
			//Logger.WriteErrorOn(cGameObject == null, "Could not find network object with ViewId({0}) SubViewId({1})", _cNetworkViewId.Id, _cNetworkViewId.ChildId);
		}

		return (cGameObject);
	}


    void Start()
    {
        CNetwork.Server.EventShutdown += new CNetworkServer.NotifyShutdown(OnEventServerShutdown);
        CNetwork.Connection.EventDisconnect += new CNetworkConnection.OnDisconnect(OnEventConnectionDisconnect);
    }


    void OnEventConnectionDisconnect()
    {
		// Ensure we are not the server
        if (!CNetwork.IsServer)
        {
            DestoryAllCreatedObjects();
        }
    }


    void OnEventServerShutdown()
    {
		DestoryAllCreatedObjects();
    }


	void DestoryAllCreatedObjects()
	{
		foreach (KeyValuePair<TNetworkViewId, TObjectInfo> tEntry in m_mCreatedObjects)
		{
			GameObject.Destroy(tEntry.Value.cGameObject);
		}

		m_mCreatedObjects.Clear();
	}


    [ANetworkRpc]
    void RemoteCreateLocalObject(ushort _usPrefabId, TNetworkViewId _cNetworkViewId)
    {
		//((APrefabInfo)typeof(EPrefab).GetField(_ePrefab.ToString()).GetCustomAttributes(typeof(APrefabInfo), true)[0]).GetResourceName()
        // Extract prefab resource name
        string sPrefabName = m_mPrefabs[_usPrefabId];

        // Create the game object
        GameObject cNewgameObject = Resources.Load(k_sPrefabDir + sPrefabName, typeof(GameObject)) as GameObject;

        if (cNewgameObject == null)
        {
            Debug.LogError(string.Format("Prefab could not be found. PrefabId({0}) PrefabName({1})", _usPrefabId, sPrefabName));
        }

		//cNewgameObject = HierarchicalPrefabUtility.Instantiate(cNewgameObject);
        cNewgameObject = GameObject.Instantiate(cNewgameObject) as GameObject;

		// Extract network view component from created object
        CNetworkView cNetworkView = cNewgameObject.GetComponent<CNetworkView>();

		// Ensure the created object has a network view component
		Logger.WriteErrorOn(cNetworkView == null, "The created prefab ({0}), name ({1}) does not have a network view!!!", _cNetworkViewId, cNewgameObject.name);

		cNetworkView.ViewId = _cNetworkViewId;
		m_mCreatedObjects.Add(_cNetworkViewId, new TObjectInfo(_usPrefabId, cNewgameObject));

		// Notice
        Logger.Write("Created new game object with prefab ({0}), name ({1}) and network view id ({2})", _usPrefabId, cNewgameObject.name, _cNetworkViewId);
    }


    [ANetworkRpc]
    void RemoteDestroyLocalObject(TNetworkViewId _cObjectNetworkViewId)
    {
        TObjectInfo tObject = m_mCreatedObjects[_cObjectNetworkViewId];

		tObject.cGameObject.GetComponent<CNetworkView>().NotifyPreDestory();
        GameObject.Destroy(tObject.cGameObject);


        m_mCreatedObjects.Remove(_cObjectNetworkViewId);
    }


// Member Variables

    // protected:


    // private:


	Dictionary<ushort, string> m_mPrefabs = new Dictionary<ushort, string>();
	Dictionary<TNetworkViewId, TObjectInfo> m_mCreatedObjects = new Dictionary<TNetworkViewId, TObjectInfo>();


};
