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
	
	public bool HasAttachedFacility
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

	public void Attach(uint _portID, GameObject _objNewFacility)
	{	
		if(!m_bhasAttachedHull)
		{
			//Get all the attached expansion ports
			Transform[] attachedObjects = _objNewFacility.GetComponentsInChildren<Transform>();			
			foreach(Transform obj in attachedObjects)
			{
				if(obj.name == "ExpansionPort")
				{
	                m_attachedPorts.Add(obj);               
				}			
			}
		
			//Line up this expansion port with the new expansion port
			Orient((int)_portID, _objNewFacility);
			
			m_bhasAttachedHull = true;
		}
	}


	public void Detach()
	{

	}


	public void Orient(int _portID, GameObject _objNewFacility)
	{
//		_objNewFacility.transform.position = transform.position;
//        _objNewFacility.transform.rotation = Quaternion.identity;
//        Debug.Log("Current Port Pos " + transform.position.x + "," + transform.position.y + "," + transform.position.z);
//
//        //m_attachedPorts[_portID].renderer.material.color = Color.green;
//
//        //Subtract the offset of the selected port from the position of the new object
//        Vector3 offset = m_attachedPorts[_portID].localPosition;
//        Vector3 currentPos = _objNewFacility.transform.position - offset;
//        Debug.Log("Offset: " + offset.x + "," + offset.y + "," + offset.z);
//        _objNewFacility.transform.position = currentPos;
//
//        //Adjust rotation
//        //The center of rotation should be the currently selected port of the new hull
//		CExpansionPortInterface newPort = (CExpansionPortInterface)m_attachedPorts[_portID].GetComponent("CExpansionPortInterface");
//		newPort.HasAttachedFacility = true;		
//		
//		AllignForwardVector(newPort, _objNewFacility);
//		//AllignRightVector(newPort, _objNewFacility);
//		//AllignUpVector(newPort, _objNewFacility);
//		
		
		CExpansionPortInterface newPort = m_attachedPorts[_portID].GetComponent<CExpansionPortInterface>();
		newPort.HasAttachedFacility = true;		
		
		/*** Code for rotating the new facility using quaternions. Rotates all the axis to be aligned correctly ***/
		
		Quaternion Rot1 = transform.rotation * m_attachedPorts[_portID].parent.rotation * Quaternion.Inverse(m_attachedPorts[_portID].rotation);
		Quaternion Rot2 = Quaternion.AngleAxis(180.0f, Vector3.up);
		
		_objNewFacility.transform.rotation = Rot1 * Rot2;
		
		// Apply the new position (this expansion port position plus the rotated other expansionport local position)
		_objNewFacility.transform.position = transform.position - (_objNewFacility.transform.rotation * m_attachedPorts[_portID].localPosition);
	}
	
	void AllignForwardVector(CExpansionPortInterface _newPort, GameObject _objNewFacility)
	{
		//Forward rotation//
					
        //The normal of this new port should be the inverse of the normal attached to this port		
        Vector3 inverseNormal = transform.forward * -1;
        float rotationAngle = Vector3.Angle(_newPort.transform.forward, inverseNormal);

        //Figure out if this is a left or right rotation
        Vector3 crossResult = Vector3.Cross(_newPort.transform.forward, inverseNormal);
        if (crossResult.y > 0.0f)
        {
            Debug.Log("Cross product result is positive");
            
        }       
		else if (crossResult.y < -0.0001f &&
				 crossResult.y > -1.0001f)
        {
            Debug.Log("Cross product result is negative" + crossResult.ToString("n10"));
            rotationAngle = 360 - rotationAngle;
			crossResult *= -1;
        }
		//If the result is neither positive nor negative, it is approximately zero.
		else
        {
            Debug.Log("Cross product result is zero " + crossResult.ToString("n10"));
            crossResult = _newPort.transform.up;
			Debug.Log("Setting result to up vector" + crossResult.ToString("n10"));
        }
		
        //Apply rotation
        Vector3 rotationPos = transform.position;
        _objNewFacility.transform.RotateAround(rotationPos, crossResult, rotationAngle);   	
		Debug.Log("Rotated " + rotationAngle.ToString() + " around " + crossResult.ToString());
	}
	
	void AllignRightVector(CExpansionPortInterface _newPort, GameObject _objNewFacility)
	{
		Vector3 RightVector;
		
		if(m_bInvertRightVector)
		{
			RightVector = transform.right * -1;
		}
		else
		{
			RightVector = transform.right; 
		}
		
		//Align the right vector
		float rotationAngle = Vector3.Angle(_newPort.transform.right, RightVector);		
		Vector3 crossResult = Vector3.Cross(_newPort.transform.right, RightVector);
      
		if (crossResult.y > 0.0f)
        {
            Debug.Log("Cross product result is positive");
            
        }       
		else if (crossResult.y < -0.0001f &&
				 crossResult.y > -1.0001f)
        {
            Debug.Log("Cross product result is negative");
            rotationAngle = 360 - rotationAngle;
			crossResult *= -1;
        }
		//If the result is neither positive nor negative, it is approximately zero.
		else 
        {
            Debug.Log("Cross product result is zero");
            crossResult = _newPort.transform.forward;
        }
        		
		//Apply rotation
		Vector3 rotationPos = transform.position;
		_objNewFacility.transform.RotateAround(rotationPos, crossResult, rotationAngle);  		
	}
	
	void AllignUpVector(CExpansionPortInterface _newPort, GameObject _objNewFacility)
	{
		Vector3 UpVector;
		
		if(m_bInvertUpVector)
		{
			UpVector = transform.up * -1;
		}
		else
		{
			UpVector = transform.up; 
		}
		
		//Allign the Up vector
		float rotationAngle = Vector3.Angle(_newPort.transform.up, UpVector);		
		Vector3 crossResult = Vector3.Cross(_newPort.transform.up, UpVector);
      
		if (crossResult.y > 0.0f)
        {
            Debug.Log("Cross product result is positive");
            
        }       
		else if (crossResult.y < -0.0001f &&
				 crossResult.y > -1.0001f)
        {
            Debug.Log("Cross product result is negative");
            rotationAngle = 360 - rotationAngle;
			crossResult *= -1;
        }
		//If the result is neither positive nor negative, it is approximately zero.
		else 
        {
            Debug.Log("Cross product result is zero");
            crossResult = _newPort.transform.right;
        }
		
		//Apply rotation
		Vector3 rotationPos = transform.position;
		_objNewFacility.transform.RotateAround(rotationPos, crossResult, rotationAngle);
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
