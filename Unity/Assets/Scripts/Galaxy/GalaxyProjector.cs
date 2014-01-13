using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleEmitter))]
public class GalaxyProjector : MonoBehaviour
{
    public float radius = 3.0f;

    public CGalaxy.SCellPos absoluteCellCentreOfProjection = new CGalaxy.SCellPos(0,0,0);
    public float zoom = 1.0f;   // 1 = Maximum zoom out (-1 to +1). 0 = infinite zoom.

    public int samplesPerAxis = 28; // 25 with particle scale 2.5f
    public float particleScale = 2.0f;  // This changes how much each particle overlaps neighbouring particles.

    private bool mUpToDate = false;

	void Start()
    {
        RefreshProjection();
	}

    // Update is called once per frame
    void Update()
    {
        zoom = 0.01f + 1.5f + Mathf.Cos(Mathf.PingPong(Time.time*0.25f, Mathf.PI)) * 1.5f;
        mUpToDate = false;

        if (!mUpToDate)
            RefreshProjection();
	}

    void RefreshProjection()
    {
        Profiler.BeginSample("RefreshProjection");
        ParticleEmitter emitter = GetComponent<ParticleEmitter>();
        emitter.ClearParticles();

        CGalaxy galaxy = CGalaxy.instance;
        if (galaxy && samplesPerAxis > 0)
        {
            Vector3 centreSample = new Vector3((samplesPerAxis - 1) * 0.5f, (samplesPerAxis - 1) * 0.5f, (samplesPerAxis - 1) * 0.5f);
            
            for (int x = 0; x < samplesPerAxis; ++x)
                for (int y = 0; y < samplesPerAxis; ++y)
                    for (int z = 0; z < samplesPerAxis; ++z)
                    {
                        Vector3 unitPos = new Vector3(x - centreSample.x, y - centreSample.y, z - centreSample.z) / (0.5f * samplesPerAxis);    // -1 to +1 on each axis.
                        Vector3 centreOfProjectionSamplePos = galaxy.AbsoluteCellNoiseSamplePoint(absoluteCellCentreOfProjection);
                        float asteroidDensity = galaxy.SampleNoise(centreOfProjectionSamplePos.x + unitPos.x /** zoom*/, centreOfProjectionSamplePos.y + unitPos.y /** zoom*/, centreOfProjectionSamplePos.z + unitPos.z /** zoom*/, CGalaxy.ENoiseLayer.AsteroidDensity);
                        float asteroidDensityAlpha = asteroidDensity * asteroidDensity * asteroidDensity * asteroidDensity;
                        asteroidDensityAlpha = (1.0f - unitPos.sqrMagnitude) * asteroidDensityAlpha;
                        //if (asteroidDensityAlpha < 0.0f)
                        //    asteroidDensityAlpha = 0.0f;

                        if(asteroidDensityAlpha >= 0.0625f)
                            emitter.Emit(unitPos * radius, Vector3.zero, particleScale * (radius * 2) / samplesPerAxis, float.PositiveInfinity, new Color(0.5f, 0.5f, 0.75f, asteroidDensityAlpha));
                    }

            mUpToDate = true;

        }

        Profiler.EndSample();
    }
}