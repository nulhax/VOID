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

	GameObject Ragdoll;

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
                    OnDeath(gameObject);
                    
                    break;                
                }
                
                case ENetworkAction.EventRevive:
                {
                    OnRevive(gameObject);
                
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
        GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);
        CPlayerRagdoll ragdoll = cPlayerActor.GetComponent<CPlayerRagdoll>();

        while (_cStream.HasUnreadData)
        {
            // Extract action
            ragdoll.m_bRagdollState.Set(_cStream.ReadByte());
        }
    }
    
    // Use this for initialization
    void Start ()
	{
		//Find ragdoll
		foreach (Transform child in gameObject.GetComponentsInChildren<Transform>()) 
		{
			if (child.name == "Player Ragdoll") 
			{
				Ragdoll = child.gameObject;
				Debug.Log("Found ragdoll");
			}
		}

		Ragdoll.SetActive (false);

		gameObject.GetComponent<CPlayerHealth> ().EventDeath += OnDeath;
		gameObject.GetComponent<CPlayerHealth> ().EventRevive += OnRevive;
    }
	
	// Update is called once per frame
	void Update () 
	{
			
	}

	void OnDeath(GameObject _SourcePlayer)
	{                
        s_cSerializeStream.Write((byte)ENetworkAction.EventDeath);  

        //Disable all collisions
        Vector3 parentVelocity = rigidbody.velocity;    
        
        //Disable all rendering
        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        
        //Disable animations
        gameObject.GetComponent<Animator>().enabled = false;
        
        //Enable ragdoll and set position
        Ragdoll.SetActive(true);
        Ragdoll.transform.rotation = transform.rotation;
        Ragdoll.transform.position = transform.position;
              
        //Apply velocity
        foreach (Rigidbody body in gameObject.GetComponentsInChildren<Rigidbody>())
        {
            if (!body.isKinematic)
            {
                body.velocity = parentVelocity;
            }
        }

        foreach (Renderer renderer in Ragdoll.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
        
        //Transfer camera to ragdoll head
        foreach (Transform child in Ragdoll.GetComponentsInChildren<Transform>())
        {
            if (child.name == "Head")
            {
                gameObject.GetComponent<CPlayerHead>().m_cActorHead = child.gameObject;
                gameObject.GetComponent<CPlayerHead>().TransferPlayerPerspectiveToShipSpace();
            }
        }
    }
    
    void OnRevive(GameObject _SourcePlayer)
    {
        s_cSerializeStream.Write((byte)ENetworkAction.EventRevive);  

        //Enable all rendering
        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
        
        //enabled animations
        gameObject.GetComponent<Animator>().enabled = true;
        
        //Disable ragdoll and set position
        Ragdoll.SetActive(false);
        //transform.position = Ragdoll.transform.position;
        
        foreach (Rigidbody body in gameObject.GetComponentsInChildren<Rigidbody>())
        {
            if (!body.isKinematic)
            {
                body.velocity = new Vector3(0, 0, 0);
            }
        }
    }
}
