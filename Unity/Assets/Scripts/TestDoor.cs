using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestDoor : MonoBehaviour
{
    public enum EState
    {
        Opened,
        Opening,
        Closed,
        Closing
    }

    // Delegates
    public delegate void DoorStateHandler(TestDoor _sender);
    public event DoorStateHandler StateChanged;


    // Member Properties
    public EState m_state { get; set; }


    // Member Methods
    private void Awake()
    {
        m_state = EState.Closed;
    }

    public void OpenDoor()
    {
        StartCoroutine("Open");
    }

    public void CloseDoor()
    {
        StartCoroutine("Close");
    }

    public void SetColorRed()
    {
        renderer.material.color = Color.red;
    }

    public void SetColorBlue()
    {
        renderer.material.color = Color.blue;
    }

    public void SetColorGreen()
    {
        renderer.material.color = Color.green;
    }

    public void SetColorYellow()
    {
        renderer.material.color = Color.yellow;
    }

    private IEnumerator Open()
    {
        float d = 0.0f;
        Vector3 pos = transform.position;

        m_state = EState.Opening;
        OnStateChange();

        while (d < 2.0f)
        {
            d += Time.deltaTime;
            if (d > 2.0f)
                d = 2.0f;

            Vector3 newPos = pos;
            newPos.y += d;

            transform.position = newPos;

            yield return null;
        }

        m_state = EState.Opened;
        OnStateChange();
    }

    private IEnumerator Close()
    {
        float d = 0.0f;
        Vector3 pos = transform.position;

        m_state = EState.Closing;
        OnStateChange();

        while (d < 2.0f)
        {
            d += Time.deltaTime;
            if (d > 2.0f)
                d = 2.0f;

            Vector3 newPos = pos;
            newPos.y -= d;

            transform.position = newPos;

            yield return null;
        }

        m_state = EState.Closed;
        OnStateChange();
    }

    private void OnStateChange()
    {
        if (StateChanged != null)
            StateChanged(this);
    }
}
