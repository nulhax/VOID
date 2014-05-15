using UnityEngine;
using System.Collections;

public class CThrusterSFX : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        CGameShips.GalaxyShip.GetComponent<CGalaxyShipMotor>().EventShipThrustChanged += OnThrustStateChange;
    }

    void OnThrustStateChange(bool _bThrustEngaged)
    {
      
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
