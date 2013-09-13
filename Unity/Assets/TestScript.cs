using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestScript : MonoBehaviour
{
    public GameObject m_Screen;

    // Use this for initialization
    void Start()
    {
        ButtonUI[] buttons = m_Screen.transform.FindChild(m_Screen.name + "_UI").GetComponentsInChildren<ButtonUI>();

        buttons[0].RegisterListener(StartBouncing);
        buttons[1].RegisterListener(StopBounching);
        buttons[2].RegisterListener(() => renderer.material.color = Color.red);
        buttons[3].RegisterListener(() => renderer.material.color = Color.green);
        buttons[4].RegisterListener(() => renderer.material.color = Color.blue);
        buttons[5].RegisterListener(() => renderer.material.color = Color.yellow);
    }

    public void StartBouncing()
    {
        StartCoroutine("Bounce");
    }

    public void StopBounching()
    {
        StopCoroutine("Bounce");
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
