//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */
using System.Linq;
using System;


public class CPlayerInteractor : CNetworkMonoBehaviour
{
// Member Types


	public enum EInputInteractionType
	{
		INVALID = -1,
		
		Hover,
		Primary,
		Secondary,
		Use,
		
		MAX
	}


    public enum ENetworkAction
    {
        UpdateTarget,
    }


// Member Delegates & Events


    [AServerOnly]
    public delegate void HandleServerTargetChange(GameObject _cInteractableObject);
    public event HandleServerTargetChange EventServerTargetChange;


    [ALocalOnly]
    public delegate void HandleTargetChange(GameObject _cOldTargetObject,  GameObject _CNewTargetObject, RaycastHit _cRaycastHit);
    public event HandleTargetChange EventTargetChange;


    public delegate void HandleInputInteraction(EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown);
    public event HandleInputInteraction EventPrimary;
    public event HandleInputInteraction EventSecondary;
    public event HandleInputInteraction EventUse;
	

// Member Properties


    public GameObject TargetActorObject
    {
        get { return (m_cTargetActorObject); }
    }


    public RaycastHit TargetRaycastHit
    {
        get { return (m_cTargetRaycastHit); }
    }


    public GameObject OldTargetActorObject
    {
        get { return (m_cTargetActorObject); }
    }


    public static float RayRange
    {
        get { return (s_fRayRange); }
    }

	
// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		// Empty
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
        if (CGamePlayers.SelfActor == null)
        {
            return;
        }

        CPlayerInteractor cPlayerInteractor = CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>();

