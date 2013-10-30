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


public class ActorMotor : CNetworkMonoBehaviour
{

// Member Types
	
// Member Fields
    CNetworkVar<float> m_cPositionX    = null;
    CNetworkVar<float> m_cPositionY    = null;
    CNetworkVar<float> m_cPositionZ    = null;
	
	CNetworkVar<float> m_cVelocityX    = null;
    CNetworkVar<float> m_cVelocityY    = null;
    CNetworkVar<float> m_cVelocityZ    = null;
	
    CNetworkVar<float> m_cRotationX    = null;
    CNetworkVar<float> m_cRotationY    = null;
    CNetworkVar<float> m_cRotationZ    = null;
	CNetworkVar<float> m_cRotationW    = null;
	
	
	public float m_MovementVelocity = 10.0f;
	
	public float m_SensitivityX = 0.5f;
	public float m_SensitivityY = 0.5f;

	public float m_MinimumX = -360.0f;
	public float m_MaximumX = 360.0f;

	public float m_MinimumY = -60.0f;
	public float m_MaximumY = 60.0f;

	public float m_RotationX = 0.0f;
	public float m_RotationY = 0.0f;
	
	
	GameObject m_Camera = null;


    bool m_bMoveForward;
    bool m_bMoveBackward;
    bool m_bMoveLeft;
    bool m_bMoveRight;
	bool m_bJump;
	bool m_bRotateYaw;
	
	
	static bool s_bStateChanged;


    static KeyCode m_eMoveForwardKey = KeyCode.W;
    static KeyCode m_eMoveBackwardsKey = KeyCode.S;
    static KeyCode m_eMoveLeftKey = KeyCode.A;
    static KeyCode m_eMoveRightKey = KeyCode.D;
	
	
// Member Properties	
	public Vector3 Position
    {
        set 
		{ 
			m_cPositionX.Set(value.x); m_cPositionY.Set(value.y); m_cPositionZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_cPositionX.Get(), m_cPositionY.Get(), m_cPositionZ.Get())); 
		}
    }
				
	public Vector3 Velocity
    {
        set 
		{ 
			m_cVelocityX.Set(value.x); m_cVelocityY.Set(value.y); m_cVelocityZ.Set(value.z); 
		}
        get 
		{ 
			return (new Vector3(m_cVelocityX.Get(), m_cVelocityY.Get(), m_cVelocityZ.Get())); 
		}
    }
	
	public Quaternion Rotation
    {
        set 
		{ 
			m_cRotationX.Set(value.x); m_cRotationY.Set(value.y); m_cRotationZ.Set(value.z); m_cRotationW.Set(value.w);
		}
        get 
		{ 
			return (new Quaternion(m_cRotationX.Get(), m_cRotationY.Get(), m_cRotationZ.Get(), m_cRotationW.Get())); 
		}
    }

