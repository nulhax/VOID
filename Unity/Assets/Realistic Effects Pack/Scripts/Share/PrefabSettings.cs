using System;
using UnityEngine;

public class PrefabSettings : MonoBehaviour
{
  public float ColliderRadius = 0.2f;
  public float FPS = 40;
  public PrefabStatus PrefabStatus;
  public GameObject Target;
  public float MoveSpeed = 1, MoveDistance = 20, FadeInSpeed = 1, FadeOutSpeed = 1, UVAnimationSpeed = 1;
  public bool IsHomingMove;

  public event EventHandler<CollisionInfo> CollisionEnter;

  public void OnCollision(CollisionInfo e)
  {
    var handler = CollisionEnter;
    if (handler != null)
      handler(this, e);
  }
}