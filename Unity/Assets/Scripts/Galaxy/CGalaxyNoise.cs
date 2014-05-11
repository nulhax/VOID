//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGalaxy_Noise.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CNetworkView))]
public class CGalaxyNoise : CNetworkMonoBehaviour
{
	public enum ENoiseLayer
	{
		SparseAsteroids,
		AsteroidClustersLF,
		AsteroidClustersHF,
		DebrisDensity,
		FogDensity,
		AsteroidResource,
		EnemyShips,
		MAX
	}

	public enum ENoise
	{
		SparseAsteroids,
		AsteroidClusters,
		DebrisDensity,
		FogDensity,
		AsteroidResource,
		EnemyShips,
		MAX
	}

	public struct SNoiseLayerMeta
	{
		public float start;
		public float end;
		public float sampleSize;
		public string displayName;

		public SNoiseLayerMeta(float _start, float _end, float _sampleSize, string _displayName) { start = _start; end = _end; sampleSize = _sampleSize; displayName = _displayName; }
	}

	public struct SNoiseMeta
	{
		public delegate void NoiseFunction(CGalaxy.SCellPos absoluteCell, out float sample);

		public string displayName;
		public NoiseFunction noiseFunction;

		public SNoiseMeta(string _displayName, NoiseFunction _noiseFunction) { displayName = _displayName; noiseFunction = _noiseFunction; }
	}

	SNoiseMeta[] mNoiseMeta = new SNoiseMeta[(uint)ENoise.MAX];
	SNoiseLayerMeta[] mNoiseLayerMeta = new SNoiseLayerMeta[(uint)ENoiseLayer.MAX];
	private PerlinSimplexNoise[] mRawNoises = new PerlinSimplexNoise[(uint)ENoiseLayer.MAX];
	protected CNetworkVar<int>[] mNoiseSeeds = new CNetworkVar<int>[(uint)ENoiseLayer.MAX];

	public delegate void Debug_CallbackOnNoiseChange();
	public Debug_CallbackOnNoiseChange debug_CallbackOnNoiseChange;
	public float Debug_SampleNoise(CGalaxy.SCellPos absoluteCell) { return debug_UsingNoiseLayer ? SampleNoise(absoluteCell, debug_NoiseLayer) : SampleNoise(absoluteCell, debug_Noise); }
	public string Debug_SampleNoiseName() { return debug_UsingNoiseLayer ? mNoiseLayerMeta[(uint)debug_NoiseLayer].displayName : mNoiseMeta[(uint)debug_Noise].displayName; }
	public ENoise debug_Noise { get { return debug_Noise_Internal; } set { if (debug_Noise_Internal != value) { debug_Noise_Internal = value; if (debug_CallbackOnNoiseChange != null && !debug_UsingNoiseLayer) debug_CallbackOnNoiseChange(); } } }
	private ENoise debug_Noise_Internal = ENoise.SparseAsteroids;
	public ENoiseLayer debug_NoiseLayer { get { return debug_NoiseLayer_Internal; } set { if (debug_NoiseLayer_Internal != value) { debug_NoiseLayer_Internal = value; if (debug_CallbackOnNoiseChange != null && debug_UsingNoiseLayer) debug_CallbackOnNoiseChange(); } } }
	private ENoiseLayer debug_NoiseLayer_Internal = ENoiseLayer.SparseAsteroids;
	public bool debug_UsingNoiseLayer { get { return debug_UsingNoiseLayer_Internal; } set { if (debug_UsingNoiseLayer_Internal != value) { debug_UsingNoiseLayer_Internal = value; if (debug_CallbackOnNoiseChange != null) debug_CallbackOnNoiseChange(); } } }
	private bool debug_UsingNoiseLayer_Internal = false;

	public SNoiseLayerMeta[] noiseMeta { get { return mNoiseLayerMeta; } }