// Member Methods
    public override void InitialiseNetworkVars()
    {
		m_cPositionX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cPositionZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
		m_cVelocityX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cVelocityY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cVelocityZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		
        m_cRotationX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cRotationY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_cRotationZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_cRotationW = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }


    public void Start()
	{
		if (gameObject != CGame.Actor)
		{
			//gameObject.GetComponent<Rigidbody>().isKinematic = true;
		}
	}


    public void OnDestroy()
    {
    }


    public void Update()
    {
        if (gameObject == CGame.Actor)
        {
			UpdatePlayerInput();
        }
    }
	
	public void FixedUpdate()
    {
        if (gameObject == CGame.Actor)
        {
			ProcessMovement();
			ProcessRotations();
        }
    }
	
	public void CreatePlayerClientCamera()
    {
		// Disable the current camera
        Camera.main.enabled = false;
		
		// Create te camera object for the camera
		m_Camera = GameObject.Instantiate(Resources.Load("Prefabs/Player/Actor Camera", typeof(GameObject))) as GameObject;
        m_Camera.transform.parent = transform;
        m_Camera.transform.localPosition = Vector3.zero;
    }

    public void OnNetworkVarSync(INetworkVar _rSender)
    {
		// Position
        if (_rSender == m_cPositionX || _rSender == m_cPositionY || _rSender == m_cPositionZ)
		{
			if(gameObject.GetComponent<CNetworkView>().ViewId != CGame.ActorViewId)
				rigidbody.position = Position;
		}
		
		// Velocity
        else if (_rSender == m_cVelocityX || _rSender == m_cVelocityY || _rSender == m_cVelocityZ)
        {	
			if(gameObject.GetComponent<CNetworkView>().ViewId != CGame.ActorViewId)
            	rigidbody.velocity = Velocity;
        }
		
		// Rotation
        else if (_rSender == m_cRotationX || _rSender == m_cRotationY || _rSender == m_cRotationZ || _rSender == m_cRotationW)
        {	
			if(gameObject.GetComponent<CNetworkView>().ViewId != CGame.ActorViewId)
            	transform.rotation = Rotation;
        }

    }
	

    public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(s_bStateChanged)
		{	
			_cStream.Write(CGame.Actor.rigidbody.position.x);
			_cStream.Write(CGame.Actor.rigidbody.position.y);
			_cStream.Write(CGame.Actor.rigidbody.position.z);
			
	        _cStream.Write(CGame.Actor.rigidbody.velocity.x);
			_cStream.Write(CGame.Actor.rigidbody.velocity.y);
			_cStream.Write(CGame.Actor.rigidbody.velocity.z);
			
			_cStream.Write(CGame.Actor.rigidbody.rotation.x);
			_cStream.Write(CGame.Actor.rigidbody.rotation.y);
			_cStream.Write(CGame.Actor.rigidbody.rotation.z);
			_cStream.Write(CGame.Actor.rigidbody.rotation.w);
		}
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
			ActorMotor cActorMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<ActorMotor>();

            float x = _cStream.ReadFloat();
			float y = _cStream.ReadFloat();
			float z = _cStream.ReadFloat();
			
			cActorMotor.Position = new Vector3(x, y, z);
			
			x = _cStream.ReadFloat();
			y = _cStream.ReadFloat();
			z = _cStream.ReadFloat();
			
			cActorMotor.Velocity = new Vector3(x, y, z);
			
			x = _cStream.ReadFloat();
			y = _cStream.ReadFloat();
			z = _cStream.ReadFloat();
			float w = _cStream.ReadFloat();
			
			cActorMotor.Rotation = new Quaternion(x, y, z, w);
        }
    }
	
	
    protected void UpdatePlayerInput()
	{
		// Move forwards
        if (Input.GetKeyDown(KeyCode.W))
        {
           m_bMoveForward = true;
        }
        
        // Stop moving forwards
        else if (Input.GetKeyUp(m_eMoveForwardKey))
        {
            m_bMoveForward = false;
        }

        // Move backwards
        if (Input.GetKeyDown(m_eMoveBackwardsKey))
        {
            m_bMoveBackward = true;
        }

        // Stop moving backwards
        else if (Input.GetKeyUp(m_eMoveBackwardsKey))
        {
            m_bMoveBackward = false;
        }

        // Move left
        if ( Input.GetKeyDown(m_eMoveLeftKey))
        {
            m_bMoveLeft = true;
        }

        // Stop moving left
        else if (Input.GetKeyUp(m_eMoveLeftKey))
        {
            m_bMoveLeft = false;
        }

        // Move right
        if (Input.GetKeyDown(m_eMoveRightKey))
        {
            m_bMoveRight = true;
        }

        // Stop moving right
        else if (Input.GetKeyUp(m_eMoveRightKey))
        {
            m_bMoveRight = false;
        }
		
		// Rotate
		if (Input.GetAxis("Mouse X") != 0.0f)
        {
            m_bRotateYaw = true;
        }
		else
		{
			 m_bRotateYaw = false;
		}

		// Jump
		if (Input.GetKeyDown(KeyCode.Space))
		{
			m_bJump = true;
		}
		
		// If any of the above are true, the state will change
		if(m_bMoveForward || m_bMoveBackward || m_bMoveLeft || m_bMoveRight || m_bRotateYaw || m_bJump)
		{
			s_bStateChanged = true;
		}
	}
	
	
	protected void ProcessMovement()
    {
		Transform cRidgetBodyTrans = gameObject.GetComponent<Rigidbody>().transform;
			

		Vector3 vDirForward = cRidgetBodyTrans.TransformDirection(Vector3.forward);
		Vector3 vDirLeft = cRidgetBodyTrans.TransformDirection(Vector3.left);
        Vector3 vVelocity = new Vector3(0.0f, rigidbody.velocity.y, 0.0f);
		
		
		// Moving 
        if (m_bMoveForward &&
            !m_bMoveBackward)
        {
            vVelocity += vDirForward * m_MovementVelocity;
        }
        else if (m_bMoveBackward &&
                 !m_bMoveForward)
        {
            vVelocity -= vDirForward * m_MovementVelocity;
        }

		
		// Strafing
        if (m_bMoveLeft &&
            !m_bMoveRight)
        {
            vVelocity += vDirLeft * m_MovementVelocity;
        }
        else if (m_bMoveRight &&
                !m_bMoveLeft)
        {
            vVelocity -= vDirLeft * m_MovementVelocity;
        }
		
		// Jumping
		if(m_bJump)
		{
			if (vVelocity.y < 0.1 && vVelocity.y > -0.1) 
				rigidbody.AddForce(new Vector3(0.0f, 325.0f, 0.0f));
			
			m_bJump = false;
		}

        rigidbody.velocity = vVelocity;
    }
	
	protected void ProcessRotations()
	{
		// Yaw rotation
		if(m_bRotateYaw)
		{
			m_RotationX += Input.GetAxis("Mouse X") * m_SensitivityX;
			
			if(m_RotationX > 360.0f)
				m_RotationX -= 360.0f;
			else if(m_RotationX < -360.0f)
				m_RotationX += 360.0f;
				
			m_RotationX = Mathf.Clamp (m_RotationX, m_MinimumX, m_MaximumX);	
		}
		
		// Pitch rotation
		m_RotationY += Input.GetAxis("Mouse Y") * m_SensitivityY;
		m_RotationY = Mathf.Clamp (m_RotationY, m_MinimumY, m_MaximumY);
		
		// Apply the yaw to the camera
		m_Camera.transform.eulerAngles = new Vector3(-m_RotationY, m_RotationX, 0.0f);
		
		// Apply the pitch to the actor
		transform.eulerAngles = new Vector3(0.0f, m_RotationX, 0.0f);
		
		// Lock the cursor to the screen
		Screen.lockCursor = true;
	}
};
