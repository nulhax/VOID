using UnityEngine;
using System.Collections;

public class CCannon : MonoBehaviour
{
	[HideInInspector] public Rigidbody parent = null;
	float velocity = 75.0f;

	public void Fire(Vector3 targetPos)
	{
		GameObject projectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.CannonProjectile);
		projectile.GetComponent<CNetworkMonoBehaviour>().InvokeRpcAll("RpcInitialise", parent.GetComponent<CNetworkView>().NetworkViewId, transform.position, (targetPos - gameObject.transform.position).normalized * velocity, false);
	}
}