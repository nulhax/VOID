﻿using UnityEngine;
using System.Collections;

public class CCannon : MonoBehaviour
{
	[HideInInspector] public Rigidbody parent = null;
	float velocity = 75.0f;

	public void Fire(Vector3 targetPos)
	{
		GameObject projectile = CNetwork.Factory.CreateGameObject(CGameRegistrator.ENetworkPrefab.CannonProjectile);
		projectile.transform.position = transform.position;
		projectile.transform.rotation = Quaternion.LookRotation((targetPos - gameObject.transform.position).normalized);
		projectile.rigidbody.AddForce((targetPos - gameObject.transform.position).normalized * velocity, ForceMode.VelocityChange);
		projectile.GetComponent<CCannonProjectile>().parent = parent;
	}
}