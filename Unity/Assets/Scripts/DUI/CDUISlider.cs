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
using System.Reflection;
using System.Linq;
using System;


/* Implementation */

[RequireComponent(typeof(UIProgressBar))]
[RequireComponent(typeof(CDUIElement))]
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
	private CNetworkVar<float> m_Value = null;
	private bool m_IsModifyingValueLocally = false;
	private bool m_IgnoreLocalValueChange = false;
	
	private float m_TimeSinceModification = 0.0f;
	private float m_WaitTillLocalUpdateTime = 1.0f;
	
	private float m_TimeSinceIgnoreValueChange = 0.0f;
	private float m_WaitTillValueHandle = 0.5f;
	
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
			// Other clients should update the value immediately
			if(!m_IsModifyingValueLocally)
				UpdateLocalSliderValue(m_Value.Get());
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
				duiSlider.SetValue(value);
				break;
				
			default:break;
			}
		}
	}
	
	public void Update()
	{
		if(m_IsModifyingValueLocally)
			m_TimeSinceModification += Time.deltaTime;
		
		if(m_TimeSinceModification > m_WaitTillLocalUpdateTime)
		{
			m_IsModifyingValueLocally = false;
		}
		
		if(m_IgnoreLocalValueChange)
			m_TimeSinceIgnoreValueChange += Time.deltaTime;
		
		if(m_TimeSinceIgnoreValueChange > m_WaitTillValueHandle)
		{
			m_IgnoreLocalValueChange = false;
		}
		
		// Only update after a period of not changing the value and if
		// local value differs from the synced value
		float localValue = gameObject.GetComponent<UIProgressBar>().value;
		float syncValue = m_Value.Get();
		if(!m_IsModifyingValueLocally && localValue != syncValue)
		{
			UpdateLocalSliderValue(syncValue);
		}
	}
	
	public void HandleValueChange() 
	{
		if(m_IgnoreLocalValueChange)
			return;
		
		if(!s_SliderNotificationStream.HasUnreadData)
		{
			// Serialise the event to the server
			s_SliderNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
			s_SliderNotificationStream.Write((byte)ESliderNotificationType.OnValueChange);
			s_SliderNotificationStream.Write(UIProgressBar.current.value);
			
			m_IsModifyingValueLocally = true;
			m_TimeSinceModification = 0.0f;
		}
	}
	
	[AServerOnly]
	private void SetValue(float _Value)
	{
		m_Value.Set(_Value);
	}
	
	private void UpdateLocalSliderValue(float _Value)
	{
		UISlider slider = gameObject.GetComponent<UISlider>();
		
		m_IgnoreLocalValueChange = true;
		m_TimeSinceIgnoreValueChange = 0.0f;
		
		if(slider != null)
			slider.value = _Value;
	}
	
}
