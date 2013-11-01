//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CExpansionPortInterface.cs
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


public class CDoorMotor : MonoBehaviour
{
	
// Member Types
	
	
	public enum DoorState
    {
        Opened,
        Opening,
        Closed,
        Closing
    }
	
	
// Member Delegates & Events
	
	
	public delegate void DoorStateHandler(CDoorMotor _sender);
    public event DoorStateHandler StateChanged;
	
	
// Member Fields	
	DoorState m_State = DoorState.Closed;
  

// Member Properties
    public DoorState State 
	{ 
		get { return(m_State); }
		set { m_State = value; }
	}


// Member Methods
    public void OpenDoor()
    {
        StartCoroutine("Open");
    }

    public void CloseDoor()
    {
        StartCoroutine("Close");
    }

    private IEnumerator Open()
    {
        float d = 0.0f;
        Vector3 pos = transform.position;

        State = DoorState.Opening;
        OnStateChange();

        while (d < 2.0f)
        {
            d += Time.deltaTime;
            if (d > 2.0f)
                d = 2.0f;

            Vector3 newPos = pos;
            newPos.y += d;

            transform.position = newPos;

            yield return null;
        }

        State = DoorState.Opened;
        OnStateChange();
    }

    private IEnumerator Close()
    {
        float d = 0.0f;
        Vector3 pos = transform.position;

        State = DoorState.Closing;
        OnStateChange();

        while (d < 2.0f)
        {
            d += Time.deltaTime;
            if (d > 2.0f)
                d = 2.0f;

            Vector3 newPos = pos;
            newPos.y -= d;

            transform.position = newPos;

            yield return null;
        }

        State = DoorState.Closed;
        OnStateChange();
    }

    private void OnStateChange()
    {
        if (StateChanged != null)
            StateChanged(this);
    }
}
