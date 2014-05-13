using UnityEngine;
using System.Collections;

public class LiquidMatAnimation : MonoBehaviour
{
  public float Speed = 1;
  public float FPS = 40;

  private PrefabSettings prefabSettings;
  private ProceduralMaterial proceduralMaterial;
  private float deltaFps;
  private bool isVisible;
  private bool isCorutineStarted;

  private void Awake()
  {
    prefabSettings = transform.root.GetComponent<PrefabSettings>();
    deltaFps = prefabSettings==null ? 1f / FPS : 1f / prefabSettings.FPS;

    proceduralMaterial = renderer.sharedMaterial as ProceduralMaterial;
    ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
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
    var lerp = Mathf.PingPong(Time.time * Speed, 500);
    proceduralMaterial.SetProceduralFloat("Flow", lerp);
    proceduralMaterial.RebuildTextures();
  }
}
