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


// Member Functions


	public void Start()
	{
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}
	
	public void SetPrimaryActive(bool _bSetActive)
	{
		if(_bSetActive == true && m_bPrimaryActive != _bSetActive)
		{
			if(EventActivatePrimary != null)
			{
				EventActivatePrimary();	
			}
		}
		if(_bSetActive == false && m_bPrimaryActive != _bSetActive)
		{
			if(EventDeactivatePrimary != null)
			{
				EventDeactivatePrimary();	
			}
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
        }
    }

    public void SetDropped(bool _bSetPickedUp)
    {
        if (m_bHeldByPlayer == true)
        {
            if (EventDropped != null)
            {
                EventDropped();
            }
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
