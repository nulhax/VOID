using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
  public float LightDestroyTime = 1;
  public float GameObjectDestroyTime = 3;

  private float maxLight;
  private bool isFirst = true;
	// Use this for initialization
	void Start ()
	{
	  if(light != null) {
	    maxLight = light.intensity;
	    light.intensity = 0;
	  }
    Destroy(gameObject, GameObjectDestroyTime);
	}
	
	// Update is called once per frame
	void Update ()
	{
	  if (light==null)
	    return;
	  if (isFirst) {
	    light.intensity = maxLight;
	    isFirst = false;
	  }
    light.intensity -= (Time.deltaTime * maxLight) / LightDestroyTime;
    if(light.intensity <=0) Destroy(light);
	}
}
