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
	public Transform m_ActiveUIPosition = null;
	public Transform m_InactiveUIPosition = null;

	public GameObject m_DUI = null;

	public float m_UITransitionTime = 0.5f;
	private float m_TransitionTimer = 0.0f;

	private bool m_Activating = false;
	private bool m_Deactivating = false;

	private CNetworkVar<bool> m_Active = null;


	// Member Properties
	
	
	// Member Functions
	
	
	public override void InstanceNetworkVars()
	{
		m_Active = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_Active)
		{
			if (m_Active.Get())
			{
				TransitionIn();
			}
			else
			{
				TransitionOut();
			}
		}
	}
	
	
	public void Start()
	{
		CToolInterface ti = gameObject.GetComponent<CToolInterface>();

		ti.EventSecondaryActivate += OnSecondaryStart;
		ti.EventSecondaryDeactivate += OnSecondaryEnd;
	}
	

	public void Update()
	{
		if(!m_Activating && !m_Deactivating && gameObject.GetComponent<CToolInterface>().IsHeld)
			return;

		bool finished = false;

		if(m_Activating)
		{
			m_TransitionTimer += Time.deltaTime;
		}
		else if(m_Deactivating)
		{
			m_TransitionTimer -= Time.deltaTime;
		}
		
		if(m_TransitionTimer > m_UITransitionTime)
		{
			m_TransitionTimer = m_UITransitionTime;
			finished = true;
		}
		else if(m_TransitionTimer < 0)
		{
			m_TransitionTimer = 0.0f;
			finished = true;
		}
		
		UpdateUITransform();
		
		if(finished)
		{
			if(m_Activating) 
			{
				m_Activating = false;

//				if(EventFadeOutFinished != null)
//					EventFadeOutFinished();
			}
			if(m_Deactivating) 
			{
				m_Deactivating = false;

//				if(EventFadeOutFinished != null)
//					EventFadeOutFinished();
			}
		}
	}
	
	private void TransitionIn()
	{
		m_Activating = true;
		m_TransitionTimer = 0.0f;
		
		UpdateUITransform();
	}
	
	private void TransitionOut()
	{
		m_Deactivating = true;
		m_TransitionTimer = m_UITransitionTime;
		
		UpdateUITransform();
	}
	
	private void UpdateUITransform()
	{
		m_DUI.transform.position = Vector3.Lerp(m_InactiveUIPosition.position, m_ActiveUIPosition.position, m_TransitionTimer);
		m_DUI.transform.rotation = Quaternion.Slerp(m_InactiveUIPosition.rotation, m_ActiveUIPosition.rotation, m_TransitionTimer);
		m_DUI.transform.localScale = Vector3.Lerp(m_InactiveUIPosition.localScale, m_ActiveUIPosition.localScale, m_TransitionTimer);
	}














	
	[AServerOnly]
	public void OnSecondaryStart(GameObject _cInteractableObject)
	{
		m_Active.Set(true);
	}
	
	
	[AServerOnly]
	public void OnSecondaryEnd()
	{
		m_Active.Set(false);
	}
};
