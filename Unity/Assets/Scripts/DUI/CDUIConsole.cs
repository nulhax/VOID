//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CDUIConsole.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


[RequireComponent(typeof(CActorInteractable))]
public class CDUIConsole : CNetworkMonoBehaviour 
{
	// Member Delegates & Events


	// Member Fields 
	public GameObject m_ScreenObject = null;
	public CDUIRoot.EType m_DUI = CDUIRoot.EType.INVALID;

	private bool m_Initialised = false;
	private CNetworkVar<CNetworkViewId> m_DUIViewId = null;
	
    // Member Properties
	public CNetworkViewId DUIViewId
	{ 
		get { return(m_DUIViewId.Get()); } 

		[AServerOnly]
		set { m_DUIViewId.Set(value); }
	}

	public GameObject DUI 
	{ 
		get 
		{ 
			if(!m_Initialised && CNetwork.IsServer)
				Start();

			return(m_DUIViewId.Get().GameObject); 
		} 
	}

	public GameObject ConsoleScreen
	{
		get { return(m_ScreenObject); } 
	}
	
	// Member Methods
	public override void InstanceNetworkVars(CNetworkViewRegistrar _Registrar)
	{
		m_DUIViewId = _Registrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
	}
	
	public void OnNetworkVarSync(INetworkVar _SyncedVar)
	{

	}
	
	public void Awake()
	{
		// Register the interactable object events
		CActorInteractable IO = GetComponent<CActorInteractable>();
		IO.EventHover += HandlePlayerHover;
	}

	public void Start()
	{
		if(CNetwork.IsServer && !m_Initialised)
		{
			if(m_DUI != CDUIRoot.EType.INVALID)
			{
				// Instantiate the DUI object
				GameObject DUIObj = CNetwork.Factory.CreateObject(CDUIRoot.GetPrefabType(m_DUI));

				// Set the view ids
				CDUIRoot dr = DUIObj.GetComponent<CDUIRoot>();
				dr.ConsoleViewId = ViewId;
				DUIViewId = dr.ViewId;

				m_Initialised = true;
			}
			else
			{
				Debug.LogWarning("DUIConsole has not had a UI defined for it! (" + gameObject.name + "). Check that it is set in the prefab.");
			}
		}
	}

	[AClientOnly]
	private void HandlePlayerHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
	{	
		// Update the camera viewport positions
		DUI.GetComponent<CDUIRoot>().UpdateCameraViewportPositions(_RayHit.textureCoord);
	}
}
