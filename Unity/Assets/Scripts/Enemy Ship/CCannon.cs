using UnityEngine;
using System.Collections;

public class CCannon : MonoBehaviour
{
	[HideInInspector] public Rigidbody parent = null;
	float velocity = 7500.0f;

	public void Fire(Vector3 targetPos)
	{
		GameObject projectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.CannonProjectile);
		projectile.transform.position = transform.position;
		projectile.rigidbody.AddForce((targetPos - gameObject.transform.position).normalized * velocity, ForceMode.Impulse);
		projectile.GetComponent<CCannonProjectile>().parent = parent;
	}
}