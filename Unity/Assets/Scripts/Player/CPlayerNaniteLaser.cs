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


public class CPlayerNaniteLaser : CNetworkMonoBehaviour
{

// Member Types


    [ABitSize(4)]
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


// Member Delegates & Events


    public EState State
    {
        get { return (m_eState.Get()); }
    }


// Member Properties


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
        while (CGamePlayers.SelfActor != null &&
               _cStream.HasUnreadData)
        {
            // Extract action
            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            // Extract sending nanite pistol behaviour
            CPlayerNaniteLaser cNaniteLaserBehaviour = CGamePlayers.SelfActor.GetComponent<CPlayerNaniteLaser>();

            switch (eAction)
            {
                case ENetworkAction.BuildModule:
                    {
                        // Debug.LogError("Build modules start");

                        cNaniteLaserBehaviour.m_eState.Set(EState.BuildingModule);
                        cNaniteLaserBehaviour.m_cTargetModule = _cStream.Read<CNetworkViewId>().GameObject;
                    }
                    break;

                case ENetworkAction.MineMinerals:
                    {
                        // Debug.LogError("Mine minerals start");

                        cNaniteLaserBehaviour.m_eState.Set(EState.Mining);
                        cNaniteLaserBehaviour.m_cTargetMinerals = _cStream.Read<CNetworkViewId>().GameObject;
                    }
                    break;

                case ENetworkAction.RequireHullBreach:
                    {
                        // Debug.LogError("Reparing hull breach start");
                    }
                    break;

                case ENetworkAction.Stop:
                    {
                        cNaniteLaserBehaviour.m_eState.Set(EState.Idle);
                    }
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eAction);
                    break;
            }
        }
    }


	void Start()
	{
        if (gameObject == CGamePlayers.SelfActor)
        {
            CUserInput.SubscribeInputChange(CUserInput.EInput.Use, OnEventInputChange);

            GetComponent<CPlayerInteractor>().EventTargetChange += OnEventActorInteractableTargetChange;
        }

        // Load mining effects
        m_cBuildingHitParticles = GameObject.Instantiate(m_cMingingHitParticles) as GameObject;
        m_cBuildingHitParticles.SetActive(false);

        // Load mining effects
        m_cMingingHitParticles = GameObject.Instantiate(m_cMingingHitParticles) as GameObject;
        m_cMingingHitParticles.SetActive(false);

        // Load repairing effects
        m_cRepairingHitParticles = GameObject.Instantiate(m_cMingingHitParticles) as GameObject;
        m_cRepairingHitParticles.SetActive(false);
	}


	void OnDestroy()
	{
        if (gameObject == CGamePlayers.SelfActor)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Use, OnEventInputChange);

            GetComponent<CPlayerInteractor>().EventTargetChange -= OnEventActorInteractableTargetChange;
        }
	}


	void Update()
	{
        if (CNetwork.IsServer)
        {
            UpdateState();
        }

        if (gameObject == CGamePlayers.SelfActor)
        {
            ProcessTargetRange();
        }

        UpdateMiningEffects();
        UpdateBuildingEffects();
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

                        if (m_cTargetModule.GetComponent<CModuleInterface>().IsBuilt)
                        {
                            m_eState.Set(EState.Idle);
                        }
                    }
                    break;

                case EState.RepairingHull:
                    break;

                case EState.Mining:
                    if (m_cTargetMinerals != null)
                    {
                        m_cTargetMinerals.GetComponent<CMineralsBehaviour>().DecrementQuanity(k_fMiningRate * Time.deltaTime);

                        if (m_cTargetMinerals.GetComponent<CMineralsBehaviour>().IsDepleted)
                        {
                            m_eState.Set(EState.Idle);
                        }
                    }
                    break;

                default:
                    Debug.LogError("Unknown state: " + State);
                    break;
            }
        }
    }


    [ALocalOnly]
    void UpdateMiningEffects()
    {
        if (m_bShowMiningLaser)
        {
            Transform cActorHeadTransform = GetComponent<CPlayerHead>().Head.transform;

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
                        m_cMingingHitParticles.layer = GetComponent<CPlayerHead>().Head.layer;
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
    void UpdateBuildingEffects()
    {
        if (m_bShowBuildingLaser)
        {
            Transform cActorHeadTransform = GetComponent<CPlayerHead>().Head.transform;

            RaycastHit tRaycastHit = new RaycastHit();
            Ray cRay = new Ray(cActorHeadTransform.position, cActorHeadTransform.forward);

            if (Physics.Raycast(cRay, out tRaycastHit, CPlayerInteractor.RayRange))
            {
                CModuleInterface cModule = tRaycastHit.collider.gameObject.transform.parent.GetComponent<CModuleInterface>();

                if ( cModule != null &&
                    !cModule.IsBuilt)
                {
                    // Make particles visible
                    if (!m_cMingingHitParticles.activeSelf)
                    {
                        m_cMingingHitParticles.layer = GetComponent<CPlayerHead>().Head.layer;
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
    void OnEventInputChange(CUserInput.EInput _eInput, bool _bDown)
    {
        switch (_eInput)
        {
        case CUserInput.EInput.Use:
            HandleUseKeyChange(_bDown);
            break;

        default:
            Debug.LogError("Unknown user input: " + _eInput);
            break;
        }
    }


    [ALocalOnly]
    void HandleUseKeyChange(bool _bUseKeyDown)
    {
        m_cTargetModule     = null;
        m_cTargetMinerals   = null;
        m_cTargetHullBreach = null;

        // Check primary is down
        if (_bUseKeyDown)
        {
            GameObject cTargetActor      = GetComponent<CPlayerInteractor>().TargetActorObject;
            RaycastHit cTargetRaycastHit = GetComponent<CPlayerInteractor>().TargetRaycastHit;

            // Check has target
            if (cTargetActor != null)
            {
                // Start building
                if ( cTargetActor.GetComponent<CModuleInterface>() != null &&
                    !cTargetActor.GetComponent<CModuleInterface>().IsBuilt &&
                     cTargetRaycastHit.distance < k_fBuildingRange)
                {
                    m_cTargetModule = cTargetActor;

                    s_cSerializeStream.Write(ENetworkAction.BuildModule);
                    s_cSerializeStream.Write(cTargetActor.GetComponent<CNetworkView>().ViewId);

                    m_bLocalIdle = false;
                }

                // Start mining
                else if (cTargetActor.GetComponent<CMineralsBehaviour>() != null &&
                         cTargetRaycastHit.distance < k_fMiningRange)
                {
                    m_cTargetMinerals = cTargetActor;

                    s_cSerializeStream.Write(ENetworkAction.MineMinerals);
                    s_cSerializeStream.Write(m_cTargetMinerals.GetComponent<CNetworkView>().ViewId);

                    m_bLocalIdle = false;

                    // Subscribe to deplete event
                    m_cTargetMinerals.GetComponent<CMineralsBehaviour>().EventDeplete += (GameObject _cMineral) =>
                    {
                        LocalStop();
                    };
                }

                // Stop if target is not applicable to this tool
                else
                {
                    LocalStop();
                }
            }

            // Stop if no target
            else
            {
                LocalStop();
            }
        }

        // Stop if user key is not down
        else
        {
            m_cTargetModule     = null;
            m_cTargetMinerals   = null;
            m_cTargetHullBreach = null;

            LocalStop();
        }
    }


    [ALocalOnly]
    void ProcessTargetRange()
    {
        if (!m_bLocalIdle)
        {
            // Get range to target module
            RaycastHit cTargetRaycastHit = GetComponent<CPlayerInteractor>().TargetRaycastHit;

            // Check we are still in range of module
            if (m_cTargetModule != null &&
                cTargetRaycastHit.distance > k_fBuildingRange)
            {
                LocalStop();
            }

            // Check we are still in range of minerals
            else if (m_cTargetMinerals != null &&
                     cTargetRaycastHit.distance > k_fMiningRange)
            {
                LocalStop();
            }

            // Check we are still in rnage of hull peice
            else if (m_cTargetHullBreach != null &&
                     cTargetRaycastHit.distance > k_fHullRepairingRange)
            {
                LocalStop();
            }
        }
    }


    [ALocalOnly]
    void LocalStop()
    {
        // Dont send if we have not sent a non idle action to server
        if (!m_bLocalIdle)
        {
            s_cSerializeStream.Write(ENetworkAction.Stop);

            m_bLocalIdle = true;
        }
    }


    [ALocalOnly]
    void OnEventActorInteractableTargetChange(GameObject _cOldTarget, GameObject _cNewTarget, RaycastHit _cRaycastHit)
    {
        // Stop if player changes target 
        LocalStop();
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


    const float k_fBuildRatioPower      = 0.05f; // 5% per sec
    const float k_fBuildingRange        = 3.0f;
    const float k_fMiningRate           = 5.0f;
    const float k_fMiningRange          = 10.0f;
    const float k_fHullRepairingRange   = 5.0f;


    public GameObject m_cBuildingHitParticles   = null;
    public GameObject m_cMingingHitParticles    = null;
    public GameObject m_cRepairingHitParticles  = null;


    CNetworkVar<EState> m_eState    = null;


    GameObject m_cTargetModule      = null;
    GameObject m_cTargetMinerals    = null;
    GameObject m_cTargetHullBreach  = null;


    bool m_bShowBuildingLaser   = false;
    bool m_bShowMiningLaser     = false;
    bool m_bShowRepairingLaser  = false;


    bool m_bSubscribedToMineralsEvent = false;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


// Owner Member Variables


    EState m_eLocalState = EState.Idle;
    bool   m_bLocalIdle  = true;


};
