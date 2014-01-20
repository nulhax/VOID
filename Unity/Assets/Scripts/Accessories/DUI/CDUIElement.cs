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
	public enum EEventNotificationType : byte
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
		OnScroll
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	static protected CNetworkStream s_ElementNotificationStream = new CNetworkStream();

	// Member Properties

	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		//m_NetworkedTarget = new CNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
	}
	
	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
//		if(_cSyncedNetworkVar == m_NetworkedTarget)
//		{
//			
//		}
	}

	[AClientOnly]
	protected void OnClick() 
	{
		//Debug.Log("OnClick [" + gameObject.name + "] " + UICamera.currentTouchID.ToString());

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnClick);
		s_ElementNotificationStream.Write(UICamera.currentTouchID);
	}

	[AClientOnly]
	protected void OnDoubleClick() 
	{
		//Debug.Log("OnDoubleClick [" + gameObject.name + "] " + UICamera.currentTouchID.ToString());

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnDoubleClick);
		s_ElementNotificationStream.Write(UICamera.currentTouchID);
	}

	[AClientOnly]
	protected void OnPress(bool _IsPressed)
	{
		//Debug.Log("OnPress [" + gameObject.name + "] " + _IsPressed.ToString());

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnPress);
		s_ElementNotificationStream.Write((byte)(_IsPressed ? 1 : 0));
	}

	[AClientOnly]
	protected void OnSelect(bool _IsSelected)
	{
		//Debug.Log("OnSelect [" + gameObject.name + "] " + _IsSelected.ToString());

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnSelect);
		s_ElementNotificationStream.Write((byte)(_IsSelected ? 1 : 0));
	}

	[AClientOnly]
	protected void OnHover(bool _IsHovered) 
	{
		//Debug.Log("OnHover [" + gameObject.name + "] " + b.ToString());

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnHover);
		s_ElementNotificationStream.Write((byte)(_IsHovered ? 1 : 0));
	}

	[AClientOnly]
	private void OnDrag(Vector2 _Delta) 
	{
		//Debug.Log("OnDrag [" + gameObject.name + "] " + d.ToString());

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnDrag);
		s_ElementNotificationStream.Write(_Delta.x);
		s_ElementNotificationStream.Write(_Delta.y);
	}

	[AClientOnly]
	protected void OnDragStart() 
	{
		//Debug.Log("OnDragStart[" + gameObject.name + "] ");

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnDragStart);
	}

	[AClientOnly]
	protected void OnDragEnd() 
	{
		//Debug.Log("OnDragEnd[" + gameObject.name + "] ");

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnDragEnd);
	}

	[AClientOnly]
	protected void OnScroll(float _Delta)
	{
		//Debug.Log("OnScroll [" + gameObject.name + "] " + d.ToString());

		// Serialise the event to the server
		s_ElementNotificationStream.Write(GetComponent<CNetworkView>().ViewId);
		s_ElementNotificationStream.Write((byte)EEventNotificationType.OnScroll);
		s_ElementNotificationStream.Write(_Delta);
	}

	[AClientOnly]
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
			CDUIElement duiElement = CNetwork.Factory.FindObject(_cStream.ReadNetworkViewId()).GetComponent<CDUIElement>();

			// Get the interaction notification
			EEventNotificationType notification = (EEventNotificationType)_cStream.ReadByte();

			// Based on the notification type, update the clients of the event
			switch(notification) 
			{
			case EEventNotificationType.OnClick:
				int click = _cStream.ReadInt();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { click });
				break;
			
			case EEventNotificationType.OnDoubleClick:
				int dClick = _cStream.ReadInt();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { dClick });
				break;

			case EEventNotificationType.OnPress:
				bool isPressed = _cStream.ReadByte() == 1 ? true : false;
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { isPressed });
				break;

			case EEventNotificationType.OnSelect: 
				bool isSelected = _cStream.ReadByte() == 1 ? true : false;
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { isSelected });
				break;

			case EEventNotificationType.OnHover:
				bool isHovered = _cStream.ReadByte() == 1 ? true : false;
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { isHovered });
				break;

			case EEventNotificationType.OnDrag:
				float deltaX = _cStream.ReadFloat();
				float deltaY = _cStream.ReadFloat();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { deltaX, deltaY });
				break;

			case EEventNotificationType.OnDragStart:
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, null);
				break;

			case EEventNotificationType.OnDragEnd:
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, null);
				break;

			case EEventNotificationType.OnScroll:
				float delta = _cStream.ReadFloat();
				NotifyOnEvent(_cNetworkPlayer, duiElement, notification, new object[] { delta });
				break;

			default:
			break;
			}
		}
	}

	[AServerOnly]
	static protected void NotifyOnEvent(CNetworkPlayer _cNetworkPlayer, CDUIElement _DUIElement, EEventNotificationType _NotificationType, object[] _Arguments)
	{
		ulong ignorePlayer = _cNetworkPlayer.PlayerId;

		foreach(ulong playerId in CNetwork.Server.FindNetworkPlayers().Keys)
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
		InvokeWithinComponents(EEventNotificationType.OnClick, null);
	}
	
	[ANetworkRpc]
	protected void InvokeOnDoubleClick(int _Click) 
	{
		InvokeWithinComponents(EEventNotificationType.OnDoubleClick, null);
	}
	
	[ANetworkRpc]
	protected void InvokeOnPress(bool _IsPressed)
	{
		InvokeWithinComponents(EEventNotificationType.OnPress, new object[] { _IsPressed });
	}
	
	[ANetworkRpc]
	protected void InvokeOnSelect(bool _IsSelected)
	{
		InvokeWithinComponents(EEventNotificationType.OnSelect, new object[] { _IsSelected });
	}
	
	[ANetworkRpc]
	protected void InvokeOnHover(bool _IsHovered) 
	{
		InvokeWithinComponents(EEventNotificationType.OnHover, new object[] { _IsHovered });
	}
	
	[ANetworkRpc]
	protected void InvokeOnDrag(float _DeltaX, float _DeltaY) 
	{
		InvokeWithinComponents(EEventNotificationType.OnDrag, new object[] { new Vector2(_DeltaX, _DeltaY) });
	}
	
	[ANetworkRpc]
	protected void InvokeOnDragStart() 
	{
		InvokeWithinComponents(EEventNotificationType.OnDragStart, null);
	}
	
	[ANetworkRpc]
	protected void InvokeOnDragEnd() 
	{
		InvokeWithinComponents(EEventNotificationType.OnDragEnd, null);
	}
	
	[ANetworkRpc]
	protected void InvokeOnScroll(float _Delta)
	{
		InvokeWithinComponents(EEventNotificationType.OnScroll, new object[] { _Delta });
	}

	private void InvokeWithinComponents(EEventNotificationType _Notification, object[] _Arguments)
	{
		// Get all of the components that are not this component within the element
		var targets = 
			from component in GetComponents<MonoBehaviour>()
				where component != this
				select component;
		
		// Invoke the correct callback method on all other components
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
		
		// Debug...
		Debug.Log("Recieved Notification: " + _Notification.ToString() + " [" + gameObject.name + "]");
	}
}
