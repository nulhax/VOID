//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


public class CDoorBehaviour : CNetworkMonoBehaviour
{
	// Member Types
    public enum EEventType
    {
        OpenStart,
        Opened,
        CloseStart,
        Closed
    }


	// Member Delegates & Events
    public delegate void HandleDoorEvent(CDoorBehaviour _Sender);

	public event HandleDoorEvent EventOpenStart;
	public event HandleDoorEvent EventOpenFinished;
	public event HandleDoorEvent EventCloseStart;
	public event HandleDoorEvent EventCloseFinish;


	// Member Fields
	public float m_FlowVolume = 100.0f;
	public float m_OpenCloseSpeed = 1.0f;

	private CNetworkVar<bool> m_Opened = null;


	// Member Properties
    public bool IsOpened
    {
        get { return(m_Opened.Value); }
    }


	// Member Methods
    public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
    {
		m_Opened = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
    }

	private void OnNetworkVarSync(INetworkVar _SyncedVar)
	{
		if(m_Opened == _SyncedVar)
		{
			if(IsOpened)
			{
				if(EventOpenStart != null)
					EventOpenStart(this);

				animation.CrossFadeQueued("Door_Open");
			}
			else
			{
				if(EventCloseStart != null) 
					EventCloseStart(this);
				
				animation.CrossFadeQueued("Door_Close");
			}
		}
	}
	
	[AServerOnly]
	public void OnTriggerEnter(Collider _Collider)
	{
		if(!CNetwork.IsServer)
			return;

		bool isPlayer = _Collider.gameObject.GetComponent<CPlayerInterface>();

		if(isPlayer)
			m_Opened.Value = true;
	}

	[AServerOnly]
	public void OnTriggerExit(Collider _Collider)
	{
		if(!CNetwork.IsServer)
			return;

		bool isPlayer = _Collider.gameObject.GetComponent<CPlayerInterface>();
		
		if(isPlayer)
			m_Opened.Value = false;
	}

	public void DoorOpenFinished()
	{
		if (EventOpenFinished != null) 
			EventOpenFinished(this);
	}

	public void DoorCloseFinished()
	{
		if (EventOpenFinished != null) 
			EventOpenFinished(this);
	}

    private void OnEventDuiDoorControlClick(CDuiDoorControlBehaviour.EButton _eButton)
    {
        switch (_eButton)
        {
            case CDuiDoorControlBehaviour.EButton.OpenDoor:
                m_Opened.Value = true;
                break;

            case CDuiDoorControlBehaviour.EButton.CloseDoor:
				m_Opened.Value = false;
                break;
        }
    }
};
