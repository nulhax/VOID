using System;
using UnityEngine;
using System.Collections;

public class BallCollisionBehaviour : MonoBehaviour
{
  public float TimeDestroyLightAfterCollision = 0.05f;
  public float TimeDestroyThisAfterCollision = 0.05f;
  public float TimeDestroyRootAfterCollision = 4f;
  public GameObject Explosion;
  public GameObject Glow;
  public GameObject GoLight;
  public bool IsLookAt;

  public event EventHandler<CollisionInfo> OnCollision;

  private ParticleSystem ps;
  private PrefabSettings prefabSettings;
  private Transform t, tTarget;
  private Vector3 targetPos;
  private RaycastHit hit;

  // Use this for initialization
  private void Start()
  {
    t = transform.root;
    prefabSettings = t.GetComponent<PrefabSettings>();
    if (prefabSettings==null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    tTarget = prefabSettings.Target.transform;
    if (particleSystem!=null)
      ps = particleSystem;
    if (IsLookAt)
      t.LookAt(tTarget);
    targetPos = t.position + Vector3.Normalize(tTarget.position - t.position) * prefabSettings.MoveDistance;
    FadeInOut(false);
  }

  private void Update()
  {
    switch (prefabSettings.PrefabStatus) {
    case PrefabStatus.FadeIn: {
      FadeInOut(true);
      prefabSettings.PrefabStatus = PrefabStatus.WaitHandle;
      break;
    }
    case PrefabStatus.FadeInMoveToTarget: {
      FadeInOut(true);
      prefabSettings.PrefabStatus = PrefabStatus.MoveToTarget;
      break;
    }
    case PrefabStatus.MoveToTarget: {
      UpdateDistance();
      break;
    }
    case PrefabStatus.CollisionEnter: {
      if (ps!=null)
        ps.Stop();
      if (Explosion!=null) {
        var expl = Instantiate(Explosion, t.position, new Quaternion()) as GameObject;
        expl.transform.parent = t;
      }
      prefabSettings.PrefabStatus = PrefabStatus.Destroy;
      prefabSettings.OnCollision(new CollisionInfo {Hit = hit});
      break;
    }
    case PrefabStatus.Destroy: {
      if (GoLight!=null)
        Destroy(GoLight, TimeDestroyLightAfterCollision);
      if (Glow!=null)
        Destroy(Glow.gameObject, TimeDestroyLightAfterCollision);
      Destroy(gameObject, TimeDestroyThisAfterCollision);
      Destroy(t.gameObject, TimeDestroyRootAfterCollision);
      prefabSettings.PrefabStatus = PrefabStatus.WaitDestroyTime;
      break;
    }
    }
  }

  private void FadeInOut(bool isVisible)
  {
    if (ps!=null) {
      if (isVisible)
        ps.Play();
      if (!isVisible)
        ps.Stop();
    }
    if (GoLight!=null)
      GoLight.SetActive(isVisible);
    if (Glow!=null)
      Glow.SetActive(isVisible);
  }

  private void UpdateDistance()
  {
    if (tTarget==null)
      return;
    
    
    if (prefabSettings.IsHomingMove) {
      if (Vector3.Distance(t.position, targetPos) <= prefabSettings.ColliderRadius)
        prefabSettings.PrefabStatus = PrefabStatus.CollisionEnter;
      var direction = (tTarget.position - t.position).normalized;
      if (Physics.Raycast(t.position, direction, out hit, prefabSettings.MoveDistance + 1)) {
        targetPos = hit.point - direction * prefabSettings.ColliderRadius;
      }
      if (IsLookAt)
        t.LookAt(tTarget);
      t.position = Vector3.MoveTowards(t.position, targetPos, prefabSettings.MoveSpeed * Time.deltaTime);
    }
    else {
      if (Vector3.Distance(t.position, targetPos) <= prefabSettings.ColliderRadius)
        prefabSettings.PrefabStatus = PrefabStatus.CollisionEnter;
        var direction = (targetPos - t.position).normalized;
        if (Physics.Raycast(t.position, direction, out hit, prefabSettings.MoveDistance + 1)) {
          targetPos = hit.point - direction * prefabSettings.ColliderRadius;
        }
      t.position = Vector3.MoveTowards(t.position, targetPos, prefabSettings.MoveSpeed * Time.deltaTime);
    }
  }
}