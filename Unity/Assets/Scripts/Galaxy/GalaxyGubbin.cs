using UnityEngine;
using System.Collections;

public class GalaxyGubbin : MonoBehaviour
{
    public bool registeredWithGalaxy = true;

    void OnDestroy()
    {
        if (CGalaxy.instance != null && registeredWithGalaxy)
            CGalaxy.instance.DeregisterGubbin(this);
    }
}
