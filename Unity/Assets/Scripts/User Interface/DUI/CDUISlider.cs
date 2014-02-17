//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDUISlider.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


[RequireComponent(typeof(CDUIElement))]
[RequireComponent(typeof(UIProgressBar))]
public class CDUISlider : CNetworkMonoBehaviour 
{
	// Member Types
	public enum ESliderNotificationType : byte
	{
		INVALID,
		
		OnValueChange,
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	private bool m_IsModifyingValueLocally = false;
	private bool m_IsDirty = false;

	private CNetworkVar<float> m_Value = null;
	private UIProgressBar m_CachedProgressBar = null;

	private float m_TimeSinceModification = 0.0f;
	private float m_WaitTillLocalUpdateTime = 1.0f;

	static private CNetworkStream s_SliderNotificationStream = new CNetworkStream();
	
	
	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_Value = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	private void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if(_cSyncedNetworkVar == m_Value)
		{
			m_IsDirty = true;
		}
	}

	[AClientOnly]
	static public void SerializeSliderEvents(CNetworkStream _cStream)
	{
		_cStream.Write(s_SliderNotificationStream);
		s_SliderNotificationStream.Clear();
	}
	
	[AServerOnly]
	static public void UnserializeSliderEvents(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		while(_cStream.HasUnreadData)
		{
			// Get the DUISlider and its network view
			CDUISlider duiSlider = CNetwork.Factory.FindObject(_cStream.ReadNetworkViewId()).GetComponent<CDUISlider>();
			
			// Get the interaction notification
			ESliderNotificationType notification = (ESliderNotificationType)_cStream.ReadByte();
			
			// Based on the notification type, update the clients of the event
			switch(notification) 
			{
			case ESliderNotificationType.OnValueChange:
				float value = _cStream.ReadFloat();
				duiSlider.SetSliderValue(value);
				break;
				
			default:break;
			}
		}
	}

	public void Start()
	{
		m_CachedProgressBar = gameObject.GetComponent<UIProgressBar>();
	}
	
	public void Update()
	{
		if(m_IsModifyingValueLocally)
		{
			m_TimeSinceModification += Time.deltaTime;
			m_IsDirty = false;

			if(m_TimeSinceModification > m_WaitTillLocalUpdateTime)
			{
				m_IsModifyingValueLocally = false;
			}
		}

		// If is not modifying locally and value is dirty, update it
		if(!m_IsModifyingValueLocally && m_IsDirty)
		{
			UpdateLocalBarValue();
		}
	}
	
	public void HandleValueChange() 
	{
		if(CNetwork.IsServer)
		{
			// Server updates the network var
			SetSliderValue(UIProgressBar.current.value);
		}
		else
		{
			// Only add to the stream when it is empty
			if(!s_SliderNotificationStream.HasUnreadData && !m_IsDirty)
			{
				// Serialise the event to the server
				s_SliderNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
				s_SliderNotificationStream.Write((byte)ESliderNotificationType.OnValueChange);
				s_SliderNotificationStream.Write(UIProgressBar.current.value);
				
				m_IsModifyingValueLocally = true;
				m_TimeSinceModification = 0.0f;
			}
		}
	}

	[AServerOnly]
	protected void SetSliderValue(float _Value)
	{
		m_Value.Set(_Value);
	}
	
	protected void UpdateLocalBarValue()
	{
		if(!Mathf.Approximately(m_CachedProgressBar.value, m_Value.Get()))
			m_CachedProgressBar.value = m_Value.Get();
		
		m_IsDirty = false;
	}
}
