//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomFactory.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;


/* Implementation */


public class CRoomGeneral : MonoBehaviour
{

// Member Types


// Member Delegates & Events
	
	
// Member Fields
	private GameObject m_RoomControlConsole = null;
	private List<GameObject> m_Doors = new List<GameObject>();
	
    private Dictionary<DUIButton, CDoorMotor> m_buttonDoorPairs = new Dictionary<DUIButton, CDoorMotor>();
	private Dictionary<DUIField, CDoorMotor> m_fieldDoorPairs = new Dictionary<DUIField, CDoorMotor>();
	
	private Dictionary<DUIButton, CExpansionPortInterface> m_buttonPortPairs = new Dictionary<DUIButton, CExpansionPortInterface>();
	private Dictionary<DUIButton, CRoomInterface.ERoomType> m_buttonRoomTypePairs = new Dictionary<DUIButton, CRoomInterface.ERoomType>();
	
	CExpansionPortInterface m_ExpansionPortSelected = null;
	CRoomInterface.ERoomType m_RoomSelected = CRoomInterface.ERoomType.INVALID;
	
// Member Properties


// Member Methods
	public void Start()
	{
		// Get the console script from the children
		DUIConsole console = GetComponentInChildren<DUIConsole>();
		
		// Store the room control console game object
		m_RoomControlConsole = console.gameObject;
		
		// Get the door interface scripts from the children
		foreach(CDoorInterface door in GetComponentsInChildren<CDoorInterface>())
		{
			m_Doors.Add(door.gameObject);
		}
		
		 // Initialise the console
        console.Initialise();
		
		// Add the room control subview
        DUISubView duiRC = console.m_DUIMV.AddSubview("DoorControl");
		
		// For each door add a button
        for(int i = 0; i < m_Doors.Count; ++i)
        {
			CDoorMotor door = m_Doors[i].GetComponent<CDoorMotor>();
			
			// Add the door buttons
            DUIButton duiBut = duiRC.AddButton("Open");
            duiBut.PressDown += OpenCloseDoor;

            m_buttonDoorPairs[duiBut] = door;
			
			// Add the door status field
			DUIField duiDoorState = duiRC.AddField(string.Format("Door {0}: Closed", i + 1));
		
			m_fieldDoorPairs[duiDoorState] = door;
			
			// Set their positions
			duiDoorState.m_viewPos = new Vector2(0.1f, 0.5f - (((i + 0.5f) - m_Doors.Count * 0.5f) / m_Doors.Count) * 1.0f / (m_Doors.Count - 1));
			duiBut.transform.localPosition = duiDoorState.transform.localPosition + new Vector3(duiDoorState.m_dimensions.x * 0.5f + duiBut.m_dimensions.x * 0.5f, 0.0f);
			duiBut.m_viewPos = new Vector2(duiBut.m_viewPos.x + 0.1f, duiBut.m_viewPos.y);
			
			// Register the statechange
			door.StateChanged += DoorStateChanged;
        }
		
		// Add the expansion control subview
        DUISubView duiEC = console.m_DUIMV.AddSubview("ExpansionControl");
		
		SetupExpansionSubviewStageOne();
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			// Get the console script from the console object
			DUIConsole console = m_RoomControlConsole.GetComponent<DUIConsole>();
			
			// Check all actors for collisions with the screen
			foreach(GameObject actor in CGame.Actors)
			{
				CPlayerMotor actorMotor = actor.GetComponent<CPlayerMotor>();
				
				Vector3 orig = actorMotor.ActorHead.transform.position;
				Vector3 direction = actorMotor.ActorHead.transform.TransformDirection(Vector3.forward);
				
				if((actorMotor.CurrentInputState & (uint)CPlayerMotor.EInputStates.Action) != 0 && 
					(actorMotor.PreviousInputState & (uint)CPlayerMotor.EInputStates.Action) == 0)
				{
					console.CheckScreenCollision(orig, direction);
				}
			}
		}
	}
	
	public void ServerCreateDoors()
	{
		foreach(GameObject expansionPort in GetComponent<CRoomInterface>().ExpansionPorts)
		{
			CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.Door;
			GameObject newDoorObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
		
			newDoorObject.transform.position = expansionPort.transform.position + new Vector3(0.0f, newDoorObject.collider.bounds.extents.y, 0.0f);
			newDoorObject.transform.rotation = expansionPort.transform.rotation;
			newDoorObject.transform.parent = transform;	
			
			newDoorObject.GetComponent<CDoorMotor>().OpenDoor();
			
			newDoorObject.GetComponent<CDoorInterface>().DoorId = (uint)m_Doors.Count;
			
			newDoorObject.GetComponent<CNetworkView>().SyncParent();
			newDoorObject.GetComponent<CNetworkView>().SyncTransformPosition();
			newDoorObject.GetComponent<CNetworkView>().SyncTransformRotation();
		}
	}
	
	
	public void CreateRoomControlConsole()
	{
		Transform consoleTransform = transform.FindChild("ControlConsole");

		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.ControlConsole;
		GameObject newConsoleObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
	
		newConsoleObject.transform.position = consoleTransform.position;
		newConsoleObject.transform.rotation = consoleTransform.rotation;
		
		newConsoleObject.GetComponent<CNetworkView>().InvokeRpcAll("SetParent", GetComponent<CNetworkView>().ViewId);	
		
		newConsoleObject.GetComponent<CNetworkView>().SyncParent();
		newConsoleObject.GetComponent<CNetworkView>().SyncTransformPosition();
		newConsoleObject.GetComponent<CNetworkView>().SyncTransformRotation();
	}
	
	
	private void SetupExpansionSubviewStageOne()
	{
		DUIConsole console = m_RoomControlConsole.GetComponent<DUIConsole>();
		DUISubView duiEC = console.m_DUIMV.GetSubView("ExpansionControl");
		
		// Clear the existing elements
		duiEC.ClearDUIElements();
		
		// Add the title field
		DUIField diuField = duiEC.AddField("Select an expansion port to use.");
		diuField.m_viewPos = new Vector2(0.5f, 1.0f);
		
		// For each expansion port add a button
		List<GameObject> expansionPorts = GetComponent<CRoomInterface>().ExpansionPorts;
		for(int i = 0; i < expansionPorts.Count; ++i)
		{
			GameObject expansionPort = expansionPorts[i];
			CExpansionPortInterface epi = expansionPort.GetComponent<CExpansionPortInterface>();
				
			// Add the expansion port buttons
            DUIButton duiBut = duiEC.AddButton(string.Format("Expansion Port: {0}", epi.ExpansionPortId));
            duiBut.PressDown += ExpansionSubviewSelectPort;

			// Set the positions
			duiBut.m_viewPos = new Vector2(0.1f, (float)(i + 1) / (float)(expansionPorts.Count + 1));
			
			m_buttonPortPairs[duiBut] = epi;
		}
	}
	
	
	private void SetupExpansionSubviewStageTwo()
	{
		DUIConsole console = m_RoomControlConsole.GetComponent<DUIConsole>();
		DUISubView duiEC = console.m_DUIMV.GetSubView("ExpansionControl");
		
		// Clear the existing elements
		duiEC.ClearDUIElements();
		
		// Add the title field
		DUIField diuField = duiEC.AddField("Select a room to create.");
		diuField.m_viewPos = new Vector2(0.5f, 1.0f);
		
		// For each room type
		for(int i = 0; i < (int)CRoomInterface.ERoomType.MAX; ++i)
		{
			CRoomInterface.ERoomType roomType = (CRoomInterface.ERoomType)i;
			
			// Add the expansion port buttons
            DUIButton duiBut = duiEC.AddButton(string.Format("Room: {0}", roomType.ToString()));
            duiBut.PressDown += ExpansionSubviewSelectRoom;

			// Set the positions
			duiBut.m_viewPos = new Vector2(0.1f, (float)(i + 1) / (float)((int)CRoomInterface.ERoomType.MAX + 1));
			
			m_buttonRoomTypePairs[duiBut] = roomType;
		}	
	}
	
	
	private void ExpansionSubviewSelectPort(DUIButton _sender)
    {
        m_ExpansionPortSelected = m_buttonPortPairs[_sender];
		
		SetupExpansionSubviewStageTwo();
    }
	
	
	private void ExpansionSubviewSelectRoom(DUIButton _sender)
    {
        m_RoomSelected = m_buttonRoomTypePairs[_sender];
		
		CGame.Ship.GetComponent<CShipRooms>().CreateRoom(m_RoomSelected, GetComponent<CRoomInterface>().RoomId, m_ExpansionPortSelected.ExpansionPortId);
    }
	
	
	private void OpenCloseDoor(DUIButton _sender)
    {
        CDoorMotor door = m_buttonDoorPairs[_sender];

        if (door.State == CDoorMotor.EDoorState.Closed)
        {
            m_buttonDoorPairs[_sender].OpenDoor();
            
        }
        else if (door.State == CDoorMotor.EDoorState.Opened)
        {
            m_buttonDoorPairs[_sender].CloseDoor();
        }
    }

    private void DoorStateChanged(CDoorMotor _sender)
    {	
		foreach(DUIButton button in m_buttonDoorPairs.Keys)
		{
			if(m_buttonDoorPairs[button] == _sender)
			{
				switch (_sender.State)
                {
					case CDoorMotor.EDoorState.Opened: 
                    	button.m_text = "Close";
                    	break;
                    
					case CDoorMotor.EDoorState.Closed:
                   	 	button.m_text = "Open";
                    	break;
					
					case CDoorMotor.EDoorState.Opening:
                	case CDoorMotor.EDoorState.Closing:
                    	button.m_text = "...";
                    	break;
					
                	default:
                    	break;
                }
			}
		}
		
		foreach(DUIField field in m_fieldDoorPairs.Keys)
		{
			if(m_fieldDoorPairs[field] == _sender)
			{
				switch (_sender.State)
                {
                    case CDoorMotor.EDoorState.Opened: 
                        field.m_text = field.m_text.Replace("Opening...", "Open");
                        break;
                    case CDoorMotor.EDoorState.Opening:
                        field.m_text = field.m_text.Replace("Closed", "Opening...");
                        break;
                    case CDoorMotor.EDoorState.Closed:
                        field.m_text = field.m_text.Replace("Closing...", "Closed");
                        break;
                    case CDoorMotor.EDoorState.Closing:
                        field.m_text = field.m_text.Replace("Open", "Closing...");
                        break;
                    default:
                        break;
                }
			}
		}
    }

};
