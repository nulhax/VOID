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


public class CRoomGeneral : CNetworkMonoBehaviour
{

// Member Types
	public enum EExpansionCreatePhase
	{
		INVALID,
		SelectFacilityType,
		SelectLocalExpansionPort,
		SelectOtherExpansionPort,
		CreateExpansion,
	}

// Member Delegates & Events
	
	
// Member Fields
	private GameObject m_RoomControlConsole = null;
	private List<GameObject> m_Doors = new List<GameObject>();
	
	private GameObject m_DoorControlSubView;
	
    private Dictionary<CDUIButton, CDoorMotor> m_buttonDoorPairs = new Dictionary<CDUIButton, CDoorMotor>();
	private Dictionary<CDUIField, CDoorMotor> m_fieldDoorPairs = new Dictionary<CDUIField, CDoorMotor>();
	
	private GameObject m_ExpansionControlSubView;
	
	private Dictionary<CDUIButton, uint> m_buttonLocalPortPairs = new Dictionary<CDUIButton, uint>();
	private Dictionary<CDUIButton, uint> m_buttonOtherPortPairs = new Dictionary<CDUIButton, uint>();
	private Dictionary<CDUIButton, CRoomInterface.ERoomType> m_buttonRoomTypePairs = new Dictionary<CDUIButton, CRoomInterface.ERoomType>();
	
	private CNetworkVar<int> m_ServerCreateExpansionStage    	 	= null;
	private EExpansionCreatePhase m_CreateExpansionStage 			= EExpansionCreatePhase.INVALID;
	
	private uint m_LocalExpansionPortIdSelected = 0;
	private uint m_OtherExpansionPortIdSelected = 0;
	private CRoomInterface.ERoomType m_FacilitySelected = CRoomInterface.ERoomType.INVALID;
	
// Member Properties
	public GameObject RoomControlConsole { get{ return(m_RoomControlConsole); } }

// Member Methods
	public override void InstanceNetworkVars()
    {
		m_ServerCreateExpansionStage = new CNetworkVar<int>(OnNetworkVarSync, (int)EExpansionCreatePhase.INVALID);
	}
	
	
	public void OnNetworkVarSync(INetworkVar _rSender)
    {
		
    }
	

