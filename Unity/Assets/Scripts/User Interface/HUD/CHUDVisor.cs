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


public class CHUDVisor : MonoBehaviour
{
	// Member Types
	
	
	// Member Delegates & Events
	public delegate void NotifyVisorHUDState();

	public event NotifyVisorHUDState EventVisorHUDActivated = null;
	public event NotifyVisorHUDState EventVisorHUDDeactivated = null;


	// Member Fields
	private bool m_VisorDown = false;


	// Member Properties
	public bool IsVisorDown
	{
		get { return(m_VisorDown); }
	}
	

	// Member Methods
	public void Start()
	{
		//CUserInput.SubscribeInputChange(CUserInput.EInput.Visor, OnEventInput);
	}

	public void SetVisorState(bool _Down)
	{
		if(_Down && !m_VisorDown)
		{
			animation.CrossFade("VisorDown");
 		}
		else if(!_Down && m_VisorDown)
		{
			animation.CrossFade("VisorUp");
		}

		m_VisorDown = _Down;
	}
//
//	[ALocalOnly]
//	private void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
//	{
//		// Toggle between up/down visor
//		if(_eInput == CUserInput.EInput.Visor && _bDown)
//		{
//			SetVisorState(!m_VisorDown);
//		}
//	}

	private void ActivateHUD()
	{
		if(EventVisorHUDActivated != null)
			EventVisorHUDActivated();
	}

	private void DeactivateHUD()
	{
		if(EventVisorHUDDeactivated != null)
			EventVisorHUDDeactivated();
	}
}
