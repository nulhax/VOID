//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityFactory.cs
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


public class CFacilityGeneral : CNetworkMonoBehaviour
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
	public GameObject m_FacilityControlConsole = null;
	public List<GameObject> m_Doors = new List<GameObject>();
	
	private GameObject m_DoorControlSubView;
	
    //private Dictionary<CDUIButton, CDoorMotor> m_buttonDoorPairs = new Dictionary<CDUIButton, CDoorMotor>();
	//private Dictionary<CDUIField, CDoorMotor> m_fieldDoorPairs = new Dictionary<CDUIField, CDoorMotor>();
	
	private GameObject m_ExpansionControlSubView;
	
	//private Dictionary<CDUIButton, uint> m_buttonLocalPortPairs = new Dictionary<CDUIButton, uint>();
	//private Dictionary<CDUIButton, uint> m_buttonOtherPortPairs = new Dictionary<CDUIButton, uint>();
	//private Dictionary<CDUIButton, CFacilityInterface.EType> m_buttonFacilityTypePairs = new Dictionary<CDUIButton, CFacilityInterface.EType>();
	
	private CNetworkVar<int> m_ServerCreateExpansionStage    	 	= null;
	private EExpansionCreatePhase m_CreateExpansionStage 			= EExpansionCreatePhase.INVALID;
	
	private CNetworkVar<uint> m_ServerLocalExpansionPortIdSelected  = null;
	private CNetworkVar<uint> m_ServerOtherExpansionPortIdSelected  = null;
	private CNetworkVar<int> m_ServerFacilitySelected  = null;
	
	private uint m_LocalExpansionPortIdSelected = 0;
	private uint m_OtherExpansionPortIdSelected = 0;
	private CFacilityInterface.EType m_FacilitySelected = CFacilityInterface.EType.INVALID;
	
// Member Properties
	public GameObject FacilityControlConsole 
	{ 
		get { return(m_FacilityControlConsole); } 
	}

