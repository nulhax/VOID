﻿//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   DUIEventHandler.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CDUIElement : CNetworkMonoBehaviour 
{
	// Member Types
	public enum EElementNotificationType : byte
	{
		INVALID,

		OnClick,
		OnDoubleClick,
		OnPress,
		OnSelect,
		OnHover,
		OnDrag,
		OnDragStart,
		OnDragEnd,
		OnScroll,
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	public bool m_SyncOnClick = false;
	public bool m_SyncOnDoubleClick = false;
	public bool m_SyncOnPress = false;
	public bool m_SyncOnSelect = false;
	public bool m_SyncOnHover = true;
	public bool m_SyncOnDrag = false;
	public bool m_SyncOnDragStart = false;
	public bool m_SyncOnDragEnd = false;
	public bool m_SyncOnScroll = false;
	
	static public bool s_IsSyncingNetworkCallbacks = false;

	static private CNetworkStream s_ElementNotificationStream = new CNetworkStream();

	// Member Properties
	public CNetworkStream ElementNotificationStream
	{
		get { return(s_ElementNotificationStream); }
	}
	
	// Member Methods
	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
        _cRegistrar.RegisterRpc(this, "InvokeOnClick");
        _cRegistrar.RegisterRpc(this, "InvokeOnDoubleClick");
        _cRegistrar.RegisterRpc(this, "InvokeOnPress");
        _cRegistrar.RegisterRpc(this, "InvokeOnSelect");
        _cRegistrar.RegisterRpc(this, "InvokeOnHover");
        _cRegistrar.RegisterRpc(this, "InvokeOnDrag");
        _cRegistrar.RegisterRpc(this, "InvokeOnDragStart");
        _cRegistrar.RegisterRpc(this, "InvokeOnDragEnd");
        _cRegistrar.RegisterRpc(this, "InvokeOnScroll");
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{

	}
	
	protected void OnClick() 
	{
		if(!m_SyncOnClick)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnClick);
		s_ElementNotificationStream.Write(UICamera.currentTouchID);
	}
	
	protected void OnDoubleClick() 
	{
		if(!m_SyncOnDoubleClick)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnDoubleClick);
		s_ElementNotificationStream.Write(UICamera.currentTouchID);
	}
	
	protected void OnPress(bool _IsPressed)
	{
		if(!m_SyncOnPress)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnPress);
		s_ElementNotificationStream.Write((byte)(_IsPressed ? 1 : 0));
	}
	
	protected void OnSelect(bool _IsSelected)
	{
		if(!m_SyncOnSelect)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnSelect);
		s_ElementNotificationStream.Write((byte)(_IsSelected ? 1 : 0));
	}

	protected void OnHover(bool _IsHovered) 
	{
		if(!m_SyncOnHover)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnHover);
		s_ElementNotificationStream.Write((byte)(_IsHovered ? 1 : 0));
	}
	
	private void OnDrag(Vector2 _Delta) 
	{
		if(!m_SyncOnDrag)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnDrag);
		s_ElementNotificationStream.Write(_Delta.x);
		s_ElementNotificationStream.Write(_Delta.y);
	}
	
	protected void OnDragStart() 
	{
		if(!m_SyncOnDragStart)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnDragStart);
	}
	
	protected void OnDragEnd() 
	{
		if(!m_SyncOnDragEnd)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnDragEnd);
	}
	
	protected void OnScroll(float _Delta)
	{
		if(!m_SyncOnScroll)
			return;

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EElementNotificationType.OnScroll);
		s_ElementNotificationStream.Write(_Delta);
	}
	
	static public void SerializeElementEvents(CNetworkStream _cStream)
	{
		_cStream.Write(s_ElementNotificationStream);
		s_ElementNotificationStream.Clear();
	}

	[AServerOnly]
	static public void UnserializeElementEvents(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		while(_cStream.HasUnreadData)
		{
			// Get the DUIElement and its network view
			CDUIElement duiElement = CNetwork.Factory.FindGameObject(_cStream.Read<TNetworkViewId>()).GetComponent<CDUIElement>();

			// Get the interaction notification
			EElementNotificationType notification = (EElementNotificationType)_cStream.Read<byte>();

			// Based on the notification type, update the clients of the event
			switch(notification) 
			{
			case EElementNotificationType.OnClick:
				int click = _cStream.Read<int>();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { click });
				break;
			
			case EElementNotificationType.OnDoubleClick:
				int dClick = _cStream.Read<int>();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { dClick });
				break;

			case EElementNotificationType.OnPress:
				bool isPressed = _cStream.Read<byte>() == 1 ? true : false;
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { isPressed });
				break;

			case EElementNotificationType.OnSelect: 
				bool isSelected = _cStream.Read<byte>() == 1 ? true : false;
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { isSelected });
				break;

			case EElementNotificationType.OnHover:
				bool isHovered = _cStream.Read<byte>() == 1 ? true : false;
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { isHovered });
				break;

			case EElementNotificationType.OnDrag:
				float deltaX = _cStream.Read<float>();
				float deltaY = _cStream.Read<float>();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { deltaX, deltaY });
				break;

			case EElementNotificationType.OnDragStart:
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, null);
				break;

			case EElementNotificationType.OnDragEnd:
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, null);
				break;

			case EElementNotificationType.OnScroll:
				float delta = _cStream.Read<float>();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { delta });
				break;

			default:
			break;
			}
		}
	}

	[AServerOnly]
	static protected void NotifyOnEvent(CNetworkPlayer _cNetworkPlayer, CDUIElement _DUIElement, EElementNotificationType _NotificationType, object[] _Arguments)
	{
		ulong ignorePlayer = _cNetworkPlayer.PlayerId;

		foreach(ulong playerId in CNetwork.Server.GetNetworkPlayers().Keys)
		{
			if(playerId != ignorePlayer)
			{
				_DUIElement.InvokeRpc(playerId, "Invoke" + _NotificationType.ToString(), _Arguments);
			}
		}
	}

	[ANetworkRpc]
	protected void InvokeOnClick(int _Click) 
	{
		int temp = UICamera.currentTouchID;
		UICamera.currentTouchID = _Click;
		InvokeWithinComponents(EElementNotificationType.OnClick, null);
		UICamera.currentTouchID = temp;
	}
	
	[ANetworkRpc]
	protected void InvokeOnDoubleClick(int _Click) 
	{
		int temp = UICamera.currentTouchID;
		UICamera.currentTouchID = _Click;
		InvokeWithinComponents(EElementNotificationType.OnDoubleClick, null);
		UICamera.currentTouchID = temp;
	}
	
	[ANetworkRpc]
	protected void InvokeOnPress(bool _IsPressed)
	{
		InvokeWithinComponents(EElementNotificationType.OnPress, new object[] { _IsPressed });
	}
	
	[ANetworkRpc]
	protected void InvokeOnSelect(bool _IsSelected)
	{
		InvokeWithinComponents(EElementNotificationType.OnSelect, new object[] { _IsSelected });
	}
	
	[ANetworkRpc]
	protected void InvokeOnHover(bool _IsHovered) 
	{
		InvokeWithinComponents(EElementNotificationType.OnHover, new object[] { _IsHovered });
	}
	
	[ANetworkRpc]
	protected void InvokeOnDrag(float _DeltaX, float _DeltaY) 
	{
		InvokeWithinComponents(EElementNotificationType.OnDrag, new object[] { new Vector2(_DeltaX, _DeltaY) });
	}
	
	[ANetworkRpc]
	protected void InvokeOnDragStart() 
	{
		InvokeWithinComponents(EElementNotificationType.OnDragStart, null);
	}
	
	[ANetworkRpc]
	protected void InvokeOnDragEnd() 
	{
		InvokeWithinComponents(EElementNotificationType.OnDragEnd, null);
	}
	
	[ANetworkRpc]
	protected void InvokeOnScroll(float _Delta)
	{
		InvokeWithinComponents(EElementNotificationType.OnScroll, new object[] { _Delta });
	}

	private void InvokeWithinComponents(EElementNotificationType _Notification, object[] _Arguments)
	{
		// Get all of the components that are not this component within the element
		var targets = 
			from component in GetComponents<Component>()
				where component != this
				select component;

		// Invoke the correct callback method on all other components
		s_IsSyncingNetworkCallbacks = true;
		foreach(Component component in targets)
		{
			Type type = component.GetType();
			string methodName = _Notification.ToString();
			MethodInfo mi = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if(mi != null)
			{
				mi.Invoke(component, _Arguments);
			}
		}
		s_IsSyncingNetworkCallbacks = false;
		
		// Debug...
        // Commented out by Nathan to avoid extraneous debug information.
        // Feel free to uncomment for debugging purposes when required.
		//Debug.Log("Recieved Notification: " + _Notification.ToString() + " [" + gameObject.name + "]");
	}
}
