//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   PlayerStatus.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System;


/* Implementation */


public class CPlayerStatus : MonoBehaviour, IObserver<INetworkVar>
{

// Member Types


// Member Functions

    // public:


	public void Awake()
	{
		m_fFuckYou = new CNetworkVar<byte>(this, 4);
	}


    public void Start()
	{
	}

    public void OnDestroy()
    {
    }


	static bool bOnce = false;


    public void Update()
    {
		if (!bOnce)
		{
			//GetComponent<CNetworkView>().InvokeRpc(this, "Revive");

			bOnce = true;
		}
    }


	public void Notify(INetworkVar _rSender, short _sSubject, byte[] _baData)
	{
	}

	public void Custom()
	{

	}

    [ANetworkRpc]
    public void Revive()
    {
    }


    // protected:


    protected void ProcessMovement()
    {
        /*
        if (m_bMovingForward &&
            Input.GetKeyUp(KeyCode.W))
        {
            GetComponent<CNetworkView>().SendServer(m_bMovingForward);
        }


        if (m_bMovingBackwards &&
            Input.GetKeyUp(KeyCode.S))
        {
            m_bMovingBackwards = false;
            GetComponent<CNetworkView>().Sync(m_bMovingForward);
        }
         * */
    }


    // private:


// Member Variables

    // protected:


    // private:


    CNetworkVar<byte> m_fFuckYou = null;


	float m_fCAddawdek;
	byte m_bFuckdawYou;


};
