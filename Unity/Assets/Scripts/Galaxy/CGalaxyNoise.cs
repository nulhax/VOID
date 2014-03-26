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
		SparseAsteroidCount,
		AsteroidClusterCount,
		DebrisDensity,
		FogDensity,
		AsteroidResourceAmount,
		EnemyShipCount,
		MAX
	}

	public struct SNoiseMeta
	{
		public float start;
		public float end;
		public float sampleSize;
		public string displayName;

		public SNoiseMeta(float _start, float _end, float _sampleSize, string _displayName) { start = _start; end = _end; sampleSize = _sampleSize; displayName = _displayName; }
	}

	SNoiseMeta[] mNoiseMeta = new SNoiseMeta[(uint)ENoiseLayer.MAX];
	private PerlinSimplexNoise[] mRawNoises = new PerlinSimplexNoise[(uint)ENoiseLayer.MAX];
	protected CNetworkVar<int>[] mNoiseSeeds = new CNetworkVar<int>[(uint)ENoiseLayer.MAX];

	public ENoiseLayer debug_RenderNoise = ENoiseLayer.SparseAsteroidCount;

	public SNoiseMeta[] noiseMeta { get { return mNoiseMeta; } }

	private void Awake()
	{
		mNoiseMeta[(uint)ENoiseLayer.SparseAsteroidCount] =		new SNoiseMeta(0.50f, 0.90f, 100000.00f, "Sparse Asteroids");
		mNoiseMeta[(uint)ENoiseLayer.AsteroidClusterCount] =	new SNoiseMeta(0.80f, 0.90f, 1000000.0f, "Asteroid Clusters");
		mNoiseMeta[(uint)ENoiseLayer.DebrisDensity] =			new SNoiseMeta(0.00f, 1.00f, 250000.00f, "Debris Density");
		mNoiseMeta[(uint)ENoiseLayer.FogDensity] =				new SNoiseMeta(0.40f, 0.80f, 100000.00f, "Fog Density");
		mNoiseMeta[(uint)ENoiseLayer.AsteroidResourceAmount]=	new SNoiseMeta(0.75f, 0.90f, 100000.00f, "Asteroid Resource");
		mNoiseMeta[(uint)ENoiseLayer.EnemyShipCount] =			new SNoiseMeta(1.10f, 1.20f, 100000.00f, "Enemy Ships");

		// Instantiate galaxy noises.
		for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
			mRawNoises[ui] = new PerlinSimplexNoise();
	}

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		for (uint ui = 0; ui < (uint)ENoiseLayer.MAX; ++ui)
			mNoiseSeeds[ui] = _cRegistrar.CreateNetworkVar<int>(SyncNoiseSeed);
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

	private float SampleNoise(CGalaxy.SCellPos absoluteCell, float sampleScale, ENoiseLayer noiseLayer)
	{
		Vector3 samplePoint = CGalaxy.instance.AbsoluteCellToAbsolutePoint(absoluteCell) * (sampleScale / CGalaxy.instance.galaxyRadius);
		return 0.5f + 0.5f * mRawNoises[(uint)noiseLayer].Generate(samplePoint.x, samplePoint.y, samplePoint.z);
	}

	public float SampleNoise(CGalaxy.SCellPos absoluteCell, ENoiseLayer noiseLayer)
	{
		float rawSample =	SampleNoise(absoluteCell, mNoiseMeta[(uint)noiseLayer].sampleSize, noiseLayer);
		float start =		mNoiseMeta[(uint)noiseLayer].start;
		float end =			mNoiseMeta[(uint)noiseLayer].end;
		float filteredSample = (rawSample - start) / (end - start);
		return filteredSample < 0.0f ? 0.0f : filteredSample > 1.0f ? 1.0f : filteredSample;
	}
}