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


/* Implementation */


public class CRoomControlConsole : MonoBehaviour
{

// Member Types


// Member Delegates & Events
	
	
// Member Fields
	public GameObject m_ControlConsole = null;
	public List<GameObject> m_Doors = new List<GameObject>();
	
    private Dictionary<DUIButton, CDoorMotor> m_buttonDoorPairs = new Dictionary<DUIButton, CDoorMotor>();
	private Dictionary<DUIField, CDoorMotor> m_fieldDoorPairs = new Dictionary<DUIField, CDoorMotor>();
	
// Member Properties


// Member Methods


	public void Start()
	{
		// Get the console script from the console object
		DUIConsole console = m_ControlConsole.GetComponent<DUIConsole>();
		
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
	}
	
	private void Update()
	{
		if(CNetwork.IsServer)
		{
			// Get the console script from the console object
			DUIConsole console = m_ControlConsole.GetComponent<DUIConsole>();
			
			// Check all actors for collisions with the screen
			foreach(GameObject actor in CGame.Actors)
			{
				ActorMotor actorMotor = actor.GetComponent<ActorMotor>();
				
				Vector3 orig = actorMotor.ActorHead.transform.position;
				Vector3 direction = actorMotor.ActorHead.transform.TransformDirection(Vector3.forward);
				
				if((actorMotor.CurrentInputState & (uint)ActorMotor.InputStates.Action) != 0 && 
					(actorMotor.PreviousInputState & (uint)ActorMotor.InputStates.Action) == 0)
				{
					console.CheckScreenCollision(orig, direction);
				}
			}
		}
	}
	
	private void OpenCloseDoor(DUIButton _sender)
    {
        CDoorMotor door = m_buttonDoorPairs[_sender];

        if (door.State == CDoorMotor.DoorState.Closed)
        {
            m_buttonDoorPairs[_sender].OpenDoor();
            
        }
        else if (door.State == CDoorMotor.DoorState.Opened)
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
					case CDoorMotor.DoorState.Opened: 
                    	button.m_text = "Close";
                    	break;
                    
					case CDoorMotor.DoorState.Closed:
                   	 	button.m_text = "Open";
                    	break;
					
					case CDoorMotor.DoorState.Opening:
                	case CDoorMotor.DoorState.Closing:
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
                    case CDoorMotor.DoorState.Opened: 
                        field.m_text = field.m_text.Replace("Opening...", "Open");
                        break;
                    case CDoorMotor.DoorState.Opening:
                        field.m_text = field.m_text.Replace("Closed", "Opening...");
                        break;
                    case CDoorMotor.DoorState.Closed:
                        field.m_text = field.m_text.Replace("Closing...", "Closed");
                        break;
                    case CDoorMotor.DoorState.Closing:
                        field.m_text = field.m_text.Replace("Open", "Closing...");
                        break;
                    default:
                        break;
                }
			}
		}
    }

};
