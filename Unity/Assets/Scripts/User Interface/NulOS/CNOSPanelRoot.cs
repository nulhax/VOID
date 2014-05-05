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
public class CNOSPanelRoot : CNetworkMonoBehaviour 
{
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIPanel m_SmallWidgetContainer = null;
	public UIPanel m_MainWidgetContainer = null;

	public List<CNOSWidget.EType> m_DefaultMenuWidgetNodes = null;

	private UIGrid m_SmallWidgetGrid = null;
	private List<GameObject> m_Widgets = new List<GameObject>();


	// Member Properties
	public UIGrid SmallWidgetGrid
	{
		get { return(m_SmallWidgetGrid); }
	}

	
	// Member Methods
	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		
	}
	
	private void Start()
	{
		// Cache the small widget grid
		m_SmallWidgetGrid = m_SmallWidgetContainer.GetComponentInChildren<UIGrid>();

		if(CNetwork.IsServer)
		{
			// Initialise the default widgets
			foreach(CNOSWidget.EType type in m_DefaultMenuWidgetNodes)
			{
				CreateWidget(type);
			}
		}
	}

	[AServerOnly]
	public void CreateWidget(CNOSWidget.EType _WidgetType)
	{
		// Create the widget
		GameObject widget = CNetwork.Factory.CreateGameObject(CNOSWidget.GetPrefabType(_WidgetType));
		CNetworkView widgetNV = widget.GetComponent<CNetworkView>();

		// Parent to grid initially
		widgetNV.SetParent(m_SmallWidgetGrid.GetComponent<CNetworkView>().ViewId);
	}

	public void RegisterWidget(CNOSWidget _Widget)
	{
		// Add the widget
		m_Widgets.Add(_Widget.gameObject);

		// Sort the depth
		SortWidgetDepth();
	}

	public void FocusWidget(CNOSWidget _Widget)
	{
		// Remove the widget in the list
		m_Widgets.Remove(_Widget.gameObject);

		// Add the widget to the back
		m_Widgets.Add(_Widget.gameObject);

		// Sort the depth
		SortWidgetDepth();
	}

	public void SortWidgetDepth()
	{
		// Resort the widgets depths
		int depth = 0;
		foreach(GameObject widget in m_Widgets)
		{
			// Adjust the depth of children panels
			depth = NGUITools.SetPanelsDepthNested(widget, ++depth);
		}
	}
}
