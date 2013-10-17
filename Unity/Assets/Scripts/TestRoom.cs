using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestRoom : MonoBehaviour 
{
    public TestDoor[] m_doors       { get; set; }
    public DUIConsole[] m_consoles  { get; set; }

	private void Start () 
    {
        m_doors = transform.FindChild("Doors").GetComponentsInChildren<TestDoor>();
        m_consoles = transform.FindChild("Consoles").GetComponentsInChildren<DUIConsole>();

        // Add the subviews for the consoles
        foreach (DUIConsole console in m_consoles)
        {
            // Initialise the console
            console.Initialise();

            // Get the console Main View
            DUIMainView consoleMV = console.m_DUIMV;

            // Add the general room subview
            consoleMV.AddSubview("RoomControl");
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
            // Deinitilise it first
            console.Deinitialise();

            // Initialise the console
            console.Initialise();

            // Get the console Main View
            DUIMainView consoleMV = console.m_DUIMV;

            // Add the general room subview
            consoleMV.AddSubview("RoomControl");
        }
    }
}
