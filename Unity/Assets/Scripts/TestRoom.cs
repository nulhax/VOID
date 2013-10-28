using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TestRoom : MonoBehaviour 
{
    private TestDoor[] m_doors;
    private DUIConsole[] m_consoles;

    private Dictionary<DUIButton, TestDoor> m_buttonDoorPairs;
	private Dictionary<DUIField, TestDoor> m_fieldDoorPairs;
	
	private List<DUIField> m_PEOutputFields;

	private void Awake() 
    {
        m_buttonDoorPairs = new Dictionary<DUIButton, TestDoor>();
		m_fieldDoorPairs = new Dictionary<DUIField, TestDoor>();
		
		m_PEOutputFields = new List<DUIField>();
		
        m_doors = transform.FindChild("Doors").GetComponentsInChildren<TestDoor>();
        m_consoles = transform.FindChild("Consoles").GetComponentsInChildren<DUIConsole>();

        // Add the subviews for the consoles
        foreach (DUIConsole console in m_consoles)
        {
            // Initialise the console
            console.Initialise();
			
            // Add the room control subview
            DUISubView duiRC = console.m_DUIMV.AddSubview("DoorControl");

            // For each door add a button
            int count = 0;
            foreach (TestDoor door in m_doors)
            {
				// Add the door buttons
                DUIButton duiBut = duiRC.AddButton("Open");
                duiBut.PressDown += OpenCloseDoor;

                m_buttonDoorPairs[duiBut] = door;
				
				// Add the door status field
				DUIField duiDoorState = duiRC.AddField(string.Format("Door {0}: Closed", count + 1));
			
				m_fieldDoorPairs[duiDoorState] = door;
				
				// Set their positions
				duiDoorState.m_viewPos = new Vector2(0.5f, 0.5f - (((count + 0.5f) - m_doors.Length * 0.5f) / m_doors.Length) * 1.0f / (m_doors.Length - 1));
				duiBut.transform.localPosition = duiDoorState.transform.localPosition - new Vector3(0.0f, duiBut.m_dimensions.y);
               
                ++count;
            }
			
			// Add the facility Control
			DUISubView duiFC = console.m_DUIMV.AddSubview("FacilityControl");

            // Add buttons for the emmiter
            DUIButton duiButLeft = duiFC.AddButton("Increment Power");
            DUIButton duiButRight = duiFC.AddButton("Decrement Power");

            duiButLeft.m_viewPos = new Vector2(0.5f, 0.8f);
            duiButRight.m_viewPos = new Vector2(0.5f, 0.2f);

            duiButLeft.PressDown += IncreasePEOutput;
            duiButRight.PressDown += DecreasePEOutput;
			
			// Add the field to represent the current power
			DUIField duiCurrPower = duiFC.AddField(string.Empty);
			
			duiCurrPower.m_viewPos = new Vector2(0.5f, 0.5f);
			
			m_PEOutputFields.Add(duiCurrPower);
			
			// Disable them both
			duiRC.gameObject.SetActive(false);
			duiFC.gameObject.SetActive(false);
        }
		
		// Update all of the output fields
		UpdatePEOutputFields();

        // Register the door state change event
		foreach (TestDoor door in m_doors)
		{
		    door.StateChanged += DoorStateChanged;
		}  
	}

    private void Update()
    {
        // Check for resetting the UI
        if (Input.GetKeyUp(KeyCode.F1))
        {
            ResetMonitors();
        }
    }

    private void ResetMonitors()
    {
        // Add the subviews for the consoles
        foreach (DUIConsole console in m_consoles)
        {
            // Deinitialise it first
            console.Deinitialise();

            // Initialise the console
            console.Initialise();

            // Add the general room subview
            console.m_DUIMV.AddSubview("RoomControl");
        }
    }
	
    private void OpenCloseDoor(DUIButton _sender)
    {
        TestDoor door = m_buttonDoorPairs[_sender];
        TestDoor.EState doorState = door.m_state;

        if (doorState == TestDoor.EState.Closed)
        {
            m_buttonDoorPairs[_sender].OpenDoor();
            
        }
        else if (doorState == TestDoor.EState.Opened)
        {
            m_buttonDoorPairs[_sender].CloseDoor();
        }
    }

    private void DoorStateChanged(TestDoor _sender)
    {
        TestDoor.EState doorState = _sender.m_state;
		
		foreach(DUIButton button in m_buttonDoorPairs.Keys)
		{
			if(m_buttonDoorPairs[button] == _sender)
			{
				switch (doorState)
                {
					case TestDoor.EState.Opened: 
                    	button.m_text = "Close";
                    	break;
                    
					case TestDoor.EState.Closed:
                   	 	button.m_text = "Open";
                    	break;
					
					case TestDoor.EState.Opening:
                	case TestDoor.EState.Closing:
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
				switch (doorState)
                {
                    case TestDoor.EState.Opened: 
                        field.m_text = field.m_text.Replace("Opening...", "Open");
                        break;
                    case TestDoor.EState.Opening:
                        field.m_text = field.m_text.Replace("Closed", "Opening...");
                        break;
                    case TestDoor.EState.Closed:
                        field.m_text = field.m_text.Replace("Closing...", "Closed");
                        break;
                    case TestDoor.EState.Closing:
                        field.m_text = field.m_text.Replace("Open", "Closing...");
                        break;
                    default:
                        break;
                }
			}
		}
    }
	
	private void IncreasePEOutput(DUIButton _sender)
    {
        ParticleSystem PE = GetComponentInChildren<ParticleSystem>();

        PE.emissionRate += 1;
		
		UpdatePEOutputFields();
    }

    private void DecreasePEOutput(DUIButton _sender)
    {
        ParticleSystem PE = GetComponentInChildren<ParticleSystem>();

        PE.emissionRate -= 1;
		
		UpdatePEOutputFields();
    }
	
	private void UpdatePEOutputFields()
	{
		ParticleSystem PE = GetComponentInChildren<ParticleSystem>();
		
		foreach (DUIField PEField in m_PEOutputFields) 
		{
			PEField.m_text = string.Format("Current Output: {0}", PE.emissionRate);
		}
	}
}
