using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExpansionPort : MonoBehaviour 
{	
	public enum BuildState
	{
		state_default,
		state_Construction,
		state_Orientation,
		state_unaviable,
		state_max
	};
	
	private int portID = 1;
    public int SegmentID = 1;
    public int portLevel = 0;
	public BuildState buildState;
	GameObject AttachedHull;
	bool infoLogged = false;
	bool hasAttachedHull = false;
    float lastClick = 0;
    float lastScroll = 0;
	List<Transform> Ports = new List<Transform>();
    static int NumHullTypes;
	
	static bool canBuild = true;
	
	// Use this for initialization
	void Start () 
	{
		buildState = BuildState.state_Construction;	
		renderer.material.color = Color.blue;
        portID = 0;	
        SegmentID = 1;
        NumHullTypes = 4;

       // Screen.lockCursor = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(buildState == BuildState.state_Construction && hasAttachedHull == false)
		{		
			//Raycast. See if this thing got hit			
			RaycastHit hit;
		 	Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, 100))
			{		
				if(hit.collider.gameObject == gameObject && canBuild)
				{
					if(infoLogged == false)
					{
						renderer.material.color = Color.red;
						//Debug.Log("Normal of this port: " + portNormal);
						infoLogged = true;
					}
					
					if(Input.GetMouseButtonDown(0))
					{
						//Create new hull segment
						AttachHullSegment();
                        lastClick = Time.time;
					}
				}
				
				else
				{
					renderer.material.color = Color.blue;
					infoLogged = false;
				}
			}           
		}
		
		else if(buildState == BuildState.state_Orientation)
		{
			if(Input.GetAxis("Mouse ScrollWheel") > 0)
			{
				//Select next port and reorient accordingly
				if(portID < Ports.Count - 1)
				{
					portID++;
				}
				else
				{
					portID = 0;
				}
				Debug.Log("Now using port " + portID); 
				ReorientHullSegment();
			}
			else if(Input.GetAxis("Mouse ScrollWheel") < 0)
			{
				//Select previous port and reorient accordingly
				if(portID > 0)
				{
					portID--;
				}
				else
				{
					portID = Ports.Count - 1;	
				}
				Debug.Log("Now using port " + portID);
				ReorientHullSegment();
			}
			else if(Input.GetMouseButton(0))
			{
                if (Time.time > lastClick + 1.0f)
                {
                    ConfirmCOnstruction();
                    Debug.Log("Constructed");
                }
			}
            else if (Input.GetMouseButton(1))
            {
               CancelConstruction();
               Debug.Log("Cancelled");
            }

            if (Input.GetKeyDown(KeyCode.Q) && Time.time > lastScroll + 0.1f)
            {
                lastScroll = Time.time;

                if (SegmentID < NumHullTypes)
                {
                    SegmentID++;
                    Debug.Log("HullSegment: " + SegmentID + " Selected");
                }
                else
                {
                    SegmentID = 1;
                    Debug.Log("HullSegment: " + SegmentID + " Selected");
                }

                CancelConstruction();
                AttachHullSegment();
            }
            else if (Input.GetKeyDown(KeyCode.E) && Time.time > lastScroll + 0.1f)
            {
                lastScroll = Time.time;

                if (SegmentID > 1)
                {
                    SegmentID--;					
                    Debug.Log("HullSegment: " + SegmentID + " Selected");
                }
                else
                {
                    SegmentID = NumHullTypes;
					Debug.Log("HullSegment: " + SegmentID + " Selected");
                }

                CancelConstruction();
                AttachHullSegment();
            }
		}
		
		RenderNormals();
	}
	
	
	void RenderNormals()
	{
        Debug.DrawRay(transform.position, transform.forward, Color.magenta);
	}
	
	void AttachHullSegment()
	{

        //Create new segment
        Debug.Log("HullSegment" + SegmentID.ToString());
        AttachedHull = (GameObject)Instantiate(Resources.Load("HullSegment" + SegmentID.ToString()));          
        	
		//Set this position to the position of the selected expansion port
		AttachedHull.transform.position = transform.position;	
				
		//Get all the attached expansion ports
		Transform[] attachedObjects = AttachedHull.GetComponentsInChildren<Transform>();			
		foreach(Transform obj in attachedObjects)
		{
			if(obj.renderer != null)
			{
				Color hullColour = obj.renderer.material.color;
				hullColour.a *= 0.25f;
				obj.renderer.material.color = hullColour;
				
				if(obj.name == "ExpansionPort")
				{
	                //Only add ports on the same level
	                ExpansionPort portScript = obj.GetComponent<ExpansionPort>();
	              	Ports.Add(obj);               
				}
			}
		}
	
		//Line up this expansion port with the new expansion port
		ReorientHullSegment();
		//Update states
		buildState = BuildState.state_Orientation;
		hasAttachedHull = true;
		canBuild = false;
		portID = 0;
	}
	
	void ReorientHullSegment()
	{
		foreach (Transform Port in Ports)
        {
            ExpansionPort expPort = (ExpansionPort)Port.GetComponent("ExpansionPort");
            expPort.renderer.material.color = Color.grey;
        }

        AttachedHull.transform.position = transform.position;
        AttachedHull.transform.rotation = Quaternion.identity;
        //Debug.Log("Current Port Pos " + transform.position.x + "," + transform.position.y + "," + transform.position.z);

        Ports[portID].renderer.material.color = Color.green;

        //Subtract the offset of the selected port from the position of the new object
        Vector3 offset = Ports[portID].localPosition;
        Vector3 currentPos = AttachedHull.transform.position - offset;
       // Debug.Log("Offset: " + offset.x + "," + offset.y + "," + offset.z);
        AttachedHull.transform.position = currentPos;

        //Adjust rotation
        //The center of rotation should be the currently selected port of the new hull
		ExpansionPort newPort = (ExpansionPort)Ports[portID].GetComponent("ExpansionPort");
		
		//**Forward rotation**//
		
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
        AttachedHull.transform.RotateAround(rotationPos, crossResult, rotationAngle);   
		Debug.Log(crossResult.ToString());			
	}
	
	void ConfirmCOnstruction()
	{
		Transform[] attachedObjects = AttachedHull.GetComponentsInChildren<Transform>();			
		foreach(Transform obj in attachedObjects)
		{
			if(obj.renderer != null)
			{				
				Color hullColour = obj.renderer.material.color;
				hullColour.a = 1.0f;
				obj.renderer.material.color = hullColour;
			}
		}
		
		buildState = BuildState.state_Construction;
		canBuild = true;
	}

    void CancelConstruction()
    {
        Destroy(AttachedHull);
        AttachedHull = null;
        Ports.Clear();
        buildState = BuildState.state_Construction;
        canBuild = true;
        hasAttachedHull = false;
		portID = 0;
    }
}
