using UnityEngine;
using System.Collections;

public class GalaxyShiftable : MonoBehaviour
{
	public delegate void OnSetCallback(GameObject gameObject, Vector3 translation);
	public event OnSetCallback EventOnSetCallback;

    void Start()
    {
        CGalaxy.instance.RegisterShiftableEntity(this);
    }

    void OnDestroy()
    {
        CGalaxy galaxy = CGalaxy.instance;
        if(galaxy)
            galaxy.DeregisterShiftableEntity(this);
    }

	public void Shift(Vector3 translation)
	{
		transform.position += translation;

		if (EventOnSetCallback != null)
			EventOnSetCallback(gameObject, translation);
	}
	
    //void Update ()
    //{
	
    //}
}
