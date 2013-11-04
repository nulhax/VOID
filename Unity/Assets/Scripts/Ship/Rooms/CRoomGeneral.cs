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
		SelectLocalExpansionPort,
		SelectFacilityType,
		SelectOtherExpansionPort,
		CreateExpansion,
	}

// Member Delegates & Events
	
	
// Member Fields
	private GameObject m_RoomControlConsole = null;
	private List<GameObject> m_Doors = new List<GameObject>();
	
	private DUISubView m_duiDoorControl;
	
    private Dictionary<DUIButton, CDoorMotor> m_buttonDoorPairs = new Dictionary<DUIButton, CDoorMotor>();
	private Dictionary<DUIField, CDoorMotor> m_fieldDoorPairs = new Dictionary<DUIField, CDoorMotor>();
	
	private DUISubView m_duiExpansionControl;
	
	private Dictionary<DUIButton, uint> m_buttonLocalPortPairs = new Dictionary<DUIButton, uint>();
	private Dictionary<DUIButton, uint> m_buttonOtherPortPairs = new Dictionary<DUIButton, uint>();
	private Dictionary<DUIButton, CRoomInterface.ERoomType> m_buttonRoomTypePairs = new Dictionary<DUIButton, CRoomInterface.ERoomType>();
	
	private CNetworkVar<int> m_ServerCreateExpansionStage    	 	= null;
	private EExpansionCreatePhase m_CreateExpansionStage 			= EExpansionCreatePhase.INVALID;
	
	private uint m_LocalExpansionPortIdSelected = 0;
	private uint m_OtherExpansionPortIdSelected = 0;
	private CRoomInterface.ERoomType m_FacilitySelected = CRoomInterface.ERoomType.INVALID;
	
// Member Properties
	public EExpansionCreatePhase ServerCreateExpansionStage 
	{ 
		get 
		{ 
			return((EExpansionCreatePhase)m_ServerCreateExpansionStage.Get()); 
		}
		set 
		{ 
			m_ServerCreateExpansionStage.Set((int)value);
			m_CreateExpansionStage = value;
		}
	}

