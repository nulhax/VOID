using UnityEngine;
using System.Collections;

public class DeadTime: MonoBehaviour
{
  public float deadTime = 1.5f;
  void Awake()
  {
    Destroy(gameObject, deadTime);
  }
}
