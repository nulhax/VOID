//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CPlayerHeadMotor.cs
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
	
	
	private CHeadMotorState m_HeadMotorState = new CHeadMotorState();
	
	
	private Vector3 m_Rotation = Vector3.zero;
	private bool m_FreezeHeadInput = false;
	private GameObject m_ActorHead = null;
	
	
	
	private CNetworkVar<float> m_HeadEulerX    = null;
    private CNetworkVar<float> m_HeadEulerY    = null;
    private CNetworkVar<float> m_HeadEulerZ    = null;
	
	
	
// Member Properties	
	public GameObject ActorHead 
	{ 
		get
		{ 
			return(m_ActorHead); 
		} 
	}
	
	public bool FreezeHeadInput
	{
		set { m_FreezeHeadInput = value; }
		get { return(m_FreezeHeadInput); }
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
			
			if(!actorHeadMotor.FreezeHeadInput)
			{
				_cStream.Write(actorHeadMotor.m_HeadMotorState.CurrentRotationState.x);
				_cStream.Write(actorHeadMotor.m_HeadMotorState.CurrentRotationState.y);
				_cStream.Write(actorHeadMotor.m_HeadMotorState.TimeStamp);
			}
			
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
		// Create the actor head object
		m_ActorHead = transform.FindChild("Head").gameObject;
	}
	
	public void Start()
	{
		GameObject worldActor = CGame.Ship.GetComponent<CShipPhysicsSimulatior>().GetWorldActor(gameObject);
		
		if(CGame.PlayerActor == gameObject)
		{
			// Add the player camera to this player world actor
			worldActor.transform.FindChild("Head").gameObject.AddComponent<CPlayerCamera>();
		}
		
		// Add the galaxy observer to the world actor
		worldActor.AddComponent<GalaxyObserver>();
	}

    public void Update()
    {	
		if(CGame.PlayerActor == gameObject && !FreezeHeadInput)
		{
			UpdateHeadMotorInput();
		}
    }
	
	public void FixedUpdate()
	{
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
	
    private void UpdateHeadMotorInput()
	{	
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
	
	
	private void ProcessRotations()
	{
		// Pitch rotation
		if(m_HeadMotorState.CurrentRotationState.y != 0.0f)
		{
			m_Rotation.x += m_HeadMotorState.CurrentRotationState.y * m_SensitivityY;
			m_Rotation.x =  Mathf.Clamp(m_Rotation.x, m_MinimumY, m_MaximumY);
		}
		
		// Yaw rotation
		if(m_HeadMotorState.CurrentRotationState.x != 0.0f)
		{
			m_Rotation.y += m_HeadMotorState.CurrentRotationState.x * m_SensitivityX;
		}
		
		// Apply the yaw to the player actor
		Quaternion shipRot = CGame.Ship.transform.rotation;
		transform.rotation = shipRot * Quaternion.AngleAxis(m_Rotation.y, Vector3.up);
		
		// Apply the pitch to the camera
		m_ActorHead.transform.localEulerAngles = new Vector3(-m_Rotation.x, 0.0f, 0.0f);
	}
	
	private void OnApplicationFocus(bool _focusStatus) 
	{
		FreezeHeadInput = !_focusStatus;
	}
};
