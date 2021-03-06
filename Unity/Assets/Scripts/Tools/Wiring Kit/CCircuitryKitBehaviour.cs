//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRachetBehaviour.cs
//  Description :   --------------------------
//
//  Author      :  
//  Mail        :  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


[RequireComponent(typeof(CToolInterface))]
public class CCircuitryKitBehaviour : CNetworkMonoBehaviour
{
    
    // Member Types
    enum ENetworkAction
    {
        Invalid,
        
        SetRepairState,
        
        Max,
    }
    
    enum ERepairState
    {
        Invalid,
        
        RepairInactive,
        RepairActive,
        
        Max,
    }
    
    
    // Member Delegates & Events
    
    
    // Member Properties
    
    
    // Member Functions
    
    
    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_bRepairState = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync);
    }
    
    public void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if(_cSyncedVar == m_bRepairState)
        {
            if( (m_bRepairState.Get() & (uint)ERepairState.RepairActive) > 0)
            {
                m_eRepairState = ERepairState.RepairActive;
            }
            
            if( (m_bRepairState.Get() & (uint)ERepairState.RepairInactive) > 0)
            {
                m_eRepairState = ERepairState.RepairInactive;
            }
        }
    }
    
    [ALocalOnly]
    public static void Serialize(CNetworkStream _cStream)
    {
        // Write in internal stream
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
        
    }
    
    [AServerOnly]
    public static void Unserialize(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            // Extract action
            ENetworkAction eAction = (ENetworkAction)_cStream.Read<byte>();
            
            switch (eAction)
            {
                case ENetworkAction.SetRepairState:
                {
                    //Figure out which kits sent it's new state
                    CCircuitryKitBehaviour CircuitryKit = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CCircuitryKitBehaviour>();
                    
                    CircuitryKit.m_TargetComponent = _cStream.Read<TNetworkViewId>().GameObject.GetComponent<CComponentInterface>();
                    CircuitryKit.m_eRepairState = (ERepairState)_cStream.Read<byte>();
                    
                    break;
                }
            }
        }
    }
    
    void Start()
    {       
        m_TargetList = new List<Transform>();
        m_eRepairState = ERepairState.RepairInactive;

        GetComponent<CToolInterface>().EventPrimaryActiveChange += (bool _bDown) =>
        {
            if (_bDown &&
                m_eRepairState == ERepairState.RepairInactive)
            {
                GameObject cTargetActorInteractable = GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerInteractor>().TargetActorObject;

                if (cTargetActorInteractable != null)
                {
                    CComponentInterface cActorComponentInterface = cTargetActorInteractable.GetComponent<CComponentInterface>();

                    if (cActorComponentInterface != null &&
                        cActorComponentInterface.ComponentType == CComponentInterface.EType.Circuitry)
                    {
                        BeginRepair(cTargetActorInteractable);
                    }
                }
            }
            else if (m_eRepairState == ERepairState.RepairActive)
            {
                EndRepairs();
            }
        };
    }   
    
    void Update()
    {
        if(m_eRepairState == ERepairState.RepairActive) 
        {
            //Do repairs here
            if(CNetwork.IsServer)
            {
                m_TargetComponent.gameObject.GetComponent<CActorHealth>().health += (m_fRepairRate * Time.deltaTime);               
            }

            if (GetComponent<CToolInterface>().OwnerPlayerActor == CGamePlayers.SelfActor &&
                m_eRepairState == ERepairState.RepairActive)
            {
                //Update target for IK here
                UpdateTarget();
            }
        }
    }
    
    void UpdateTarget()
    {
        m_fTargetSwitchTimer += Time.deltaTime;
        
        if(m_fTargetSwitchTimer > m_fTargetSwitchFrequency)
        {
            if(m_iTargetIndex < m_iTotalTargets - 1)
            {
                m_iTargetIndex++;
            }
            else
            {
                m_iTargetIndex = 0;
            }       
            
            m_IKController.RightHandIKPos = m_TargetList[m_iTargetIndex].position;    
            m_IKController.RightHandIKRot = m_TargetList[m_iTargetIndex].rotation;          

            m_fTargetSwitchTimer = 0.0f;
            Debug.Log("switched target.");
        }
    }
    
    public void BeginRepair(GameObject _damagedComponent)
    {       
        m_iTotalTargets = 0;        
        
        m_TargetComponent = _damagedComponent.GetComponent<CComponentInterface>();

        List<Transform> repairPositions = m_TargetComponent.GetComponent<CCircuitryComponent>().ComponentRepairPosition;
        
        foreach(Transform child in repairPositions)
        {
            m_TargetList.Add(child);
            m_iTotalTargets++;
        }   
        
        m_eRepairState = ERepairState.RepairActive;
        
        m_fTargetSwitchTimer = 0.0f;
        
        m_IKController = gameObject.GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerIKController>();

        m_IKController.RightHandIKPos = m_TargetList[m_iTargetIndex].position;    
        m_IKController.RightHandIKRot = m_TargetList[m_iTargetIndex].rotation; 

        TNetworkViewId senderID = gameObject.GetComponent<CNetworkView>().ViewId;
        TNetworkViewId targetID = _damagedComponent.GetComponent<CNetworkView>().ViewId;
        
        s_cSerializeStream.Write((byte)ENetworkAction.SetRepairState);
        s_cSerializeStream.Write(senderID);
        s_cSerializeStream.Write(targetID);     
        s_cSerializeStream.Write((byte)m_eRepairState);
        
        Debug.Log("Beginning repairs");
    }
    
    public void EndRepairs()
    {
        m_eRepairState = ERepairState.RepairInactive;
        m_TargetComponent = null;
        m_IKController.RightHandIKWeight = 0;
        m_TargetList.Clear();
    }
    
    
    // Member Fields
    
    Vector3                 m_ToolTarget;
    List<Transform>         m_TargetList;   
    int                     m_iTotalTargets;
    int                     m_iTargetIndex;         
    float                   m_fRepairRate = 30.0f;
    
    float                   m_fTargetSwitchTimer = 0.0f;
    float                   m_fTargetSwitchFrequency = 0.75f;
    
    CComponentInterface     m_TargetComponent;  
    ERepairState            m_eRepairState;
    
    CPlayerIKController     m_IKController;
    
    CNetworkVar<byte>       m_bRepairState;
    static CNetworkStream   s_cSerializeStream = new CNetworkStream();  
};
