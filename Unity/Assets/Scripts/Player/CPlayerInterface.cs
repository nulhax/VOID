//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


[RequireComponent(typeof(CPlayerBelt))]
[RequireComponent(typeof(CPlayerMotor))]
[RequireComponent(typeof(CPlayerHead))]
[RequireComponent(typeof(CPlayerHealth))]
[RequireComponent(typeof(CPlayerIKController))]
[RequireComponent(typeof(CPlayerInteractor))]
[RequireComponent(typeof(CPlayerNaniteLaser))]
[RequireComponent(typeof(CPlayerSuit))]
public class CPlayerInterface : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public ulong PlayerId
    {
        get { return (m_ulPlayerId); }
    }


    public GameObject Model
    {
        get
        {
            if (IsOwnedByMe)
            {
                return (m_cModelFirstPerson);
            }
            else
            {
                return (m_cModelThirdPerson);
            }
        }
    }

	public GameObject PlayerHand
	{
		get{ return (m_cRightHand); }
	}

    public bool IsOwnedByMe
    {
        get { return (m_bOwnedByMe); }
    }


// Member Properties


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


    void Awake()
    {
        CGamePlayers.Instance.EventActorRegister += OnEventPlayerActorRegister;
        CGamePlayers.Instance.EventActorUnregister += OnEventPlayerActorUnregister;
    }


	void Start()
	{
       //empty
	}


	void OnDestroy()
	{
        CGamePlayers.Instance.EventActorRegister   -= OnEventPlayerActorRegister;
        CGamePlayers.Instance.EventActorUnregister -= OnEventPlayerActorUnregister;
	}


	void Update()
	{

	}


    void OnEventPlayerActorRegister(ulong _ulPlayerId, GameObject _cPlayerActor)
    {
        if (_cPlayerActor == gameObject)
        {
            m_ulPlayerId = _ulPlayerId;

            m_bOwnedByMe = (m_ulPlayerId == CNetwork.PlayerId);

            if (m_bOwnedByMe)
            {
                m_cModelFirstPerson = GameObject.Instantiate(m_cModelFirstPerson) as GameObject;
                m_cModelFirstPerson.transform.parent = transform;
                m_cModelFirstPerson.transform.localPosition = Vector3.zero;
                m_cModelFirstPerson.transform.localRotation = Quaternion.identity;

				//Apply skeleton variables from model to ragdoll script
				CPlayerSkeleton skeleton = m_cModelFirstPerson.GetComponent<CPlayerSkeleton>();
				CPlayerRagdoll ragdoll = gameObject.GetComponent<CPlayerRagdoll>();
				Animator animator = gameObject.GetComponent<Animator>();

				animator.avatar = skeleton.PlayerAvatar;
				animator.enabled = false;
				animator.enabled = true;

				ragdoll.m_RootSkeleton 	= 	skeleton.SkeletonRoot.transform;
				ragdoll.m_PlayerHead 	= 	skeleton.PlayerHead;
				ragdoll.m_RagdollHead 	= 	skeleton.RagdollHead; 

				ragdoll.Initialise();

				m_cRightHand = skeleton.PlayerRightHand;

            }
            else
            {
                m_cModelThirdPerson = GameObject.Instantiate(m_cModelThirdPerson) as GameObject;
                m_cModelThirdPerson.transform.parent = transform;
                m_cModelThirdPerson.transform.localPosition = Vector3.zero;
                m_cModelThirdPerson.transform.localRotation = Quaternion.identity;

				//Apply skeleton variables from model to ragdoll script
				CPlayerSkeleton skeleton = m_cModelThirdPerson.GetComponent<CPlayerSkeleton>();
				CPlayerRagdoll ragdoll = gameObject.GetComponent<CPlayerRagdoll>();
				Animator animator = gameObject.GetComponent<Animator>();
				
				animator.avatar = skeleton.PlayerAvatar;
				animator.enabled = false;
				animator.enabled = true;
				
				ragdoll.m_RootSkeleton 	= 	skeleton.SkeletonRoot.transform;
				ragdoll.m_PlayerHead 	= 	skeleton.PlayerHead;
				ragdoll.m_RagdollHead 	= 	skeleton.RagdollHead; 

				ragdoll.Initialise();

				m_cRightHand = skeleton.PlayerRightHand;
            }
        }
    }


    void OnEventPlayerActorUnregister(ulong _ulPlayerId)
    {
        // Empty
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
    }


// Member Fields


    public GameObject m_cModelThirdPerson = null;
    public GameObject m_cModelFirstPerson = null;

	GameObject m_cRightHand = null;

    ulong m_ulPlayerId = 0;

    bool m_bOwnedByMe = false;


};
