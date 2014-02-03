//
//  (c) 2013 VOID
//
//  File Name   :   CPlayerShipCamera.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CPlayerGalaxyCamera : MonoBehaviour 
{
	// Member Methods
	public void OnGalaxyCameraShifted(GameObject _GameObject, Vector3 _Translation)
	{
		foreach(ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
		{
			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
			int num = ps.GetParticles(particles);
			for(int i = 0; i < num; ++i)
			{
				particles[i].position += _Translation;
			}
			ps.SetParticles(particles, num);
		}
	}
}
