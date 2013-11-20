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


/* Implementation */


public class CPlayerHeadMotor : CNetworkMonoBehaviour
{

// Member Types
	public class CHeadMotorState
	{
		Vector2 m_CurrentRotation = Vector2.zero;
		float m_LastUpdateTime = 0.0f;
		
		public Vector2 CurrentRotationState
		{
			set 
			{
				m_CurrentRotation = value;
				m_LastUpdateTime = Time.time;
			}
			get 
			{ 
				return(m_CurrentRotation); 
			}
		}
		
		public float TimeStamp { get { return(m_LastUpdateTime); } }
		
		public void SetCurrentRotation(Vector2 _NewState, float _TimeStamp)
		{
			if(CNetwork.IsServer)
			{
				if(m_LastUpdateTime < _TimeStamp)
				{
					m_CurrentRotation = _NewState;
				}
			}
			else
			{
				Logger.Write("Player HeadRotationState: Only server can direcly set the motor state!");
			}
		}

		public void ResetStates()
		{
			CurrentRotationState = Vector2.zero;
		}
	}
	
// Member Fields
	public float m_SensitivityX = 2.0f;
	public float m_SensitivityY = 2.0f;

	public float m_MinimumX = -360.0f;
	public float m_MaximumX = 360.0f;

	public float m_MinimumY = -60.0f;
	public float m_MaximumY = 60.0f;

	public float m_RotationX = 0.0f;
	public float m_RotationY = 0.0f;
		
	
	public GameObject m_ActorHead = null;
	
	
	CHeadMotorState m_HeadMotorState = new CHeadMotorState();
	
	
	CNetworkVar<float> m_HeadEulerX    = null;
    CNetworkVar<float> m_HeadEulerY    = null;
    CNetworkVar<float> m_HeadEulerZ    = null;

	
// Member Properties	
	public GameObject ActorHead 
	{ 
		get
		{ 
			return(m_ActorHead); 
		} 
	}
	
	public Vector3 HeadEuler
    {
        set 
		{ 
			m_HeadEulerX.Set(value.x); m_HeadEulerY.Set(value.y); m_HeadEulerZ.Set(value.z);
		}
        get 
		{ 
			return (new Vector3(m_HeadEulerX.Get(), m_HeadEulerY.Get(), m_HeadEulerZ.Get())); 
		}
    }
	
// Member Methods
    public override void InstanceNetworkVars()
    {
		m_HeadEulerX = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_HeadEulerY = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
        m_HeadEulerZ = new CNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }
	
	
    public void OnNetworkVarSync(INetworkVar _rSender)
    {
		if(!CNetwork.IsServer)
		{
			// Head Rotation
	        if (_rSender == m_HeadEulerX || _rSender == m_HeadEulerY || _rSender == m_HeadEulerZ)
	        {	
	        	m_ActorHead.transform.eulerAngles = HeadEuler;
	        }
		}
    }
	
	public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.PlayerActorViewId != 0)
		{
			CPlayerHeadMotor actorHeadMotor = CGame.PlayerActor.GetComponent<CPlayerHeadMotor>();
			
			_cStream.Write(actorHeadMotor.m_HeadMotorState.CurrentRotationState.x);
			_cStream.Write(actorHeadMotor.m_HeadMotorState.CurrentRotationState.y);
			_cStream.Write(actorHeadMotor.m_HeadMotorState.TimeStamp);
			
			actorHeadMotor.m_HeadMotorState.ResetStates();
		}	
    }


	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		float rotationX = _cStream.ReadFloat();
		float rotationY = _cStream.ReadFloat();
		float timeStamp = _cStream.ReadFloat();
		
		CPlayerHeadMotor actorHeadMotor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerHeadMotor>();
		
		actorHeadMotor.m_HeadMotorState.SetCurrentRotation(new Vector2(rotationX, rotationY), timeStamp);
    }
	
    public void Awake()
	{	
		
	}

    public void Update()
    {	
		if(CGame.PlayerActor == gameObject)
		{
			UpdateHeadMotorInput();
		}
		
		if(CNetwork.IsServer)
		{	
			// Process the actor rotations
			ProcessRotations();
			
			// Syncronize the head rotation
			HeadEuler = m_ActorHead.transform.eulerAngles;
		}
    }
	
	public void AttatchPlayerCamera()
    {
		// Attach the player camera script
		m_ActorHead.AddComponent<CPlayerCamera>();
    }

	static bool m_bFocused = true;
	void OnApplicationFocus(bool _bFocued) {
		m_bFocused = _bFocued;
	}
	
    protected void UpdateHeadMotorInput()
	{
		if (!m_bFocused)
		{
			return;
		}

		Vector2 rotationState = m_HeadMotorState.CurrentRotationState;
		
		// Rotate around Y
		if (Input.GetAxis("Mouse X") != 0.0f)
        {
            rotationState.x += Input.GetAxis("Mouse X");
        }
		
		// Rotate around X
		if (Input.GetAxis("Mouse Y") != 0.0f)
        {
            rotationState.y += Input.GetAxis("Mouse Y");
        }
		
		m_HeadMotorState.CurrentRotationState = rotationState;
	}
	
	
	protected void ProcessRotations()
	{
		// Yaw rotation
		if(m_HeadMotorState.CurrentRotationState.x != 0.0f)
		{
			m_RotationX += m_HeadMotorState.CurrentRotationState.x * m_SensitivityX;
			
			if(m_RotationX > 360.0f)
				m_RotationX -= 360.0f;
			else if(m_RotationX < -360.0f)
				m_RotationX += 360.0f;
				
			m_RotationX = Mathf.Clamp(m_RotationX, m_MinimumX, m_MaximumX);	
		}
		
		// Pitch rotation
		if(m_HeadMotorState.CurrentRotationState.y != 0.0f)
		{
			m_RotationY += m_HeadMotorState.CurrentRotationState.y * m_SensitivityY;
			m_RotationY = Mathf.Clamp(m_RotationY, m_MinimumY, m_MaximumY);
		}
		
		// Apply the pitch to the actor
		transform.eulerAngles = new Vector3(0.0f, m_RotationX, 0.0f);
		
		// Apply the yaw to the camera
		m_ActorHead.transform.eulerAngles = new Vector3(-m_RotationY, m_RotationX, 0.0f);
	}
};
