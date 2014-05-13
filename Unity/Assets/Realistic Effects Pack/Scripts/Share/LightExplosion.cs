using UnityEngine;
using System.Collections;

public class LightExplosion : MonoBehaviour
{

  public float Speed = 1;

  private float maxLight;
  private bool isFirst = true;
	// Use this for initialization
	void Start () {
    if (light != null)
    {
      maxLight = light.intensity;
      light.intensity = 0;
    }
	}
	
	// Update is called once per frame
	void Update () {
    if (light == null)
      return;
    if (isFirst)
    {
      light.intensity = maxLight;
      isFirst = false;
    }
    light.intensity -= Time.deltaTime * maxLight * Speed;
    if (light.intensity <= 0) Destroy(light);
	}
}
