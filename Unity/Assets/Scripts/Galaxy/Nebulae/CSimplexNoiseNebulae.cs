using UnityEngine;
using System.Collections;

public class CSimplexNoiseTexture : MonoBehaviour 
{

	public int m_TextureDimensions = 512;


	public Gradient m_ColorGradient;
	public float m_CutOffDistance = 0.7f;
	public Vector3 m_PositionOffset = Vector3.zero;

	public int m_VolumeSamples = 3;
	public float m_SampleDistance = 0.01f;

	Texture2D rt = null;
	CMultiFractial mf = null;

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
		mf = GetComponent<CMultiFractial>();
		mf.Seed();


		Color[] pixels = new Color[m_TextureDimensions * m_TextureDimensions];



		for(int i = 0; i < m_TextureDimensions; ++i)
		{
			for(int j = 0; j < m_TextureDimensions; ++j)
			{
				float x = (float)i / m_TextureDimensions;
				float y = (float)j / m_TextureDimensions;

				// Bring pos to -1, 1 space
				Vector2 pos = new Vector2((x * 2.0f) - 1.0f, (y * 2.0f) - 1.0f);

				Color col = Color.black;

				for(int k = 0; k < m_VolumeSamples; ++k)
				{
					Vector3 samplePos = new Vector3(pos.x, pos.y, (float)k * m_SampleDistance) + m_PositionOffset;
					float sampleNoise = mf.Noise(samplePos);

					if(mf.m_FractalType == CMultiFractial.EFractalType.BrownianMotion)
					{
						sampleNoise = (sampleNoise + 1.0f) / 2.0f;
					}
					else if(mf.m_FractalType == CMultiFractial.EFractalType.Ridged)
					{
						sampleNoise = (sampleNoise - 0.5f) * 2.0f;
					}

					sampleNoise = Mathf.Clamp01(sampleNoise);
					Color sampleCol = m_ColorGradient.Evaluate(sampleNoise);

					Vector2 clampPos = Vector2.ClampMagnitude(pos, 1.0f);
					if(clampPos.magnitude > m_CutOffDistance)
					{
						sampleCol *= (1.0f - clampPos.magnitude) / (1.0f - m_CutOffDistance);
					}

					col += sampleCol;
				}

				col /= (float)m_VolumeSamples;

				pixels[i + (j * m_TextureDimensions)] = col;
			}
		}




		rt.SetPixels(pixels);
		rt.Apply();

		renderer.material.mainTexture = rt;
	}
}