	private void Awake()
	{
		mNoiseLayerMeta[(uint)ENoiseLayer.SparseAsteroids] =	new SNoiseLayerMeta(0.50f, 0.90f, 100000.00f, "Sparse Asteroids");
		mNoiseLayerMeta[(uint)ENoiseLayer.AsteroidClustersHF] =	new SNoiseLayerMeta(0.80f, 0.90f, 1000000.0f, "Asteroid Clusters HF");
		mNoiseLayerMeta[(uint)ENoiseLayer.AsteroidClustersLF] =	new SNoiseLayerMeta(0.30f, 0.90f, 2.5000000f, "Asteroid Clusters LF");
		mNoiseLayerMeta[(uint)ENoiseLayer.DebrisDensity] =		new SNoiseLayerMeta(0.00f, 1.00f, 250000.00f, "Debris Density");
		mNoiseLayerMeta[(uint)ENoiseLayer.FogDensity] =			new SNoiseLayerMeta(0.40f, 0.80f, 100000.00f, "Fog Density");
		mNoiseLayerMeta[(uint)ENoiseLayer.AsteroidResource]=	new SNoiseLayerMeta(0.75f, 0.90f, 100000.00f, "Asteroid Resource");
		mNoiseLayerMeta[(uint)ENoiseLayer.EnemyShips] =			new SNoiseLayerMeta(0.80f, 0.90f, 1000000.0f, "Enemy Ships");

		mNoiseMeta[(uint)ENoise.SparseAsteroids] =	new SNoiseMeta("Sparse Asteroids",	(CGalaxy.SCellPos absoluteCell, out float sample) => {
			sample = SampleNoise(absoluteCell, ENoiseLayer.SparseAsteroids);
		});

		mNoiseMeta[(uint)ENoise.AsteroidClusters] =	new SNoiseMeta("Asteroid Clusters",	(CGalaxy.SCellPos absoluteCell, out float sample) => {
			sample =	1.0f;
			//sample *=	SampleNoise(absoluteCell, ENoiseLayer.AsteroidClustersHF);
			sample *=	SampleNoise(absoluteCell, ENoiseLayer.AsteroidClustersLF);
			float distToCentreOfGalaxy = Mathf.Clamp01(CGalaxy.instance.AbsoluteCellToAbsolutePoint(absoluteCell).magnitude / CGalaxy.instance.galaxyRadius);
			//sample -= distToCentreOfGalaxy;
			sample -= 1.0f - Mathf.Pow(1.0f - distToCentreOfGalaxy, 2.0f);
		});

		mNoiseMeta[(uint)ENoise.DebrisDensity] =	new SNoiseMeta("Debris Density",	(CGalaxy.SCellPos absoluteCell, out float sample) => {
			sample = SampleNoise(absoluteCell, ENoiseLayer.DebrisDensity);
		});

		mNoiseMeta[(uint)ENoise.FogDensity] =		new SNoiseMeta("Fog Density",		(CGalaxy.SCellPos absoluteCell, out float sample) => {
			sample = SampleNoise(absoluteCell, ENoiseLayer.FogDensity);
		});

		mNoiseMeta[(uint)ENoise.AsteroidResource] =	new SNoiseMeta("Asteroid Resource",	(CGalaxy.SCellPos absoluteCell, out float sample) => {
			sample = SampleNoise(absoluteCell, ENoiseLayer.AsteroidResource);
		});

		mNoiseMeta[(uint)ENoise.EnemyShips] =		new SNoiseMeta("Enemy Ships",		(CGalaxy.SCellPos absoluteCell, out float sample) => {
			sample = SampleNoise(absoluteCell, ENoiseLayer.EnemyShips);
		});

		// Instantiate galaxy noises.
		for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
			mRawNoises[ui] = new PerlinSimplexNoise();
	}

	public override void RegisterNetworkEntities(CNetworkViewRegistrar _cRegistrar)
	{
		for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
			mNoiseSeeds[ui] = _cRegistrar.CreateReliableNetworkVar<int>(SyncNoiseSeed);
	}

	private void Start()
	{
		if (CNetwork.IsServer)
		{
			// Seed galaxy noises through the network variable to sync the seed across all clients.
			for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
				mNoiseSeeds[ui].Set(Random.Range(int.MinValue, int.MaxValue));
		}
	}

	private void SyncNoiseSeed(INetworkVar sender)
	{
		for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
			if (mNoiseSeeds[ui] == sender)
				mRawNoises[ui].Seed(mNoiseSeeds[ui].Get());
	}

	//public float SampleNoise(CGalaxy.SCellPos absoluteCell, ENoiseLayer noiseLayer)
	//{
	//    float start =			mNoiseMeta[(uint)noiseLayer].start;
	//    float end =				mNoiseMeta[(uint)noiseLayer].end;
	//    float sampleSize =		mNoiseMeta[(uint)noiseLayer].sampleSize;
	//    float rawSample =		0.5f + 0.5f * mRawNoises[(uint)noiseLayer].Generate(CGalaxy.instance.AbsoluteCellToAbsolutePoint(absoluteCell) * (sampleSize / CGalaxy.instance.galaxyRadius));
	//    float filteredSample =	(rawSample - start) / (end - start);
	//    float saturatedSample =	filteredSample < 0.0f ? 0.0f : filteredSample > 1.0f ? 1.0f : filteredSample;

	//    return saturatedSample;
	//}

	public float SampleNoise(CGalaxy.SCellPos absoluteCell, ENoiseLayer noiseLayer)
	{
		float start =		mNoiseLayerMeta[(uint)noiseLayer].start;
		float end =			mNoiseLayerMeta[(uint)noiseLayer].end;
		float sampleSize =	mNoiseLayerMeta[(uint)noiseLayer].sampleSize;

		float rawSample = 0.5f + 0.5f * mRawNoises[(uint)noiseLayer].Generate(CGalaxy.instance.AbsoluteCellToAbsolutePoint(absoluteCell) * (sampleSize / CGalaxy.instance.galaxyRadius));
		float filteredSample = (rawSample - start) / (end - start);
		float saturatedSample = filteredSample < 0.0f ? 0.0f : filteredSample > 1.0f ? 1.0f : filteredSample;

		return saturatedSample;
	}

	public float SampleNoise(CGalaxy.SCellPos absoluteCell, ENoise noise)
	{
		float sample;
		mNoiseMeta[(uint)noise].noiseFunction(absoluteCell, out sample);
		return sample;
	}

	public float SampleNoise(Vector3 absolutePoint, ENoiseLayer noiseLayer)
	{
		float start = mNoiseLayerMeta[(uint)noiseLayer].start;
		float end = mNoiseLayerMeta[(uint)noiseLayer].end;
		float sampleSize = mNoiseLayerMeta[(uint)noiseLayer].sampleSize;

		float rawSample = 0.5f + 0.5f * mRawNoises[(uint)noiseLayer].Generate(absolutePoint * (sampleSize / CGalaxy.instance.galaxyRadius));
		float filteredSample = (rawSample - start) / (end - start);
		float saturatedSample = filteredSample < 0.0f ? 0.0f : filteredSample > 1.0f ? 1.0f : filteredSample;

		return saturatedSample;
	}

	//public float SampleNoise(Vector3 absolutePoint, ENoise noise)
	//{

	//}
}