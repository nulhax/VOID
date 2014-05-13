using System.ComponentModel;
using UnityEngine;
using System.Collections;

public class RenderColorFadeInOut : MonoBehaviour
{
  public bool IsParticlesFadeInOut = false;
  public string ColorName = "_TintColor";

  private PrefabSettings prefabSettings;
  private Material mat;
  private Color matColor;
  private float colorAlpha;
  private float deltaFps;
  private bool isVisible;
  private bool isCorutineStarted;

  private void Start()
  {
    prefabSettings = transform.root.GetComponent<PrefabSettings>();
    if (prefabSettings==null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    deltaFps = 1f / prefabSettings.FPS;

    if (!IsParticlesFadeInOut) {
      mat = renderer.material;
      matColor = mat.GetColor(ColorName);
      mat.SetColor(ColorName, new Color(matColor.r, matColor.g, matColor.b, 0));
    }
    else {
      particleSystem.playOnAwake = false;
    }
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
    while (isVisible) {
      UpdateCorutineFrame();
      yield return new WaitForSeconds(deltaFps);
    }
    isCorutineStarted = false;
  }

  #endregion CorutineCode

  private void UpdateCorutineFrame()
  {
    if(prefabSettings==null) return;

    switch (prefabSettings.PrefabStatus) {
    case PrefabStatus.FadeIn: {
      if (!IsParticlesFadeInOut) {
        mat.SetColor(ColorName, new Color(matColor.r, matColor.g, matColor.b, colorAlpha));
        if (colorAlpha >= matColor.a)
          prefabSettings.PrefabStatus = PrefabStatus.WaitHandle;
        colorAlpha += matColor.a / (prefabSettings.FPS * prefabSettings.FadeInSpeed);
      }
      else {
        if (!particleSystem.isPlaying)
          particleSystem.Play();
      }
      break;
    }
    case PrefabStatus.FadeOut: {
      if (!IsParticlesFadeInOut) {
        mat.SetColor(ColorName, new Color(matColor.r, matColor.g, matColor.b, colorAlpha));
        if (colorAlpha <= 0) {
          colorAlpha = 0;
          prefabSettings.PrefabStatus = PrefabStatus.WaitDestroyTime;
        }
        colorAlpha -= matColor.a / (prefabSettings.FPS * prefabSettings.FadeOutSpeed);
      }
      else {
        if (particleSystem.isPlaying)
          particleSystem.Stop();
      }
      break;
    }
    case PrefabStatus.Destroy: {
      prefabSettings.PrefabStatus = PrefabStatus.WaitDestroyTime;
      Destroy(transform.root.gameObject);
      break;
    }
    }
  }
}
