using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CNetworkView))]
public class CActorNetworkSyncronized : CNetworkMonoBehaviour 
{
	// Member Types


	// Member Delegates and Events


	// Member Fields
	private CNetworkVar<Vector3> m_Position = null;
	private CNetworkVar<Vector3> m_EulerAngles = null;

	[AServerOnly]
	public bool m_SyncPosition = true;

	[AServerOnly]
	public bool m_SyncRotation = true;


	// Member Properties		


	// Member Methods
	public void Start() 
	{
		// Set rigidbody to kinematic on the client
		if(!CNetwork.IsServer && rigidbody != null)
		{
			rigidbody.isKinematic = true;
		}
	}

	public void Update()
	{
		if(CNetwork.IsServer)
		{
			SyncTransform();
		}
	}

	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		m_Position = _cRegistrar.CreateReliableNetworkVar<Vector3>(OnNetworkVarSync, Vector3.zero);
		m_EulerAngles = _cRegistrar.CreateReliableNetworkVar<Vector3>(OnNetworkVarSync, Vector3.zero);
	}

	public void OnNetworkVarSync(INetworkVar _rSender)
	{
		if(!CNetwork.IsServer)
		{
			// Position
			if (_rSender == m_Position)
			{
				transform.position = m_Position.Get();
			}
			
			// Rotation
			else if (_rSender == m_EulerAngles)
			{	
				transform.eulerAngles = m_EulerAngles.Get();
			}
		}
	}

	public void SyncTransform()
	{
		if(m_SyncPosition)
			m_Position.Set(transform.position);

		if(m_SyncRotation)
			m_EulerAngles.Set(transform.eulerAngles);
	}
}
