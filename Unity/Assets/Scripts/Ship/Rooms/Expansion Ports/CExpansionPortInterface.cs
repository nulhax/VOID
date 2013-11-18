//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CExpansionPortInterface.cs
//  Description :   This script is used for alligning new hull segments to expansion ports.
//
//  Author  	:  Daniel Langsford
//  Mail    	:  folduppugg@hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */
 

public class CExpansionPortInterface : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties
	
	
	public uint ExpansionPortId 
	{
		get{return(m_uiPortID);}			
		set
		{
			if(m_uiPortID == 0)
			{
				m_uiPortID = value;
			}
			else
			{
				Debug.LogError("Cannot set ID value twice");
			}			
		}			
	}
	
	public bool HasAttachedRoom
	{
		get{return(m_bhasAttachedHull);}
		set{m_bhasAttachedHull = value;}
	}

// Member Functions


	public void Start()
	{
		// Empty
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{		
		RenderNormals();
	}
	
	void RenderNormals()
	{
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
		Debug.DrawRay(transform.position, transform.up, Color.green);
		Debug.DrawRay(transform.position, transform.right, Color.red);
	}

	public void Attach(uint _portID, GameObject _objNewRoom)
	{	
		if(!m_bhasAttachedHull)
		{
			//Get all the attached expansion ports
			Transform[] attachedObjects = _objNewRoom.GetComponentsInChildren<Transform>();			
			foreach(Transform obj in attachedObjects)
			{
				if(obj.name == "ExpansionPort")
				{
	                m_attachedPorts.Add(obj);               
				}			
			}
		
			//Line up this expansion port with the new expansion port
			Orient((int)_portID, _objNewRoom);
			
			m_bhasAttachedHull = true;
		}
	}


	public void Detach()
	{

	}


	public void Orient(int _portID, GameObject _objNewRoom)
	{
		_objNewRoom.transform.position = transform.position;
        _objNewRoom.transform.rotation = Quaternion.identity;
        Debug.Log("Current Port Pos " + transform.position.x + "," + transform.position.y + "," + transform.position.z);

        //m_attachedPorts[_portID].renderer.material.color = Color.green;

        //Subtract the offset of the selected port from the position of the new object
        Vector3 offset = m_attachedPorts[_portID].localPosition;
        Vector3 currentPos = _objNewRoom.transform.position - offset;
        Debug.Log("Offset: " + offset.x + "," + offset.y + "," + offset.z);
        _objNewRoom.transform.position = currentPos;

        //Adjust rotation
        //The center of rotation should be the currently selected port of the new hull
		CExpansionPortInterface newPort = (CExpansionPortInterface)m_attachedPorts[_portID].GetComponent("CExpansionPortInterface");
		newPort.HasAttachedRoom = true;		
			
		/*** Code for rotating the new facility using quaternions. Rotates all the axis to be aligned correctly ***/
		
		Quaternion Rot1 = transform.rotation * m_attachedPorts[_portID].parent.rotation * Quaternion.Inverse(m_attachedPorts[_portID].rotation);
		Quaternion Rot2 = Quaternion.AngleAxis(180.0f, Vector3.up);
		
		_objNewRoom.transform.rotation = Rot1 * Rot2;
		
		// Apply the new position (this expansion port position plus the rotated other expansionport local position)
		_objNewRoom.transform.position = transform.position - (_objNewRoom.transform.rotation * m_attachedPorts[_portID].localPosition);
	}
	
	public void Cancel()
	{

	}


	public void Confirm()
	{

	}
	
	//Members
	public enum BuildState
	{
		state_default,
		state_Construction,
		state_Orientation,
		state_unaviable,
		state_max
	};
	
	public static string s_GameObjectName = "ExpansionPort";
	public bool m_bInvertRightVector = false;
	public bool m_bInvertUpVector = false;
	
	private uint m_uiPortID = 0;  	
	private bool m_bhasAttachedHull = false; 	
	private List<Transform> m_attachedPorts = new List<Transform>();
	
};
