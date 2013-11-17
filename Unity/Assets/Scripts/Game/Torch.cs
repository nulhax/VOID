using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour {
	
bool bTorchLit;

	// Use this for initialization
	void Start ()
	{
        gameObject.GetComponent<CToolInterface>().EventActivatePrimary += new CToolInterface.ActivatePrimary(TurnOn);
        gameObject.GetComponent<CToolInterface>().EventDeactivatePrimary += new CToolInterface.DeactivatePrimary(TurnOff);

		bTorchLit = true;
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

    private void TurnOn()
    {
        if (bTorchLit == false)
        {
            bTorchLit = true;
            light.intensity = 1;
        }
    }
    private void TurnOff()
    {
        if(bTorchLit == true)
        {
            bTorchLit = false;
            light.intensity = 0;
        }
    }
}
