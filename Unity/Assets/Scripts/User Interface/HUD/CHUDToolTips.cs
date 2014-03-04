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
	}
	
	private void OnTargetChange(GameObject _cOldTargetObject,  GameObject _CNewTargetObject, RaycastHit _cRaycastHit)
	{
		if(_CNewTargetObject == null)
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
		else
		{
			// Check if the target has a tooltip component
			CActorInteractable ai = _CNewTargetObject.GetComponent<CActorInteractable>();

			if(ai.m_ToolTipPrefab != null)
			{
				// Instantiate the tooltip
				GameObject newTooltip = (GameObject)GameObject.Instantiate(ai.m_ToolTipPrefab);
				newTooltip.transform.parent = m_ActiveToolTip.transform;
				newTooltip.transform.localPosition = Vector3.zero;
				newTooltip.transform.localRotation = Quaternion.identity;
				newTooltip.transform.localScale = Vector3.one;

				// Activate the tooltip
				m_ActiveToolTip.gameObject.SetActive(true);
				m_ActiveToolTip.Target = _CNewTargetObject.transform;
				m_ActiveToolTip.m_ToolTip = newTooltip;
			}
			else
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
		}
	}
}
