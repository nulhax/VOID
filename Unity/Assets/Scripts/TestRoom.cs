using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
                DUIButton duiBut = duiRC.AddButton("SimpleButton");

                duiBut.m_viewPos = new Vector2(0.5f, 0.5f - (((count + 0.5f) - m_doors.Length * 0.5f) / m_doors.Length) * 1.0f / (m_doors.Length - 1));
                duiBut.m_text = string.Format("Door {0}: Open", (count + 1));
                duiBut.Press += OpenCloseDoor;

                m_buttonDoorPairs[duiBut] = door;
               
                ++count;
            }
            
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

        if (door.m_state == TestDoor.EState.Closed)
        {
            m_buttonDoorPairs[_sender].OpenDoor();
            _sender.m_text = _sender.m_text.Replace("Open", "Close");
        }
        else if (door.m_state == TestDoor.EState.Opened)
        {
            m_buttonDoorPairs[_sender].CloseDoor();
            _sender.m_text = _sender.m_text.Replace("Close", "Open");
        }
    }
}
