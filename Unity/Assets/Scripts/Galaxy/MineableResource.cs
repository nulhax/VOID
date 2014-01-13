using UnityEngine;
using System.Collections;

public class MineableResource : CNetworkMonoBehaviour
{
    public float resourceAmount { get { return m_ResourceAmount.Get(); } set { m_ResourceAmount.Set(value); } }
    CNetworkVar<float> m_ResourceAmount;

	void Start()
    {
		if(CNetwork.IsServer)
		{
       	 	Vector3 samplePoint = CGalaxy.instance.AbsoluteCellNoiseSamplePoint(CGalaxy.instance.RelativePointToAbsoluteCell(transform.position));
        	resourceAmount = CGalaxy.instance.SampleNoise(samplePoint.x, samplePoint.y, samplePoint.z, CGalaxy.ENoiseLayer.AsteroidResourceAmount);
		}
	}

    public override void InstanceNetworkVars()
    {
        m_ResourceAmount = new CNetworkVar<float>(SyncResourceAmount, 0.0f);
    }

    // This function is for convenience.
    public float Mine(float amountWanted)
    {
        if (amountWanted <= m_ResourceAmount.Get())
        {
            m_ResourceAmount.Set(m_ResourceAmount.Get() - amountWanted);
        }
        else
        {
            amountWanted = m_ResourceAmount.Get();
            m_ResourceAmount.Set(0.0f);
        }

        return amountWanted;
    }

    public void SyncResourceAmount(INetworkVar sender)
    {
        // Todo: Retexture asteroid based on resource amount.
    }
}
