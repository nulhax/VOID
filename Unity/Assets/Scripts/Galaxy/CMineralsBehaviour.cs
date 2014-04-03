//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CMinerals.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


[RequireComponent(typeof(CNetworkView))]
[RequireComponent(typeof(CActorInteractable))]
public class CMineralsBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void HandleDepleted(GameObject _cMineral);
    public event HandleDepleted EventDeplete;


// Member Properties


	public float Quantity
	{ 
		set { m_fQuantity.Set(value); }
		
		get { return m_fQuantity.Get(); } 
	}


    public bool IsDepleted
    {
        get { return (m_bDepleted); }
    }


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_fQuantity = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 300.0f);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_fQuantity">The amount of minerals ATTEMPTING to be extracted.</param>
	/// <returns>The amount of minerals ACTUALLY extracted.</returns>
	public float DecrementQuanity(float _fQuantity)
	{
        if (!IsDepleted)
        {
            if (_fQuantity <= m_fQuantity.Get())
            {
                m_fQuantity.Set(m_fQuantity.Get() - _fQuantity);
            }
            else
            {
                _fQuantity = m_fQuantity.Get();
                m_fQuantity.Set(0.0f);
                m_bDepleted = true;

                CNetwork.Factory.DestoryObject(gameObject);
            }

            CGameShips.Ship.GetComponent<CShipNaniteSystem>().AddNanites(_fQuantity);
        }

        return (m_fQuantity.Get());
	}


	void Start()
	{
		// COMMENTED OUT: CMineralDeposits assigns mineral quantity.

		//if (CNetwork.IsServer)
		//{
		//    float resourceAmount = CGalaxy.instance.ResourceAmount(CGalaxy.instance.RelativePointToAbsoluteCell(transform.position));
		//    //Debug.Log("Resource amount: " + resourceAmount);
		//    m_fQuantity.Set(resourceAmount);
		//}
	}


	void Update()
	{
		// Empty
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_fQuantity)
		{
            if (m_fQuantity.Get() <= 0.0f)
            {
                if (EventDeplete != null) EventDeplete(gameObject);
            }
			//gameObject.GetComponentInChildren<Renderer>.material.SetColor("_Color", new Color(1.0f - Quantity, 1.0f - Quantity, 1.0f));
		}
	}


// Member Fields


    public GameObject[] m_caPieces = null;


	CNetworkVar<float> m_fQuantity = null;


    bool m_bDepleted = false;


};
