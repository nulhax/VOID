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

	private static string k_UpAnimation = "VisorUp";
	private static string k_DownAnimation = "VisorDown";


	// Member Properties
	public bool IsVisorDown
	{
		get { return(m_VisorDown); }
	}
	

	// Member Methods
	public void Start()
	{
		//CUserInput.SubscribeInputChange(CUserInput.EInput.Visor, OnEventInput);
		m_particleEmitter = transform.FindChild("Gas Emission").particleSystem;

		SetVisorState(false);
	}

	public void SetVisorState(bool _Down)
	{
		if(!_Down)
		{
			if(animation.IsPlaying(k_UpAnimation))
				return;

			float startTime = animation.IsPlaying(k_DownAnimation) ? 
				animation[k_UpAnimation].length - animation[k_DownAnimation].time : 0.0f;
			
			animation.Play(k_UpAnimation, PlayMode.StopSameLayer);
			animation[k_UpAnimation].time = startTime;

			CAudioSystem.Instance.SetOccludeAll(false);

			//Play audio...
			//GetComponent<CAudioCue>().Play(1.0f, false, 1);

 		}
		else
		{
			if(animation.IsPlaying(k_DownAnimation))
				return;
			
			float startTime = animation.IsPlaying(k_UpAnimation) ? 
				animation[k_DownAnimation].length - animation[k_UpAnimation].time : 0.0f;
			
			animation.Play(k_DownAnimation, PlayMode.StopSameLayer);
			animation[k_DownAnimation].time = startTime;

			CAudioSystem.Instance.SetOccludeAll(true);

			GetComponent<CAudioCue>().Play(1.0f, false, 0);
			m_particleEmitter.Play();
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
