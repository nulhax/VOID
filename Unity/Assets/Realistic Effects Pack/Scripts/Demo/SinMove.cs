using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class SinMove : MonoBehaviour
{
  public float Speed = 1;
  public Vector3 MoveOffset = new Vector3(1, 1, 1);

  private float time;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
    var offset = new Vector3(Time.deltaTime * Mathf.Sin(time * Speed) * MoveOffset.x,
                        Time.deltaTime * Mathf.Sin(time * Speed) * MoveOffset.y,
                         Time.deltaTime * Mathf.Sin(time * Speed) * MoveOffset.z);
	  time += Time.deltaTime;
    transform.position += offset * Time.deltaTime;
	}
}