// Member Methods
	public override void InstanceNetworkVars()
    {
		m_ServerCreateExpansionStage = new CNetworkVar<int>(OnNetworkVarSync, (int)EExpansionCreatePhase.INVALID);
		m_ServerLocalExpansionPortIdSelected = new CNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
		m_ServerOtherExpansionPortIdSelected = new CNetworkVar<uint>(OnNetworkVarSync, uint.MaxValue);
		m_ServerFacilitySelected = new CNetworkVar<int>(OnNetworkVarSync, (int)CFacilityInterface.EType.INVALID);
	}
	
	
	public void OnNetworkVarSync(INetworkVar _rSender)
    {
		m_CreateExpansionStage = (EExpansionCreatePhase)m_ServerCreateExpansionStage.Get();
		m_LocalExpansionPortIdSelected = m_ServerLocalExpansionPortIdSelected.Get();
		m_OtherExpansionPortIdSelected = m_ServerOtherExpansionPortIdSelected.Get();
		m_FacilitySelected = (CFacilityInterface.EType)m_ServerFacilitySelected.Get();
    }
	
	public void Start()
	{		
//		// Add the room control subview
//        m_DoorControlSubView = console.DUI.AddSubView().gameObject;
//		
//		// Setup the doors subview
//		SetupDoorsSubview();
//		
//		// Add the expansion control subview
//        m_ExpansionControlSubView = console.DUI.AddSubView().gameObject;
//		
//		// Set the initialise create expansion stage.
//		SetupExpansionSubviewStageOne();
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{	
			if(m_CreateExpansionStage == EExpansionCreatePhase.CreateExpansion)
			{
				CGameShips.Ship.GetComponent<CShipFacilities>().CreateFacility(m_FacilitySelected, GetComponent<CFacilityInterface>().FacilityId, m_LocalExpansionPortIdSelected, m_OtherExpansionPortIdSelected);
				
				m_FacilitySelected = CFacilityInterface.EType.INVALID;
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
	
	private void SetupDoorsSubview()
	{
//		CDUISubView duiDoorControl = m_DoorControlSubView.GetComponent<CDUISubView>();
//		
//		// For each door add a button
//        for(int i = 0; i < m_Doors.Count; ++i)
//        {
//			CDoorMotor door = m_Doors[i].GetComponent<CDoorMotor>();
//			
//			// Add the door buttons
//            CDUIButton duiBut = duiDoorControl.AddButton("Open");
//            duiBut.PressDown += OpenCloseDoor;
//
//            m_buttonDoorPairs[duiBut] = door;
//			
//			// Add the door status field
//			CDUIField duiDoorState = duiDoorControl.AddField(string.Format("Door {0}: Closed", i + 1));
//		
//			m_fieldDoorPairs[duiDoorState] = door;
//			
//			// Set their positions
//			duiDoorState.MiddleLeftViewPos = new Vector2(0.1f, (float)(i + 1) / (float)(m_Doors.Count + 1));
//			duiBut.MiddleLeftViewPos = new Vector2(duiDoorState.MiddleRightViewPos.x + 0.1f, duiDoorState.MiddleRightViewPos.y);
//			
//			// Register the statechange
//			door.EventDoorStateOpened += DoorStateChanged;
//			door.EventDoorStateOpening += DoorStateChanged;
//			door.EventDoorStateClosed += DoorStateChanged;
//			door.EventDoorStateClosing += DoorStateChanged;
//        }
	}
	
	private void SetupExpansionSubviewStageOne()
	{
//		// Clear the existing elements
//		CDUISubView duiExpansionControl = m_ExpansionControlSubView.GetComponent<CDUISubView>();
//		duiExpansionControl.ClearDUIElements();
//		
//		// Add the title field
//		CDUIField diuField = duiExpansionControl.AddField("Select a room to create.");
//		diuField.LowerLeftViewPos = new Vector2(0.1f, 1.0f);
//		
//		// For each room type
//		for(int i = 0; i < (int)CFacilityInterface.EType.MAX; ++i)
//		{
//			CFacilityInterface.EType roomType = (CFacilityInterface.EType)i;
//			
//			// Add the expansion port buttons
//            CDUIButton duiBut = duiExpansionControl.AddButton(string.Format("Facility: {0}", roomType.ToString()));
//            duiBut.PressDown += ExpansionSubviewSelectFacility;
//
//			// Set the positions
//			duiBut.MiddleLeftViewPos = new Vector2(0.1f, (float)(i + 1) / (float)((int)CFacilityInterface.EType.MAX + 1));
//			
//			m_buttonFacilityTypePairs[duiBut] = roomType;
//		}
	}
	
	private void SetupExpansionSubviewStageTwo()
	{
//		// Clear the existing elements
//		CDUISubView duiExpansionControl = m_ExpansionControlSubView.GetComponent<CDUISubView>();
//		duiExpansionControl.ClearDUIElements();
//		
//		// Add the title field
//		CDUIField diuField = duiExpansionControl.AddField("Select a LOCAL expansion port to use.");
//		diuField.LowerLeftViewPos = new Vector2(0.1f, 1.0f);
//		
//		// For each expansion port add a button
//		List<GameObject> expansionPorts = GetComponent<CFacilityExpansion>().ExpansionPorts;
//		for(int i = 0; i < expansionPorts.Count; ++i)
//		{
//			GameObject expansionPort = expansionPorts[i];
//				
//			// Add the expansion port buttons
//            CDUIButton duiBut = duiExpansionControl.AddButton(string.Format("Expansion Port: {0}", expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId + 1));
//            duiBut.PressDown += ExpansionSubviewSelectLocalPort;
//
//			// Set the positions
//			duiBut.MiddleLeftViewPos = new Vector2(0.1f, (float)(i + 1) / (float)(expansionPorts.Count + 1));
//			
//			m_buttonLocalPortPairs[duiBut] = expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId;
//		}
	}
	
	private void SetupExpansionSubviewStageThree()
	{
//		// Clear the existing elements
//		CDUISubView duiExpansionControl = m_ExpansionControlSubView.GetComponent<CDUISubView>();
//		duiExpansionControl.ClearDUIElements();
//
//		// Add the title field
//		CDUIField diuField = duiExpansionControl.AddField("Select an OTHER expansion port to use.");
//		diuField.LowerLeftViewPos = new Vector2(0.1f, 1.0f);
//		
//		// Get the local prefab string
//		string prefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CFacilityInterface.GetPrefabType(m_FacilitySelected));
//		GameObject tempFacilityObject = GameObject.Instantiate(Resources.Load("Prefabs/" + prefabFile, typeof(GameObject))) as GameObject;
//		
//		// For each expansion port add a button
//		List<GameObject> expansionPorts = tempFacilityObject.GetComponent<CFacilityExpansion>().ExpansionPorts;
//		for(int i = 0; i < expansionPorts.Count; ++i)
//		{
//			GameObject expansionPort = expansionPorts[i];
//				
//			// Add the expansion port buttons
//            CDUIButton duiBut = duiExpansionControl.AddButton(string.Format("Expansion Port: {0}", expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId + 1));
//            duiBut.PressDown += ExpansionSubviewSelectOtherPort;
//
//			// Set the positions
//			duiBut.MiddleLeftViewPos = new Vector2(0.1f, (float)(i + 1) / (float)(expansionPorts.Count + 1));
//			
//			m_buttonOtherPortPairs[duiBut] = expansionPort.GetComponent<CExpansionPortInterface>().ExpansionPortId;
//		}
//		
//		// Destory it
//		Destroy(tempFacilityObject);
	}
	
//	[AServerOnly]
//	private void ExpansionSubviewSelectLocalPort(CDUIButton _sender)
//    {
//        m_ServerLocalExpansionPortIdSelected.Set(m_buttonLocalPortPairs[_sender]);
//			
//		m_ServerCreateExpansionStage.Set((int)EExpansionCreatePhase.SelectOtherExpansionPort);
//    }
//	
//	[AServerOnly]
//	private void ExpansionSubviewSelectFacility(CDUIButton _sender)
//    {
//   	 	m_ServerFacilitySelected.Set((int)m_buttonFacilityTypePairs[_sender]);
//	
//		m_ServerCreateExpansionStage.Set((int)EExpansionCreatePhase.SelectLocalExpansionPort);
//    }
//	
//	[AServerOnly]
//	private void ExpansionSubviewSelectOtherPort(CDUIButton _sender)
//    {
//       	m_ServerOtherExpansionPortIdSelected.Set(m_buttonOtherPortPairs[_sender]);
//		
//		m_ServerCreateExpansionStage.Set((int)EExpansionCreatePhase.CreateExpansion);
//    }
//	
//	[AServerOnly]
//	private void OpenCloseDoor(CDUIButton _sender)
//    {
//        CDoorMotor door = m_buttonDoorPairs[_sender];
//
//        if (door.DoorState == CDoorMotor.EDoorState.Closed)
//        {
//            m_buttonDoorPairs[_sender].OpenDoor();
//            
//        }
//		else if (door.DoorState == CDoorMotor.EDoorState.Opened)
//        {
//            m_buttonDoorPairs[_sender].CloseDoor();
//        }
//    }

    private void DoorStateChanged(GameObject _Sender)
    {	
//		CDoorMotor dm = _Sender.GetComponent<CDoorMotor>();
//		Vector2 buttonMiddleLeftViewPos = Vector2.zero;
//		
//		foreach(CDUIField field in m_fieldDoorPairs.Keys)
//		{
//			if(m_fieldDoorPairs[field] == dm)
//			{
//				switch (dm.DoorState)
//                {
//                case CDoorMotor.EDoorState.Opened: 
//                    field.Text = field.Text.Replace("Opening", "Open");
//                    break;
//                case CDoorMotor.EDoorState.Opening:
//                    field.Text = field.Text.Replace("Closed", "Opening");
//                    break;
//                case CDoorMotor.EDoorState.Closed:
//                    field.Text = field.Text.Replace("Closing", "Closed");
//                    break;
//                case CDoorMotor.EDoorState.Closing:
//                    field.Text = field.Text.Replace("Open", "Closing");
//                    break;
//                default:
//                    break;
//                }
//				
//				field.MiddleLeftViewPos = new Vector2(0.1f, field.MiddleLeftViewPos.y);
//				buttonMiddleLeftViewPos = new Vector2(field.MiddleRightViewPos.x + 0.1f, field.MiddleRightViewPos.y);
//				break;
//			}
//		}
//		
//		foreach(CDUIButton button in m_buttonDoorPairs.Keys)
//		{
//			if(m_buttonDoorPairs[button] == dm)
//			{
//				switch (dm.DoorState)
//                {
//					case CDoorMotor.EDoorState.Opened: 
//                    	button.Text = "Close";
//                    	break;
//                    
//					case CDoorMotor.EDoorState.Closed:
//                   	 	button.Text = "Open";
//                    	break;
//					
//					case CDoorMotor.EDoorState.Opening:
//                	case CDoorMotor.EDoorState.Closing:
//                    	button.Text = "Please Wait";
//                    	break;
//					
//                	default:
//                    	break;
//                }
//				
//				button.MiddleLeftViewPos = buttonMiddleLeftViewPos;
//				break;
//			}
//		}
    }

};
