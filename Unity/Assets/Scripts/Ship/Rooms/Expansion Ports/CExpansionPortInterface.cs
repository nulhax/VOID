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
		if(!hasAttachedHull)
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
			
			hasAttachedHull = true;
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
		
		//Forward rotation//
		
        //The normal of this new port should be the inverse of the normal attached to this port		
        Vector3 inverseNormal = transform.forward * -1;
        float rotationAngle = Vector3.Angle(newPort.transform.forward, inverseNormal);

        //Figure out if this is a left or right rotation
        Vector3 crossResult = Vector3.Cross(newPort.transform.forward, inverseNormal);
        if (crossResult.y > 0)
        {
            Debug.Log("Cross product result is positive");
            
        }
        else if (crossResult.y < 0)
        {
            Debug.Log("Cross product result is negative");
            rotationAngle = 360 - rotationAngle;
			crossResult *= -1;
        }
        else if (crossResult.x == 0 && crossResult.y == 0 && crossResult.z == 0)
        {
            Debug.Log("Cross product result is 0");
            crossResult = newPort.transform.up;
        }

        //Apply rotation
        Vector3 rotationPos = transform.position;
        _objNewRoom.transform.RotateAround(rotationPos, crossResult, rotationAngle);   
		Debug.Log(crossResult.ToString());
		
		//Align the right vector
		rotationAngle = Vector3.Angle(newPort.transform.right, transform.right);
		
		crossResult = Vector3.Cross(newPort.transform.right, transform.right);
        if (crossResult.y > 0)
        {
            Debug.Log("Cross product result is positive");
            
        }
        else if (crossResult.y < 0)
        {
            Debug.Log("Cross product result is negative");
            rotationAngle = 360 - rotationAngle;
			crossResult *= -1;
        }
        else if (crossResult.x == 0 && crossResult.y == 0 && crossResult.z == 0)
        {
            Debug.Log("Cross product result is 0");
            crossResult = newPort.transform.forward;
        }
		
		//Apply rotation
		_objNewRoom.transform.RotateAround(rotationPos, crossResult, rotationAngle);  
		
		//Allign the Up vector
		rotationAngle = Vector3.Angle(newPort.transform.up, transform.up);
		
		crossResult = Vector3.Cross(newPort.transform.up, transform.up);
        if (crossResult.y > 0)
        {
            Debug.Log("Cross product result is positive");
            
        }
        else if (crossResult.y < 0)
        {
            Debug.Log("Cross product result is negative");
            rotationAngle = 360 - rotationAngle;
			crossResult *= -1;
        }
        else if (crossResult.x == 0 && crossResult.y == 0 && crossResult.z == 0)
        {
            Debug.Log("Cross product result is 0");
            crossResult = newPort.transform.right;
        }
		
		//Apply rotation
		_objNewRoom.transform.RotateAround(rotationPos, crossResult, rotationAngle);
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
	
	private uint m_uiPortID = 0;
   	bool hasAttachedHull = false; 	
	List<Transform> m_attachedPorts = new List<Transform>();
	
};
