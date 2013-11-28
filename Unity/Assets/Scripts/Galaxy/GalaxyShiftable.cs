using UnityEngine;
using System.Collections;

public class GalaxyShiftable : MonoBehaviour
{
    void Start()
    {
        CGalaxy.instance.RegisterShiftableEntity(gameObject.transform);
    }

    void OnDestroy()
    {
        CGalaxy galaxy = CGalaxy.instance;
        if(galaxy)
            galaxy.DeregisterShiftableEntity(gameObject.transform);
    }
	
    //void Update ()
    //{
	
    //}
}