// Member Methods
	public override void InstanceNetworkVars()
    {
		m_ServerCreateExpansionStage = new CNetworkVar<int>(OnNetworkVarSync, (int)EExpansionCreatePhase.INVALID);
	}
	
	
	public void OnNetworkVarSync(INetworkVar _rSender)
    {
		if(!Network.isServer)
		{
			// Create Expansion State
			if(_rSender == m_ServerCreateExpansionStage)
			{
				m_CreateExpansionStage = ServerCreateExpansionStage;
			}
		}
    }
	

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
        m_duiDoorControl = console.m_DUIMV.AddSubview("DoorControl");
		
		SetupDoorsSubview();
		
		// Add the expansion control subview
        m_duiExpansionControl = console.m_DUIMV.AddSubview("ExpansionControl");
		
		SetupExpansionSubviewStageOne();
	}
	
	static int blah = 0;
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
				
				if((actorMotor.CurrentInputState & (uint)CPlayerMotor.EInputStates.Action) != 0)
				{
					Debug.Log(string.Format("Click: {0}", blah++));
					console.CheckScreenCollision(orig, direction);
				}
			}
			
			if(ServerCreateExpansionStage == EExpansionCreatePhase.CreateExpansion)
			{
				CGame.Ship.GetComponent<CShipRooms>().CreateRoom(m_FacilitySelected, GetComponent<CRoomInterface>().RoomId, m_LocalExpansionPortIdSelected, m_OtherExpansionPortIdSelected);
				
				m_FacilitySelected = CRoomInterface.ERoomType.INVALID;
				m_LocalExpansionPortIdSelected = 0;
				m_OtherExpansionPortIdSelected = 0;
				
				ServerCreateExpansionStage = EExpansionCreatePhase.SelectLocalExpansionPort;
			}
		}
		
		if(m_CreateExpansionStage == EExpansionCreatePhase.SelectLocalExpansionPort)
		{
			SetupExpansionSubviewStageOne();
			m_CreateExpansionStage = EExpansionCreatePhase.INVALID;
		}
		else if(m_CreateExpansionStage == EExpansionCreatePhase.SelectFacilityType)
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
	
	
	private void SetupDoorsSubview()
	{
		// For each door add a button
        for(int i = 0; i < m_Doors.Count; ++i)
        {
			CDoorMotor door = m_Doors[i].GetComponent<CDoorMotor>();
			
			// Add the door buttons
            DUIButton duiBut = m_duiDoorControl.AddButton("Open");
            duiBut.PressDown += OpenCloseDoor;

            m_buttonDoorPairs[duiBut] = door;
			
			// Add the door status field
			DUIField duiDoorState = m_duiDoorControl.AddField(string.Format("Door {0}: Closed", i + 1));
		
			m_fieldDoorPairs[duiDoorState] = door;
			
			// Set their positions
			duiDoorState.m_viewPos = new Vector2(0.1f, (float)(i + 1) / (float)(m_Doors.Count + 1));
			duiBut.transform.localPosition = duiDoorState.transform.localPosition + new Vector3(duiDoorState.m_dimensions.x * 0.5f + duiBut.m_dimensions.x * 0.5f, 0.0f);
			duiBut.m_viewPos = new Vector2(duiBut.m_viewPos.x + 0.1f, duiBut.m_viewPos.y);
			
			// Register the statechange
			door.StateChanged += DoorStateChanged;
        }
	}
	
	
	private void SetupExpansionSubviewStageOne()
	{
		// Clear the existing elements
		m_duiExpansionControl.ClearDUIElements();
		
		// Add the title field
		DUIField diuField = m_duiExpansionControl.AddField("Select a LOCAL expansion port to use.");
		diuField.m_viewPos = new Vector2(0.5f, 1.0f);
		
		// For each expansion port add a button
		List<GameObject> expansionPorts = GetComponent<CRoomInterface>().ExpansionPorts;
		for(int i = 0; i < expansionPorts.Count; ++i)
		{
			GameObject expansionPort = expansionPorts[i];
				
			// Add the expansion port buttons
            DUIButton duiBut = m_duiExpansionControl.AddButton(string.Format("Expansion Port: {0}", expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId + 1));
            duiBut.PressDown += ExpansionSubviewSelectLocalPort;

			// Set the positions
			duiBut.m_viewPos = new Vector2(0.0f, (float)(i + 1) / (float)(expansionPorts.Count + 1));
			
			m_buttonLocalPortPairs[duiBut] = expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId;
		}
	}
	
	
	private void SetupExpansionSubviewStageTwo()
	{
		// Clear the existing elements
		m_duiExpansionControl.ClearDUIElements();
		
		// Add the title field
		DUIField diuField = m_duiExpansionControl.AddField("Select a room to create.");
		diuField.m_viewPos = new Vector2(0.5f, 1.0f);
		
		// For each room type
		for(int i = 0; i < (int)CRoomInterface.ERoomType.MAX; ++i)
		{
			CRoomInterface.ERoomType roomType = (CRoomInterface.ERoomType)i;
			
			// Add the expansion port buttons
            DUIButton duiBut = m_duiExpansionControl.AddButton(string.Format("Room: {0}", roomType.ToString()));
            duiBut.PressDown += ExpansionSubviewSelectFacility;

			// Set the positions
			duiBut.m_viewPos = new Vector2(0.0f, (float)(i + 1) / (float)((int)CRoomInterface.ERoomType.MAX + 1));
			
			m_buttonRoomTypePairs[duiBut] = roomType;
		}	
	}
	
	
	private void SetupExpansionSubviewStageThree()
	{
		// Clear the existing elements
		m_duiExpansionControl.ClearDUIElements();

		// Add the title field
		DUIField diuField = m_duiExpansionControl.AddField("Select an OTHER expansion port to use.");
		diuField.m_viewPos = new Vector2(0.5f, 1.0f);
		
		// Get the local prefab string
		string prefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CRoomInterface.GetRoomPrefab(m_FacilitySelected));
		GameObject tempRoomObject = GameObject.Instantiate(Resources.Load("Prefabs/" + prefabFile, typeof(GameObject))) as GameObject;
		
		// For each expansion port add a button
		List<GameObject> expansionPorts = tempRoomObject.GetComponent<CRoomInterface>().ExpansionPorts;
		for(int i = 0; i < expansionPorts.Count; ++i)
		{
			GameObject expansionPort = expansionPorts[i];
				
			// Add the expansion port buttons
            DUIButton duiBut = m_duiExpansionControl.AddButton(string.Format("Expansion Port: {0}", expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId + 1));
            duiBut.PressDown += ExpansionSubviewSelectOtherPort;

			// Set the positions
			duiBut.m_viewPos = new Vector2(0.0f, (float)(i + 1) / (float)(expansionPorts.Count + 1));
			
			m_buttonOtherPortPairs[duiBut] = expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId;
		}
		
		// Destory it
		Destroy(tempRoomObject);
	}
	
	
	private void ExpansionSubviewSelectLocalPort(DUIButton _sender)
    {
        m_LocalExpansionPortIdSelected = m_buttonLocalPortPairs[_sender];
		
		ServerCreateExpansionStage = EExpansionCreatePhase.SelectFacilityType;
    }
	
	
	private void ExpansionSubviewSelectFacility(DUIButton _sender)
    {
        m_FacilitySelected = m_buttonRoomTypePairs[_sender];
		
		ServerCreateExpansionStage = EExpansionCreatePhase.SelectOtherExpansionPort;
    }
	
	
	private void ExpansionSubviewSelectOtherPort(DUIButton _sender)
    {
        m_OtherExpansionPortIdSelected = m_buttonOtherPortPairs[_sender];
		
		ServerCreateExpansionStage = EExpansionCreatePhase.CreateExpansion;
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
