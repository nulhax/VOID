//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUIPanelTweener.cs
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
public class CDUIPanelTransitioner : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public CDUIPanel m_StartingPanel = null;

	private CNetworkVar<TNetworkViewId> m_ActivePanelId = null;

	
	// Member Properties
	public GameObject ActivePanel
	{
		get { return(CNetwork.Factory.FindGameObject(m_ActivePanelId.Get())); }
	}

	public GameObject PreviouslyActivePanel
	{
		get { return(CNetwork.Factory.FindGameObject(m_ActivePanelId.GetPrevious())); }
	}
	
	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		m_ActivePanelId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		if(_SyncedNetworkVar == m_ActivePanelId)
		{
			UpdatePanels();
		}
	}

	public void Start()
	{
		// Start on beginning panel
		if(CNetwork.IsServer && m_StartingPanel != null)
			SwitchToPanel(m_StartingPanel);
	}

	[AServerOnly]
	public void SwitchToPanel(CDUIPanel _Panel)
	{
		CNetworkView nv = _Panel.GetComponent<CNetworkView>();

		if(nv == null)
			Debug.LogError("CNetworkView was not found in panel");
		else
			m_ActivePanelId.Set(nv.ViewId);
	}

	private void UpdatePanels()
	{
		if(m_ActivePanelId.GetPrevious() != null)
		{
			// Register the transition out handler
			CDUIPanel panel = PreviouslyActivePanel.GetComponent<CDUIPanel>();
			panel.EventTransitionOutFinished += PanelFinisehdTranstionOut;

			// Transition this panel out
			panel.TransitionOut();
		}
		else
		{
			// Set active and transition the current panel in
			CDUIPanel panel = ActivePanel.GetComponent<CDUIPanel>();
			panel.EventTransitionInFinished += PanelFinishedTranstionIn;

			// Transition this panel in
			panel.TransitionIn();
		}
	}

	private void PanelFinisehdTranstionOut(GameObject _Panel)
	{
		// Set inactive and Unregister the transition out handler
		CDUIPanel panel = _Panel.GetComponent<CDUIPanel>();
		panel.EventTransitionOutFinished -= PanelFinisehdTranstionOut;

		// Set active and transition the current panel in
		panel = ActivePanel.GetComponent<CDUIPanel>();
		panel.EventTransitionInFinished += PanelFinishedTranstionIn;
		
		// Transition this panel in
		panel.TransitionIn();
	}

	private void PanelFinishedTranstionIn(GameObject _Panel)
	{
		// Unregister the transition in handler
		CDUIPanel panel = _Panel.GetComponent<CDUIPanel>();
		panel.EventTransitionInFinished -= PanelFinishedTranstionIn;
	}
}
