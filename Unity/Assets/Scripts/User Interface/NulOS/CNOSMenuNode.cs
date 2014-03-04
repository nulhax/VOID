//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNOSMenuNode.cs
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


[RequireComponent(typeof(CDUIElement))]
public class CNOSMenuNode : CNetworkMonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public UILabel m_Label = null;

	private CNetworkVar<CDUIRoot.EType> m_MenuNodeType = null;
	
	
	// Member Properties
	public CDUIRoot.EType MenuNodeType
	{
		get { return(m_MenuNodeType.Value); }

		[AServerOnly]
		set { m_MenuNodeType.Value = value; }
	}

	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_MenuNodeType = _cRegistrar.CreateNetworkVar(OnNetworkVarSync, CDUIRoot.EType.INVALID);
	}
	
	private void OnNetworkVarSync(INetworkVar _SyncedNetworkVar)
	{
		if(_SyncedNetworkVar == m_MenuNodeType)
		{
			ConfigureNode();
		}
	}
	
	private void Start()
	{
		// Set default transforms
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;

		// Force the parent grid to reposition
		transform.parent.GetComponent<UIGrid>().Reposition();
	}

	private void ConfigureNode()
	{
		// Initialise the correct label for the node type
		m_Label.text = CUtility.SplitCamelCase(m_MenuNodeType.Value.ToString());
	}
}
