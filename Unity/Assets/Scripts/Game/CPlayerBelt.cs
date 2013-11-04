//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerBelt.cs
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

//The player belt holds tools, and interacts with Tools on behalf of the Player.
//This Script contains:
//		an array which contains two to three tools.
//		an Id for the currently held/active tool
//		
//This Script Can:
//		Pick Up Tools
//		drop tools
//		get tool info
//		


public class CPlayerBelt : MonoBehaviour
{

// Member Types
    const uint k_uiMaxTools = 3;

// Member Delegates & Events
	
	
// Member Properties
	
	
// Member Functions


	public void Start()
	{
        m_uiActiveToolId = 0;
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
        if (Input.GetMouseButtonDown(1))
        {
            m_cTools[m_uiActiveToolId].GetComponent<CToolInterface>().SetPrimaryActive(true);
        }
        else if(Input.GetKeyDown("f"))
        {
            Debug.LogError("F Key Pressed");
            PickUpTool();
        }
        else if (Input.GetKeyDown("g"))
        {
            DropTool(m_uiActiveToolId);
        }
        else if (Input.GetKeyDown("e"))
        {
            IncrementTool();
        }
        else if (Input.GetKeyDown("r"))
        {
            DecrementTool();
        }
	}
	

	public void PickUpTool()
	{
		//check if targeted GameObject is a tool
		//check if there is a free inventory slot
		//if not, then swap currently held tool for tool on ground

		//ray casting

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(transform.position, ray.direction, Color.blue, 30);
        Debug.DrawRay(transform.position, transform.forward, Color.red, 30);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawRay(transform.position, ray.direction, Color.green, 15);

            Debug.Log("Hit " + hit.transform.gameObject.name);

            //have a check to make sure it is lees than say, 2 meters,
            //so that the player cannot pick up tools across the map.

            if (hit.transform.gameObject.GetComponent<CToolInterface>() != null)
            {
                Debug.Log("HIT A DAMN TOOL");

                for (uint i = 0; i < m_uiToolCapacity; i++)
                {
                    if (m_cTools[i] == null)
                    {
                        m_cTools[i] = hit.transform.gameObject;
                        m_cTools[i].GetComponent<CToolInterface>().SetPickedUp();
                        m_cTools[i].rigidbody.isKinematic = true;

                        //make Tool a child of the Player, maybe change it's position to the camera or some such.
                        hit.transform.parent = transform;

                        //tell Tool it is now being held
                        ChangeTool(i);

                        break;
                    }
                }

                //if not, do nothing
                //if yes to either, add Tool to m_cTools[]
            }
		}
	}


    public void DropTool(uint _uiToolId)
	{
        //if there is a tool in the slot, drop it, activating physics
        if (m_cTools[_uiToolId] != null)
        {
            m_cTools[_uiToolId].transform.parent = null;
            m_cTools[_uiToolId].rigidbody.isKinematic = false;
            m_cTools[_uiToolId] = null;
        }
		
		//the dropped tool should always be the currently held tool, except on death
		//remove the tool from parent, possibly with transfrom.DetachChildren();
	}
	

	public void ChangeTool(uint _uiToolId)
	{
        if (_uiToolId >= 0 && _uiToolId <= (m_uiToolCapacity - 1))
        {
            if (m_cTools[_uiToolId] != null)
            {
                UnEquipTool(m_uiActiveToolId);

                m_uiActiveToolId = _uiToolId;

                EquipTool(m_uiActiveToolId);
            }
        }
	}


    public void IncrementTool()
    {
        if ((m_uiActiveToolId + 1) <= (m_uiToolCapacity - 1))
        {
            ChangeTool(m_uiActiveToolId + 1);
        }
    }


    public void DecrementTool()
    {
        if (m_uiActiveToolId != 0)
        {
            ChangeTool(m_uiActiveToolId - 1);
        }
    }


    public void SetToolCapacity(uint _uiNewCapacity)
    {
        for (uint i = _uiNewCapacity; i < m_uiToolCapacity; i++)
        {
            DropTool(i);
        }
        m_uiToolCapacity = _uiNewCapacity;
    }


    public void StoreTool(uint _uiToolId)
    {
        if (m_uiActiveToolId == _uiToolId)
        {

        }
    }


    public void EquipTool(uint _uiToolId)
    {
        if(_uiToolId == m_uiActiveToolId)
        {
            Vector3 ToolOffset = new Vector3(-1, 1, 0);

            //m_cTools[_uiToolId].GetComponent<GameObject>();
            m_cTools[_uiToolId].transform.rotation = transform.rotation;
            m_cTools[_uiToolId].transform.position = transform.position + (transform.forward);
            m_cTools[_uiToolId].transform.localPosition = ToolOffset;
            Debug.Log("Tool set to player position");
        }
    }


    public void UnEquipTool(uint _uiToolId)
    {
        if(_uiToolId != m_uiActiveToolId)
        {
            Vector3 ToolOffset2 = new Vector3(1, -1, 0);

            //m_cTools[_uiToolId].GetComponent<GameObject>();
            m_cTools[_uiToolId].transform.rotation = transform.rotation;
            m_cTools[_uiToolId].transform.position = transform.position + (transform.forward);
            m_cTools[_uiToolId].transform.localPosition = ToolOffset2;
            Debug.Log("Tool set to player position");
        }
    }


    // Member Fields
    GameObject[] m_cTools = new GameObject[k_uiMaxTools];

    uint m_uiToolCapacity = 2;

	public uint m_uiActiveToolId;
};
