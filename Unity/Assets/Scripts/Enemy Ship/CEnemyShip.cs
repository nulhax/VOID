using UnityEngine;
using System.Collections;

public class CEnemyShip : MonoBehaviour
{
	void Start ()
    {
        transform.rotation = Random.rotation;
	}
	
	void Update ()
    {
        GameObject playerShip = CGameShips.GalaxyShip;
        if (playerShip)
        {
            Vector3 dirToPlayerShip = (playerShip.transform.position - gameObject.transform.position).normalized;
            gameObject.rigidbody.AddTorque(Quaternion.Angle(gameObject.transform.rotation, Quaternion.LookRotation(dirToPlayerShip)));
            gameObject.rigidbody.AddForce(dirToPlayerShip * 10.0f, ForceMode.VelocityChange);
        }
	}
}
