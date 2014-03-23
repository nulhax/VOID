//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CGalaxyShipShield.cs
//  Description :   --------------------------
//
//  Author      :  Scott Emery
//  Mail        :  scott.ipod@gmail.com
//


// Namespaces

using UnityEngine;
using System.Collections;

public class CShieldAnimate : MonoBehaviour {


	public GameObject Shield;
	public float fSpeed = 2.0f;

	private Material m_Material;
	private float m_fTime;

	// Use this for initialization
	void Start ()
	{
		m_Material = Shield.renderer.material;

		m_fTime = 1.0f;

	}
	
	// Update is called once per frame
	void Update ()
	{
		m_fTime += Time.deltaTime * fSpeed;

		m_Material.SetFloat("_Offset", Mathf.Repeat(m_fTime, 1.0f));
	}
}
