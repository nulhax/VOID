using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CMultiFractial : MonoBehaviour 
{
	// Member Types
	public enum EFractalType
	{
		BrownianMotion,
		Turbulance,
		Ridged
	}

	// Member Fields
	public int m_Seed = 0;
	public int m_Octaves = 4;
	public float m_Frequency = 5.0f;
	public float m_Lacunarity = 2.0f;
	public float m_Persistance = 0.5f;

	public EFractalType m_FractalType = EFractalType.BrownianMotion;
	public float m_RidgedOffset = 1.0f;

	private PerlinSimplexNoise m_NoiseSampler = null;

	// Member Methods
	public void Seed() 
	{
		if(m_NoiseSampler == null)
		{
			m_NoiseSampler = new PerlinSimplexNoise(m_Seed);
		}
		else
		{
			m_NoiseSampler.Seed(m_Seed);
		}
	}

	public float Noise(float _Postion)
	{
		float freq = m_Frequency;
		float amp = m_Persistance;
		float prev = 1.0f;
		float sum = 0;	

		for(int i = 0; i < m_Octaves; i++) 
		{
			// Get the noise based on the type of fractal we want
			switch (m_FractalType) 
			{
			case EFractalType.BrownianMotion: 
				sum += m_NoiseSampler.Generate(_Postion * freq) * amp;
				break;

			case EFractalType.Turbulance: 
				sum += Mathf.Abs(m_NoiseSampler.Generate(_Postion * freq)) * amp;
				break;

			case EFractalType.Ridged: 
				float value = Ridge(m_NoiseSampler.Generate(_Postion * freq) * amp);
				sum += value * amp * prev;
				prev = value;
				break;

			default: 
				break;
			}

			freq *= m_Lacunarity;
			amp *= m_Persistance;
		}
		return sum;
	}

	public float Noise(Vector2 _Postion)
	{
		float freq = m_Frequency;
		float amp = m_Persistance;
		float prev = 1.0f;
		float sum = 0;	
		
		for(int i = 0; i < m_Octaves; i++) 
		{
			// Get the noise based on the type of fractal we want
			switch (m_FractalType) 
			{
			case EFractalType.BrownianMotion: 
				sum += m_NoiseSampler.Generate(_Postion * freq) * amp;
				break;
				
			case EFractalType.Turbulance: 
				sum += Mathf.Abs(m_NoiseSampler.Generate(_Postion * freq)) * amp;
				break;
				
			case EFractalType.Ridged: 
				float value = Ridge(m_NoiseSampler.Generate(_Postion * freq) * amp);
				sum += value * amp * prev;
				prev = value;
				break;
				
			default: 
				break;
			}
			
			freq *= m_Lacunarity;
			amp *= m_Persistance;
		}
		return sum;
	}

	public float Noise(Vector3 _Postion)
	{
		float freq = m_Frequency;
		float amp = m_Persistance;
		float prev = 1.0f;
		float sum = 0;	
		
		for(int i = 0; i < m_Octaves; i++) 
		{
			// Get the noise based on the type of fractal we want
			switch (m_FractalType) 
			{
			case EFractalType.BrownianMotion: 
				sum += m_NoiseSampler.Generate(_Postion * freq) * amp;
				break;
				
			case EFractalType.Turbulance: 
				sum += Mathf.Abs(m_NoiseSampler.Generate(_Postion * freq)) * amp;
				break;
				
			case EFractalType.Ridged: 
				float value = Ridge(m_NoiseSampler.Generate(_Postion * freq) * amp);
				sum += value * amp * prev;
				prev = value;
				break;
				
			default: 
				break;
			}
			
			freq *= m_Lacunarity;
			amp *= m_Persistance;
		}
		return sum;
	}

//	public float Noise(Vector4 _Postion)
//	{
//		float freq = m_Frequency;
//		float amp = m_Persistance;
//		float prev = 1.0f;
//		float sum = 0;	
//		
//		for(int i = 0; i < m_Octaves; i++) 
//		{
//			// Get the noise based on the type of fractal we want
//			switch (m_FractalType) 
//			{
//			case EFractalType.BrownianMotion: 
//				sum += m_NoiseSampler.Generate(_Postion * freq) * amp;
//				break;
//				
//			case EFractalType.Turbulance: 
//				sum += Mathf.Abs(m_NoiseSampler.Generate(_Postion * freq)) * amp;
//				break;
//				
//			case EFractalType.Ridged: 
//				float value = Ridge(m_NoiseSampler.Generate(_Postion * freq) * amp);
//				sum += value * amp * prev;
//				prev = value;
//				break;
//				
//			default: 
//				break;
//			}
//			
//			freq *= m_Lacunarity;
//			amp *= m_Persistance;
//		}
//		return sum;
//	}

	private float Ridge(float _Value)
	{
		_Value = Mathf.Abs(_Value);
		_Value = m_RidgedOffset - _Value;
		_Value = _Value * _Value;
		return _Value;
	}
}
