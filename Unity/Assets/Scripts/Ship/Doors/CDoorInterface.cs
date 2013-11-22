//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDoorInterface.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CDoorInterface : MonoBehaviour 
{

// Member Types


// Member Delegates & Events
	
	
// Member Fields	
	
	private uint m_uiDoorID = 0;
	

// Member Properties
	
	
	public uint DoorId 
	{
		get{ return(m_uiDoorID); }			
		set
		{
			if(m_uiDoorID == 0)
			{
				m_uiDoorID = value;
			}
			else
			{
				Debug.LogError("Cannot set ID value twice");
			}			
		}			
	}
	
	
// Member Methods


	public void Start()
	{
		// Empty
	}

	public void Update()
	{
	}
	
}
