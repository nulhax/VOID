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


	public GameObject CreateObject(object _cPrefabId)
	{
		// Ensure only servers call this function
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only the server can create objects", (ushort)_cPrefabId);

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
            Logger.WriteError("A Player joined. Sending them the objects and states");

            foreach (KeyValuePair<ushort, TObjectInfo> tEntry in m_mCreatedObjects)
            {
                // Tell player to instantiate already created object
                InvokeRpc(_cNetworkPlayer.PlayerId, "CreateLocalObject", tEntry.Value.usPrefab, tEntry.Key);

                // Extract the network view from this object
                CNetworkView cNetworkView = tEntry.Value.cGameObject.GetComponent<CNetworkView>();

				// Sync object's position
				cNetworkView.InvokeRpc(_cNetworkPlayer.PlayerId, "SetTransformPosition", tEntry.Value.cGameObject.transform.position.x,
																						 tEntry.Value.cGameObject.transform.position.x,
																						 tEntry.Value.cGameObject.transform.position.z);

				// Sync object's rotation
				cNetworkView.InvokeRpc(_cNetworkPlayer.PlayerId, "SetTransformRotation", tEntry.Value.cGameObject.transform.localRotation.eulerAngles.x,
																						 tEntry.Value.cGameObject.transform.localRotation.eulerAngles.x,
																						 tEntry.Value.cGameObject.transform.localRotation.eulerAngles.x);

				// Tell object to sync all their network vars with the player
				cNetworkView.SyncPlayerNetworkVarValues(_cNetworkPlayer.PlayerId);
            }

			// Sync parents for each transform
			foreach (KeyValuePair<ushort, TObjectInfo> tEntry in m_mCreatedObjects)
			{
				if (tEntry.Value.cGameObject.transform.parent != null)
				{
					// Extract parent network view
					CNetworkView cParentView = tEntry.Value.cGameObject.transform.parent.GetComponent<CNetworkView>();

					Logger.WriteError("Networked object's parent is not a networked object. Wtf are you doing??!?");

					// Extract the network view from this object
					CNetworkView cSelfView = tEntry.Value.cGameObject.GetComponent<CNetworkView>();

					// Invoke set parent rpc
					cSelfView.InvokeRpc(_cNetworkPlayer.PlayerId, "SetParent", cParentView.ViewId);
				}
			}
        }
    }


	public GameObject FindObject(ushort _usNetworkViewId)
	{
		GameObject go = null;
		
		if(m_mCreatedObjects.ContainsKey(_usNetworkViewId))
		{
			go = m_mCreatedObjects[_usNetworkViewId].cGameObject;
		}
		else
		{
			Debug.LogError("Network Factory Find Object NetworkViewId doesn't exsist yet! Something must be wrong here...");
		}
		
		return (go);
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
		Logger.WriteErrorOn(cNetworkView == null, "The created prefab ({0}) does not have a network view!!!", _usNetworkViewId);

        cNetworkView.ViewId = _usNetworkViewId;
		m_mCreatedObjects.Add(_usNetworkViewId, new TObjectInfo(_usPrefabId, cNewgameObject));

		// Notice
        Logger.Write("Created new game object with prefab ({0}) and network view id ({1})", _usPrefabId, _usNetworkViewId);



		// Testing 
		cNewgameObject.renderer.material = new Material(Shader.Find("Diffuse"));


		switch (_usNetworkViewId)
		{
			case 500: cNewgameObject.renderer.material.color = Color.red; break;
			case 501: cNewgameObject.renderer.material.color = Color.blue; break;
			case 502: cNewgameObject.renderer.material.color = Color.yellow; break;
			case 503: cNewgameObject.renderer.material.color = Color.cyan; break;
			case 504: cNewgameObject.renderer.material.color = Color.green; break;
			case 505: cNewgameObject.renderer.material.color = Color.magenta; break;
			case 506: cNewgameObject.renderer.material.color = Color.black; break;
		}
    }


    [ANetworkRpc]
    void DestroyLocalObject(ushort _usObjectNetworkViewId)
    {
        TObjectInfo tObject = m_mCreatedObjects[_usObjectNetworkViewId];


        GameObject.Destroy(tObject.cGameObject);


        m_mCreatedObjects.Remove(_usObjectNetworkViewId);
    }


// Member Variables

    // protected:


    // private:


	Dictionary<ushort, string> m_mPrefabs = new Dictionary<ushort, string>();
    Dictionary<ushort, TObjectInfo> m_mCreatedObjects = new Dictionary<ushort, TObjectInfo>();


};
