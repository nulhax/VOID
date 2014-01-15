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
            resourceAmount = CGalaxy.instance.CalculateAsteroidResourceAmount(CGalaxy.instance.RelativePointToAbsoluteCell(transform.position));
		}
	}

    public override void InstanceNetworkVars()
    {
        m_ResourceAmount = new CNetworkVar<float>(SyncResourceAmount, 0.0f);
    }

    // This function is for convenience.
    // Returns the amount actually obtained from an amount wanted.
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
        gameObject.renderer.material.SetColor("_Color", new Color(1.0f - resourceAmount, 1.0f - resourceAmount, 1.0f));
    }
}
