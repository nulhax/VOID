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
        //Debug.LogError("Torch");
        if (bTorchLit == false)
        {
            bTorchLit = true;
            light.intensity = 3;
        }
        else
        {
            bTorchLit = false;
            light.intensity = 0;
        }
    }   
}
