using System;
using UnityEngine;
using System.Collections;

public class Circulation : MonoBehaviour {
  public int Fps = 60;
  public float Radius = 1, Speed = 1, OffsetDegree = 0;
  public bool IsPointCirculation;
  public bool IsActive = true;
  
  private float deltaTime;
  private Transform t;
  // Use this for initialization
  void Start()
  {
    deltaTime = 1f / Fps;
    t = transform;
    StartCoroutine(UpdatePosition());
  }

  // Update is called once per frame
  IEnumerator UpdatePosition()
  {
    while (true) {
      if(IsActive) {
        if (!IsPointCirculation) {
          var temp = Time.time * Speed + OffsetDegree / 180 * Mathf.PI;
          t.localPosition = new Vector3(Mathf.Sin(temp) * Radius, Mathf.Sin(temp + Mathf.PI / 2) * Radius, 0);
        }
        else {
          t.Rotate(0, Speed*deltaTime, 0);
        }
      }
      yield return new WaitForSeconds(deltaTime);
    }
  }
}
