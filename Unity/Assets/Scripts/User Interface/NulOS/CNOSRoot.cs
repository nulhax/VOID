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
public class CNOSRoot : CNetworkMonoBehaviour 
{
	// Member Types

	
	// Member Delegates & Events
	
	
	// Member Fields
	public UIGrid m_MenuNodeGrid = null;

	public List<CDUIRoot.EType> m_DefaultNodes = null;

	
	// Member Properties
	
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		
	}
	
	private void Start()
	{
		if(CNetwork.IsServer)
		{
			// Initialise the default nodes
			foreach(CDUIRoot.EType type in m_DefaultNodes)
			{
				AddNode(type);
			}
		}
	}

	[AServerOnly]
	public void AddNode(CDUIRoot.EType _UIType)
	{
		// Create the menu node within the menu
		GameObject menuNode = CNetwork.Factory.CreateObject(CGameRegistrator.ENetworkPrefab.NOSMenuNode);
		CNetworkView menuNodeNV = menuNode.GetComponent<CNetworkView>();
		CNOSMenuNode menuNodeNos = menuNode.GetComponent<CNOSMenuNode>();

		// Parent to grid and reposition grid
		menuNodeNV.SetParent(m_MenuNodeGrid.GetComponent<CNetworkView>().ViewId);

		// Set the type for the node
		menuNodeNos.MenuNodeType = _UIType;
	}
}
