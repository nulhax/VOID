using System.Collections;
using UnityEngine;

public class WaterUvAnimation : MonoBehaviour
{
  public bool isReverse;
  private Material mat;
  private PrefabSettings prefabSettings;
  private float deltaFps;
  private bool isVisible;
  private bool isCorutineStarted;
  private float offset, delta;
  
  private void Awake()
  {
    prefabSettings = transform.root.GetComponent<PrefabSettings>();
    if (prefabSettings == null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    deltaFps = 1f / prefabSettings.FPS;
   
    delta = 1f / prefabSettings.FPS * prefabSettings.UVAnimationSpeed;
    mat = renderer.material;
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
    if (isReverse)
    {
      offset -= delta;
      if (offset < 0)
        offset = 1;
    }
    else
    {
      offset += delta;
      if (offset > 1)
        offset = 0;
    }
    var vec = new Vector2(0, offset);
    mat.SetTextureOffset("_BumpMap", vec);
    mat.SetFloat("_OffsetYHeightMap", offset);
  }
}