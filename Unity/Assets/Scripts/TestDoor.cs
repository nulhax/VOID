using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestDoor : MonoBehaviour
{
    public void StartBouncing()
    {
        StartCoroutine("Bounce");
    }

    public void StopBouncing()
    {
        StopCoroutine("Bounce");
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

    private IEnumerator Bounce()
    {
        while(true)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.PingPong(Time.time, 0.5f);
            transform.position = pos;

            yield return null;
        }
    }
}
