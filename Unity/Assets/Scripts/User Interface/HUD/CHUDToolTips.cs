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
	public CHUDLocator m_ComponentToolTip = null;

	
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
			// Disable all tooltips
			m_ComponentToolTip.gameObject.SetActive(false);
		}
		else
		{
			// Check if the target is a component
			CComponentInterface ci = _CNewTargetObject.GetComponent<CComponentInterface>();
			if(ci != null)
			{
				UpdateComponentToolTip(ci);
			}
			else
			{
				// Disable the componet tooltip
				m_ComponentToolTip.gameObject.SetActive(false);
			}
		}
	}

	private void UpdateComponentToolTip(CComponentInterface _ComponentInterface)
	{
		// Activate the component tooltip
		m_ComponentToolTip.gameObject.SetActive(true);
		m_ComponentToolTip.Target = _ComponentInterface.transform;

		// Disable the old tooltip
		if(m_ComponentToolTip.m_WithinBoundsIcon != null)
		{
			m_ComponentToolTip.m_WithinBoundsIcon.SetActive(false);
		}
		
		// Select the tracker icon to use
		switch (_ComponentInterface.ComponentType) 
		{
		case CComponentInterface.EType.CalibratorComp:
			m_ComponentToolTip.m_WithinBoundsIcon = m_ComponentToolTip.transform.FindChild("Calibration").gameObject;
			m_ComponentToolTip.m_WithinBoundsIcon.SetActive(true);
			break;

		case CComponentInterface.EType.MechanicalComp:
			m_ComponentToolTip.m_WithinBoundsIcon = m_ComponentToolTip.transform.FindChild("Mechanical").gameObject;
			m_ComponentToolTip.m_WithinBoundsIcon.SetActive(true);
			break;

		case CComponentInterface.EType.CircuitryComp:
			m_ComponentToolTip.m_WithinBoundsIcon = m_ComponentToolTip.transform.FindChild("Circuitry").gameObject;
			m_ComponentToolTip.m_WithinBoundsIcon.SetActive(true);
			break;

		case CComponentInterface.EType.FluidComp:
			m_ComponentToolTip.m_WithinBoundsIcon = m_ComponentToolTip.transform.FindChild("Fluids").gameObject;
			m_ComponentToolTip.m_WithinBoundsIcon.SetActive(true);
			break;
			
		default:
			break;
		}
	}
}
