using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour {
	
bool bTorchLit;

	// Use this for initialization
	void Start ()
	{
        gameObject.GetComponent<CToolInterface>().EventActivatePrimary += new CToolInterface.ActivatePrimary(ToggleActivate);

		bTorchLit = true;
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

    private void ToggleActivate()
    {
        if (bTorchLit == false)
        {
            bTorchLit = true;
            light.intensity = 2;
        }
        else
        {
            bTorchLit = false;
            light.intensity = 0;
        }
    }   
}
