//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNOSRoot.cs
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
public class CNOSWidget : CNetworkMonoBehaviour 
{
	// Member Types
	public enum EType
	{
		INVALID,

		ShipPropulsion = 1,

		FacilityControl = 100,
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	public EType m_WidgetType = EType.INVALID;

	public UIPanel m_WidgetPanel = null;
	public UIWidget m_MainWidget = null;
	public UIWidget m_SmallWidget = null;
	
	private CNOSPanelRoot m_NOSPanelRoot = null;
	
	private bool m_MainWidgetActive = false;
	private bool m_KeepWithinBounds = false;

	private bool m_MainWidgetMaximised = false;
	private Vector3 m_PreMaximisePosition = Vector2.zero;
	private Vector2 m_PreMaximiseDimensions = Vector2.zero;

	private static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_RegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


	// Member Properties
	public bool IsMainWidgetActive
	{
		get { return(m_MainWidgetActive); }
	}

	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		
	}
	
	private void Start()
	{
		// Set default transforms
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	
		// Find the panel root and register self to it
		m_NOSPanelRoot = CUtility.FindInParents<CNOSPanelRoot>(gameObject);
		m_NOSPanelRoot.RegisterWidget(this);

		// Set the main widget hidden
		HideMainWidget();
	}

	private void Update()
	{
		if(m_KeepWithinBounds)
		{
			// Ensure the widget is within the bounds of the content area
			m_NOSPanelRoot.m_MainWidgetContainer.ConstrainTargetToBounds(transform, false);
		}
	}

	public void ShowMainWidget(bool _DragDropEvent)
	{
		m_MainWidgetActive = true;
		m_KeepWithinBounds = true;

		// Set the anchors of the panel to the main widget
		m_WidgetPanel.SetAnchor(m_MainWidget.gameObject, 0, 0, 0, 0);
		
		// Play the small tweener in reverse
		UITweener tweenSmall = m_SmallWidget.GetComponent<UITweener>();
		tweenSmall.PlayReverse();

		// Focus this widget
		m_NOSPanelRoot.FocusWidget(this);
	}

	public void HideMainWidget()
	{
		m_MainWidgetActive = false;
		m_KeepWithinBounds = false;

		// Set the anchors of the panel to the small widget
		m_WidgetPanel.SetAnchor(m_SmallWidget.gameObject, 0, 0, 0, 0);

		// Play the main tweener in reverse
		UITweener tweenMain = m_MainWidget.GetComponent<UITweener>();
		tweenMain.PlayReverse();

		if(m_MainWidgetMaximised)
		{
			ToggleMainWidgetMaximise();
		}
	}

	public void ToggleMainWidgetMaximise()
	{
		if(!m_MainWidgetMaximised)
		{
			m_MainWidgetMaximised = true;
			m_KeepWithinBounds = true;

			// Save the previous dimensions and position
			m_PreMaximiseDimensions.x = (float)m_MainWidget.width;
			m_PreMaximiseDimensions.y = (float)m_MainWidget.height;
			m_PreMaximisePosition = transform.localPosition;

			// Tween the width of the widget to be the same of the panel
			TweenWidth tw = TweenWidth.Begin(m_MainWidget, 0.2f, (int)m_NOSPanelRoot.m_MainWidgetContainer.width);

			// Tween the height of the widget to be the same of the panel
			TweenHeight th = TweenHeight.Begin(m_MainWidget, 0.2f, (int)m_NOSPanelRoot.m_MainWidgetContainer.height);

			// Focus this widget
			m_NOSPanelRoot.FocusWidget(this);
		}
		else
		{
			m_MainWidgetMaximised = false;
			m_KeepWithinBounds = false;
			
			// Tween the width of the widget to be the original
			TweenWidth tw = TweenWidth.Begin(m_MainWidget, 0.2f, (int)m_PreMaximiseDimensions.x);
			
			// Tween the height of the widget to be the original
			TweenHeight th = TweenHeight.Begin(m_MainWidget, 0.2f, (int)m_PreMaximiseDimensions.y);

			// Spring the position to the original
			SpringPosition sp = SpringPosition.Begin(gameObject, m_PreMaximisePosition, 13.0f);
		}
	}

	public void OnSmallWidgetTweenFinish()
	{
		if(m_MainWidgetActive)
		{
			// Disable the small widget
			m_SmallWidget.transform.localPosition = Vector3.zero;
			m_SmallWidget.gameObject.SetActive(false);

			// Play the main tweener forwards and activate
			m_MainWidget.gameObject.SetActive(true);
			UITweener tweenMain = m_MainWidget.GetComponent<UITweener>();
			tweenMain.PlayForward();
		}
	}

	public void OnMainWidgetTweenFinish()
	{
		if(!m_MainWidgetActive)
		{
			// Disable the main widget
			m_MainWidget.transform.localPosition = Vector3.zero;
			m_MainWidget.gameObject.SetActive(false);

			// Send the widget back to the small widget grid
			UIGrid smallWidgetGrid = m_NOSPanelRoot.SmallWidgetGrid;
			transform.parent = smallWidgetGrid.transform;
			smallWidgetGrid.Reposition();
			
			// Mark the parent as changed
			NGUITools.MarkParentAsChanged(gameObject);

			// Play the small tweener forwards and activate
			m_SmallWidget.gameObject.SetActive(true);
			UITweener tweenSmall = m_SmallWidget.GetComponent<UITweener>();
			tweenSmall.PlayForward();
		}
		else
		{
			// Transition finished, should be able to keep self in bounds
			m_KeepWithinBounds = false;
		}
	}

	public static void RegisterPrefab(EType _Type, CGameRegistrator.ENetworkPrefab _NetworkPrefab)
	{
		s_RegisteredPrefabs.Add(_Type, _NetworkPrefab);
	}
	
	public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _Type)
	{
		if (!s_RegisteredPrefabs.ContainsKey(_Type))
		{
			Debug.LogError(string.Format("NOS type ({0}) has not been registered a prefab", _Type));
			
			return (CGameRegistrator.ENetworkPrefab.INVALID);
		}
		
		return (s_RegisteredPrefabs[_Type]);
	}
}