	public void Start()
	{
		// Get the console script from the children
		CDUIConsole console = GetComponentInChildren<CDUIConsole>();
		
		// Store the room control console game object
 		m_RoomControlConsole = console.gameObject;
		
		// Get the door interface scripts from the children
		foreach(CDoorInterface door in GetComponentsInChildren<CDoorInterface>())
		{
			m_Doors.Add(door.gameObject);
		}
		
		 // Initialise the console
        console.Initialise(EQuality.High, ELayoutStyle.Layout_1, new Vector2(2.0f, 1.0f));
		
		// Add the room control subview
        m_DoorControlSubView = console.DUI.AddSubView().gameObject;
		
		// Setup the doors subview
		SetupDoorsSubview();
		
		// Add the expansion control subview
        m_ExpansionControlSubView = console.DUI.AddSubView().gameObject;
		
		// Set the initialise create expansion stage.
		m_CreateExpansionStage = EExpansionCreatePhase.SelectFacilityType;
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{	
			if(m_CreateExpansionStage == EExpansionCreatePhase.CreateExpansion)
			{
				CGame.Ship.GetComponent<CShipRooms>().CreateRoom(m_FacilitySelected, GetComponent<CRoomInterface>().RoomId, m_LocalExpansionPortIdSelected, m_OtherExpansionPortIdSelected);
				
				m_FacilitySelected = CRoomInterface.ERoomType.INVALID;
				m_LocalExpansionPortIdSelected = 0;
				m_OtherExpansionPortIdSelected = 0;
			}
		}
		
		if(m_CreateExpansionStage == EExpansionCreatePhase.CreateExpansion)
		{
			m_CreateExpansionStage = EExpansionCreatePhase.SelectFacilityType;
		}
		else if(m_CreateExpansionStage == EExpansionCreatePhase.SelectFacilityType)
		{
			SetupExpansionSubviewStageOne();
			m_CreateExpansionStage = EExpansionCreatePhase.INVALID;
		}
		else if(m_CreateExpansionStage == EExpansionCreatePhase.SelectLocalExpansionPort)
		{
			SetupExpansionSubviewStageTwo();
			m_CreateExpansionStage = EExpansionCreatePhase.INVALID;
		}
		else if(m_CreateExpansionStage == EExpansionCreatePhase.SelectOtherExpansionPort)
		{
			SetupExpansionSubviewStageThree();
			m_CreateExpansionStage = EExpansionCreatePhase.INVALID;
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
			
			newDoorObject.GetComponent<CDoorInterface>().DoorId = (uint)m_Doors.Count;
			
			newDoorObject.GetComponent<CNetworkView>().SyncParent();
			newDoorObject.GetComponent<CNetworkView>().SyncTransformPosition();
			newDoorObject.GetComponent<CNetworkView>().SyncTransformRotation();
			
			// Test: Make the door open by default
			newDoorObject.GetComponent<CDoorMotor>().OpenDoor();
		}
	}
	
	
	public void ServerCreateControlConsole()
	{
		Transform consoleTransform = transform.FindChild("ControlConsole");

		CGame.ENetworkRegisteredPrefab eRegisteredPrefab = CGame.ENetworkRegisteredPrefab.ControlConsole;
		GameObject newConsoleObject = CNetwork.Factory.CreateObject(eRegisteredPrefab);
	
		newConsoleObject.transform.position = consoleTransform.position;
		newConsoleObject.transform.rotation = consoleTransform.rotation;
		newConsoleObject.transform.parent = transform;	
		
		newConsoleObject.GetComponent<CNetworkView>().SyncParent();
		newConsoleObject.GetComponent<CNetworkView>().SyncTransformPosition();
		newConsoleObject.GetComponent<CNetworkView>().SyncTransformRotation();
	}
	
	
	private void SetupDoorsSubview()
	{
		CDUISubView duiDoorControl = m_DoorControlSubView.GetComponent<CDUISubView>();
		
		// For each door add a button
        for(int i = 0; i < m_Doors.Count; ++i)
        {
			CDoorMotor door = m_Doors[i].GetComponent<CDoorMotor>();
			
			// Add the door buttons
            CDUIButton duiBut = duiDoorControl.AddButton("Open");
            duiBut.PressDown += OpenCloseDoor;

            m_buttonDoorPairs[duiBut] = door;
			
			// Add the door status field
			CDUIField duiDoorState = duiDoorControl.AddField(string.Format("Door {0}: Closed", i + 1));
		
			m_fieldDoorPairs[duiDoorState] = door;
			
			// Set their positions
			duiDoorState.m_ViewPos = new Vector2(0.1f, (float)(i + 1) / (float)(m_Doors.Count + 1));
			duiBut.transform.localPosition = duiDoorState.transform.localPosition + new Vector3(duiDoorState.Dimensions.x * 0.5f + duiBut.Dimensions.x * 0.5f, 0.0f);
			duiBut.m_ViewPos = new Vector2(duiBut.m_ViewPos.x + 0.1f, duiBut.m_ViewPos.y);
			
			// Register the statechange
			door.StateChanged += DoorStateChanged;
        }
	}
	
	
	private void SetupExpansionSubviewStageOne()
	{
		// Clear the existing elements
		CDUISubView duiExpansionControl = m_ExpansionControlSubView.GetComponent<CDUISubView>();
		duiExpansionControl.ClearDUIElements();
		
		// Add the title field
		CDUIField diuField = duiExpansionControl.AddField("Select a room to create.");
		diuField.m_ViewPos = new Vector2(0.5f, 1.0f);
		
		// For each room type
		for(int i = 0; i < (int)CRoomInterface.ERoomType.MAX; ++i)
		{
			CRoomInterface.ERoomType roomType = (CRoomInterface.ERoomType)i;
			
			// Add the expansion port buttons
            CDUIButton duiBut = duiExpansionControl.AddButton(string.Format("Room: {0}", roomType.ToString()));
            duiBut.PressDown += ExpansionSubviewSelectFacility;

			// Set the positions
			duiBut.m_ViewPos = new Vector2(0.0f, (float)(i + 1) / (float)((int)CRoomInterface.ERoomType.MAX + 1));
			
			m_buttonRoomTypePairs[duiBut] = roomType;
		}
	}
	
	
	private void SetupExpansionSubviewStageTwo()
	{
		// Clear the existing elements
		CDUISubView duiExpansionControl = m_ExpansionControlSubView.GetComponent<CDUISubView>();
		duiExpansionControl.ClearDUIElements();
		
		// Add the title field
		CDUIField diuField = duiExpansionControl.AddField("Select a LOCAL expansion port to use.");
		diuField.m_ViewPos = new Vector2(0.0f, 1.0f);
		
		// For each expansion port add a button
		List<GameObject> expansionPorts = GetComponent<CRoomInterface>().ExpansionPorts;
		for(int i = 0; i < expansionPorts.Count; ++i)
		{
			GameObject expansionPort = expansionPorts[i];
				
			// Add the expansion port buttons
            CDUIButton duiBut = duiExpansionControl.AddButton(string.Format("Expansion Port: {0}", expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId + 1));
            duiBut.PressDown += ExpansionSubviewSelectLocalPort;

			// Set the positions
			duiBut.m_ViewPos = new Vector2(0.0f, (float)(i + 1) / (float)(expansionPorts.Count + 1));
			
			m_buttonLocalPortPairs[duiBut] = expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId;
		}
	}
	
	
	private void SetupExpansionSubviewStageThree()
	{
		// Clear the existing elements
		CDUISubView duiExpansionControl = m_ExpansionControlSubView.GetComponent<CDUISubView>();
		duiExpansionControl.ClearDUIElements();

		// Add the title field
		CDUIField diuField = duiExpansionControl.AddField("Select an OTHER expansion port to use.");
		diuField.m_ViewPos = new Vector2(0.0f, 1.0f);
		
		// Get the local prefab string
		string prefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CRoomInterface.GetRoomPrefab(m_FacilitySelected));
		GameObject tempRoomObject = GameObject.Instantiate(Resources.Load("Prefabs/" + prefabFile, typeof(GameObject))) as GameObject;
		
