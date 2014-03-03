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
		get { return(m_FadeTimer/m_FadeTime ); }
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

		UpdatePanelAlpha(CurrentAlpha);
	}

	public void FadeIn(float _FadeTime)
	{
		if(_FadeTime == 0.0f)
		{
			UpdatePanelAlpha(1.0f);
			return;
		}

		m_FadingIn = true;
		m_FadeTime = _FadeTime;
		m_FadeTimer = 0.0f;

		UpdatePanelAlpha(CurrentAlpha);
	}

	public void FadeOut(float _FadeTime)
	{
		if(_FadeTime == 0.0f)
		{
			UpdatePanelAlpha(0.0f);
			return;
		}

		m_FadingOut = true;
		m_FadeTime = _FadeTime;
		m_FadeTimer = m_FadeTime;

		UpdatePanelAlpha(CurrentAlpha);
	}

	private void UpdatePanelAlpha(float _Alpha)
	{
		// Set the panel alpha
		gameObject.GetComponent<UIPanel>().alpha = _Alpha;

		if(_Alpha == 0.0f)
		{
			m_FadingOut = false;

			if(EventFadeOutFinished != null)
				EventFadeOutFinished();
		}
		else if(_Alpha == 1.0f)
		{
			m_FadingIn = false;
			
			if(EventFadeInFinished != null)
				EventFadeInFinished();
		}
	}
}
