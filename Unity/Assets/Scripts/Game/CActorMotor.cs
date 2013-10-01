//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
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


public class CActorMotor : MonoBehaviour, IObserver<INetworkVar>, INetworkComponent
{

// Member Types


// Member Functions

    // public:


    public void InitialiseNetworkVars()
    {
        m_cTestString = new CNetworkVar<string>(this, "");
        //m_cPositionX = new CNetworkVar<float>(this, 0.0f);
        //m_cPositionY = new CNetworkVar<float>(this, 0.0f);
        //m_cPositionZ = new CNetworkVar<float>(this, 0.0f);
        //m_cRotationX = new CNetworkVar<float>(this, 0.0f);
       // m_cRotationY = new CNetworkVar<float>(this, 0.0f);
        m_cRotationZ = new CNetworkVar<float>(this, 0.0f);
    }


    public void Start()
	{

	}


    public void OnDestroy()
    {
    }


    public void Update()
    {
        ProcessMovement();


        if (Input.GetKeyDown(KeyCode.W))
        {
            //GetComponent<CNetworkView>().InvokeRpc(this, "Kill", 9000, 145, 235.214f, m_cTestString.Get());
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            //m_cTestString.Set(m_cTestString.Get() + (char)UnityEngine.Random.Range(65, 91));
        }


        if (Input.GetKeyDown(KeyCode.S))
        {
            //m_cRotationZ.Set(m_cRotationZ.Get() + 5.5f);
        }
    }


    public void Notify(INetworkVar _rSender, short _sSubject, byte[] _baData)
    {
		
    }


    [ANetworkRpc]
    public void Kill(int lol, byte _b2, float _fFuck, string _sMyString)
    {
        Debug.LogError(string.Format("lol ({0}) b2 ({1}) Fuck ({2}) String ({3})", lol, _b2, _fFuck, _sMyString));
    }


    // protected:


    protected void ProcessMovement()
    {
        Vector3 vPosition = transform.position;


        if (m_bMoveForward)
        {
            vPosition.z += m_fMovementVelocity * Time.deltaTime;
        }
        else if (m_bMoveBackwards)
        {
            vPosition.z -= m_fMovementVelocity * Time.deltaTime;
        }


        if (m_bMoveLeft)
        {
            vPosition.x -= m_fMovementVelocity * Time.deltaTime;
        }
        else if (m_bMoveRight)
        {
            vPosition.x += m_fMovementVelocity * Time.deltaTime;
        }
    }


    // private:


// Member Variables

    // protected:


    // private:

    CNetworkVar<string> m_cTestString = null;



    //CNetworkVar<float> m_cPositionX    = null;
    //CNetworkVar<float> m_cPositionY    = null;
    //CNetworkVar<float> m_cPositionZ    = null;
    //CNetworkVar<float> m_cRotationX    = null;
    //CNetworkVar<float> m_cRotationY    = null;
    CNetworkVar<float> m_cRotationZ    = null;
    
    
    float m_fMovementVelocity          = 10.0f;


    bool m_bMoveForward                = false;
    bool m_bMoveBackwards              = false;
    bool m_bMoveLeft                   = false;
    bool m_bMoveRight                  = false;



};
