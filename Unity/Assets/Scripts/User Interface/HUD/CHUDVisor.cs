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
	private ParticleSystem m_particleEmitter = null;


	// Member Properties
	public bool IsVisorDown
	{
		get { return(m_VisorDown); }
	}
	

	// Member Methods
	public void Start()
	{
		CUserInput.SubscribeInputChange(CUserInput.EInput.Visor, OnEventInput);
		m_particleEmitter = transform.FindChild("Gas Emission").particleSystem;
	}

	public void SetVisorState(bool _Down)
	{
		if(_Down && !m_VisorDown)
		{
			animation.CrossFade("VisorDown");
			GetComponent<CAudioCue>().Play(1.0f,false,0);

 		}
		else if(!_Down && m_VisorDown)
		{
			animation.CrossFade("VisorUp");
			GetComponent<CAudioCue>().Play(1.0f,false,1);
			m_particleEmitter.Play();
		}

		m_VisorDown = _Down;
	}

	[ALocalOnly]
	private void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
	{
		// Toggle between up/down visor
		if(_eInput == CUserInput.EInput.Visor && _bDown)
		{
			SetVisorState(!m_VisorDown);
		}
	}

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