		// For each expansion port add a button
		List<GameObject> expansionPorts = tempRoomObject.GetComponent<CRoomInterface>().ExpansionPorts;
		for(int i = 0; i < expansionPorts.Count; ++i)
		{
			GameObject expansionPort = expansionPorts[i];
				
			// Add the expansion port buttons
            CDUIButton duiBut = duiExpansionControl.AddButton(string.Format("Expansion Port: {0}", expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId + 1));
            duiBut.PressDown += ExpansionSubviewSelectOtherPort;

			// Set the positions
			duiBut.m_ViewPos = new Vector2(0.0f, (float)(i + 1) / (float)(expansionPorts.Count + 1));
			
			m_buttonOtherPortPairs[duiBut] = expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId;
		}
		
		// Destory it
		Destroy(tempRoomObject);
	}
	
	
	private void ExpansionSubviewSelectLocalPort(CDUIButton _sender)
    {
        m_LocalExpansionPortIdSelected = m_buttonLocalPortPairs[_sender];
		
		m_CreateExpansionStage = EExpansionCreatePhase.SelectOtherExpansionPort;
    }
	
	
	private void ExpansionSubviewSelectFacility(CDUIButton _sender)
    {
        m_FacilitySelected = m_buttonRoomTypePairs[_sender];
		
		m_CreateExpansionStage = EExpansionCreatePhase.SelectLocalExpansionPort;
    }
	
	
	private void ExpansionSubviewSelectOtherPort(CDUIButton _sender)
    {
        m_OtherExpansionPortIdSelected = m_buttonOtherPortPairs[_sender];
		
		m_CreateExpansionStage = EExpansionCreatePhase.CreateExpansion;
    }
	
	
	private void OpenCloseDoor(CDUIButton _sender)
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
		foreach(CDUIButton button in m_buttonDoorPairs.Keys)
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
		
		foreach(CDUIField field in m_fieldDoorPairs.Keys)
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
