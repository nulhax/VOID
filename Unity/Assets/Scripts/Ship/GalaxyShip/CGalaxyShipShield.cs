//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipShield.cs
//  Description :   --------------------------
//
//  Author      :  Martin Ponichtera, Scott Emery
//  Mail        :  scott.ipod@gmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */
// http://www.geisswerks.com/ryan/BLOBS/blobs.html
// http://codeartist.mx/tutorials/liquids/
// http://wiki.unity3d.com/index.php?title=MetaBalls
// http://unitycoder.com/blog/page/5/
// http://unitycoder.com/upload/demos/VoxelDemos/Metaballs/
// http://stopsecretdesign.wordpress.com/tag/unity-spaceship-shield-shader/
public class CGalaxyShipShield : CNetworkMonoBehaviour 
{
	// Member Types
	public enum EShieldState
	{
		INVALID,
		
		PoweredUp,
		PoweredDown,
		Reacting,
		Charging,
		
		MAX
	}
	
	// Member Fields
	public GameObject m_Shield = null;
	
	private float m_ShieldPower = 100.0f;
	private const float m_MaxShieldPower = 100.0f;
	
	// Take this from the module to determine active
	private bool m_Active = true;
	
	private EShieldState m_ShieldState = EShieldState.PoweredDown;
	
	private const float cShipRechargeRate = 5.0f;
	
	// Member Properies
	
	public bool bActive
	{
		// Get if the shield is active, don't allow others to set
		get { return(m_Active); }
	}
	
	public float fShieldPower
	{
		// Get the shield power, don't allow others to set
		get { return(m_ShieldPower); }
	}
	
	// Member Methods
	void Start()
	{
		gameObject.GetComponentInChildren<CShieldEventHandler>().EventShieldCollider += ShipShieldCollider;
		gameObject.GetComponentInChildren<CShieldEventHandler>().EventShieldDamage += ShipShieldDamage;
		gameObject.GetComponentInChildren<CShieldEventHandler>().EventShieldRecharge += ShieldRecharge;
		gameObject.GetComponentInChildren<CShieldEventHandler>().EventAnimateShield += AnimateShield;

		// Sign up to shield module event and set the active shield.
		// TODO: Make module first. 
	}
	
	public void UpdateShieldBounds(Mesh _ShieldMesh)
	{	
		m_Shield.GetComponent<MeshFilter>().mesh = _ShieldMesh;
		
		m_Shield.GetComponent<MeshCollider>().sharedMesh = null;
		m_Shield.GetComponent<MeshCollider>().sharedMesh = _ShieldMesh;

		//m_Shield.GetComponent<MeshRenderer>().enabled = false;
	}
	
	void ShipShieldCollider(Collider _Collider)
	{
		if(_Collider.gameObject.tag == "Asteroid")
		{
			Debug.Log("Collider entered Trigger as asteroid");
				
			// Take the ship velocity and add it to the asteroid velocity
			//TODO: Make this betterer
			Vector3 Move = CGameShips.GalaxyShip.rigidbody.velocity;
				
			_Collider.gameObject.transform.rigidbody.velocity = Move;
		}
	}
	
	void ShipShieldDamage(Collider _Collider)
	{
		if(CNetwork.IsServer)
		{
			float fDamage = _Collider.rigidbody.mass / 100.0f;

			if(m_ShieldPower <= 0.0f)
			{
				// Let shield activity be false
				m_Active = false;

				// Zero out the ship health
				m_ShieldPower = 0.0f;
				
				// Set the network var
				m_fVarShieldPower.Set(m_ShieldPower);

				
				// Set the network var
				m_bVarIsActive.Set (m_Active);
			}
			else
			{
				m_ShieldPower -= fDamage;
			}

			// Save the asteroid for the raycast to fire at
			AsteroidPos = _Collider.transform.position;

			Debug.Log ("Shield Health is at: " + m_ShieldPower);
		}
	}
	
	void ShieldRecharge()
	{
		if(CNetwork.IsServer)
		{
			if(m_Active == true)
			{
				// As long as the shield power isn't high than max power increment power
				if(m_ShieldPower < m_MaxShieldPower)
				{
					m_ShieldPower += cShipRechargeRate * Time.deltaTime;

                    // Commented out by Nathan to prevent spamming other people's screens.
                    // Please remember to remove these sorts of debugging tools before you push to develop.
                    // Just makes it easier for everyone else to see what we're working on.
                    // Thanks. ^.^
					// Debug.Log ("Shield Health: " + m_ShieldPower);
				}
			}
			else
			{
				// When the shield is not active, drain the shield power. 
				m_ShieldPower -= cShipRechargeRate * Time.deltaTime;
			}
		}
	}

	void AnimateShield(Collider _Collider)
	{
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

		RaycastHit hit;

		if(_Collider.gameObject.tag == "Asteroid")
		{
			// Get the direction of the asteroid from the origin
			Vector3 dir = (AsteroidPos - transform.position).normalized;

			if(Physics.Raycast(transform.position, dir, out hit, (AsteroidPos - transform.position).magnitude))
			{
				//if(hit.collider.gameObject.tag == "Asteroid")
				//{
					Debug.DrawLine(gameObject.transform.position, hit.point, Color.red, 10.0f);

					plane.transform.position = hit.point;
					plane.transform.parent = transform;
		
					Destroy(plane, 10.0f);
				//}
				//else
				//{

				//}
			}
		}
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{
		m_Active = m_bVarIsActive.Get();
		
		m_ShieldPower = m_fVarShieldPower.Get();
	}
	
	
	public override void InstanceNetworkVars (CNetworkViewRegistrar _cRegistrar)
	{
		m_bVarIsActive =  _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync);
		m_fVarShieldPower =  _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, m_ShieldPower);
	}


	// Save the (asteroid) collider position for the ray cast to fire at
	Vector3 AsteroidPos = new Vector3(0.0f, 0.0f, 0.0f);

	CNetworkVar<bool> m_bVarIsActive;
	CNetworkVar<float> m_fVarShieldPower;
}
