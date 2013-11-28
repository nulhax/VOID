using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleEmitter))]
public class GalaxyProjector : MonoBehaviour
{
    public Vector3 centrePoint = new Vector3(0.0f, 0.0f, -1.75f);
    public float radius = 3.0f;
    public float zoom = 1.0f;
    private uint mSamplesPerAxis = 20;
    private bool mUpToDate = false;

	void Start()
    {
        RefreshProjection();
	}

    // Update is called once per frame
    void Update()
    {
        if (!mUpToDate)
            RefreshProjection();
	}

    void RefreshProjection()
    {
        CGalaxy galaxy = CGalaxy.instance;
        if (galaxy)
        {
            Profiler.BeginSample("RefreshProjection");

            ParticleEmitter emitter = GetComponent<ParticleEmitter>();

            uint centreSample = mSamplesPerAxis / 2;
            emitter.ClearParticles();
            for (int x = 0; x < mSamplesPerAxis; ++x)
                for (int y = 0; y < mSamplesPerAxis; ++y)
                    for (int z = 0; z < mSamplesPerAxis; ++z)
                    {
                        Vector3 unitPos = new Vector3(x - centreSample, y - centreSample, z - centreSample) / (0.5f * mSamplesPerAxis);
                        float asteroidDensity = 0.5f + 0.5f * galaxy.SampleNoise(unitPos.x, unitPos.y, unitPos.z, CGalaxy.ENoiseLayer.AsteroidDensity);
                        float asteroidDensityAlpha = asteroidDensity * asteroidDensity * asteroidDensity * asteroidDensity;
                        asteroidDensityAlpha = (1.0f - unitPos.sqrMagnitude) * asteroidDensityAlpha;
                        if (asteroidDensityAlpha < 0.0f)
                            asteroidDensityAlpha = 0.0f;

                        emitter.Emit(centrePoint + unitPos * radius, Vector3.zero, 0.75f, float.PositiveInfinity, new Color(0.5f, 0.5f, 0.75f, asteroidDensityAlpha));
                    }

            mUpToDate = true;

            Profiler.EndSample();
        }
    }
}