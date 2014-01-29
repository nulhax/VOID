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
	private CDUIModuleCreationRoot m_DUIModuleCreationRoot = null;

	private float m_TransitionTimer = 0.0f;

	private Vector3 m_ToTransionPos = Vector3.zero;
	private Quaternion m_ToTransionRot = Quaternion.identity;
	private Vector3 m_ToTransionScale = Vector3.zero;

	private Vector3 m_FromTransionPos = Vector3.zero;
	private Quaternion m_FromTransionRot = Quaternion.identity;
	private Vector3 m_FromTransionScale = Vector3.zero;

	private Vector3 m_ActivatedPosition = Vector3.zero;
	
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

		// Register DUI events
		m_DUIModuleCreationRoot = m_DUI.GetComponent<CDUIConsole>().DUI.GetComponent<CDUIModuleCreationRoot>();
		m_DUIModuleCreationRoot.EventBuildModuleButtonPressed += OnDUIBuildButtonPressed;

		// Configure DUI
		m_DUI.transform.position = m_InactiveUITransform.position;
		m_DUI.transform.rotation = m_InactiveUITransform.rotation;
		m_DUI.transform.localScale = m_InactiveUITransform.localScale;
	}

	public void Update()
	{
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

				m_ActivatedPosition = m_DUI.transform.position;
			}
			if(m_Deactivating)
			{
				m_Deactivating = false;
			}
		}
	}
	
	private void ActivateDUI()
	{
		// Register mouse movement events
		CUserInput.EventMoveForwardHold += OnPlayerMovement;
		CUserInput.EventMoveBackwardHold += OnPlayerMovement;
		CUserInput.EventMoveLeftHold += OnPlayerMovement;
		CUserInput.EventMoveRightHold += OnPlayerMovement;
		CUserInput.EventMoveJumpHold += OnPlayerMovement;

		m_Activating = true;
		m_TransitionTimer = 0.0f;
		m_ActivatedPosition = Vector3.zero;

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
		// Unegister mouse movement events
		CUserInput.EventMoveForwardHold -= OnPlayerMovement;
		CUserInput.EventMoveBackwardHold -= OnPlayerMovement;
		CUserInput.EventMoveLeftHold -= OnPlayerMovement;
		CUserInput.EventMoveRightHold -= OnPlayerMovement;
		CUserInput.EventMoveJumpHold -= OnPlayerMovement;

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
			m_DUI.transform.position = m_ActivatedPosition;

			Vector3 duiToHead = (m_DUI.transform.position - m_ToolInterface.OwnerPlayerActor.GetComponent<CPlayerHead>().ActorHead.transform.position).normalized;
			m_DUI.transform.rotation = Quaternion.LookRotation(duiToHead);
		}
	}

	[AServerOnly]
	private void OnPrimaryStart(GameObject _InteractableObject)
	{
		if(_InteractableObject != null && !IsDUIActive)
		{
			// Only conserned with selecting module ports
			CModulePortInterface mpi = _InteractableObject.GetComponent<CModulePortInterface>();
			if(mpi != null)
			{
				// Make the UI active
				m_DUIActive.Set(true);

				// Change the port selected on the UI
				m_DUIModuleCreationRoot.SetSelectedPort(_InteractableObject.GetComponent<CNetworkView>().ViewId);
			}
		}
	}

	[AClientOnly]
	private void OnPlayerMovement()
	{
		m_DUIActive.Set(false);
	}

	[AServerOnly]
	private void OnSecondaryStart(GameObject _InteractableObject)
	{
		m_DUIActive.Set(false);
	}

	[AServerOnly]
	private void OnDUIBuildButtonPressed()
	{
		CModulePortInterface currentPort = m_DUIModuleCreationRoot.CurrentPortSelected.GetComponent<CModulePortInterface>();

		// Debug: Create the module instantly
		currentPort.CreateModule(m_DUIModuleCreationRoot.SelectedModuleType);

		// Deactivate the UI
		m_DUIActive.Set(false);
	}

	private void OnDestory()
	{
		// Unegister mouse movement events
		CUserInput.EventMoveForwardHold -= OnPlayerMovement;
		CUserInput.EventMoveBackwardHold -= OnPlayerMovement;
		CUserInput.EventMoveLeftHold -= OnPlayerMovement;
		CUserInput.EventMoveRightHold -= OnPlayerMovement;
		CUserInput.EventMoveJumpHold -= OnPlayerMovement;
	}
};
