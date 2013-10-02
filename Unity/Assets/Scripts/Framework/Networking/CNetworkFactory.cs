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


public class CNetworkFactory : MonoBehaviour
{

// Member Types


    public enum EPrefab : ushort
    {
        [APrefabInfo("Player Actor")]
        Player1,


        MAX = ushort.MaxValue
    }


    public struct TCreatedObject
    {
        public TCreatedObject(EPrefab _ePrefab, ushort _usNetworkViewId)
        {
            ePrefab = _ePrefab;
            usNetworkViewId = _usNetworkViewId;
        }

        public EPrefab ePrefab;
        public ushort usNetworkViewId;
    }


// Member Functions

    // public:


    public void Start()
    {
        CGame.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(OnNetworkPlayerJoin);
    }


    public void OnDestory()
    {
        // Empty
    }


    public void Update()
    {
        // Empty
    }


    /* Used by the server to create game objects that will also be created on all the clients */
    public ushort CreateGameObject(EPrefab _ePrefab)
    {
        ushort usObjectViewId = 0;


        if (!CGame.IsServer())
        {
            Debug.LogError(string.Format("Only the server is allowed to create objects fool!!! Prefab({0})", _ePrefab));
        }
        else
        {
            usObjectViewId = CNetworkView.GenerateDynamicViewId();
            m_aCreatedObjects.Add(new TCreatedObject(_ePrefab, usObjectViewId));


            GetComponent<CNetworkView>().InvokeRpcAll(this, "InstantiateGameObject", _ePrefab, usObjectViewId);
        }


        return (usObjectViewId);
    }


    /* Used by the server to create game objects that will also be destroyed on all the clients */
    void DestoryGameObject(GameObject _cGameObject)
    {

    }


    // protected:


    void OnNetworkPlayerJoin(CNetworkPlayer _cNetworkPlayer)
    {
        // Sync new player with all the current game objects are their network var values
        if (!_cNetworkPlayer.IsHost())
        {
            Debug.LogError("A Player joined. Sending them the objects and states");


            foreach (TCreatedObject tObjectInfo in m_aCreatedObjects)
            {
                // Tell player to instantiate already created object
                GetComponent<CNetworkView>().InvokeRpc(_cNetworkPlayer.PlayerId, this, "InstantiateGameObject", tObjectInfo.ePrefab, tObjectInfo.usNetworkViewId);

                // Extract the network view from this object
                CNetworkView cNetworkView = CNetworkView.FindUsingViewId(tObjectInfo.usNetworkViewId);

                // Tell object to sync all their network vars with the player
                cNetworkView.SyncPlayerNetworkVarValues(_cNetworkPlayer.PlayerId);
            }
        }


        // Create the new game object with the player
        _cNetworkPlayer.ActorNetworkViewId = CreateGameObject(EPrefab.Player1);


        GameObject cGameObject = CNetworkView.FindUsingViewId(_cNetworkPlayer.ActorNetworkViewId).gameObject;
        cGameObject.GetComponent<CActorMotor>().PositionZ = 2 * _cNetworkPlayer.PlayerId;
    }


    // private:


    [ANetworkRpc]
    void InstantiateGameObject(EPrefab _ePrefab, ushort _usNetworkViewId)
    {
        // Extract prefab resource name
        string sPrefabName = ((APrefabInfo)typeof(EPrefab).GetField(_ePrefab.ToString()).GetCustomAttributes(typeof(APrefabInfo), true)[0]).GetResourceName();


        // Create the game object
        GameObject cNewgameObject = GameObject.Instantiate(Resources.Load("Prefabs/" + sPrefabName, typeof(GameObject))) as GameObject;


        CNetworkView cNetworkView = cNewgameObject.GetComponent<CNetworkView>();


        if (cNetworkView == null)
        {
            Debug.LogError(string.Format("The created prefab ({0}) does not have a network view!!!", _usNetworkViewId));
        }
        else
        {
            cNetworkView.ViewId = _usNetworkViewId;


            Debug.LogError(string.Format("Created new game object with prefab ({0}) and network view id ({1})", _ePrefab, _usNetworkViewId));
        }
    }


    [ANetworkRpc]
    void DestoryGameObject(ushort _usNetworkViewId)
    {

    }


// Member Variables

    // protected:


    // private:


    List<TCreatedObject> m_aCreatedObjects = new List<TCreatedObject>();


};
