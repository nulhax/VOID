//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CHUDRoot.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CHUDToolTips : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CHUDToolTip m_ActiveToolTip = null;

	
	// Member Properties
	
	
	// Member Methods
	private void Start()
	{
		// Register to target change events
		CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().EventTargetChange += OnTargetChange;

		// Disable active tooltip
		m_ActiveToolTip.gameObject.SetActive(false);
	}

	private void Update()
	{
		// Update the scale of the current tooltip
		if(m_ActiveToolTip.gameObject.activeSelf)
		{
			float maxRange = CPlayerInteractor.s_InteractionRange;
			float distance = CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().TargetRaycastHit.distance;

			Vector3 newScale = Vector3.one * (1.0f - (distance / maxRange));
			m_ActiveToolTip.m_ToolTip.transform.localScale = newScale;
		}
	}
	
	private void OnTargetChange(GameObject _cOldTargetObject,  GameObject _CNewTargetObject, RaycastHit _cRaycastHit)
	{
		bool destoryActiveTooltip = false;
		bool createNewTooltip = false;

		if(_CNewTargetObject != null && _cRaycastHit.collider != null)
		{
			// Check if the target has a tooltip component
			GameObject colliderGameObject = _cRaycastHit.collider.gameObject;
			CActorToolTip att = colliderGameObject.GetComponent<CActorToolTip>();
			if(att != null && att.m_ToolTipPrefab != null)
			{
				createNewTooltip = true;
			}

			destoryActiveTooltip = true;
		}
		else
		{
			destoryActiveTooltip = true;
		}

		if(destoryActiveTooltip)
		{
			// Destroy the old tooltip
			if(m_ActiveToolTip.m_ToolTip != null)
			{
				Destroy(m_ActiveToolTip.m_ToolTip);
				m_ActiveToolTip.m_ToolTip = null;
			}
			
			// Disable the tooltip
			m_ActiveToolTip.gameObject.SetActive(false);
		}

		if(createNewTooltip)
		{
			GameObject colliderGameObject = _cRaycastHit.collider.gameObject;
			CActorToolTip att = colliderGameObject.GetComponent<CActorToolTip>();

			// Instantiate the tooltip
			GameObject newTooltip = (GameObject)GameObject.Instantiate(att.m_ToolTipPrefab);
			newTooltip.transform.parent = m_ActiveToolTip.transform;
			newTooltip.transform.localPosition = Vector3.zero;
			newTooltip.transform.localRotation = Quaternion.identity;
			newTooltip.transform.localScale = Vector3.one;
			
			// Activate the tooltip
			m_ActiveToolTip.gameObject.SetActive(true);
			m_ActiveToolTip.Target = att.m_ToolTipPosition != null ? att.m_ToolTipPosition : colliderGameObject.transform;
			m_ActiveToolTip.m_ToolTip = newTooltip;
		}
	}
}
