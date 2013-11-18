using UnityEngine;
using System.Collections;

public class Torch : CNetworkMonoBehaviour {
	
    CNetworkVar<bool> m_bTorchLit = null;
	CNetworkVar<byte> m_bTorchColour = null;

    public override void InstanceNetworkVars()
    {
        m_bTorchLit = new CNetworkVar<bool>(OnNetworkVarSync, true);
		m_bTorchColour = new CNetworkVar<byte>(OnNetworkVarSync, 0);
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
		if (_cVarInstance == m_bTorchLit)
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
		else if (_cVarInstance == m_bTorchColour)
		{
			switch (m_bTorchColour.Get())
			{
				case 0:
					light.color = new Color(174.0f / 255.0f, 208.0f / 255.0f, 1.0f);
					break;
				case 1:
					light.color = new Color(1.0f, 0, 0);
					break;
				case 2:
					light.color = new Color(0, 1.0f, 0);
					break;
				case 3:
					light.color = new Color(0, 0, 1.0f);
					break;
			}
		}
    }




	// Use this for initialization
	void Start ()
	{
		gameObject.GetComponent<CToolInterface>().EventPrimaryActivate += new CToolInterface.NotifyPrimaryActivate(ToggleActivate);
		gameObject.GetComponent<CToolInterface>().EventSecondaryActivate += new CToolInterface.NotifySecondaryActivate(ToggleColour);

		if (CNetwork.IsServer)
		{
			m_bTorchLit.Set(false);
		}
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
  

	private void ToggleColour()
    {
		byte bNextNumber = (byte)(m_bTorchColour.Get() + 1);


		if (bNextNumber > 3)
		{
			bNextNumber = 0;
		}

		m_bTorchColour.Set(bNextNumber);
	}

}
