//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerIKController
//  Description :   Controls player IK, allowing a rigged character to place body parts at specified locations
//
//  Author      :  
//  Mail        :  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;

/* Implementation */

public class CPlayerRagdoll : CNetworkMonoBehaviour 
{
    //Member Types
    public enum ENetworkAction : byte
    {
        EventInvalid,

        EventDeath,
        EventRevive,

        EventMax,
    }
    
    //Member Delegates & Events   
    
    //Member Properties  
    
    //Member variables

    public Transform m_RootSkeleton = null;
    public Transform m_RootSkeletonRagdoll = null;

	public GameObject m_Ragdoll = null;
    public GameObject m_PlayerModel = null;
    public GameObject m_RagdollModel = null;

    public GameObject m_PlayerHead = null;
    public GameObject m_RagdollHead = null;

    CNetworkVar<byte>       m_bRagdollState;

    static CNetworkStream   s_cSerializeStream = new CNetworkStream();  

    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
       m_bRagdollState = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync);
    }
    
    void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    { 
        if (_cSyncedNetworkVar == m_bRagdollState)
        {
            switch((ENetworkAction) m_bRagdollState.Get())
            {
                case ENetworkAction.EventDeath:
                {
                    SetRagdollActive();                    
                    break;                
                }
                
                case ENetworkAction.EventRevive:
                {
                    DeactivateRagdoll();                
                    break;                
                }
            }
        }
    } 

    [AClientOnly]
    public static void Serialize(CNetworkStream _cStream)
    {
        // Write in internal stream
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();        
    }
    
    [AServerOnly]
    public static void Unserialize(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        //Extract network action
        ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();
        
        switch (eNetworkAction)
        {
            case ENetworkAction.EventDeath:
            {
                ulong uPlayerID = _cStream.ReadULong();

                GameObject cPlayerActor = CGamePlayers.GetPlayerActor(uPlayerID);
                CPlayerRagdoll ragdoll = cPlayerActor.GetComponent<CPlayerRagdoll>();

                ragdoll.m_bRagdollState.Set((byte)eNetworkAction);

                break;
            }
            case ENetworkAction.EventRevive:
            {
                ulong uPlayerID = _cStream.ReadULong();
                
                GameObject cPlayerActor = CGamePlayers.GetPlayerActor(uPlayerID);
                CPlayerRagdoll ragdoll = cPlayerActor.GetComponent<CPlayerRagdoll>();

                ragdoll.m_bRagdollState.Set((byte)eNetworkAction);

                break;
            }
        }
    }
    
    // Use this for initialization
    void Start ()
	{
		m_Ragdoll.SetActive (false);
        gameObject.GetComponent<CPlayerHealth>().EventHealthStateChanged += OnHealthStateChanged;
    }
	
	// Update is called once per frame
	void Update () 
	{
        transform.position = m_Ragdoll.transform.position;
	}

    [AServerOnly]
    void OnHealthStateChanged(GameObject _SourcePlayer, CPlayerHealth.HealthState _eHealthCurrentState, CPlayerHealth.HealthState _eHealthPreviousState)
	{  
        switch (_eHealthCurrentState)
        {
            case CPlayerHealth.HealthState.DOWNED:
            {
                s_cSerializeStream.Write((byte)ENetworkAction.EventDeath);
                //Send in player ID
                s_cSerializeStream.Write((ulong)CGamePlayers.GetPlayerActorsPlayerId(_SourcePlayer));

                break;
            }           
            case CPlayerHealth.HealthState.ALIVE:
            {           
                s_cSerializeStream.Write((byte)ENetworkAction.EventRevive);
                //Send in player ID
                s_cSerializeStream.Write((ulong)CGamePlayers.GetPlayerActorsPlayerId(_SourcePlayer));
                break;
            }
        }       
    }

    [AClientOnly]
    void SetRagdollActive()
    {
        //Disable all collisions
        Vector3 parentVelocity = rigidbody.velocity;    
    
        //Disable all rendering
        m_PlayerModel.renderer.enabled = false;   

        //Disable collisions
        rigidbody.collider.enabled = false;

        //Disable animations
        gameObject.GetComponent<Animator>().enabled = false;
    
        //Enable ragdoll and set position

        m_Ragdoll.SetActive(true);

        //Position Ragdoll
        SynchBones();       

        //Transfer camera to ragdoll head, if the ragdolling player is the local player
        if (gameObject == CGamePlayers.SelfActor)
        {
            gameObject.GetComponent<CPlayerHead>().m_cActorHead = m_RagdollHead;
            gameObject.GetComponent<CPlayerHead>().TransferPlayerPerspectiveToShipSpace();
            Debug.Log("Attempted to set player head to " + m_RagdollHead.name.ToString());
        }
    }

    [AClientOnly]
    void DeactivateRagdoll()
    {
        //Enable all rendering
        m_PlayerModel.renderer.enabled = true;
    
        //enabled animations
        gameObject.GetComponent<Animator>().enabled = true;        

        //Transfer camera to player head, if the ragdolling player is the local player
        if (gameObject == CGamePlayers.SelfActor)
        {
            gameObject.GetComponent<CPlayerHead>().m_cActorHead = m_PlayerHead;
            gameObject.GetComponent<CPlayerHead>().TransferPlayerPerspectiveToShipSpace();
            Debug.Log("Attempted to set player head to " + m_RagdollHead.name.ToString());
        }

        //Disable ragdoll and set position
        m_Ragdoll.SetActive(false);
    }

    //This function synchronises all ragdoll bone positions to player bone positions
    void SynchBones()
    {
        m_RootSkeletonRagdoll.position = m_RootSkeleton.position; 

        Transform[] ragdollBones = m_RootSkeletonRagdoll.GetComponentsInChildren<Transform>();
        Transform[] playerBones = m_RootSkeleton.GetComponentsInChildren<Transform>();

        for (int i = 0; i < ragdollBones.Length; i++)
        {
            ragdollBones[i].position = playerBones[i].position;
            ragdollBones[i].rotation = playerBones[i].rotation;

            //Attempt to apply velocity
            if(ragdollBones[i].rigidbody != null)
            {
                ragdollBones[i].rigidbody.velocity = rigidbody.velocity;                   
            }           
        }       
    }
}
