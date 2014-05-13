using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

internal class UVTextureAnimator : MonoBehaviour
{
  public Material[] AnimatedMaterialsNotInstance = null;
  public int Rows = 4;
  public int Columns = 4;
  public float Fps = 20;
  public int OffsetMat = 0;
  public bool IsLoop = true;
  public bool IsRandomOffsetForInctance = false;
  public bool IsBump = false;
  public bool IsHeight = false;
  public bool UsePrefabStatus = false;

  private PrefabSettings prefabSettings;
  private int index;
  private int count, allCount;
  private float deltaFps;
  private bool isVisible;
  private bool isCorutineStarted;

  #region Non-public methods

  private void Awake()
  {
    if (UsePrefabStatus) {
      prefabSettings = transform.root.GetComponent<PrefabSettings>();
      if (prefabSettings==null)
        Debug.Log("Prefab root have not script \"PrefabSettings\"");
    }
    deltaFps = 1f / Fps;
    count = Rows * Columns;
    index += Columns - 1;
    var offset = new Vector2((float)index / Columns - (index / Columns),
          1 - (index / Columns) / (float)Rows);
    OffsetMat = !IsRandomOffsetForInctance
      ? OffsetMat - (OffsetMat / count) * count
      : Random.Range(0, count);
    var size = new Vector2(1f / Columns, 1f / Rows);
    if (AnimatedMaterialsNotInstance.Length > 0)
      foreach (var mat in AnimatedMaterialsNotInstance) {
        mat.SetTextureScale("_MainTex", size);
        mat.SetTextureOffset("_MainTex", Vector2.zero);
        if (IsBump) {
          mat.SetTextureScale("_BumpMap", size);
          mat.SetTextureOffset("_BumpMap", Vector2.zero);
        }
        if (IsHeight) {
          mat.SetTextureScale("_HeightMap", size);
          mat.SetTextureOffset("_HeightMap", Vector2.zero);
        }
      }
    else {
      renderer.material.SetTextureScale("_MainTex", size);
      renderer.material.SetTextureOffset("_MainTex", offset);
      if (IsBump) {
        renderer.material.SetTextureScale("_BumpMap", size);
        renderer.material.SetTextureOffset("_BumpMap", offset);
      }
      if (IsBump) {
        renderer.material.SetTextureScale("_HeightMap", size);
        renderer.material.SetTextureOffset("_HeightMap", offset);
      }
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
    while (isVisible && (IsLoop || allCount!=count)) {
      if (UsePrefabStatus)  VerifyPrefabStatus();
      else UpdateCorutineFrame();
      if (!IsLoop && allCount==count)
        break;
      yield return new WaitForSeconds(deltaFps);
    }
    isCorutineStarted = false;
  }

  #endregion CorutineCode

  private void VerifyPrefabStatus()
  {
    switch (prefabSettings.PrefabStatus) {
    case PrefabStatus.FadeIn: {
      UpdateCorutineFrame();
      break;
    }
    case PrefabStatus.FadeOut: {
      break;
    }
    case PrefabStatus.Destroy: {
      prefabSettings.PrefabStatus = PrefabStatus.WaitDestroyTime;
      Destroy(transform.root.gameObject);
      break;
    }
    }
  }

  private void UpdateCorutineFrame()
  {
    allCount++;
    index++;
    if (index >= count)
      index = 0;
    
    if (AnimatedMaterialsNotInstance.Length > 0)
      for (int i = 0; i < AnimatedMaterialsNotInstance.Length; i++) {
        var idx = i * OffsetMat + index;
        idx = idx - (idx / count) * count;
        var offset = new Vector2((float) idx / Columns - (idx / Columns),
          1 - (idx / Columns) / (float) Rows);
        AnimatedMaterialsNotInstance[i].SetTextureOffset("_MainTex", offset);
        if (IsBump)
          AnimatedMaterialsNotInstance[i].SetTextureOffset("_BumpMap", offset);
        if (IsHeight)
          AnimatedMaterialsNotInstance[i].SetTextureOffset("_HeightMap", offset);
      }
    else {
      Vector2 offset;
      if (IsRandomOffsetForInctance) {
        var idx = index + OffsetMat;
        offset = new Vector2((float) idx / Columns - (idx / Columns),
          1 - (idx / Columns) / (float) Rows);
      }
      else
        offset = new Vector2((float) index / Columns - (index / Columns),
          1 - (index / Columns) / (float) Rows);
      renderer.material.SetTextureOffset("_MainTex", offset);
      if (IsBump)
        renderer.material.SetTextureOffset("_BumpMap", offset);
      if (IsHeight)
        renderer.material.SetTextureOffset("_HeightMap", offset);
    }
  }

  #endregion
}