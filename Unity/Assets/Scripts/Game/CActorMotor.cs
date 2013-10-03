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


public class CActorMotor : CNetworkMonoBehaviour, IObserver<INetworkVar>
{

// Member Types


    public enum EAction
    {
        MoveForward,
        MoveForwardStop,
        MoveBackward,
        MoveBackwardStop,
        MoveLeft,
        MoveLeftStop,
        MoveRight,
        MoveRightStop,
    }


// Member Functions

    // public:


    public override void InitialiseNetworkVars()
    {
        //m_cTestString = new CNetworkVar<string>(this, "");
        m_cPositionX = new CNetworkVar<float>(this, 0.0f);
        m_cPositionY = new CNetworkVar<float>(this, 0.0f);
        m_cPositionZ = new CNetworkVar<float>(this, 0.0f);
        //m_cRotationX = new CNetworkVar<float>(this, 0.0f);
        //m_cRotationY = new CNetworkVar<float>(this, 0.0f);
        //m_cRotationZ = new CNetworkVar<float>(this, 0.0f);
    }


    public void Awake()
	{
        lol2 = lol;
        ++lol;
	}


    public void OnDestroy()
    {
    }

    static int lol = 0;
    int lol2 = 0;

    public void Update()
    {

        ProcessMovement();

        /*
        if (lol2 == 0&&
            Input.GetKeyDown(KeyCode.W))
        {
            bool lol =networkView.enabled;
            InvokeRpcAll("Kill", 9000, 145, 235.214f, "rewgfewgaew");
        }


        if (lol2 == 0 &&
            Input.GetKeyDown(KeyCode.S))
        {
            m_cPositionY.Set(m_cPositionY.Get() + 1.0f);
        }


        if (lol2 == 0 && Input.GetKeyDown(KeyCode.A))
        {
            ///m_cTestString.Set(m_cTestString.Get() + (char)UnityEngine.Random.Range(65, 91));
        }


        if (lol2 == 0 && Input.GetKeyDown(KeyCode.S))
        {
            //m_cRotationZ.Set(m_cRotationZ.Get() + 5.5f);
        }
         * */
    }


    public void Notify(INetworkVar _rSender, short _sSubject, byte[] _baData)
    {
        if (_rSender == m_cPositionX ||
            _rSender == m_cPositionY ||
            _rSender == m_cPositionZ)
        {
            gameObject.transform.position = new Vector3(PositionX, PositionY, PositionZ);
        }
    }


    [ANetworkRpc]
    public void Kill(int lol, byte _b2, float _fFuck, string _sMyString)
    {
        Logger.WriteError("lol ({0}) b2 ({1}) Fuck ({2}) String ({3})", lol, _b2, _fFuck, _sMyString);
    }


    public float PositionX
    {
        set { m_cPositionX.Set(value); }
        get { return (m_cPositionX.Get()); }
    }


    public float PositionY
    {
        set { m_cPositionY.Set(value); }
        get { return (m_cPositionY.Get()); }
    }


    public float PositionZ
    {
        set { m_cPositionZ.Set(value); }
        get { return (m_cPositionZ.Get()); }
    }


    public static void CompilePlayerControllerOutput(CPacketStream _cStream)
    {
        // Move forwards
        if ( Input.GetKeyDown(KeyCode.W))
        {
            _cStream.Write((byte)EAction.MoveForward);
        }
        
        // Stop moving forwards
        else if (Input.GetKeyUp(m_eMoveForwardKey))
        {
            _cStream.Write((byte)EAction.MoveForwardStop);
        }

        // Move backwards
        if (Input.GetKeyDown(m_eMoveBackwardsKey))
        {
            _cStream.Write((byte)EAction.MoveBackward);
        }

        // Stop moving backwards
        else if (Input.GetKeyUp(m_eMoveBackwardsKey))
        {
            _cStream.Write((byte)EAction.MoveBackwardStop);
        }

        // Move left
        if ( Input.GetKeyDown(m_eMoveLeftKey))
        {
            _cStream.Write((byte)EAction.MoveLeft);
        }

        // Stop moving left
        else if (Input.GetKeyUp(m_eMoveLeftKey))
        {
            _cStream.Write((byte)EAction.MoveLeftStop);
        }

        // Move right
        if (Input.GetKeyDown(m_eMoveRightKey))
        {
            _cStream.Write((byte)EAction.MoveRight);
        }

        // Stop moving right
        else if (Input.GetKeyUp(m_eMoveRightKey))
        {
            _cStream.Write((byte)EAction.MoveRightStop);
        }
    }


    public static void ProcessPlayerControllerInput(CNetworkPlayer _cNetworkPlayer, CPacketStream _cStream)
    {
        while (_cStream.HasUnreadData())
        {
            EAction eAction = (EAction)_cStream.ReadByte();


            switch (eAction)
            {
                case EAction.MoveForward: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveForward = true; break;
                case EAction.MoveForwardStop: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveForward = false; break;
                case EAction.MoveBackward: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveBackward = true; break;
                case EAction.MoveBackwardStop: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveBackward = false; break;

                case EAction.MoveLeft: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveLeft = true; break;
                case EAction.MoveLeftStop: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveLeft = false; break;
                case EAction.MoveRight: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveRight = true; break;
                case EAction.MoveRightStop: _cNetworkPlayer.Actor.GetComponent<CActorMotor>().m_bMoveRight = false; break;
            }
        }
    }


    // protected:


    protected void ProcessMovement()
    {
        Vector3 vPosition = transform.position;


        if (m_bMoveForward &&
            !m_bMoveBackward)
        {
            vPosition.x += m_fMovementVelocity * Time.deltaTime;
            m_cPositionX.Set(vPosition.x);
        }
        else if (m_bMoveBackward &&
                 !m_bMoveForward)
        {
            vPosition.x -= m_fMovementVelocity * Time.deltaTime;
            m_cPositionX.Set(vPosition.x);
        }


        if (m_bMoveLeft &&
            !m_bMoveRight)
        {
            vPosition.z += m_fMovementVelocity * Time.deltaTime;
            m_cPositionZ.Set(vPosition.z);
        }
        else if (m_bMoveRight &&
                !m_bMoveLeft)
        {
            vPosition.z -= m_fMovementVelocity * Time.deltaTime;
            m_cPositionZ.Set(vPosition.z);
        }

        transform.position = vPosition;
    }


    // private:


// Member Variables

    // protected:


    // private:

    //CNetworkVar<string> m_cTestString = null;



    CNetworkVar<float> m_cPositionX    = null;
    CNetworkVar<float> m_cPositionY    = null;
    CNetworkVar<float> m_cPositionZ    = null;
    //CNetworkVar<float> m_cRotationX    = null;
    //CNetworkVar<float> m_cRotationY    = null;
    //CNetworkVar<float> m_cRotationZ    = null;


    bool m_bMoveForward;
    bool m_bMoveBackward;
    bool m_bMoveLeft;
    bool m_bMoveRight;
    
    
    float m_fMovementVelocity = 10.0f;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;



};
