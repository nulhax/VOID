using UnityEngine;
using System.Collections;

public class CCannon : MonoBehaviour
{
	[HideInInspector] public Rigidbody parent = null;
	float velocity = 100.0f;

	public void Fire(Vector3 targetPos)
	{
		GameObject projectile = (GameObject)GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/EnemyShips/EnemyShipProjectile"));
		projectile.transform.position = transform.position;
		projectile.rigidbody.AddForce((targetPos - gameObject.transform.position).normalized * velocity, ForceMode.Impulse);
		projectile.GetComponent<CCannonProjectile>().parent = parent;
	}
}