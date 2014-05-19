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
    public enum ERagdollState : byte
    {
        Invalid,

        PlayerDown,
        PlayerRevive,

        Max,
    }
    
    //Member Delegates & Events   
    
    //Member Properties  
    
    //Member variables

    public Transform m_RootSkeleton = null;

    public GameObject m_PlayerHead = null;
    public GameObject m_RagdollHead = null;

    private Vector3 m_initialOffset = new Vector3(0,0,0);

	private Vector3 m_headInitialOffset = new Vector3(0, 1.6f, 0.05f);
	private Vector3 m_headRagdollOffset = new Vector3(-0.13f , -0.03f, 0.05f);

    CNetworkVar<byte>       m_bRagdollState;

    static CNetworkStream   s_cSerializeStream = new CNetworkStream();  

	bool m_bRagdolling = false;

    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
       m_bRagdollState = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, (byte)ERagdollState.Invalid);
    }
    
    void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    { 
        if (_cSyncedNetworkVar == m_bRagdollState)
        {
            switch((ERagdollState) m_bRagdollState.Get())
            {
                case ERagdollState.PlayerDown:
                {
                    SetRagdollActive();                    
                    break;                
                }
                
                case ERagdollState.PlayerRevive:
                {
                    DeactivateRagdoll();                
                    break;                
                }
            }
        }
    } 

    public static void Serialize(CNetworkStream _cStream)
    {
        // Write in internal stream
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();        
    }
    
    public static void Unserialize(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        //Extract network action
		ERagdollState eNetworkAction = (ERagdollState)_cStream.ReadType(typeof(ERagdollState));
        
        switch (eNetworkAction)
        {
            case ERagdollState.PlayerDown:
            {
				ulong uPlayerID = (ulong)_cStream.ReadType(typeof(ulong));

                GameObject cPlayerActor = CGamePlayers.GetPlayerActor(uPlayerID);
                CPlayerRagdoll ragdoll = cPlayerActor.GetComponent<CPlayerRagdoll>();

                ragdoll.m_bRagdollState.Set((byte)eNetworkAction);

                break;
            }
            case ERagdollState.PlayerRevive:
            {
				ulong uPlayerID = (ulong)_cStream.ReadType(typeof(ulong));
                
                GameObject cPlayerActor = CGamePlayers.GetPlayerActor(uPlayerID);
                CPlayerRagdoll ragdoll = cPlayerActor.GetComponent<CPlayerRagdoll>();

                ragdoll.m_bRagdollState.Set((byte)eNetworkAction);

                break;
            }
        }
    }
    
    // Use this for initialization
    public void Initialise ()
	{
		IntitializeLimbs();

        SetKinematicRagdoll();		
        SetRagdollLayer();

        gameObject.rigidbody.isKinematic = false;

        gameObject.GetComponent<CPlayerHealth>().m_EventHealthStateChanged += OnHealthStateChanged;

		m_initialOffset = m_RootSkeleton.localPosition;
		m_bRagdolling = false;

		//Disable client side rigidbody
		if (!CNetwork.IsServer) 
		{
			//rigidbody.isKinematic = true; // Sorry I need to disable this - Bryce
		}
    }
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.R))
		{
			if(m_bRagdolling)
			{
				DeactivateRagdoll();
				m_bRagdolling = false;
			}
			else
			{
				SetRagdollActive();
				m_bRagdolling = true;
			}
		}
	}

    [AServerOnly]
    void OnHealthStateChanged(GameObject _SourcePlayer, CPlayerHealth.HealthState _eHealthCurrentState, CPlayerHealth.HealthState _eHealthPreviousState)
	{  
        switch (_eHealthCurrentState)
        {
            case CPlayerHealth.HealthState.DOWNED:
            {
                s_cSerializeStream.Write((byte)ERagdollState.PlayerDown);
                //Send in player ID
                s_cSerializeStream.Write((ulong)CGamePlayers.GetPlayerActorsPlayerId(_SourcePlayer));

                break;
            }           
            case CPlayerHealth.HealthState.ALIVE:
            {           
                s_cSerializeStream.Write((byte)ERagdollState.PlayerRevive);
                //Send in player ID
                s_cSerializeStream.Write((ulong)CGamePlayers.GetPlayerActorsPlayerId(_SourcePlayer));
                break;
            }
        }       
    }

    void SetRagdollActive()
    {       
        //Enable ragdoll and set position
        SetDynamicRagdoll();

		if (CGamePlayers.SelfActor == gameObject) 
		{
			//Set head parent
			m_PlayerHead.transform.parent = m_RagdollHead.transform;
			m_PlayerHead.transform.localPosition = m_headRagdollOffset;
		}
    }

    void DeactivateRagdoll()
    {         
        //Disable ragdoll and set position
        SetKinematicRagdoll(); 

		if (CGamePlayers.SelfActor == gameObject) 
		{
			m_PlayerHead.transform.parent = transform;
			m_PlayerHead.transform.localPosition = m_headInitialOffset;
		}

		m_RagdollHead.transform.rotation = Quaternion.identity;

    }

    void SetKinematicRagdoll()
    {
        Transform[] ragdollBones = m_RootSkeleton.GetComponentsInChildren<Transform>();

        foreach (Transform body in ragdollBones)
        {
            if(body.gameObject.GetComponent<Rigidbody>())
            {
                body.rigidbody.isKinematic = true;
                
                body.rigidbody.mass = 9.0f;
            }
            if(body.gameObject.GetComponent<Collider>())
            {
                body.collider.enabled = false;
            }
        } 

		rigidbody.collider.enabled = true;
    }

	void IntitializeLimbs()
	{
		Transform[] ragdollBones = m_RootSkeleton.GetComponentsInChildren<Transform>();
		
		foreach (Transform body in ragdollBones)
		{
			if(body.gameObject.GetComponent<Rigidbody>())
			{
				//Add sound effects for each limb with a rigidbody
				body.gameObject.AddComponent<CRagDollSFX>();
			}
		}
	}

    void SetDynamicRagdoll()
    {
        Transform[] ragdollBones = m_RootSkeleton.GetComponentsInChildren<Transform>();

        foreach (Transform body in ragdollBones)
        {
            if(body.gameObject.GetComponent<Rigidbody>())
            {
                body.rigidbody.isKinematic = false;
                body.rigidbody.velocity = rigidbody.velocity;
				body.rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

				if(GetComponent<CActorBoardable>().BoardingState == CActorBoardable.EBoardingState.Onboard)
				{
					body.rigidbody.useGravity = true;
				}
				else
				{
					body.rigidbody.useGravity = false;
				}
            }
            if(body.gameObject.GetComponent<Collider>())
            {
                body.collider.enabled = true;
            }
        }

		rigidbody.collider.enabled = false;
	}

    public void SetRagdollLayer()
    {
        Transform[] ragdollBones = m_RootSkeleton.GetComponentsInChildren<Transform>();

        foreach (Transform body in ragdollBones)
        {
            body.gameObject.layer = 14;
        }
    }
}
