//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDuiFacilityDoorBehaviour.cs
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


public class CDUIDoorControl : CNetworkMonoBehaviour
{
	// Member Types
    public enum EButton
    {
        INVLAID,

        OpenDoor,
        CloseDoor,

        MAX
    };
	
    public enum EPanel
    {
        INVALID,

        OpenDoor,
        CloseDoor,
    }


	// Member Delegates & Events
	public delegate void HandleControlPanelEvent(CDUIDoorControl _Self);

    public event HandleControlPanelEvent EventOpenDoorButtonPressed;
    public event HandleControlPanelEvent EventCloseDoorButtonPressed;


	// Member Fields
	public UIPanel m_OpenPanel = null;
	public UIPanel m_ClosePanel = null;
	
	CNetworkVar<EPanel> m_Panel = null;


	// Member Properties


	// Member Methods
    public override void RegisterNetworkComponents(CNetworkViewRegistrar _Registrar)
    {
        m_Panel = _Registrar.CreateReliableNetworkVar<EPanel>(OnNetworkVarSync, EPanel.INVALID);
    }

	private void OnNetworkVarSync(INetworkVar _SynedVar)
	{
		if(_SynedVar == m_Panel)
		{
			switch(m_Panel.Value)
			{
			case EPanel.OpenDoor:
				m_OpenPanel.gameObject.SetActive(true);
				m_ClosePanel.gameObject.SetActive(false);
				break;
				
			case EPanel.CloseDoor:
				m_OpenPanel.gameObject.SetActive(false);
				m_ClosePanel.gameObject.SetActive(true);
				break;
				
			default:
				Debug.LogError("Unknown panel: " + m_Panel.Value);
				break;
			}
		}
	}

	[AServerOnly]
    public void OnClickOpen()
    {
        if(!CNetwork.IsServer)
			return;

    	if(EventOpenDoorButtonPressed != null) 
			EventOpenDoorButtonPressed(this);
    }

	[AServerOnly]
    public void OnClickClose()
    {
		if(!CNetwork.IsServer)
			return;

   	 	if(EventCloseDoorButtonPressed != null) 
			EventCloseDoorButtonPressed(this);
    }
	
    [AServerOnly]
    public void SetPanel(EPanel _Panel)
    {
        m_Panel.Set(_Panel);
    }
	
	private void Start()
    {
        if(!CNetwork.IsServer)
			return;

		SetPanel(EPanel.OpenDoor);
    }
};
