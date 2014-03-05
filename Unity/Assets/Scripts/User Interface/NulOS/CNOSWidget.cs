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

	public CDUIPanel m_MainWidget = null;
	public CDUIPanel m_SmallWidget = null;

	private CNOSPanelRoot m_NOSPanelRoot = null;
	private bool m_MainWidgetActive = false;

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
		m_NOSPanelRoot.RegisterWidget(gameObject);

		// Mark the parent as changed
		NGUITools.MarkParentAsChanged(gameObject);

		// Reposition the menu items
		UIGrid smallWidgetGrid = m_NOSPanelRoot.SmallWidgetGrid;
		smallWidgetGrid.Reposition();
	}

	public void ShowMainWidget()
	{
		m_MainWidgetActive = true;

		if(CNetwork.IsServer)
		{
			// Transition to the main widget
			gameObject.GetComponent<CDUIRoot2D>().SwitchToPanel(m_MainWidget);
		}

		// Focus this widget
		m_NOSPanelRoot.FocusWidget(gameObject);
	}

	public void HideMainWidget()
	{
		if(CNetwork.IsServer)
		{
			// Send the widget back to the small widget grid
			UIGrid smallWidgetGrid = m_NOSPanelRoot.SmallWidgetGrid;
			transform.parent = smallWidgetGrid.transform;
			smallWidgetGrid.Reposition();

			// Transition back to the small widget
			gameObject.GetComponent<CDUIRoot2D>().SwitchToPanel(m_SmallWidget);

			// Mark the parent as changed
			NGUITools.MarkParentAsChanged(gameObject);
		}

		m_MainWidgetActive = false;

		// Sort the widget depth
		m_NOSPanelRoot.SortWidgetDepth();
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
