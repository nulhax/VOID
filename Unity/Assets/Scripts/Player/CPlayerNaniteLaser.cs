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
        INVALID,

        ChangeTarget,

        MAX,
    }


    public enum ETargetType
    {
        INVALID,

        None,
        Module,
        Minerals,
        Hull,

        MAX
    }


// Member Delegates & Events


    public ETargetType TargetType
    {
        get { return (m_eTargetType.Get()); }
    }


    public TNetworkViewId TargetViewId
    {
        get { return (m_tTargetViewId.Value); }
    }


    public GameObject Target
    {
        get
        {
            if (TargetViewId == null)
                return (null);

            return (TargetViewId.GameObject);
        }
    }


// Member Properties


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_eTargetType = _cRegistrar.CreateReliableNetworkVar<ETargetType>(OnNetworkVarSync, ETargetType.None);
        m_tTargetViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
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
        // Get player actor
        GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);

        if (cPlayerActor == null)
            return;

        // Get component
        CPlayerNaniteLaser cPlayerNaniteLaser = cPlayerActor.GetComponent<CPlayerNaniteLaser>();

        // Process data
        while (_cStream.HasUnreadData)
        {
            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            switch (eAction)
            {
                case ENetworkAction.ChangeTarget:
                    {
                        ETargetType eTargetType = _cStream.Read<ETargetType>();

                        if (eTargetType == ETargetType.None)
                        {
                            cPlayerNaniteLaser.SetTarget(eTargetType, null);
                        }
                        else
                        {
                            cPlayerNaniteLaser.SetTarget(eTargetType,
                                                         _cStream.Read<TNetworkViewId>().GameObject);
                        }
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
        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
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
        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Use, OnEventInputChange);

            GetComponent<CPlayerInteractor>().EventTargetChange -= OnEventActorInteractableTargetChange;
        }
	}


	void Update()
	{
        if (gameObject.GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            ProcessTargetRange();
        }

        if (CNetwork.IsServer)
        {
            UpdateTarget();
        }

        UpdateEffects();
	}


    [ALocalOnly]
    void ProcessTargetRange()
    {
        if (TargetType == ETargetType.None)
            return;

        // Get range to target module
        RaycastHit cTargetRaycastHit = GetComponent<CPlayerInteractor>().TargetRaycastHit;

        // Check we are still in range of module
        if (TargetType == ETargetType.Module &&
            cTargetRaycastHit.distance > k_fBuildRange)
        {
            SetTarget(ETargetType.None, null);
        }

        // Check we are still in range of minerals
        else if (TargetType == ETargetType.Minerals != null &&
                 cTargetRaycastHit.distance > k_fMineRange)
        {
            SetTarget(ETargetType.None, null);
        }

        // Check we are still in rnage of hull peice
        else if (TargetType == ETargetType.Hull &&
                 cTargetRaycastHit.distance > k_fHullRepaireRange)
        {
            SetTarget(ETargetType.None, null);
        }
    }


    [AServerOnly]
    void UpdateTarget()
    {
        if (TargetType == ETargetType.None)
            return;

        switch (TargetType)
        {
            case ETargetType.Module:
                {
                    Target.GetComponent<CModuleInterface>().Build(k_fBuildSpeed * Time.deltaTime);

                    if (Target.GetComponent<CModuleInterface>().IsBuilt)
                    {
                        SetTarget(ETargetType.None, null);
                    }
                }
                break;

            case ETargetType.Hull:
                Debug.LogError("TODO");
                break;

            case ETargetType.Minerals:
                {
                    Target.GetComponent<CMineralsBehaviour>().DecrementQuanity(k_fMineRate * Time.deltaTime);

                    if (Target.GetComponent<CMineralsBehaviour>().IsDepleted)
                    {
                        SetTarget(ETargetType.None, null);
                    }
                }
                break;

            default:
                Debug.LogError("Unknown state: " + TargetType);
                break;
        }
    }


    void UpdateEffects()
    {
        if (TargetType == ETargetType.None)
            return;

        GameObject cEffectHitParticles = null;

        switch (TargetType)
        {
            case ETargetType.Module:
                cEffectHitParticles = m_cBuildingHitParticles;
                break;

            case ETargetType.Minerals:
                cEffectHitParticles = m_cMingingHitParticles;
                break;

            case ETargetType.Hull:
                cEffectHitParticles = m_cRepairingHitParticles;
                break;
        }

        // Raycast to target based on player head direction
        GameObject cActorHead = GetComponent<CPlayerHead>().Head;

        RaycastHit tRaycastHit = new RaycastHit();
        Ray cRay = new Ray(cActorHead.transform.position, cActorHead.transform.forward);

        if (Target.collider.Raycast(cRay, out tRaycastHit, CPlayerInteractor.RayRange))
        {
            // Make particles visible
            if (!cEffectHitParticles.activeSelf)
            {
                cEffectHitParticles.layer = cActorHead.layer;
                cEffectHitParticles.SetActive(true);
            }

            // Rotate particles relative to head
            cEffectHitParticles.transform.position = tRaycastHit.point;
            cEffectHitParticles.transform.eulerAngles = Vector3.Reflect(tRaycastHit.point - cActorHead.transform.position, tRaycastHit.normal);
        }
    }


    [ALocalOnly]
    void OnEventInputChange(CUserInput.EInput _eInput, bool _bDown)
    {
        switch (_eInput)
        {
            case CUserInput.EInput.Use:
            {
                if (_bDown)
                {
                    HandleInputUseDown();
                }
                else
                {
                    HandleInputUseUp();
                }
            }
            break;

        default:
            Debug.LogError("Unknown user input: " + _eInput);
            break;
        }
    }


    [ALocalOnly]
    void HandleInputUseDown()
    {
        GameObject cTargetActor      = GetComponent<CPlayerInteractor>().TargetActorObject;
        RaycastHit cTargetRaycastHit = GetComponent<CPlayerInteractor>().TargetRaycastHit;

        // Check has target
        if (cTargetActor == null)
            return;

        // Start building
        if ( cTargetActor.GetComponent<CModuleInterface>() != null &&
            !cTargetActor.GetComponent<CModuleInterface>().IsBuilt &&
                cTargetRaycastHit.distance < k_fBuildRange)
        {
            SetTarget(ETargetType.Module, cTargetActor);
        }

        // Start mining
        else if (cTargetActor.GetComponent<CMineralsBehaviour>() != null &&
                    cTargetRaycastHit.distance < k_fMineRange)
        {
            SetTarget(ETargetType.Minerals, cTargetActor);
        }

        // Stop if target is not applicable to this tool
        else
        {
            SetTarget(ETargetType.None, null);
        }
    }


    [AServerOnly]
    void HandleInputUseUp()
    {
        if (m_eTargetType.Value != ETargetType.None)
        {
            SetTarget(ETargetType.None, null);
        }
    }


    [AOwnerAndServerOnly]
    void SetTarget(ETargetType _eTargetType, GameObject _cTargetObject)
    {
        if (CNetwork.IsServer)
        {
            m_eTargetType.Value = _eTargetType;

            if (_cTargetObject == null)
            {
                m_tTargetViewId.Value = null;
            }
            else
            {
                m_tTargetViewId.Value = _cTargetObject.GetComponent<CNetworkView>().ViewId;
            }
        }
        else if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            switch (_eTargetType)
            {
                case ETargetType.Module:
                case ETargetType.Minerals:
                case ETargetType.Hull:
                    s_cSerializeStream.Write(ENetworkAction.ChangeTarget);
                    s_cSerializeStream.Write(_eTargetType);
                    s_cSerializeStream.Write(_cTargetObject.GetComponent<CNetworkView>().ViewId);
                    break;

                case ETargetType.None:
                    s_cSerializeStream.Write(ENetworkAction.ChangeTarget);
                    s_cSerializeStream.Write(_eTargetType);
                    break;

                default:
                    Debug.LogError("Unknown target type: " + _eTargetType);
                    break;
            }
        }
    }


    [ALocalOnly]
    void OnEventActorInteractableTargetChange(GameObject _cOldTarget, GameObject _cNewTarget, RaycastHit _cRaycastHit)
    {
        // Stop if player changes target 
        if (m_eTargetType.Value != ETargetType.None)
        {
            SetTarget(ETargetType.None, null);
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_eTargetType)
        {
            if (m_cBuildingHitParticles.activeSelf)
                m_cBuildingHitParticles.SetActive(false);

            if (m_cMingingHitParticles.activeSelf)
                m_cMingingHitParticles.SetActive(false);

            if (m_cRepairingHitParticles.activeSelf)
                m_cRepairingHitParticles.SetActive(false);
        }
        else if (_cSyncedVar == m_tTargetViewId)
        {
        }
    }


// Member Fields


    const float k_fBuildSpeed       = 0.50f; // 5% per sec
    const float k_fBuildRange       = 4.0f;
    const float k_fMineRate         = 5.0f;
    const float k_fMineRange        = 10.0f;
    const float k_fHullRepaireRange = 5.0f;


    public GameObject m_cBuildingHitParticles   = null;
    public GameObject m_cMingingHitParticles    = null;
    public GameObject m_cRepairingHitParticles  = null;


    CNetworkVar<ETargetType> m_eTargetType = null;
    CNetworkVar<TNetworkViewId> m_tTargetViewId = null;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
