//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDoorMotor.cs
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


public class CDoorMotor : CNetworkMonoBehaviour
{
	
// Member Types
	public enum EDoorState
    {
		INVALID,
        Opened,
        Opening,
        Closed,
        Closing
    }
	
	
// Member Delegates & Events
	public delegate void DoorStateHandler(CDoorMotor _sender);
    public event DoorStateHandler StateChanged;
	
	
// Member Fields
	EDoorState m_DoorState 					= EDoorState.Closed;
	bool m_StateChanged						= true;
	
	CNetworkVar<int> m_ServerDoorState    	= null;
  
	
// Static Fields
	float s_OpenAmount						= 3.5f;
	

// Member Properties
    public EDoorState State 
	{ 
		get 
		{ 
			return(m_DoorState); 
		}
		set 
		{ 
			m_DoorState = value;
			m_StateChanged = true;
		}
	}
	
	
	public EDoorState NetworkState 
	{ 
		get 
		{ 
			return((EDoorState)m_ServerDoorState.Get()); 
		}
		set 
		{ 
			m_ServerDoorState.Set((int)value);
			State = value;
		}
	}


// Member Methods
	public void OnEnable()
	{
		// Uglyfix for on enable
		if(CNetwork.IsServer)
		{
			if(m_DoorState == EDoorState.Opening)
			{
				StartCoroutine("Open");
			}
			else if(m_DoorState == EDoorState.Closing)
			{
				StartCoroutine("Close");
			}
		}
	}
	
	
	public override void InstanceNetworkVars()
    {
		m_ServerDoorState = new CNetworkVar<int>(OnNetworkVarSync, (int)EDoorState.INVALID);
	}
	
	
	public void OnNetworkVarSync(INetworkVar _rSender)
    {
		if(!Network.isServer)
		{
			// Door State
			if(_rSender == m_ServerDoorState)
			{
				State = NetworkState;
			}
		}
    }
	
	
	public void Update()
	{
		if(m_StateChanged)
		{
			OnStateChange();
			m_StateChanged = false;
		}
	}
	
	
    public void OpenDoor()
    {
		Logger.Write("Opening Door with network id ({0})", GetComponent<CNetworkView>().ViewId);
		
        StartCoroutine("Open");
    }
	

    public void CloseDoor()
    {
		Logger.Write("Closing Door with network id ({0})", GetComponent<CNetworkView>().ViewId);
		
        StartCoroutine("Close");
    }
	

    private IEnumerator Open()
    {
        float d = 0.0f;
        Vector3 pos = transform.position;

        NetworkState = EDoorState.Opening;

        while (d < s_OpenAmount)
        {
            d += Time.deltaTime;
            if (d > s_OpenAmount)
                d = s_OpenAmount;

            Vector3 newPos = pos;
            newPos.y += d;

            transform.position = newPos;

            yield return null;
        }

        NetworkState = EDoorState.Opened;
    }
	
	
    private IEnumerator Close()
    {
        float d = 0.0f;
        Vector3 pos = transform.position;

        NetworkState = EDoorState.Closing;

        while (d < s_OpenAmount)
        {
            d += Time.deltaTime;
            if (d > s_OpenAmount)
                d = s_OpenAmount;

            Vector3 newPos = pos;
            newPos.y -= d;

            transform.position = newPos;

            yield return null;
        }

        NetworkState = EDoorState.Closed;
    }
	
	
    private void OnStateChange()
    {
        if (StateChanged != null)
            StateChanged(this);
    }
}
