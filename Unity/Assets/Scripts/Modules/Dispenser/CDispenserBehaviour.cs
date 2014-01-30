//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDispenserBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CDispenserBehaviour : CNetworkMonoBehaviour
{
    // Member Types
    enum ENetworkAction
    {
        INVALID = -1,

        ActionSpawnTool,

        MAX
    }

    // Member Delegates & Events


    // Member Properties
    public static CDispenserBehaviour Instance
    {
        get { return (m_Instance); }
    }


    // Member Methods


    public void Awake()
    {
        // Save a static reference to this class
        m_Instance = this;
    }


    void Start()
    {
        gameObject.GetComponent<CActorInteractable>().EventUse += new CActorInteractable.NotifyInteraction(OnUse);
    }


    void OnUse(RaycastHit _RayCast, CNetworkViewId _cNetworkViewId)
    {
        m_bSpawnTool = true;
    }


    public static void SerializeData(CNetworkStream _cStream)
    {
        if (m_bSpawnTool)
        {
            // Write the first byte to the stream as a network action
            _cStream.Write((byte)ENetworkAction.ActionSpawnTool);

            // Write the tool to spawn to the stream
            _cStream.Write((byte)m_skDefaultTool);

            // Reset boolean
            m_bSpawnTool = false;
        }
    }


    public static void UnserializeData(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            // Save the first byte as the network action
            ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();

            switch (eNetworkAction)
            {
                case ENetworkAction.ActionSpawnTool:
                {
                    SpawnTool((CGameRegistrator.ENetworkPrefab)_cStream.ReadByte());
                    
                    break;
                }
            }
        }
    }


    [AServerOnly]
    static void SpawnTool(CGameRegistrator.ENetworkPrefab _Tool)
    {
        // MARTIN: Uncomment the below line
        //m_bSpawnTool = true;

        // Local variables
        Quaternion TempQuat = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);

        // Create a new object
        GameObject NewTool = CNetwork.Factory.CreateObject(_Tool);

        ///// TEMPORARY /////
        Vector3 V3 = Instance.transform.position;
        V3.y += 1.5f;
        V3.z -= 2.0f;

        // Set the tool's position relative to the dispenser
        NewTool.GetComponent<CNetworkView>().SetPosition(V3);
        ///// TEMPORARY /////

        // NewTool.GetComponent<CNetworkView>().SetPosition(SOMETHING_GOES_HERE);

        // Set (null) the tool's rotation
        NewTool.GetComponent<CNetworkView>().SetRotation(TempQuat.eulerAngles);

        // Set the tool's parent
        NewTool.GetComponent<CNetworkView>().SetParent(Instance.gameObject.GetComponent<CNetworkView>().ViewId);

        
    }


    void OnDestroy() { }


    void Update() { }


    public override void InstanceNetworkVars() { }


    static bool m_bSpawnTool = false;
    static CDispenserBehaviour m_Instance;
    static CGameRegistrator.ENetworkPrefab m_skDefaultTool = CGameRegistrator.ENetworkPrefab.ToolRachet;


};