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


[RequireComponent(typeof(CToolInterface))]
public class CNanitePistolBehaviour : CNetworkMonoBehaviour
{

    // Member Types


    public enum ENetworkAction
    {
        Invalid,

        BuildModule,
        RequireHullBreach,
        MineMinerals,
        Stop,

        Max,
    }


    public enum EState
    {
        INVALID,

        Idle,
        BuildingModule,
        RepairingHull,
        Mining,

        MAX
    }


    const uint k_uiNetworkActionBitSize = 4;


    // Member Delegates & Events


    // Member Properties


    public EState State
    {
        get { return (m_eState.Get()); }
    }


    // Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_eState = _cRegistrar.CreateNetworkVar<EState>(OnNetworkVarSync, EState.Idle);
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        // Write in internal stream
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();

    }

    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            // Extract action
            ENetworkAction eAction = (ENetworkAction)_cStream.ReadBits<byte>(k_uiNetworkActionBitSize);

            // Extract sending nanite pistol behaviour
            CNanitePistolBehaviour cNanitePistolBehaviour = _cStream.Read<CNetworkViewId>().GameObject.GetComponent<CNanitePistolBehaviour>();

            switch (eAction)
            {
                case ENetworkAction.BuildModule:
                    {
                        Debug.LogError("Build modules start");

                        cNanitePistolBehaviour.m_eState.Set(EState.BuildingModule);
                        cNanitePistolBehaviour.m_cTargetModule = _cStream.Read<CNetworkViewId>().GameObject;
                    }
                    break;

                case ENetworkAction.MineMinerals:
                    {
                        Debug.LogError("Mine minerals start");

                        cNanitePistolBehaviour.m_eState.Set(EState.Mining);
                        cNanitePistolBehaviour.m_cTargetMinerals = _cStream.Read<CNetworkViewId>().GameObject;
                    }
                    break;

                case ENetworkAction.RequireHullBreach:
                    {
                        Debug.LogError("Reparing hull breach start");
                    }
                    break;

                case ENetworkAction.Stop:
                    {
                        //Debug.LogError("Stop state: " + cNanitePistolBehaviour.State);

                        cNanitePistolBehaviour.m_eState.Set(EState.Idle);
                    }
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eAction);
                    break;
            }
        }
    }


    void Awake()
    {
        m_cToolInterface = GetComponent<CToolInterface>();
    }


    void Start()
    {
        GetComponent<CToolInterface>().EventPrimaryActiveChange += OnEventPrimaryChange;
        GetComponent<CToolInterface>().EventPickedUp += OnEventPickedUp;
        GetComponent<CToolInterface>().EventDropped += OnEventDopped;

        m_cBuildingHitParticles = GameObject.Instantiate(m_cMingingHitParticles) as GameObject;
        m_cBuildingHitParticles.SetActive(false);

        m_cMingingHitParticles = GameObject.Instantiate(m_cMingingHitParticles) as GameObject;
        m_cMingingHitParticles.SetActive(false);

        m_cRepairingHitParticles = GameObject.Instantiate(m_cMingingHitParticles) as GameObject;
        m_cRepairingHitParticles.SetActive(false);
    }


    void OnDestroy()
    {
        // Empty
    }


    void Update()
    {
        if (CNetwork.IsServer)
        {
            UpdateState();
        }

        UpdateMiningLaser();
        UpdateBuildingLaser();

        if (m_eState.Get() == EState.Idle)
        {
            // Make particles invisible
            if (m_cMingingHitParticles.activeSelf)
            {
                m_cMingingHitParticles.SetActive(false);
            }
        }
    }


    [AServerOnly]
    void UpdateState()
    {
        if (State != EState.Idle)
        {
            switch (State)
            {
                case EState.BuildingModule:
                    if (m_cTargetModule != null)
                    {
                        m_cTargetModule.GetComponent<CModuleInterface>().IncrementBuiltRatio(k_fBuildRatioPower * Time.deltaTime);
                    }
                    break;

                case EState.RepairingHull:
                    break;

                case EState.Mining:
                    if (m_cTargetMinerals != null)
                    {
                        m_cTargetMinerals.GetComponent<CMineralsBehaviour>().DecrementQuanity(k_fMiningRate * Time.deltaTime);
                    }
                    break;

                default:
                    Debug.LogError("Unknown state: " + State);
                    break;
            }
        }
    }


    [ALocalOnly]
    void UpdateStates()
    {
        if (m_cToolInterface.IsPrimaryActive)
        {
            CPlayerInteractor cPlayerInteractor = m_cToolInterface.OwnerPlayerActor.GetComponent<CPlayerInteractor>();

            GameObject cTargetActorObject = cPlayerInteractor.TargetActorObject;
            RaycastHit cTargetRaycastHit = cPlayerInteractor.TargetRaycastHit;

            if (cTargetActorObject != null)
            {
                // Start building
                if (cTargetActorObject.GetComponent<CModuleInterface>() != null &&
                    !cTargetActorObject.GetComponent<CModuleInterface>().IsBuilt &&
                     cTargetRaycastHit.distance < k_fBuildingRange)
                {
                    m_cTargetModule = cTargetActorObject;

                    s_cSerializeStream.WriteBits((byte)ENetworkAction.BuildModule, k_uiNetworkActionBitSize);
                    s_cSerializeStream.Write(SelfNetworkViewId);
                    s_cSerializeStream.Write(cTargetActorObject.GetComponent<CNetworkView>().ViewId);
                }

                // Start mining
                else if (cTargetActorObject.GetComponent<CMineralsBehaviour>() != null &&
                         cTargetRaycastHit.distance < k_fMiningRange)
                {
                    m_cTargetMinerals = cTargetActorObject;

                    s_cSerializeStream.WriteBits((byte)ENetworkAction.MineMinerals, k_uiNetworkActionBitSize);
                    s_cSerializeStream.Write(SelfNetworkViewId);
                    s_cSerializeStream.Write(m_cTargetMinerals.GetComponent<CNetworkView>().ViewId);

                    // Subscribe to deplete event
                    m_cTargetMinerals.GetComponent<CMineralsBehaviour>().EventDeplete += OnEventMineralsDeplete;
                }
            }
        }
        else if (State != EState.Idle)
        {
            s_cSerializeStream.WriteBits(ENetworkAction.Stop, k_uiNetworkActionBitSize);
            s_cSerializeStream.Write(SelfNetworkViewId);
        }
    }


    [ALocalOnly]
    void UpdateMiningLaser()
    {
        if (m_bShowMiningLaser)
        {
            Transform cActorHeadTransform = GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerHead>().m_Head.transform;

            RaycastHit tRaycastHit = new RaycastHit();
            Ray cRay = new Ray(cActorHeadTransform.position, cActorHeadTransform.forward);

            if (Physics.Raycast(cRay, out tRaycastHit, CPlayerInteractor.RayRange))
            {
                CMineralsBehaviour cMinerals = tRaycastHit.collider.gameObject.GetComponent<CMineralsBehaviour>();

                if (cMinerals != null)
                {
                    // Make particles visible
                    if (!m_cMingingHitParticles.activeSelf)
                    {
                        m_cMingingHitParticles.layer = gameObject.GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerHead>().m_Head.layer;
                        m_cMingingHitParticles.SetActive(true);
                    }

                    // Position and rotate particles
                    m_cMingingHitParticles.transform.position = tRaycastHit.point;
                    m_cMingingHitParticles.transform.eulerAngles = Vector3.Reflect(tRaycastHit.point - cActorHeadTransform.position, tRaycastHit.normal);
                }

                // Make particles invisible
                else if (m_cMingingHitParticles.activeSelf)
                {
                    m_cMingingHitParticles.SetActive(false);
                }
            }

            // Make particles invisible
            else if (m_cMingingHitParticles.activeSelf)
            {
                m_cMingingHitParticles.SetActive(false);
            }
        }
    }


    [ALocalOnly]
    void UpdateBuildingLaser()
    {
        if (m_bShowBuildingLaser)
        {
            Transform cActorHeadTransform = GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerHead>().m_Head.transform;

            RaycastHit tRaycastHit = new RaycastHit();
            Ray cRay = new Ray(cActorHeadTransform.position, cActorHeadTransform.forward);

            if (Physics.Raycast(cRay, out tRaycastHit, CPlayerInteractor.RayRange))
            {
                CModuleInterface cMinerals = tRaycastHit.collider.gameObject.transform.parent.GetComponent<CModuleInterface>();

                if (cMinerals != null &&
                    !cMinerals.IsBuilt)
                {
                    // Make particles visible
                    if (!m_cMingingHitParticles.activeSelf)
                    {
                        m_cMingingHitParticles.layer = gameObject.GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerHead>().m_Head.layer;
                        m_cMingingHitParticles.SetActive(true);
                    }

                    // Position and rotate particles
                    m_cMingingHitParticles.transform.position = tRaycastHit.point;
                    m_cMingingHitParticles.transform.eulerAngles = Vector3.Reflect(tRaycastHit.point - cActorHeadTransform.position, tRaycastHit.normal);
                }

                // Make particles invisible
                else if (m_cMingingHitParticles.activeSelf)
                {
                    m_cMingingHitParticles.SetActive(false);
                }
            }

            // Make particles invisible
            else if (m_cMingingHitParticles.activeSelf)
            {
                m_cMingingHitParticles.SetActive(false);
            }
        }
    }


    [ALocalOnly]
    void OnEventPrimaryChange(bool _bDown)
    {

    }


    [ALocalOnly]
    void OnEventMineralsDeplete(GameObject _cMinerals)
    {
        s_cSerializeStream.WriteBits(ENetworkAction.Stop, k_uiNetworkActionBitSize);
        s_cSerializeStream.Write(SelfNetworkViewId);
    }


    [ALocalOnly]
    void OnEventPickedUp()
    {
        GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerInteractor>().EventTargetChange += OnEventActorInteractableTargetChange;
    }


    [ALocalOnly]
    void OnEventDopped()
    {
        if (GetComponent<CToolInterface>().OwnerPlayerActor != null)
        {
            GetComponent<CToolInterface>().OwnerPlayerActor.GetComponent<CPlayerInteractor>().EventTargetChange -= OnEventActorInteractableTargetChange;
        }

        s_cSerializeStream.WriteBits(ENetworkAction.Stop, k_uiNetworkActionBitSize);
        s_cSerializeStream.Write(SelfNetworkViewId);
    }


    [ALocalOnly]
    void OnEventActorInteractableTargetChange(GameObject _cOldTargetObject, GameObject _CNewTargetObject, RaycastHit _cRaycastHit)
    {
        s_cSerializeStream.WriteBits(ENetworkAction.Stop, k_uiNetworkActionBitSize);
        s_cSerializeStream.Write(SelfNetworkViewId);
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_eState)
        {
            if (State == EState.Idle)
            {
                m_cMingingHitParticles.SetActive(false);
                m_bShowMiningLaser = false;
                m_bShowBuildingLaser = false;
            }
            else if (State == EState.Mining)
            {
                m_bShowMiningLaser = true;
                m_bShowBuildingLaser = false;
            }
            else if (State == EState.BuildingModule)
            {
                m_bShowMiningLaser = false;
                m_bShowBuildingLaser = true;
            }
        }
    }


// Member Fields


    const float k_fBuildRatioPower  = 0.05f; // 5% per sec
    const float k_fMiningRate       = 5.0f;
    const float k_fMiningRange      = 10.0f;
    const float k_fBuildingRange    = 3.0f;


    public GameObject m_cBuildingHitParticles   = null;
    public GameObject m_cMingingHitParticles    = null;
    public GameObject m_cRepairingHitParticles  = null;


    CNetworkVar<EState> m_eState = null;


    CToolInterface m_cToolInterface = null;


    GameObject m_cTargetModule      = null;
    GameObject m_cTargetMinerals    = null;
    GameObject m_cTargetHullBreach  = null;


    bool m_bShowBuildingLaser   = false;
    bool m_bShowMiningLaser     = false;
    bool m_bShowReparingLaser   = false;


    bool m_bSubscribedToMineralsEvent = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
