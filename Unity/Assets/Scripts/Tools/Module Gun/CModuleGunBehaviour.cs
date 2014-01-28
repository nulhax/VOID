//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRachetBehaviour.cs
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
public class CModuleGunBehaviour : CNetworkMonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	public Transform m_ActiveUITransform = null;
	public Transform m_InactiveUITransform = null;

	public GameObject m_DUI = null;

	public float m_UITransitionTime = 0.5f;

	private CToolInterface m_ToolInterface = null;
	private float m_TransitionTimer = 0.0f;

	private Vector3 m_ToTransionPos = Vector3.zero;
	private Quaternion m_ToTransionRot = Quaternion.identity;
	private Vector3 m_ToTransionScale = Vector3.zero;

	private Vector3 m_FromTransionPos = Vector3.zero;
	private Quaternion m_FromTransionRot = Quaternion.identity;
	private Vector3 m_FromTransionScale = Vector3.zero;

	private Vector3 m_ActivatedPositionOffset = Vector3.zero;
	
	private bool m_Activating = false;
	private bool m_Deactivating = false;

	private CNetworkVar<bool> m_DUIActive = null;


	// Member Properties
	public bool IsDUIActive
	{
		get { return(m_DUIActive.Get()); }
	}
	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_DUIActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_DUIActive)
		{
			if (IsDUIActive)
			{
				ActivateDUI();
			}
			else
			{
				DeactivateDUI();
			}
		}
	}
	
	public void Start()
	{
		// Register the interaction events
		m_ToolInterface = gameObject.GetComponent<CToolInterface>();
		m_ToolInterface.EventPrimaryActivate += OnPrimaryStart;
		m_ToolInterface.EventSecondaryActivate += OnSecondaryStart;

		// Register mouse movement events
		CUserInput.EventMouseMoveX += OnMouseMoveX;
		CUserInput.EventMouseMoveY += OnMouseMoveY;

		// Set the DUI object as inactive
		m_DUI.transform.position = m_InactiveUITransform.position;
		m_DUI.transform.rotation = m_InactiveUITransform.rotation;
		m_DUI.transform.localScale = m_InactiveUITransform.localScale;
		m_DUI.SetActive(false);
	}

	public void Update()
	{
		if(!m_ToolInterface)
			return;

		if(m_Activating || m_Deactivating)
		{
			UpdateTransitioning();
		}

		UpdateDUITransform();
	}

	private void UpdateTransitioning()
	{
		m_TransitionTimer += Time.deltaTime;
		
		if(m_TransitionTimer > m_UITransitionTime)
		{
			m_TransitionTimer = m_UITransitionTime;

			if(m_Activating) 
			{
				m_Activating = false;
			}
			if(m_Deactivating)
			{
				m_DUI.SetActive(false);
				m_Deactivating = false;
			}
		}
	}
	
	private void ActivateDUI()
	{
		m_DUI.SetActive(true);

		m_Activating = true;
		m_TransitionTimer = 0.0f;
		m_ActivatedPositionOffset = Vector3.zero;

		Vector3 ActivePosToHead = (m_ActiveUITransform.position - m_ToolInterface.OwnerPlayerActor.GetComponent<CPlayerHead>().ActorHead.transform.position).normalized;

		m_ToTransionPos = m_ActiveUITransform.localPosition;
		m_ToTransionRot = Quaternion.Inverse(gameObject.transform.rotation) * Quaternion.LookRotation(ActivePosToHead);
		m_ToTransionScale = m_ActiveUITransform.localScale;

		m_FromTransionPos = m_DUI.transform.localPosition;
		m_FromTransionRot = m_DUI.transform.localRotation;
		m_FromTransionScale = m_DUI.transform.localScale;
	}
	
	private void DeactivateDUI()
	{
		m_Deactivating = true;
		m_TransitionTimer = 0.0f;

		m_ToTransionPos = m_InactiveUITransform.localPosition;
		m_ToTransionRot = m_InactiveUITransform.localRotation;
		m_ToTransionScale = m_InactiveUITransform.localScale;

		m_FromTransionPos = m_DUI.transform.localPosition;
		m_FromTransionRot = m_DUI.transform.localRotation;
		m_FromTransionScale = m_DUI.transform.localScale;
	}
	
	private void UpdateDUITransform()
	{
		if(m_Activating || m_Deactivating)
		{
			float lerpValue = m_TransitionTimer/m_UITransitionTime;

			m_DUI.transform.localPosition = Vector3.Lerp(m_FromTransionPos, m_ToTransionPos, lerpValue);
			m_DUI.transform.localRotation = Quaternion.Slerp(m_FromTransionRot, m_ToTransionRot, lerpValue);
			m_DUI.transform.localScale = Vector3.Lerp(m_FromTransionScale, m_ToTransionScale, lerpValue);
		}
		else if(IsDUIActive)
		{
			m_DUI.transform.localPosition = m_ToTransionPos + m_ActivatedPositionOffset;

			Vector3 duiToHead = (m_DUI.transform.position - m_ToolInterface.OwnerPlayerActor.GetComponent<CPlayerHead>().ActorHead.transform.position).normalized;
			m_DUI.transform.rotation = Quaternion.LookRotation(duiToHead);
		}
	}

	[AServerOnly]
	private void OnPrimaryStart(GameObject _InteractableObject)
	{
		if(_InteractableObject != null)
		{
			// Only conserned with selecting module ports
			CModulePortInterface mpi = _InteractableObject.GetComponent<CModulePortInterface>();
			if(mpi != null)
			{

			}
		}
	}

	[AServerOnly]
	private void OnSecondaryStart(GameObject _InteractableObject)
	{
		m_DUIActive.Set(!m_DUIActive.Get());
	}

	private void OnMouseMoveX(float _Delta)
	{
		if(IsDUIActive && !m_Deactivating && ! m_Activating)
		{
			m_ActivatedPositionOffset.x -= _Delta * 0.01f;
			m_ActivatedPositionOffset.x = Mathf.Clamp(m_ActivatedPositionOffset.x, -0.8f, 0.8f);
		}
	}

	private void OnMouseMoveY(float _Delta)
	{
		if(IsDUIActive && !m_Deactivating && ! m_Activating)
		{
			m_ActivatedPositionOffset.y += _Delta * 0.01f;
			m_ActivatedPositionOffset.y = Mathf.Clamp(m_ActivatedPositionOffset.y, -0.5f, 0.5f);
		}
	}

	private void OnDestory()
	{
		// Unregister mouse movement events
		CUserInput.EventMouseMoveX -= OnMouseMoveX;
		CUserInput.EventMouseMoveY -= OnMouseMoveY;
	}
};
