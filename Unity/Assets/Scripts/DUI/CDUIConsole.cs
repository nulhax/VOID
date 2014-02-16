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

	private CNetworkVar<CNetworkViewId> m_DUIViewId = null;


    CNetworkViewId m_cHoveringPlayerViewId = null;
    bool m_bHoveringOn = false;
	
    // Member Properties
	public CNetworkViewId DUIViewId
	{ 
		get { return(m_DUIViewId.Get()); } 

		[AServerOnly]
		set { m_DUIViewId.Set(value); }
	}

	public GameObject ConsoleScreen
	{
		get { return(m_ScreenObject); } 
	}

	public GameObject DUI
	{
		get
		{
			// Create the UI if it hasnt been created already
			if(CNetwork.IsServer && m_DUIViewId.Get() == null)
			{
				CreateUserInterface();
			}

			return(DUIViewId.GameObject);
		}
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
		// Create the UI if it hasnt been created already
		if(CNetwork.IsServer && m_DUIViewId.Get() == null)
		{
			CreateUserInterface();
		}
	}

	[AServerOnly]
	private void CreateUserInterface()
	{
		if(m_DUI != CDUIRoot.EType.INVALID)
		{
			// Instantiate the DUI object
			GameObject DUIObj = CNetwork.Factory.CreateObject(CDUIRoot.GetPrefabType(m_DUI));
			
			// Set the view ids
			CDUIRoot dr = DUIObj.GetComponent<CDUIRoot>();
			dr.ConsoleViewId = ViewId;
			DUIViewId = dr.ViewId;
		}
		else
		{
			Debug.LogWarning("DUIConsole has not had a UI defined for it! (" + gameObject.name + "). Check that it is set in the prefab.");
		}
	}

    void Update()
    {
        if (m_bHoveringOn)
        {
            DUI.GetComponent<CDUIRoot>().UpdateCameraViewportPositions(m_cHoveringPlayerViewId.GameObject.GetComponent<CPlayerInteractor>().TargetRaycastHit.textureCoord);
        }
    }

	[AClientOnly]
	private void HandlePlayerHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId, bool _bHover)
	{
        m_bHoveringOn = _bHover;
        m_cHoveringPlayerViewId = _cPlayerActorViewId;
	}
}
