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


[RequireComponent(typeof(CNetworkView))]
[RequireComponent(typeof(CActorInteractable))]
public class CDUIConsole : CNetworkMonoBehaviour 
{

// Member Delegates & Events


// Member Fields 


	public GameObject m_ScreenObject = null;
	public CDUIRoot.EType m_DUI = CDUIRoot.EType.INVALID;
    public bool m_bCreateOnStart = true;


	CNetworkVar<TNetworkViewId> m_tDuiViewId = null;
	CDUIRoot m_cDuiRoot = null;


	TNetworkViewId m_CurrentPlayer = null;
    bool m_ScreenVisible = false;
	bool m_bHovering = false;
    bool update = true;

	
// Member Properties


	public GameObject ConsoleScreen
	{
		get { return(m_ScreenObject); } 
	}


	public GameObject DuiRoot
	{
		get
		{
			// Create the UI if it hasnt been created already
			if(CNetwork.IsServer && m_tDuiViewId.Get() == null)
			{
				CreateUserInterface();
			}

			return(m_tDuiViewId.Value.GameObject);
		}
	}


    public bool IsDuiCreated
    {
        get
        {
            return (m_tDuiViewId.Value != null);
        }
    }
	

// Member Methods


	public override void RegisterNetworkComponents(CNetworkViewRegistrar _Registrar)
	{
		m_tDuiViewId = _Registrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
	}


    [AServerOnly]
    public TNetworkViewId CreateUserInterface()
    {
        if (IsDuiCreated)
            return (null);

        if (m_DUI == CDUIRoot.EType.INVALID)
        {
            Debug.LogWarning("DUIConsole has not had a UI defined for it! (" + gameObject.name + "). Check that it is set in the prefab.");
            return (null);
        }

        // Instantiate the DUI object
        GameObject cDui = CNetwork.Factory.CreateGameObject(CDUIRoot.GetPrefabType(m_DUI));

        // Set the view ids
        CDUIRoot cDuiRoot = cDui.GetComponent<CDUIRoot>();
        cDuiRoot.ConsoleViewId = NetworkViewId;

        m_tDuiViewId.Value = cDuiRoot.NetworkViewId;

        return (cDuiRoot.NetworkViewId);
    }


    void Awake()
    {
        // Register the interactable object events
        CActorInteractable cActorInteractable = GetComponent<CActorInteractable>();
        cActorInteractable.EventHover += HandlePlayerHover;
    }


    void Start()
    {
        // Create the UI if it hasnt been created already
        if (CNetwork.IsServer && 
            m_bCreateOnStart)
        {
            CreateUserInterface();
        }
    }


    void Update()
    {
        if (!IsDuiCreated)
            return;

		// Render the UI if the screen is in view
		if (m_ScreenObject.renderer.isVisible && 
            !m_ScreenVisible)
		{
			m_ScreenVisible = true;
			m_cDuiRoot.SetCamerasRenderingState(m_ScreenVisible);
		}
		// Else stop rendering completely
		else if (!m_ScreenObject.renderer.isVisible && 
            m_ScreenVisible)
		{
			m_ScreenVisible = false;
			m_cDuiRoot.SetCamerasRenderingState(m_ScreenVisible);
		}

		// Update the position on screen for the DUI
        if (m_bHovering)
        {
			m_cDuiRoot.UpdateCameraViewportPositions(m_CurrentPlayer.GameObject.GetComponent<CPlayerInteractor>().TargetRaycastHit.textureCoord);
        }

		if(!update) update = true;
    }


	void LateUpdate()
	{
		if (update)
		{
			//Debug.Log(count);
			update = false;
		}
	}


	[ALocalOnly]
	void HandlePlayerHover(RaycastHit _RayHit, TNetworkViewId _cPlayerActorViewId, bool _bHover)
	{
		m_CurrentPlayer = _cPlayerActorViewId;
        m_bHovering = _bHover;
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_tDuiViewId)
        {
            // Cache the duiroot
            m_cDuiRoot = DuiRoot.GetComponent<CDUIRoot>();
        }
    }


}
