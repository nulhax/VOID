using UnityEngine;
using System.Collections;

public class Torch : CNetworkMonoBehaviour {
	
    CNetworkVar<bool> m_bTorchLit = null;

    public override void InstanceNetworkVars()
    {
        m_bTorchLit = new CNetworkVar<bool>(OnNetworkVarSync, true);
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (!m_bTorchLit.Get())
        {
            light.intensity = 0;
        }
        else
        {
            light.intensity = 2;
        }
    }




	// Use this for initialization
	void Start ()
	{
        gameObject.GetComponent<CToolInterface>().EventActivatePrimary += new CToolInterface.ActivatePrimary(ToggleActivate);
        m_bTorchLit.Set(false);
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

    private void ToggleActivate()
    {
        if (!m_bTorchLit.Get())
        {
            m_bTorchLit.Set(true);
        }
        else
        {
            m_bTorchLit.Set(false);
        }
    }   
}
