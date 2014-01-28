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
	// Member Fields 
	public GameObject m_ScreenObject = null;
	public CDUIRoot.EType m_DefaultUI = CDUIRoot.EType.INVALID;

	private CNetworkVar<CNetworkViewId> m_DUIViewId = null;
	
	static private float s_UIOffset = 0.0f;
	
    // Member Properties
	public CNetworkViewId DUIViewId 
	{ 
		get { return(m_DUIViewId.Get()); } 
		
		[AServerOnly]
		set { m_DUIViewId.Set(value); }
	}

	public GameObject DUI 
	{ 
		get { return(CNetwork.Factory.FindObject(m_DUIViewId.Get())); } 
	}

	public GameObject ConsoleScreen
	{
		get { return(m_ScreenObject); } 
	}
	
	// Member Methods
	public override void InstanceNetworkVars()
	{
		m_DUIViewId = new CNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
	}
	
	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
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
		if(CNetwork.IsServer)
		{
			CreateDUI();
		}
	}


	[AServerOnly]
    private void CreateDUI()
	{
		// Create the DUI game object
		GameObject dui = CNetwork.Factory.CreateObject(CDUIRoot.GetPrefabType(m_DefaultUI));
		dui.GetComponent<CNetworkView>().SetPosition(new Vector3(0.0f, 0.0f, s_UIOffset));
		dui.GetComponent<CNetworkView>().SetRotation(Quaternion.identity.eulerAngles);

		// Set the view id of this console to the monitor
		dui.GetComponent<CDUIRoot>().ConsoleViewId = GetComponent<CNetworkView>().ViewId;

		// Save the network view of the DUI
		DUIViewId = dui.GetComponent<CNetworkView>().ViewId;

		// Increment the offset
		s_UIOffset += 10.0f;
	}

	[AClientOnly]
	private void HandlePlayerHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
	{	
		// Update the camera viewport positions
		DUI.GetComponent<CDUIRoot>().UpdateCameraViewportPositions(_RayHit.textureCoord);
	}
}
