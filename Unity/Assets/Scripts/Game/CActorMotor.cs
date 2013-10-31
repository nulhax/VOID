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


public class CActorMotor : CNetworkMonoBehaviour
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
        RotateLeft,
        RotateLeftStop,
        RotateRight,
        RotateRightStop,
		Jump
    }


// Member Functions

    // public:


    public override void InstanceNetworkVars()
    {
		m_cPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        //m_cRotationX = new CNetworkVar<float>(this, 0.0f);
		m_cRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        //m_cRotationZ = new CNetworkVar<float>(this, 0.0f);
    }


    public void Start()
	{
        // Empty

		if (!CNetwork.IsServer)
		{
			gameObject.GetComponent<Rigidbody>().isKinematic = true;
		}
	}


    public void OnDestroy()
    {
    }


    public void Update()
    {
        if (CNetwork.IsServer)
        {
            ProcessMovement();
            ProcessRotation();
        }
    }


    public void OnNetworkVarSync(INetworkVar _rSender)
    {
		Transform cRidgetBodyTrans = gameObject.GetComponent<Rigidbody>().transform;


        if (_rSender == m_cPositionX)
		{
			cRidgetBodyTrans.position = new Vector3(PositionX, cRidgetBodyTrans.position.y, cRidgetBodyTrans.position.z);
		}
		else if (_rSender == m_cPositionY)
		{
			cRidgetBodyTrans.position = new Vector3(cRidgetBodyTrans.position.x, PositionY, cRidgetBodyTrans.position.z);
		}
		else if (_rSender == m_cPositionZ)
		{
			cRidgetBodyTrans.position = new Vector3(cRidgetBodyTrans.position.x, cRidgetBodyTrans.position.y, PositionZ);
		}


        if (_rSender == m_cRotationY)
        {
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, m_cRotationY.Get(), gameObject.transform.eulerAngles.z);
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


    public static void SerializePlayerInput(CNetworkStream _cStream)
    {
        // Move forwards
        if ( Input.GetKeyDown(KeyCode.W))
        {
            _cStream.Write((byte)EAction.MoveForward);
			//CGame.Actor.GetComponent<CActorMotor>().m_bMoveForward = true;
        }
        
        // Stop moving forwards
        else if (Input.GetKeyUp(m_eMoveForwardKey))
        {
            _cStream.Write((byte)EAction.MoveForwardStop);
            //CGame.Actor.GetComponent<CActorMotor>().m_bMoveForward = false;
        }

        // Move backwards
        if (Input.GetKeyDown(m_eMoveBackwardsKey))
        {
            _cStream.Write((byte)EAction.MoveBackward);
            //CGame.Actor.GetComponent<CActorMotor>().m_bMoveBackward = true;
        }

        // Stop moving backwards
        else if (Input.GetKeyUp(m_eMoveBackwardsKey))
        {
            _cStream.Write((byte)EAction.MoveBackwardStop);
            //CGame.Actor.GetComponent<CActorMotor>().m_bMoveBackward = false;
        }

        // Move left
        if ( Input.GetKeyDown(m_eMoveLeftKey))
        {
            _cStream.Write((byte)EAction.MoveLeft);
            //CGame.Actor.GetComponent<CActorMotor>().m_bMoveLeft = true;
        }

        // Stop moving left
        else if (Input.GetKeyUp(m_eMoveLeftKey))
        {
            _cStream.Write((byte)EAction.MoveLeftStop);
            //CGame.Actor.GetComponent<CActorMotor>().m_bMoveLeft = false;
        }

        // Move right
        if (Input.GetKeyDown(m_eMoveRightKey))
        {
            _cStream.Write((byte)EAction.MoveRight);
            //CGame.Actor.GetComponent<CActorMotor>().m_bMoveRight = true;
        }

        // Stop moving right
        else if (Input.GetKeyUp(m_eMoveRightKey))
        {
            _cStream.Write((byte)EAction.MoveRightStop);
            //CGame.Actor.GetComponent<CActorMotor>().m_bMoveRight = false;
        }

        // Rotate left
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _cStream.Write((byte)EAction.RotateLeft);
            //CGame.Actor.GetComponent<CActorMotor>().m_bRotateLeft = true;
        }

        // Rotate left stop
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            _cStream.Write((byte)EAction.RotateLeftStop);
            //CGame.Actor.GetComponent<CActorMotor>().m_bRotateLeft = false;
        }

        // Rotate right
        if (Input.GetKeyDown(KeyCode.E))
        {
            _cStream.Write((byte)EAction.RotateRight);
            //CGame.Actor.GetComponent<CActorMotor>().m_bRotateRight = true;
        }

        // Rotate right stop
        else if (Input.GetKeyUp(KeyCode.E))
        {
            _cStream.Write((byte)EAction.RotateRightStop);
            //CGame.Actor.GetComponent<CActorMotor>().m_bRotateRight = false;
        }

		// Jump
		if (Input.GetKeyDown(KeyCode.Space))
		{
			_cStream.Write((byte)EAction.Jump);
		}
    }


	public static void UnserializePlayerInput(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            EAction eAction = (EAction)_cStream.ReadByte();
			CActorMotor cActorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CActorMotor>();
			Rigidbody cRigidBody = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<Rigidbody>();

            switch (eAction)
            {
				case EAction.MoveForward: cActorMotor.m_bMoveForward = true; break;
                case EAction.MoveForwardStop: cActorMotor.m_bMoveForward = false; break;
                case EAction.MoveBackward: cActorMotor.m_bMoveBackward = true; break;
                case EAction.MoveBackwardStop: cActorMotor.m_bMoveBackward = false; break;

                case EAction.MoveLeft: cActorMotor.m_bMoveLeft = true; break;
                case EAction.MoveLeftStop: cActorMotor.m_bMoveLeft = false; break;
                case EAction.MoveRight: cActorMotor.m_bMoveRight = true; break;
                case EAction.MoveRightStop: cActorMotor.m_bMoveRight = false; break;

                case EAction.RotateLeft: cActorMotor.m_bRotateLeft = true; break;
                case EAction.RotateLeftStop: cActorMotor.m_bRotateLeft = false; break;
                case EAction.RotateRight: cActorMotor.m_bRotateRight = true; break;
                case EAction.RotateRightStop: cActorMotor.m_bRotateRight = false; break;

				case EAction.Jump: if (cRigidBody.velocity.y < 0.1 && cRigidBody.velocity.y > -0.1) cRigidBody.AddForce(new Vector3(0.0f, 325.0f, 0.0f)); break;
            }
        }
    }


    // protected:


    protected void ProcessMovement()
    {
		Transform cRidgetBodyTrans = gameObject.GetComponent<Rigidbody>().transform;


		Vector3 vPosition = cRidgetBodyTrans.position;
		Vector3 vDirForward = cRidgetBodyTrans.TransformDirection(Vector3.forward);
		Vector3 vDirLeft = cRidgetBodyTrans.TransformDirection(Vector3.left);
        Vector3 vVelocity = new Vector3(0.0f, GetComponent<Rigidbody>().velocity.y, 0.0f);


        if (m_bMoveForward &&
            !m_bMoveBackward)
        {
            vVelocity += vDirForward * m_fMovementVelocity;
        }
        else if (m_bMoveBackward &&
                 !m_bMoveForward)
        {
            vVelocity -= vDirForward * m_fMovementVelocity;
        }


        if (m_bMoveLeft &&
            !m_bMoveRight)
        {
            vVelocity += vDirLeft * m_fMovementVelocity;
        }
        else if (m_bMoveRight &&
                !m_bMoveLeft)
        {
            vVelocity -= vDirLeft * m_fMovementVelocity;
        }


        GetComponent<Rigidbody>().velocity = vVelocity;


        m_cPositionX.Set(vPosition.x);
        m_cPositionY.Set(vPosition.y);
        m_cPositionZ.Set(vPosition.z);
    }


    protected void ProcessRotation()
    {
        Vector3 vRotation = transform.eulerAngles;


        if (m_bRotateLeft &&
            !m_bRotateRight)
        {
            vRotation.y -= m_fRotationVeloctiy * Time.deltaTime;
        }
        else if (!m_bRotateLeft &&
                 m_bRotateRight)
        {
            vRotation.y += m_fRotationVeloctiy * Time.deltaTime;
        }


        transform.eulerAngles = vRotation;


        m_cRotationY.Set(vRotation.y);
    }


    // private:


// Member Variables

    // protected:


    // private:


    CNetworkVar<float> m_cPositionX    = null;
    CNetworkVar<float> m_cPositionY    = null;
    CNetworkVar<float> m_cPositionZ    = null;
    //CNetworkVar<float> m_cRotationX    = null;
    CNetworkVar<float> m_cRotationY    = null;
    //CNetworkVar<float> m_cRotationZ    = null;


    float m_fMovementVelocity = 10.0f;
    float m_fRotationVeloctiy = 100.0f;


    bool m_bMoveForward;
    bool m_bMoveBackward;
    bool m_bMoveLeft;
    bool m_bMoveRight;
    bool m_bRotateLeft;
    bool m_bRotateRight;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;



};
