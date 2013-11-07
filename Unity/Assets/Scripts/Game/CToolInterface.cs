//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CToolInterface.cs
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

//This Script contains all the tool info, such as:
//		an enum for what type of tool the tool is
//		an ammo counter?
//		slots taken up
//		
//This Script can:
//		Get the type of tool
//		Shoot the Tool
//		Reload the Tool


public class CToolInterface : MonoBehaviour
{

// Member Types


// Member Delegates & Events
	public delegate void ActivatePrimary();
	public delegate void DeactivatePrimary();
	
	public delegate void ActivateSecondary();
	public delegate void DeactivateSecondary();
	
	public delegate void Reload();

    public delegate void PickedUp();
    public delegate void Dropped();
	
	public event ActivatePrimary EventActivatePrimary;
	public event DeactivatePrimary EventDeactivatePrimary;
	
	public event ActivateSecondary EventActivateSecondary;
	public event DeactivateSecondary EventDeactivateSecondary;
	
	public event Reload EventReload;

    public event PickedUp EventPickedUp;
    public event PickedUp EventDropped;
	
	
// Member Properties
    public bool IsHeldByPlayer
    {
        get
        {
            return (m_bHeldByPlayer);
        }
    }

// Member Functions


	public void Start()
	{
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
        if (m_bHeldByPlayer == false)
        {
            gameObject.GetComponent<CNetworkView>().SyncTransformPosition();
            gameObject.GetComponent<CNetworkView>().SyncTransformRotation();
        }
	}
	
	public void SetPrimaryActive()
	{
		if(m_bPrimaryActive == false)//if(_bSetActive == true && m_bPrimaryActive != _bSetActive)
		{
			if(EventActivatePrimary != null)
			{
				EventActivatePrimary();	
			}
            m_bPrimaryActive = true;
		}
		else if(m_bPrimaryActive == true)//if(_bSetActive == false && m_bPrimaryActive != _bSetActive)
		{
			if(EventDeactivatePrimary != null)
			{
				EventDeactivatePrimary();	
			}
            m_bPrimaryActive = false;
		}
	}
	
	public void SetSecondaryActive(bool _bSetActive)
	{
		if(_bSetActive == true && m_bSecondaryActive != _bSetActive)
		{
			if(EventActivateSecondary != null)
			{
				EventActivateSecondary();	
			}
		}
		if(_bSetActive == false && m_bSecondaryActive != _bSetActive)
		{
			if(EventDeactivateSecondary != null)
			{
				EventDeactivateSecondary();	
			}
		}
	}

    public void SetPickedUp()
    {
        if (m_bHeldByPlayer == false)
        {
            if (EventPickedUp != null)
            {
                EventPickedUp();
            }

            m_bHeldByPlayer = true;

            rigidbody.isKinematic = true;
        }
    }

    public void SetDropped()
    {
        if (m_bHeldByPlayer == true)
        {
            if (EventDropped != null)
            {
                EventDropped();
            }
            m_bHeldByPlayer = false;

            rigidbody.isKinematic = false;
        }
    }

    public void ActivateReload()
    {
        if (m_bHeldByPlayer == true)
        {
            if (EventReload != null)
            {
                EventReload();
            }
        }
    }

// Member Fields
	bool m_bPrimaryActive;
	bool m_bSecondaryActive;
    bool m_bHeldByPlayer;
	
};
