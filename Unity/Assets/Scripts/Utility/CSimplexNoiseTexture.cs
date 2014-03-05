using UnityEngine;
using System.Collections;

public class CSimplexNoiseTexture : MonoBehaviour 
{

	public int m_TextureDimensions = 512;
	public int m_Octaves = 8;
	public float m_ScaleFactor = 1.0f;
	public float m_Persistence = 0.75f;

	public bool m_RigidFractial = false;
	//public float m_RigidOffset = 0.5f;
	//public float m_RigidGain = 1.0f;

	Texture2D rt = null;

	void Start()
	{
		if(rt == null)
		{
			Rebuild();
		}
	}

	[ContextMenu("Rebuild")]
	void Rebuild() 
	{
		if(rt != null)
			DestroyImmediate(rt);

		rt = new Texture2D(m_TextureDimensions, m_TextureDimensions);



		PerlinSimplexNoise pn = new PerlinSimplexNoise(Random.seed);



		int numberOfOctaves = Mathf.CeilToInt(Mathf.Log10(m_TextureDimensions)/Mathf.Log10(2.0f));
		if(m_Octaves > numberOfOctaves)
			m_Octaves = numberOfOctaves;



		float xStart = 0.0f;
		float yStart = 0.0f;
		float xEnd = Mathf.Pow(2.0f, (float)m_Octaves) * m_ScaleFactor;
		float yEnd = Mathf.Pow(2.0f, (float)m_Octaves) * m_ScaleFactor;



		Color[] pixels = new Color[m_TextureDimensions * m_TextureDimensions];



		for(int i = 0; i < m_TextureDimensions; i++)
		{
			for(int j = 0; j < m_TextureDimensions; j++)
			{
				float x = (xStart + (float)i * ((xEnd - xStart) / m_TextureDimensions));
				float y = (yStart + (float)j * ((yEnd - yStart) / m_TextureDimensions));

				float final = 0.0f;
				float weight = 1.0f; 
			
				for(int k = 0; k < m_Octaves; ++k)
				{
					float frequency = Mathf.Pow(2.0f, k);
					float amplitude = Mathf.Pow(m_Persistence, m_Octaves - k);

					if(m_RigidFractial)
					{
						float noise = Mathf.Abs(pn.Generate(x/frequency, y/frequency, 1.0f) * amplitude);

//						noise = noise * noise;
//						noise = (m_RigidOffset - 1.0f) - noise;
//						weight = noise;
//						if (weight > 1.0f) weight = 1.0f;
//						if (weight < 0.0f) weight = 0.0f;
						final += noise;
					}
					else
					{
						float noise = pn.Generate(x/frequency, y/frequency, 1.0f) * amplitude;
						final += noise;
					}
				}

				if(!m_RigidFractial)
					final = 0.5f + 0.5f * final;

				pixels[j + (i * m_TextureDimensions)] = new Color(final, final, final);
			}
		}

		rt.SetPixels(pixels);
		rt.Apply();

		renderer.sharedMaterial.mainTexture = rt;
	}
}
