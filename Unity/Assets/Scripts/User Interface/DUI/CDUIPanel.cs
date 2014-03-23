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
		Instant,
		Fade,
	}
	
	// Member Delegates & Events
	public delegate void HandleTransitionEvent(GameObject _Self);
	
	public event HandleTransitionEvent EventTransitionInFinished = null;
	public event HandleTransitionEvent EventTransitionOutFinished = null;


	// Member Fields
	public ETransitionType m_Transition = ETransitionType.Fade;

	public float m_TransitionTime = 1.0f;

	private CDUIPanelTransitioner m_PanelTransitioner = null;
	private UITweener m_TransitionTweener = null;

	private EventDelegate m_OnTransitionOutFinish = null;
	private EventDelegate m_OnTransitionInFinish = null;


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
		if(m_Transition == ETransitionType.Fade)
		{
			// Add an alpha tweener to the panel
			m_TransitionTweener = gameObject.AddComponent<TweenAlpha>();
			m_TransitionTweener.duration = m_TransitionTime;
			m_TransitionTweener.enabled = false;
		}

		m_PanelTransitioner = CUtility.FindInParents<CDUIPanelTransitioner>(gameObject);
		m_OnTransitionOutFinish = new EventDelegate(OnTransitionOutFinish);
		m_OnTransitionInFinish = new EventDelegate(OnTransitionInFinish);
	}

	public void TransitionOut()
	{
		if(m_Transition == ETransitionType.Instant)
		{
			gameObject.GetComponent<UIPanel>().alpha = 0.0f;
			OnTransitionOutFinish();
		}
		else if(m_Transition == ETransitionType.Fade)
		{
			TweenAlpha tween = m_TransitionTweener as TweenAlpha;
			tween.SetStartToCurrentValue();
			tween.to = 0.0f;

			m_TransitionTweener.AddOnFinished(m_OnTransitionOutFinish);
			m_TransitionTweener.enabled = true;
		}
	}

	public void TransitionIn()
	{
		if(m_Transition == ETransitionType.Instant)
		{
			gameObject.GetComponent<UIPanel>().alpha = 1.0f;
			OnTransitionInFinish();
		}
		else if(m_Transition == ETransitionType.Fade)
		{
			TweenAlpha tween = m_TransitionTweener as TweenAlpha;
			tween.SetStartToCurrentValue();
			tween.to = 1.0f;

			m_TransitionTweener.AddOnFinished(m_OnTransitionInFinish);
			m_TransitionTweener.enabled = true;
		}
	}

	private void OnTransitionOutFinish()
	{
		if(EventTransitionOutFinished != null)
			EventTransitionOutFinished(gameObject);

		if(m_TransitionTweener != null)
		{
			m_TransitionTweener.RemoveOnFinished(m_OnTransitionOutFinish);
		}
	}

	private void OnTransitionInFinish()
	{
		if(EventTransitionInFinished != null)
			EventTransitionInFinished(gameObject);

		if(m_TransitionTweener != null)
		{
			m_TransitionTweener.RemoveOnFinished(m_OnTransitionInFinish);
		}
	}
}
