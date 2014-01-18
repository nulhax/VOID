//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CDUISprite : CDUIElement 
{
	// Member Fields
    private TextMesh m_textMesh;


    // Member Properties
    public string m_text
    {
        get
        {
            return m_textMesh.text;
        }
        set
        {
            m_textMesh.text = value;
        }
    }

    // Member Methods
	public void Awake()
	{
		ElementType = CDUIElement.EElementType.Sprite;
	}
	
    public void Initialise()
    {
        InitialiseSprite();
    }

	private void InitialiseSprite()
    {
		// Load the texture
		Texture sprite = ((Texture)Resources.Load("Textures/DUI/MonitorBackground1080"));
		m_Dimensions = new Vector2(2.0f, 1.0f);
		
        // Create the mesh
        Mesh spriteMesh = CreatePlaneMesh(m_Dimensions);
		
        // Create the material
        Material spriteMat = new Material(Shader.Find("Unlit/Transparent"));
        spriteMat.SetTexture("_MainTex", sprite);
        spriteMat.name = sprite.name + "_mat";

        // Add the mesh filter
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = spriteMesh;

        // Add the mesh renderer
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = spriteMat;

        // Add the mesh collider
        //MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        //mc.sharedMesh = spriteMesh;
        //mc.isTrigger = true;
	}
}
