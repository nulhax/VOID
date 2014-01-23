//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIButtonActivator.cs
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


[RequireComponent(typeof(CDUIPanel))]
public class CDUIPanelFader : MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	public delegate void HandleFadeEvent();

	public event HandleFadeEvent EventFadeInFinished = null;
	public event HandleFadeEvent EventFadeOutFinished = null;
	
	// Member Fields
	private float m_FadeTime = 0.0f;
	private float m_FadeTimer = 0.0f;

	private bool m_FadingIn = false;
	private bool m_FadingOut = false;


	// Member Properties
	public float CurrentAlpha
	{
		get { return(m_FadeTimer/m_FadeTime); }
	}

	
	// Member Methods
	public void Update()
	{
		if(!m_FadingIn && !m_FadingOut)
			return;

		if(m_FadingIn)
		{
			m_FadeTimer += Time.deltaTime;
		}
		else if(m_FadingOut)
		{
			m_FadeTimer -= Time.deltaTime;
		}

		if(m_FadeTimer > m_FadeTime)
		{
			m_FadeTimer = m_FadeTime;
		}
		else if(m_FadeTimer < 0)
		{
			m_FadeTimer = 0.0f;
		}

		UpdatePanelAlpha();

		if(CurrentAlpha == 0.0f)
		{
			m_FadingOut = false;

			if(EventFadeOutFinished != null)
				EventFadeOutFinished();
		}
		else if(CurrentAlpha == 1.0f)
		{
			m_FadingIn = false;

			if(EventFadeInFinished != null)
				EventFadeInFinished();
		}
	}

	public void FadeIn(float _FadeTime)
	{
		m_FadingIn = true;
		m_FadeTime = _FadeTime;
		m_FadeTimer = 0.0f;

		UpdatePanelAlpha();
	}

	public void FadeOut(float _FadeTime)
	{
		m_FadingOut = true;
		m_FadeTime = _FadeTime;
		m_FadeTimer = m_FadeTime;

		UpdatePanelAlpha();
	}

	private void UpdatePanelAlpha()
	{
		// Set the panel alpha
		gameObject.GetComponent<UIPanel>().alpha = CurrentAlpha;
	}
}
