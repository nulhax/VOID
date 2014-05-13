using UnityEngine;
using System.Collections;

public class LightFadeInOut : MonoBehaviour {

  private PrefabSettings prefabSettings;
  private Light goLight;
  private float maxIntensity, intensity;
  private float deltaFps;
  private bool isVisible;
  private bool isCorutineStarted;

  private void Awake()
  {
    prefabSettings = transform.root.GetComponent<PrefabSettings>();
    if (prefabSettings == null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    deltaFps = 1f / prefabSettings.FPS;

    goLight = light;
    maxIntensity = goLight.intensity;
    goLight.intensity = 0;
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
    isCorutineStarted = false;
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
      case PrefabStatus.FadeIn: {
        goLight.intensity = intensity;
        if (intensity >= maxIntensity)
          prefabSettings.PrefabStatus = PrefabStatus.WaitHandle;
        intensity += maxIntensity / (prefabSettings.FPS * prefabSettings.FadeInSpeed);
        break;
      }
    case PrefabStatus.FadeOut:
        {
          goLight.intensity = intensity;
          if (intensity <= 0) {
            intensity = 0;
            prefabSettings.PrefabStatus = PrefabStatus.WaitDestroyTime;
          }
          intensity -= maxIntensity / (prefabSettings.FPS * prefabSettings.FadeOutSpeed);
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
