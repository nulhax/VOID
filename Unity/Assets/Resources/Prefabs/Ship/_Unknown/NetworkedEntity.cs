using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CNetworkView))]
public class NetworkedEntity : CNetworkMonoBehaviour
{
	[HideInInspector]
	public bool Synchronise = true;
	public bool Position = true;
	public bool Angle = true;
	public bool PositionalVelocity = false;
	public bool AngularVelocity = false;
	public float UpdatesPerSecond = 0.0f;
	private float TimeUntilNextUpdate = float.PositiveInfinity;

	protected CNetworkVar<float> mPositionX = null;
	protected CNetworkVar<float> mPositionY = null;
	protected CNetworkVar<float> mPositionZ = null;
	protected CNetworkVar<float> mAngleX = null;
	protected CNetworkVar<float> mAngleY = null;
	protected CNetworkVar<float> mAngleZ = null;
	protected CNetworkVar<float> mPositionalVelocityX = null;
	protected CNetworkVar<float> mPositionalVelocityY = null;
	protected CNetworkVar<float> mPositionalVelocityZ = null;
	protected CNetworkVar<float> mAngularVelocityX = null;
	protected CNetworkVar<float> mAngularVelocityY = null;
	protected CNetworkVar<float> mAngularVelocityZ = null;

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		mPositionX = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*transform.position.x*/);
		mPositionY = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*transform.position.y*/);
		mPositionZ = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*transform.position.z*/);
		mAngleX = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*transform.eulerAngles.x*/);
		mAngleY = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*transform.eulerAngles.y*/);
		mAngleZ = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*transform.eulerAngles.z*/);
		mPositionalVelocityX = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*rigidbody.velocity.x*/);
		mPositionalVelocityY = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*rigidbody.velocity.y*/);
		mPositionalVelocityZ = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*rigidbody.velocity.z*/);
		mAngularVelocityX = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*rigidbody.angularVelocity.x*/);
		mAngularVelocityY = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*rigidbody.angularVelocity.y*/);
		mAngularVelocityZ = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f/*rigidbody.angularVelocity.z*/);
	}

	public void OnNetworkVarSync(INetworkVar sender)
	{
		if (!CNetwork.IsServer)
		{
			if (Position && (sender == mPositionX || sender == mPositionY || sender == mPositionZ))
				transform.position = new Vector3(mPositionX.Get(), mPositionY.Get(), mPositionZ.Get());
			else if (Angle && (sender == mAngleX || sender == mAngleY || sender == mAngleZ))
				transform.eulerAngles = new Vector3(mAngleX.Get(), mAngleY.Get(), mAngleZ.Get());
			else if (PositionalVelocity && (sender == mPositionalVelocityX || sender == mPositionalVelocityY || sender == mPositionalVelocityZ))
				rigidbody.velocity = new Vector3(mPositionalVelocityX.Get(), mPositionalVelocityY.Get(), mPositionalVelocityZ.Get());
			else if (AngularVelocity && (sender == mAngularVelocityX || sender == mAngularVelocityY || sender == mAngularVelocityZ))
				rigidbody.angularVelocity = new Vector3(mAngularVelocityX.Get(), mAngularVelocityY.Get(), mAngularVelocityZ.Get());
		}
	}

	void Start()
	{
		System.Diagnostics.Debug.Assert(UpdatesPerSecond >= 0.0f, "UpdatesPerSecond must be nonnegative");

		if (UpdatesPerSecond > 0.0f)
			TimeUntilNextUpdate = 1.0f / UpdatesPerSecond;
	}

	void Update()
	{
		if (CNetwork.IsServer)
		{
			TimeUntilNextUpdate -= Time.deltaTime;
			if (TimeUntilNextUpdate <= 0.0f)
				UpdateNetworkVars();
		}
	}

	public void UpdateNetworkVars()
	{
		System.Diagnostics.Debug.Assert(CNetwork.IsServer, "Only the server can update network vars!");

		if (!Synchronise)
			return;

		TimeUntilNextUpdate = 1.0f / UpdatesPerSecond;
		if (Position)
		{
			Vector3 position = transform.position;
			mPositionX.Set(position.x);
			mPositionY.Set(position.y);
			mPositionZ.Set(position.z);
		}

		if (Angle)
		{
			Vector3 angle = transform.eulerAngles;
			mAngleX.Set(angle.x);
			mAngleY.Set(angle.y);
			mAngleZ.Set(angle.z);
		}

		if (PositionalVelocity)
		{
			Vector3 positionalVelocity = rigidbody.velocity;
			mPositionalVelocityX.Set(positionalVelocity.x);
			mPositionalVelocityY.Set(positionalVelocity.y);
			mPositionalVelocityZ.Set(positionalVelocity.z);
		}

		if (AngularVelocity)
		{
			Vector3 angularVelocity = rigidbody.angularVelocity;
			mAngularVelocityX.Set(angularVelocity.x);
			mAngularVelocityY.Set(angularVelocity.y);
			mAngularVelocityZ.Set(angularVelocity.z);
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (CNetwork.IsServer)
			UpdateNetworkVars();
	}

	void OnCollisionExit(Collision collision)
	{
		if (CNetwork.IsServer)
			UpdateNetworkVars();
	}
}
