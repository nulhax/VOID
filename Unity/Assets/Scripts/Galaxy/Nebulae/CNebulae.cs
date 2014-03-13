using UnityEngine;
using System.Collections;

public class CNebulae : MonoBehaviour 
{	
	public int m_Seed = 0;
	public float m_Frequency = 0.001f;
	public float m_Lacunarity = 2.0f;
	public float m_Persistence = 0.5f;
	public Gradient m_Color;

	public Transform m_OffsetFrom = null;

	private CImprovedPerlinNoise m_Perlin;
	private Texture2D m_ColorGradient = null;

	private int m_PrevSeed = 0;
	private Gradient m_PrevGradiant = new Gradient();
	
	void Start () 
	{
		m_Perlin = new CImprovedPerlinNoise();

		LoadNoiseResources();

		LoadColorGradiant();
	}

	void LoadColorGradiant()
	{
		if(m_ColorGradient == null)
			m_ColorGradient = new Texture2D(256, 1); 

		Color[] pixels = new Color[256];
		for(int i = 0; i < 256; ++i)
		{
			pixels[i] = m_Color.Evaluate((float)i / 256.0f);
		}
		m_ColorGradient.SetPixels(pixels);
		m_ColorGradient.Apply();

		m_PrevGradiant.SetKeys(m_Color.colorKeys, m_Color.alphaKeys);

		renderer.material.SetTexture("_ColorGradient", m_ColorGradient);
	}

	void LoadNoiseResources()
	{
		m_Perlin.Seed(m_Seed);
		m_Perlin.LoadResourcesFor3DNoise();

		m_PrevSeed = m_Seed;

		renderer.material.SetTexture("_PermTable2D", m_Perlin.GetPermutationTable2D());
		renderer.material.SetTexture("_Gradient3D", m_Perlin.GetGradient3D());
	}
	
	void Update()
	{
		if(m_PrevGradiant != m_Color)
		{
			LoadColorGradiant();
		}

		if(m_PrevSeed != m_Seed)
		{
			LoadNoiseResources();
		}

		if(m_OffsetFrom != null)
		{
			renderer.material.SetVector("_Offset", m_OffsetFrom.transform.localPosition);
		}

		renderer.material.SetFloat("_Frequency", m_Frequency);
		renderer.material.SetFloat("_Lacunarity", m_Lacunarity);
		renderer.material.SetFloat("_Persistence", m_Persistence);
	}
	
}
