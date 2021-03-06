﻿using UnityEngine;
using System.Collections;

public class GalaxyShiftable : MonoBehaviour
{
    void Start()
    {
        if (CGalaxy.instance != null)
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
	}
}
