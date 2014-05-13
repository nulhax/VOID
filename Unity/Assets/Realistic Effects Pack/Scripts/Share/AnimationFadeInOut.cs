using UnityEngine;
using System.Collections;

public class AnimationFadeInOut : MonoBehaviour {

  private PrefabSettings prefabSettings;
  private Animation anim;
  private float deltaFps;
  private bool isVisible;
  private bool isCorutineStarted;

  private void Awake()
  {
    prefabSettings = transform.root.GetComponent<PrefabSettings>();
    if (prefabSettings == null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    deltaFps = 1f / prefabSettings.FPS;
    anim = animation;
    anim.Stop();
  }

  #region CorutineCode

  private void OnEnable()
  {
    isVisible = true;
    if (!isCorutineStarted)
      StartCoroutine(UpdateCorutine());
  }

  private void OnDisable()
  {
    isVisible = false;
  }

  private void OnBecameVisible()
  {
    isVisible = true;
    if (!isCorutineStarted)
      StartCoroutine(UpdateCorutine());
  }

  private void OnBecameInvisible()
  {
    isVisible = false;
  }

  private IEnumerator UpdateCorutine()
  {
    isCorutineStarted = true;
    while (isVisible)
    {
      UpdateCorutineFrame();
      yield return new WaitForSeconds(deltaFps);
    }
    isCorutineStarted = false;
  }

  #endregion CorutineCode

  private void UpdateCorutineFrame()
  {
    switch (prefabSettings.PrefabStatus)
    {
      case PrefabStatus.FadeIn:
        {
          if (!anim.isPlaying)
            anim.Play();
          break;
        }
      case PrefabStatus.FadeOut:
        {
          if (anim.isPlaying)
            anim.Stop();
          break;
        }
      case PrefabStatus.Destroy:
        {
          prefabSettings.PrefabStatus = PrefabStatus.WaitDestroyTime;
          Destroy(transform.root.gameObject);
          break;
        }
    }
  }
}
