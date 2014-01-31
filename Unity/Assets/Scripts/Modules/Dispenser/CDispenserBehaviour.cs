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
        // Debug: Sign up for 'use' event
        gameObject.GetComponent<CActorInteractable>().EventUse += new CActorInteractable.NotifyInteraction(OnUse);

        // Set invalid tool
        m_skTool = CGameRegistrator.ENetworkPrefab.INVALID;

        // Debug: Health set
        m_fHealth = 50.0f;
    }


    // Debug use only
    void OnUse(RaycastHit _RayCast, CNetworkViewId _cNetworkViewId)
    {
        // Default
        m_skTool = CGameRegistrator.ENetworkPrefab.ToolRachet;
    }


    public static void SerializeData(CNetworkStream _cStream)
    {
        // If there's a tool to be spawned
        if (m_skTool != CGameRegistrator.ENetworkPrefab.INVALID)
        {
            // Write the first byte to the stream as a network action
            _cStream.Write((byte)ENetworkAction.ActionSpawnTool);

            // Write the tool to spawn to the stream
            _cStream.Write((byte)m_skTool);

            // Reset tool
            m_skTool = CGameRegistrator.ENetworkPrefab.INVALID;
        }
    }


    public static void UnserializeData(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        // While there is unread data in the stream
        while (_cStream.HasUnreadData)
        {
            // Save the first byte as the network action
            ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();

            // Switch on the network action
            switch (eNetworkAction)
            {
                // Spawn tool action
                case ENetworkAction.ActionSpawnTool:
                {
                    // Spawn a new tool
                    SpawnTool((CGameRegistrator.ENetworkPrefab)_cStream.ReadByte());
                    
                    break;
                }
            }
        }
    }


    [AServerOnly]
    public static void InvokeSpawnTool(CGameRegistrator.ENetworkPrefab _Tool)
    {
        // Set the tool to be spawned
        m_skTool = _Tool;
    }


    [AServerOnly]
    private static void SpawnTool(CGameRegistrator.ENetworkPrefab _Tool)
    {
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


    void Update()
    {
        // Interpolate health
        transform.FindChild("Cube").renderer.material.color = Color.Lerp(Color.red, Color.green, m_fHealth / 100.0f);
    }


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar) { }


    static float m_fHealth = new float();
    static CDispenserBehaviour m_Instance;
    static CGameRegistrator.ENetworkPrefab m_skTool = new CGameRegistrator.ENetworkPrefab();


};