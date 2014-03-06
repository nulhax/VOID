//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIRoot2D.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CDUIPanel : CNetworkMonoBehaviour 
{
	// Member Types
	public enum ETransitionType
	{
		Fade,
	}
	
	// Member Delegates & Events
	public delegate void HandleTransitionEvent(GameObject _Self);
	
	public event HandleTransitionEvent EventTransitionInFinished = null;
	public event HandleTransitionEvent EventTransitionOutFinished = null;


	// Member Fields
	public ETransitionType m_TransitionIn = ETransitionType.Fade;
	public ETransitionType m_TransitionOut = ETransitionType.Fade;

	public float m_TransitionInTime = 1.0f;
	public float m_TransitionOutTime = 1.0f;

	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		
	}

	public void Awake()
	{
		if(m_TransitionIn == ETransitionType.Fade || m_TransitionOut == ETransitionType.Fade)
		{
			CDUIPanelFader pf = gameObject.AddComponent<CDUIPanelFader>();
			pf.EventFadeInFinished += OnTransitionInFinish;
			pf.EventFadeOutFinished += OnTransitionOutFinish;
		}
	}

	public void TransitionOut()
	{
		if(m_TransitionIn == ETransitionType.Fade || m_TransitionOut == ETransitionType.Fade)
		{
			CDUIPanelFader pf = gameObject.GetComponent<CDUIPanelFader>();
			pf.FadeOut(m_TransitionOutTime);
		}
	}

	public void TransitionIn()
	{
		if(m_TransitionIn == ETransitionType.Fade || m_TransitionOut == ETransitionType.Fade)
		{
			CDUIPanelFader pf = gameObject.GetComponent<CDUIPanelFader>();
			pf.FadeIn(m_TransitionInTime);
		}
	}

	private void OnTransitionOutFinish()
	{
		if(EventTransitionOutFinished != null)
			EventTransitionOutFinished(gameObject);
	}

	private void OnTransitionInFinish()
	{
		if(EventTransitionInFinished != null)
			EventTransitionInFinished(gameObject);
	}
}
