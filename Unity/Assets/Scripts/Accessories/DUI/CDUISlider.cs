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

[RequireComponent(typeof(UISlider))]
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
	private bool m_SlidingSelf = false;
	
	static private CNetworkStream s_SliderNotificationStream = new CNetworkStream();


	// Member Properties
	public bool SlidingSelf
	{
		get { return(m_SlidingSelf);}
		set { m_SlidingSelf = value; }
	}
	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_Value = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
	}
	
	private void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		if(_cSyncedNetworkVar == m_Value)
		{
			if(!m_SlidingSelf)
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

	public void Awake()
	{
		// Set the parent of the children to this gameobject
		foreach(CDUISliderChild sc in GetComponentsInChildren<CDUISliderChild>())
		{
			sc.SliderParent = gameObject;
		}
	}

	public void HandleValueChange() 
	{
		if(!s_SliderNotificationStream.HasUnreadData)
		{
			// Serialise the event to the server
			s_SliderNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
			s_SliderNotificationStream.Write((byte)ESliderNotificationType.OnValueChange);
			s_SliderNotificationStream.Write(UIProgressBar.current.value);
		}
	}

	public void OnPress(bool _IsPressed)
	{
		m_SlidingSelf = _IsPressed;
	}

	[AServerOnly]
	private void SetValue(float _Value)
	{
		m_Value.Set(_Value);
	}

	private void UpdateLocalSliderValue(float _Value)
	{
		UISlider slider = gameObject.GetComponent<UISlider>();
		
		if(slider != null)
			slider.value = _Value;
	}

}
