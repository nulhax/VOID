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
	public CHUDLocator m_ActiveToolTip = null;

	
	// Member Properties
	
	
	// Member Methods
	private void Start()
	{
		CGamePlayers.SelfActor.GetComponent<CPlayerInteractor>().EventTargetChange += OnTargetChange;
	}
	
	private void OnTargetChange(GameObject _cOldTargetObject,  GameObject _CNewTargetObject, RaycastHit _cRaycastHit)
	{
		if(_CNewTargetObject == null)
		{
			// Disable the tooltip
			m_ActiveToolTip.gameObject.SetActive(false);

			// Delete the old tooltip
			if(m_ActiveToolTip.m_WithinBoundsIcon != null)
			{
				Destroy(m_ActiveToolTip.m_WithinBoundsIcon);
				m_ActiveToolTip.m_WithinBoundsIcon = null;
			}
		}
		else
		{
			// Check if the target is a component
			CActorInteractable ai = _CNewTargetObject.GetComponent<CActorInteractable>();

			if(ai.m_ToolTipPrefab != null)
			{
				// Activate the tooltip
				m_ActiveToolTip.gameObject.SetActive(true);
				m_ActiveToolTip.Target = _CNewTargetObject.transform;
				m_ActiveToolTip.m_WithinBoundsIcon = (GameObject)GameObject.Instantiate(ai.m_ToolTipPrefab);
				m_ActiveToolTip.m_WithinBoundsIcon.transform.parent = m_ActiveToolTip.transform;
				m_ActiveToolTip.m_WithinBoundsIcon.transform.localPosition = Vector3.zero;
				m_ActiveToolTip.m_WithinBoundsIcon.transform.localScale = Vector3.one;
				m_ActiveToolTip.m_WithinBoundsIcon.transform.localRotation = Quaternion.identity;
			}
			else
			{
				// Delete the old tooltip
				Destroy(m_ActiveToolTip.m_WithinBoundsIcon);
				m_ActiveToolTip.m_WithinBoundsIcon = null;
			}
		}
	}

	private void UpdateComponentToolTip(CComponentInterface _ComponentInterface)
	{
		// Activate the component tooltip
		m_ActiveToolTip.gameObject.SetActive(true);
		m_ActiveToolTip.Target = _ComponentInterface.transform;

		// Disable the old tooltip
		if(m_ActiveToolTip.m_WithinBoundsIcon != null)
		{
			m_ActiveToolTip.m_WithinBoundsIcon.SetActive(false);
		}
		
		// Select the tracker icon to use
		switch (_ComponentInterface.ComponentType) 
		{
		case CComponentInterface.EType.CalibratorComp:
			m_ActiveToolTip.m_WithinBoundsIcon = m_ActiveToolTip.transform.FindChild("Calibration").gameObject;
			m_ActiveToolTip.m_WithinBoundsIcon.SetActive(true);
			break;

		case CComponentInterface.EType.MechanicalComp:
			m_ActiveToolTip.m_WithinBoundsIcon = m_ActiveToolTip.transform.FindChild("Mechanical").gameObject;
			m_ActiveToolTip.m_WithinBoundsIcon.SetActive(true);
			break;

		case CComponentInterface.EType.CircuitryComp:
			m_ActiveToolTip.m_WithinBoundsIcon = m_ActiveToolTip.transform.FindChild("Circuitry").gameObject;
			m_ActiveToolTip.m_WithinBoundsIcon.SetActive(true);
			break;

		case CComponentInterface.EType.FluidComp:
			m_ActiveToolTip.m_WithinBoundsIcon = m_ActiveToolTip.transform.FindChild("Fluids").gameObject;
			m_ActiveToolTip.m_WithinBoundsIcon.SetActive(true);
			break;
			
		default:
			break;
		}
	}
}
