//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCockpitInterface.cs
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

[RequireComponent(typeof(CActorInteractable))]
public class CCockpitInterface : CNetworkMonoBehaviour
{

// Member Types


    [ABitSize(4)]
	public enum ENetworkAction
	{
        INVALID,
	}


    public enum EType
    {
        INVALID,

        Pilot   = 10,
        Turret  = 20,
    }


// Member Delegates & Events


    public delegate void HandlePlayerEnter(CCockpitInterface _cSender, ulong _ulPlayerId);
	public event HandlePlayerEnter EventMounted;

	
	public delegate void HandlePlayerLeave(CCockpitInterface _cSender, ulong _ulPlayerId);
	public event HandlePlayerLeave EventDismounted;


// Member Properties


	public ulong MountedPlayerId
	{
		get { return (m_ulMountedPlayerId.Value); }
	}


	public GameObject MountedPlayerActor
	{
		get
		{
			if (!IsMounted)
			{
				return (null);
			}
			else
			{
				return (CNetwork.Factory.FindGameObject(CGamePlayers.GetPlayerActorViewId(MountedPlayerId)));
			}
		}
	}


    public EType CockpitType
    {
        get { return (m_eType); }
    }


	public bool IsMounted
	{
		get { return (MountedPlayerId != 0); }
	}


	public Transform SeatTrans
	{
		get { return (m_cSeat); }
	}

	
// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_ulMountedPlayerId = _cRegistrar.CreateReliableNetworkVar<ulong>(OnNetworkVarSync, 0);
	}


    [AServerOnly]
    public bool MountPlayer(ulong _ulPlayerId)
    {
        GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_ulPlayerId);
        bool bAccessGranted = false;

        if ( cPlayerActor != null && 
            !IsMounted &&
             m_cModuleInterface.IsBuilt)
        {
            TNetworkViewId cPlayerActorViewId = cPlayerActor.GetComponent<CNetworkView>().ViewId;

            // Save the position, rotation and head rotation
            m_vRemoteEnterPosition = cPlayerActor.transform.position;
            m_vRemoteEnterEuler = cPlayerActor.transform.eulerAngles;
            m_vRemoteHeadEuler = new Vector3(cPlayerActor.GetComponent<CPlayerHead>().RemoteHeadEulerX,
                                             cPlayerActor.GetComponent<CPlayerHead>().RemoteHeadEulerY,
                                             0.0f);

            // Move player to seat location and rotation
            cPlayerActor.GetComponent<CNetworkView>().SetParent(m_cSeat.GetComponent<CNetworkView>().ViewId);
            cPlayerActor.GetComponent<CNetworkView>().SetLocalPosition(0.0f, 0.0f, 0.0f);
            cPlayerActor.GetComponent<CNetworkView>().SetLocalEuler(0.0f, 0.0f, 0.0f);
            cPlayerActor.GetComponent<CPlayerHead>().SetLookDirection(0.0f, 0.0f);
            
            // Set the player kinematic
            //cPlayerActor.rigidbody.isKinematic = true;

            m_ulMountedPlayerId.Set(_ulPlayerId);

            bAccessGranted = true;
        }

        return (bAccessGranted);
    }


    [ALocalOnly]
    public void EjectPlayer()
    {
        // Allow player to leave cockpit
        if (MountedPlayerId != 0)
        {
            // Teleport player back to entered position
            GameObject cPlayerActor = CGamePlayers.GetPlayerActor(MountedPlayerId);

            if (cPlayerActor != null)
            {
                // Un-parent the player to the cockpit seat
                cPlayerActor.GetComponent<CNetworkView>().SetParent(null);

                // Move player back to positions when entered
                cPlayerActor.GetComponent<CNetworkView>().SetPosition(m_vRemoteEnterPosition);
                cPlayerActor.GetComponent<CNetworkView>().SetEuler(m_vRemoteEnterEuler);
                cPlayerActor.GetComponent<CNetworkView>().GetComponent<CPlayerHead>().SetLookDirection(m_vRemoteHeadEuler.x, m_vRemoteHeadEuler.y);

                // Turn of kinematic
				//cPlayerActor.rigidbody.isKinematic = false;
            }

            m_ulMountedPlayerId.Set(0);
        }
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            TNetworkViewId cCockpitObjectViewId = _cStream.Read<TNetworkViewId>();

            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            CCockpitInterface cCockpit = cCockpitObjectViewId.GameObject.GetComponent<CCockpitInterface>();

            switch (eAction)
            {
                default:
                    Debug.LogError(string.Format("Unknown network action ({0})"));
                    break;
            }
        }
    }


    void Awake()
    {
        m_cModuleInterface = GetComponent<CModuleInterface>();
    }


	void Start()
	{
        // Check type has been set
        if (m_eType == EType.INVALID)
            Debug.LogError(string.Format("Cockpit gameobject({0}) has not been assigned a cockpit type.", gameObject.name));
	}


	void OnDestroy()
	{
        // Empty
	}


	void Update()
	{
        // Empty
	}


    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        if (_cSynedVar == m_ulMountedPlayerId)
        {
            HandleVarSyncMountedPlayer();
        }
    }


    void HandleVarSyncMountedPlayer()
    {
        // Check player dismounted the cockpit
        if (m_ulMountedPlayerId.Value == 0)
        {
            // Notify obersvers
            if (EventDismounted != null)
                EventDismounted(this, m_ulMountedPlayerId.PreviousValue);
        }

        // Player entered the cockpit
        else
        {
            // Notify observers
            if (EventMounted != null)
                EventMounted(this, m_ulMountedPlayerId.Value);
        }
    }


// Member Fields


    public EType m_eType = EType.INVALID;
    public Transform m_cSeat = null;
    public CComponentInterface[] m_Components;


	CNetworkVar<ulong> m_ulMountedPlayerId = null;


    CModuleInterface m_cModuleInterface = null;


    Vector3 m_vRemoteEnterPosition = Vector3.zero;
    Vector3 m_vRemoteEnterEuler = Vector3.zero;
    Vector3 m_vRemoteHeadEuler = Vector3.zero;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
