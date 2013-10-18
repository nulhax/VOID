using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TestRoom : MonoBehaviour 
{
    private TestDoor[] m_doors;
    private DUIConsole[] m_consoles;

    private Dictionary<DUIButton, TestDoor> m_buttonDoorPairs;

	private void Awake() 
    {
        m_buttonDoorPairs = new Dictionary<DUIButton, TestDoor>();

        m_doors = transform.FindChild("Doors").GetComponentsInChildren<TestDoor>();
        m_consoles = transform.FindChild("Consoles").GetComponentsInChildren<DUIConsole>();

        // Add the subviews for the consoles
        foreach (DUIConsole console in m_consoles)
        {
            // Initialise the console
            console.Initialise();

            // Add the room control subview
            DUISubView duiRC = console.m_DUIMV.AddSubview("RoomControl");

            // For each door add a button
            int count = 0;
            foreach (TestDoor door in m_doors)
            {
                DUIButton duiBut = duiRC.AddButton(new Vector2(0.5f, 0.2f));

                duiBut.m_viewPos = new Vector2(0.5f, 0.5f - (((count + 0.5f) - m_doors.Length * 0.5f) / m_doors.Length) * 1.0f / (m_doors.Length - 1));
                duiBut.m_text = string.Format("Door {0}: Open", (count + 1));
               
                duiBut.Press += OpenCloseDoor;

                m_buttonDoorPairs[duiBut] = door;
               
                ++count;
            }

            // Add buttons for the emmiter
            DUIButton duiButLeft = duiRC.AddButton(new Vector2(0.2f, 0.2f));
            DUIButton duiButRight = duiRC.AddButton(new Vector2(0.2f, 0.2f));

            duiButLeft.m_viewPos = new Vector2(0.4f, 0.5f);
            duiButLeft.m_text = "Inc";

            duiButRight.m_viewPos = new Vector2(0.6f, 0.5f);
            duiButRight.m_text = "Dec";

            duiButLeft.Press += IncreasePEOutput;
            duiButRight.Press += DecreasePEOutput;
        }

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

    private void IncreasePEOutput(DUIButton _sender)
    {
        ParticleSystem PE = GetComponentInChildren<ParticleSystem>();

        PE.emissionRate += 1;
    }

    private void DecreasePEOutput(DUIButton _sender)
    {
        ParticleSystem PE = GetComponentInChildren<ParticleSystem>();

        PE.emissionRate -= 1;
    }

    private void DoorStateChanged(TestDoor _sender)
    {
        TestDoor.EState doorState = _sender.m_state;

        foreach (DUIConsole console in m_consoles)
        {
            foreach (DUISubView sv in console.m_DUIMV.m_subViews.Values)
            {
                foreach (DUIButton button in sv.m_buttons)
                {
                    if (!m_buttonDoorPairs.ContainsKey(button))
                        break;

                    if (m_buttonDoorPairs[button] == _sender)
                    {
                        switch (doorState)
                        {
                            case TestDoor.EState.Opened: 
                                button.m_text = button.m_text.Replace("Opening...", "Close");
                                break;
                            case TestDoor.EState.Opening:
                                button.m_text = button.m_text.Replace("Open", "Opening...");
                                break;
                            case TestDoor.EState.Closed:
                                button.m_text = button.m_text.Replace("Closing...", "Open");
                                break;
                            case TestDoor.EState.Closing:
                                button.m_text = button.m_text.Replace("Close", "Open");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