        while (_cStream.HasUnreadData)
        {
            ENetworkAction eAction = (ENetworkAction)_cStream.Read<byte>();
            CNetworkViewId cNewTargetViewId = _cStream.Read<CNetworkViewId>();

            switch (eAction)
            {
                case ENetworkAction.UpdateTarget:
                    if (cNewTargetViewId != null)
                    {
                        cPlayerInteractor.m_cTargetActorObject = cNewTargetViewId.GameObject;

                        if (cPlayerInteractor.EventServerTargetChange != null) cPlayerInteractor.EventServerTargetChange(cPlayerInteractor.m_cTargetActorObject);

                       // Debug.LogError("Found new target: " + cPlayerInteractor.m_cTargetActorObject);
                    }
                    else
                    {
                        cPlayerInteractor.m_cTargetActorObject = null;
                        if (cPlayerInteractor.EventServerTargetChange != null) cPlayerInteractor.EventServerTargetChange(null);

                        //Debug.LogError("Found new target: " + null);
                    }
                    break;

                default:
                    Debug.LogError("Unknown network action");
                    break;
            }
        }
    }


	void Awake()
	{
		s_InteractionRange = s_fRayRange;
	}


	void Start()
	{
		if (gameObject == CGamePlayers.SelfActor)
		{
            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Secondary, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Use, OnEventInput);
		}
	}


	void OnDestroy()
	{
        if (gameObject == CGamePlayers.SelfActor)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Secondary, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Use, OnEventInput);
        }
	}


	void Update()
	{
		// Check interaction for objects when nothing is clicked
		if (CGamePlayers.SelfActor == gameObject)
		{
            UpdateTarget();
		}
	}


    void UpdateTarget()
    {
        if (CGameCameras.MainCamera == null ||
            CGameCameras.ProjectedCamera == null)
        {
            return ;
        }

        Ray cMainCameraRay = new Ray(CGameCameras.MainCamera.transform.position, CGameCameras.MainCamera.transform.forward);
        Ray cProjectedCameraRay = new Ray(CGameCameras.ProjectedCamera.transform.position, CGameCameras.ProjectedCamera.transform.forward);
        GameObject cNewTargetActorObject = null;
        RaycastHit cTargetRaycastHit = new RaycastHit();

        //Debug.DrawRay(cMainCameraRay.origin, cMainCameraRay.direction, Color.red, 0.5f);
        Debug.DrawRay(cProjectedCameraRay.origin, cProjectedCameraRay.direction, Color.green, 0.5f);

        // Do the ray cast against all objects in path
        RaycastHit[] cMainCameraRaycastHits = Physics.RaycastAll(cMainCameraRay, s_fRayRange, 1 << CGameCameras.MainCamera.layer);

        // Do the ray cast against all objects in path
        RaycastHit[] cProjectedCameraRaycastHits = Physics.RaycastAll(cProjectedCameraRay, s_fRayRange, 1 << CGameCameras.ProjectedCamera.layer);

        RaycastHit[] cRaycastHits = new RaycastHit[cMainCameraRaycastHits.Length + cProjectedCameraRaycastHits.Length];
        Array.Copy(cMainCameraRaycastHits, cRaycastHits, cMainCameraRaycastHits.Length);
        Array.Copy(cProjectedCameraRaycastHits, 0, cRaycastHits, cMainCameraRaycastHits.Length, cProjectedCameraRaycastHits.Length);

        cRaycastHits = cRaycastHits.OrderBy(_cRay => _cRay.distance).ToArray();

		// Check each one for an interactable object
        foreach (RaycastHit cRaycastHit in cRaycastHits)
        {
            // Get the game object which owns this mesh
            GameObject cHitObject = cRaycastHit.collider.gameObject;

            // Check the object itself for the interactable script
            CActorInteractable cActorInteractable = cHitObject.GetComponent<CActorInteractable>();

            if (cHitObject.tag == "GalaxyShip")
            {
                continue;
            }

            // Check the parents until we find the one that has CActorInteractable on it
            if (cActorInteractable == null)
            {
                cActorInteractable = CUtility.FindInParents<CActorInteractable>(cHitObject);
            }

            // If found an interactable select it
            if (cActorInteractable != null)
            {
                cNewTargetActorObject = cActorInteractable.gameObject;
                cTargetRaycastHit = cRaycastHit;
				break;
            }
			else if(!cRaycastHit.collider.isTrigger)
			{
				// Break out - if the first non-trigger collider wasn't interactable. This is intentional
				break;
			}
        }

        //Debug.LogError(cNewTargetActorObject);

		if (cNewTargetActorObject != null)
		{
			m_cTargetRaycastHit = cTargetRaycastHit;
		}

        if (cNewTargetActorObject != m_cTargetActorObject)
        {
            s_cSerializeStream.Write((byte)ENetworkAction.UpdateTarget);

            if (cNewTargetActorObject == null)
            {
                s_cSerializeStream.Write((CNetworkViewId)null);
            }
            else
            {
                if (cNewTargetActorObject.GetComponent<CNetworkView>() == null)
                {
                    Debug.LogError("Actor intractable does not have a network view: " + cNewTargetActorObject.name);
                }

                s_cSerializeStream.Write(cNewTargetActorObject.GetComponent<CNetworkView>().ViewId);
            }

            m_cOldTargetActorObject = m_cTargetActorObject;
            m_cTargetActorObject = cNewTargetActorObject;

            // Notify old target about loosing hover
            if (m_cOldTargetActorObject != null)
            {
                m_cOldTargetActorObject.GetComponent<CActorInteractable>().OnInteractionHover(gameObject, new RaycastHit(), false);
            }

            // Notify new target about hover
            if (m_cTargetActorObject != null)
            {
                m_cTargetActorObject.GetComponent<CActorInteractable>().OnInteractionHover(gameObject, m_cTargetRaycastHit, true);
            }

            // Notify observers about target change
            if (EventTargetChange != null) EventTargetChange(m_cOldTargetActorObject, m_cTargetActorObject, m_cTargetRaycastHit);
        }
    }


    void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
    {
        if (m_cTargetActorObject != null)
        {
            CActorInteractable cActorInteractable = m_cTargetActorObject.GetComponent<CActorInteractable>();

            switch (_eInput)
            {
                case CUserInput.EInput.Primary:
                    cActorInteractable.OnInteractionInput(EInputInteractionType.Primary, gameObject, m_cTargetRaycastHit, _bDown);
                    if (EventPrimary != null) EventPrimary(EInputInteractionType.Primary, m_cTargetActorObject, m_cTargetRaycastHit, _bDown);
                    break;

                case CUserInput.EInput.Secondary:
                    cActorInteractable.OnInteractionInput(EInputInteractionType.Secondary, gameObject, m_cTargetRaycastHit, _bDown);
                    if (EventSecondary != null) EventSecondary(EInputInteractionType.Secondary, m_cTargetActorObject, m_cTargetRaycastHit, _bDown);
                    break;

                case CUserInput.EInput.Use:
                    cActorInteractable.OnInteractionInput(EInputInteractionType.Use, gameObject, m_cTargetRaycastHit, _bDown);
                    if (EventUse != null) EventUse(EInputInteractionType.Use, m_cTargetActorObject, m_cTargetRaycastHit, _bDown);
                    break;

                default:
                    Debug.LogError("Unknown input: " + _eInput);
                    break;
            }
        }
    }


// Member Fields


    GameObject m_cOldTargetActorObject = null;
    GameObject m_cTargetActorObject = null;
    RaycastHit m_cTargetRaycastHit;


    static CNetworkStream s_cSerializeStream = new CNetworkStream();


	public static float s_InteractionRange = 0.0f;
    static float s_fRayRange = 40.0f;
    

// Server Member Fields


}
