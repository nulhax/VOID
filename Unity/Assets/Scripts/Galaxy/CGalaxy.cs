//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGalaxy.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CNetworkView))]
[RequireComponent(typeof(CGalaxyNoise))]
public class CGalaxy : CNetworkMonoBehaviour
{
	///////////////////////////////////////////////////////////////////////////
	// Objects:

	public struct SCellPos
	{
		public int x;
		public int y;
		public int z;

		public SCellPos(int _x, int _y, int _z) { x = _x; y = _y; z = _z; }

		public static SCellPos operator +(SCellPos lhs, SCellPos rhs) { return new SCellPos(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z); }
		public static SCellPos operator -(SCellPos lhs, SCellPos rhs) { return new SCellPos(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z); }

		public override string ToString() { return '(' + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ')'; }
	}

	class CCellContent
	{
		public CCellContent(bool alternatorInitialValue, ECellState state) { mAlternator = alternatorInitialValue; mState = state; }
		public bool mAlternator;   // This is used for culling purposes.
		public ECellState mState;    // Cell loading is drawn out over time. This shows whether the cell is ready or waiting.
	}

	class CRegisteredObserver
	{
		public GameObject mEntity;
		public float mBoundingRadius;   // Bounding sphere.

		public CRegisteredObserver(GameObject entity, float boundingRadius) { mEntity = entity; mBoundingRadius = boundingRadius; }
	}

	class CRegisteredGubbin
	{
		public GameObject mEntity;
		public float mBoundingRadius;   // Bounding sphere.
		public TNetworkViewId mNetworkViewID;
		public bool mAlternator;   // This is used for culling purposes.
		public bool mAwaitingCull = false;  // Objects get culled over time.

		public CRegisteredGubbin(GameObject entity, float boundingRadius, TNetworkViewId networkViewID, bool alternatorValue) { mEntity = entity; mBoundingRadius = boundingRadius; mNetworkViewID = networkViewID; mAlternator = alternatorValue; }
	}

	public class CGubbinMeta
	{
		public CGameRegistrator.ENetworkPrefab mPrefabID;
		public SCellPos mParentAbsoluteCell;
		public Vector3 mPosition;
		public Quaternion mRotation;
		public Vector3 mScale;
		public Vector3 mLinearVelocity;
		public Vector3 mAngularVelocity;

		public bool mHasNetworkedEntityScript;
		public bool mHasRigidBody;

		public CGubbinMeta(CGameRegistrator.ENetworkPrefab prefabID, SCellPos parentAbsoluteCell, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 linearVelocity, Vector3 angularVelocity, bool hasNetworkedEntityScript, bool hasRigidBody)
		{
			mPrefabID = prefabID;
			mParentAbsoluteCell = parentAbsoluteCell;
			mPosition = position;
			mRotation = rotation;
			mScale = scale;
			mLinearVelocity = linearVelocity;
			mAngularVelocity = angularVelocity;
			mHasNetworkedEntityScript = hasNetworkedEntityScript;
			mHasRigidBody = hasRigidBody;
		}
	}

	public enum ECellState : uint
	{
		Loading,
		Loaded,
		Unloading
		// An unloaded cell does not exist.
	}

	public delegate void EventOnGalaxyShift(Vector3 translation);

	///////////////////////////////////////////////////////////////////////////
	// Variables:

	private static CGalaxy sGalaxy = null;
	public static CGalaxy instance { get { return sGalaxy; } }

	private CGalaxyNoise mNoise = null;
	public CGalaxyNoise noise { get { return mNoise; } }

	private CGalaxyBackdrop mBackdrop = null;
	public CGalaxyBackdrop backdrop { get { return mBackdrop; } }

	private SCellPos mCentreCell = new SCellPos(0, 0, 0);	// All cells are offset by this cell.
	protected CNetworkVar<int> mCentreCellX;
	protected CNetworkVar<int> mCentreCellY;
	protected CNetworkVar<int> mCentreCellZ;
	public SCellPos centreCell { get { return mCentreCell; } }

	private static int mLayerEnum_Gubbin = LayerMask.NameToLayer("Galaxy_Gubbin");
	public static int layerEnum_Gubbin { get { return mLayerEnum_Gubbin; } }
	private static int mLayerBit_Gubbin = 1 << layerEnum_Gubbin;
	public static int layerBit_Gubbin { get { return mLayerBit_Gubbin; } }

	private static int mLayerEnum_Projectile = LayerMask.NameToLayer("Galaxy_Projectile");
	public static int layerEnum_Projectile { get { return mLayerEnum_Projectile; } }
	private static int mLayerBit_Projectile = 1 << layerEnum_Projectile;
	public static int layerBit_Projectile { get { return mLayerBit_Projectile; } }

	private static int mLayerBit_All = mLayerBit_Gubbin | mLayerBit_Projectile;
	public static int layerBit_All { get { return mLayerBit_All; } }

	public event EventOnGalaxyShift eventPreGalaxyShift;
	public event EventOnGalaxyShift eventPostGalaxyShift;

	private List<GalaxyShiftable> mShiftableEntities = new List<GalaxyShiftable>();	// When everything moves too far in any direction, the transforms of these registered GameObjects are shifted back.
	private List<CRegisteredObserver> mObservers = new List<CRegisteredObserver>();	// Cells are loaded and unloaded based on proximity to observers.
	private List<CRegisteredGubbin> mGubbins;	// Gubbins ("space things") are unloaded based on proximity to cells.
	private List<CGubbinMeta> mGubbinsToLoad;
	private List<CRegisteredGubbin> mGubbinsToUnload;
	private Dictionary<SCellPos, CCellContent> mCells;
	private List<SCellPos> mCellsToLoad;
	private List<SCellPos> mCellsToUnload;

	private float mfGalaxySize = 1391000000.0f;	// (1.3 million kilometres) In metres cubed. Floats can increment up to 16777220.0f (16.7 million).
	protected CNetworkVar<float> mGalaxySize;
	public float galaxySize { get { return mfGalaxySize; } }
	public float galaxyRadius { get { return galaxySize * 0.5f; } }

	private uint muiNumCellSubsets = 20;	// Zero is just the one cell. Also, this is equivalent to the number of bits per axis required to acknowledge each cell (<= 2 for 1 byte, <= 5 for 2 bytes, <= 10 for 4 bytes, <= 21 for 8 bytes).
	protected CNetworkVar<uint> mNumCellSubsets;
	public uint numCellSubsets { get { return muiNumCellSubsets; } }

	public float mfTimeBetweenUpdateCellLoadUnloadQueues = 0.15f;
	private float mfTimeUntilNextUpdateCellLoadUnloadQueues;

	public float mfTimeBetweenCellLoads = 0.05f;
	private float mfTimeUntilNextCellLoad;

	public float mfTimeBetweenCellUnloads = 0.05f;
	private float mfTimeUntilNextCellUnload;

	public float mfTimeBetweenUpdateGubbinUnloadQueue = 0.15f;
	private float mfTimeUntilNextUpdateGubbinUnloadQueue;

	public float mfTimeBetweenGubbinLoads = 0.1f;
	private float mfTimeUntilNextGubbinLoad;

	public float mfTimeBetweenGubbinUnloads = 0.1f;
	private float mfTimeUntilNextGubbinUnload;

	public float mfTimeBetweenShiftTests = 0.5f;
	private float mfTimeUntilNextShiftTest;

	private uint mNumExtraNeighbourCells = 3;   // Number of extra cells to load in every direction (i.e. load neighbours up to some distance).
	public uint numExtraNeighbourCells { get { return mNumExtraNeighbourCells; } }

	private bool mbValidCellValue = false;  // Used for culling cells that are too far away from observers.
	private bool mbValidGubbinValue = false;    // Used for culling gubbins that are too far away from cells.

	public float cellDiameter { get { return mfGalaxySize / numCellsInRow; } }
	public float cellRadius { get { return mfGalaxySize / (numCellsInRow * 2u); } }
	public ulong numCells { get { /*return (uint)Mathf.Pow(8, muiNumCellSubsets);*/ ulong ul = 1; for (uint ui2 = 0; ui2 < muiNumCellSubsets; ++ui2)ul *= 8u; return ul; } }
	public uint numCellsInRow { get { /*return (uint)Mathf.Pow(2, muiNumCellSubsets);*/ uint ui = 1; for (uint ui2 = 0; ui2 < muiNumCellSubsets; ++ui2)ui *= 2; return ui; } }

	[HideInInspector] public bool debug_GalaxyStuff = false;

	///////////////////////////////////////////////////////////////////////////
	// Functions:

	void Awake()
	{
		sGalaxy = this;

		mfTimeUntilNextUpdateCellLoadUnloadQueues = 0.0f;
		mfTimeUntilNextCellLoad = 0.0f;
		mfTimeUntilNextCellUnload = mfTimeBetweenCellLoads / 2;
		mfTimeUntilNextUpdateGubbinUnloadQueue = mfTimeBetweenUpdateCellLoadUnloadQueues / 2;
		mfTimeUntilNextGubbinLoad = 0.0f;
		mfTimeUntilNextGubbinUnload = mfTimeBetweenGubbinLoads / 2;
		mfTimeUntilNextShiftTest = 0.0f;

		mNoise = GetComponent<CGalaxyNoise>();
		mBackdrop = new CGalaxyBackdrop(this);

		if (CNetwork.IsServer)
		{
			mGubbins = new System.Collections.Generic.List<CRegisteredGubbin>();
			mGubbinsToLoad = new System.Collections.Generic.List<CGubbinMeta>();
			mGubbinsToUnload = new System.Collections.Generic.List<CRegisteredGubbin>();
			mCells = new System.Collections.Generic.Dictionary<SCellPos, CCellContent>();
			mCellsToLoad = new System.Collections.Generic.List<SCellPos>();
			mCellsToUnload = new System.Collections.Generic.List<SCellPos>();
		}

		// Statistical data sometimes helps spot errors.
		//Debug.Log("Galaxy is " + mfGalaxySize.ToString("n0") + " units³ with " + muiNumCellSubsets.ToString("n0") + " cell subsets, thus the " + numCells.ToString("n0") + " cells are " + (mfGalaxySize / numCellsInRow).ToString("n0") + " units in diameter and " + numCellsInRow.ToString("n0") + " cells in a row.");
	}

	void OnDestroy()
	{
		sGalaxy = null;
	}

	public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
	{
		mCentreCellX = _cRegistrar.CreateReliableNetworkVar<int>(SyncCentreCell, mCentreCell.x);
		mCentreCellY = _cRegistrar.CreateReliableNetworkVar<int>(SyncCentreCell, mCentreCell.y);
		mCentreCellZ = _cRegistrar.CreateReliableNetworkVar<int>(SyncCentreCell, mCentreCell.z);
		mGalaxySize = _cRegistrar.CreateReliableNetworkVar<float>(SyncGalaxySize, mfGalaxySize);
		mNumCellSubsets = _cRegistrar.CreateReliableNetworkVar<uint>(SyncNumCellSubsets, muiNumCellSubsets);
	}

	void Update()
	{
		if (CNetwork.IsServer)
		{
			if (Input.GetKeyDown(KeyCode.KeypadMultiply)) debug_GalaxyStuff = !debug_GalaxyStuff;
			if (debug_GalaxyStuff)
			{
				bool incrementNoise = Input.GetKeyDown(KeyCode.Keypad9);
				bool decrementNoise = Input.GetKeyDown(KeyCode.Keypad6);
				bool switchNoise = Input.GetKeyDown(KeyCode.Keypad3);

				if (switchNoise)
					mNoise.debug_UsingNoiseLayer = !mNoise.debug_UsingNoiseLayer;

				if (incrementNoise != decrementNoise)
				{
					if (mNoise.debug_UsingNoiseLayer)
					{
						int newNoise = (int)mNoise.debug_NoiseLayer;

						if (incrementNoise)
						{
							newNoise += 1;
							if (newNoise < 0 || newNoise >= (int)CGalaxyNoise.ENoiseLayer.MAX)
								newNoise = 0;
						}
						else
						{
							newNoise -= 1;
							if (newNoise < 0 || newNoise >= (int)CGalaxyNoise.ENoiseLayer.MAX)
								newNoise = (int)(CGalaxyNoise.ENoiseLayer.MAX - 1);
						}

						mNoise.debug_NoiseLayer = (CGalaxyNoise.ENoiseLayer)newNoise;
					}
					else
					{
						int newNoise = (int)mNoise.debug_Noise;

						if (incrementNoise)
						{
							newNoise += 1;
							if (newNoise < 0 || newNoise >= (int)CGalaxyNoise.ENoise.MAX)
								newNoise = 0;
						}
						else
						{
							newNoise -= 1;
							if (newNoise < 0 || newNoise >= (int)CGalaxyNoise.ENoise.MAX)
								newNoise = (int)(CGalaxyNoise.ENoise.MAX - 1);
						}

						mNoise.debug_Noise = (CGalaxyNoise.ENoise)newNoise;
					}
				}
			}

			mfTimeUntilNextUpdateCellLoadUnloadQueues -= Time.deltaTime;
			mfTimeUntilNextCellUnload -= Time.deltaTime;
			mfTimeUntilNextUpdateGubbinUnloadQueue -= Time.deltaTime;
			mfTimeUntilNextGubbinUnload -= Time.deltaTime;
			mfTimeUntilNextShiftTest -= Time.deltaTime;
			mfTimeUntilNextCellLoad -= Time.deltaTime;
			mfTimeUntilNextGubbinLoad -= Time.deltaTime;

			if (mfTimeUntilNextUpdateCellLoadUnloadQueues <= 0.0f)
			{ UpdateCellLoadingUnloadingQueues(); mfTimeUntilNextUpdateCellLoadUnloadQueues = mfTimeBetweenUpdateCellLoadUnloadQueues; }

			while (mfTimeUntilNextCellUnload <= 0.0f)
			{ UnloadQueuedCell(); mfTimeUntilNextCellUnload += mfTimeBetweenCellUnloads; }

			if (mfTimeUntilNextUpdateGubbinUnloadQueue <= 0.0f)
			{ UpdateGubbinUnloadingQueue(); mfTimeUntilNextUpdateGubbinUnloadQueue = mfTimeBetweenUpdateGubbinUnloadQueue; }

			while (mfTimeUntilNextGubbinUnload <= 0.0f)
			{ UnloadQueuedGubbin(); mfTimeUntilNextGubbinUnload += mfTimeBetweenGubbinUnloads; }

			if (mfTimeUntilNextShiftTest <= 0.0f)
			{ ServerTestGalaxyShift(); mfTimeUntilNextShiftTest = mfTimeBetweenShiftTests; }

			while (mfTimeUntilNextCellLoad <= 0.0f)
			{ LoadQueuedCell(); mfTimeUntilNextCellLoad += mfTimeBetweenCellLoads; }

			while (mfTimeUntilNextGubbinLoad <= 0.0f)
			{ LoadQueuedGubbin(); mfTimeUntilNextGubbinLoad += mfTimeBetweenGubbinLoads; }
		}
	}

	private void UpdateCellLoadingUnloadingQueues()
	{
		UpdateCellLoadingQueue();
		UpdateCellUnloadingQueue();
	}

	private void UpdateCellLoadingQueue()
	{
		mbValidCellValue = !mbValidCellValue;   // Alternate the valid cell value. All cells within proximity of an observer will be updated, while all others will retain the old value making it easier to detect and cull them.;

		// Queue for loading: unloaded cells within proximity to observers.
		foreach (CRegisteredObserver observer in mObservers)
		{
			Vector3 observerPosition = observer.mEntity.transform.position;
			SCellPos occupiedRelativeCell = RelativePointToRelativeCell(observerPosition);
			int iCellsInARow = 1 /*Centre cell*/ + (int)mNumExtraNeighbourCells * 2 /*Neighbouring cell rows*/ + (Mathf.CeilToInt((observer.mBoundingRadius / cellRadius) - 1) * 2);    // Centre point plus neighbours per axis.   E.g. 1,3,5,7,9...
			int iNeighboursPerDirection = (iCellsInARow - 1) / 2;                                                                                                                       // Neighbours per direction.                E.g. 0,2,4,6,8...

			for (int x = -iNeighboursPerDirection; x <= iNeighboursPerDirection; ++x)
			{
				for (int y = -iNeighboursPerDirection; y <= iNeighboursPerDirection; ++y)
				{
					for (int z = -iNeighboursPerDirection; z <= iNeighboursPerDirection; ++z)
					{
						// Check if this cell is loaded.
						SCellPos neighbouringRelativeCell = new SCellPos(occupiedRelativeCell.x + x, occupiedRelativeCell.y + y, occupiedRelativeCell.z + z);
						if (RelativeCellWithinProximityOfPoint(neighbouringRelativeCell, observerPosition, observer.mBoundingRadius + cellDiameter * mNumExtraNeighbourCells))
						{
							SCellPos neighbouringAbsoluteCell = neighbouringRelativeCell + mCentreCell;
							CCellContent temp;
							if (mCells.TryGetValue(neighbouringAbsoluteCell, out temp))   // If this cell has already been loaded...
							{
								temp.mAlternator = mbValidCellValue;    // Update alternator to indicate the cell is within proximity to an observer.
								if (temp.mState == ECellState.Unloading) // If this cell is waiting to be unloaded...
								{
									mCellsToUnload.Remove(neighbouringAbsoluteCell);    // Stop it from unloading, as it is now back in proximity to observers.
									temp.mState = ECellState.Loaded;    // Reset cell state to 'loaded', as only loaded cells are queued for unloading.
								}
							}
							else    // This cell has not been loaded...
							{
								mCellsToLoad.Add(neighbouringAbsoluteCell); // Queue cell to load.
								mCells.Add(neighbouringAbsoluteCell, new CCellContent(mbValidCellValue, ECellState.Loading));    // Add cell to dictionary of cells as loading.
							}
						}
					}
				}
			}
		}
	}

	private void UpdateCellUnloadingQueue()
	{
		// Queue for unloading: cells too far away from observers.
		bool restart;
		do
		{
			restart = false;
			foreach (System.Collections.Generic.KeyValuePair<SCellPos, CCellContent> absoluteCell in mCells) // For every loaded cell...
			{
				if (absoluteCell.Value.mAlternator != mbValidCellValue)  // If the cell was not updated to the current alternator value...
				{
					// This cell is not within proximity of any observers.
					switch (absoluteCell.Value.mState)  // Determine how to unload the cell based on its state.
					{
						case ECellState.Loading:    // This cell, which is not within proximity to any observers, is waiting to load.
							mCellsToLoad.Remove(absoluteCell.Key);    // Deregister this cell for loading, as it is no longer necessary to load.
							mCells.Remove(absoluteCell.Key); // Remove this cell from the dictionary.
							restart = true; // Removing an element from a container while it is being iterated breaks the iterator, so the iteration must restart.
							Debug.Log("Galaxy: Hiccup occured in timing of loading/unloading cells. Performance dent is unavoidable as C# lacks required functionality to handle gracefully");
							break;

						case ECellState.Loaded:
							mCellsToUnload.Add(absoluteCell.Key);   // Register this cell for unloading.
							absoluteCell.Value.mState = ECellState.Unloading;   // Mark the cell as waiting to unload.
							break;
					}

					if (restart)    // If a restart is required...
						break;  // The break to stop the loop would have occured within the switch, if the switch didn't use the break keyword itself.
				}
			}
		} while (restart);
	}

	private void UnloadQueuedCell()
	{
		if (mCellsToUnload.Count > 0)	// If there are cells to unload...
		{
			UnloadAbsoluteCell(mCellsToUnload[0]); // Unload the cell.
			mCellsToUnload.RemoveAt(0); // Cell has been removed.
		}
	}

	private void UpdateGubbinUnloadingQueue()
	{
		mbValidGubbinValue = !mbValidGubbinValue;

		MarkGubbinsToPreserve();
		QueueGubbinsForUnloading();
	}

	private void MarkGubbinsToPreserve()
	{
		// Find gubbins that are not within proximity to the cells.

		//foreach (CRegisteredGubbin gubbin in mGubbins)
		//{
		//    foreach (System.Collections.Generic.KeyValuePair<SCellPos, CCellContent> pair in mCells)
		//    {
		//        if (RelativeCellWithinProximityOfPoint(pair.Key - mCentreCell, gubbin.mEntity.transform.position, gubbin.mBoundingRadius))
		//        {
		//            gubbin.mAlternator = mbValidGubbinValue;
		//            break;
		//        }
		//    }
		//}

		foreach (CRegisteredGubbin gubbin in mGubbins)
		{
			Vector3 gubbinPosition = gubbin.mEntity.transform.position;
			SCellPos occupiedRelativeCell = RelativePointToRelativeCell(gubbinPosition);
			int iCellsInARow = 1 + (Mathf.CeilToInt((gubbin.mBoundingRadius / cellRadius) - 1) * 2);    // Centre point plus neighbours per axis.   E.g. 1,3,5,7,9...
			int iNeighboursPerDirection = (iCellsInARow - 1) / 2;                                       // Neighbours per direction.                E.g. 0,2,4,6,8...

			// Iterate through all 3 axis, checking the centre cell first.
			int x = 0;
			int y = 0;
			int z = 0;
			do
			{
				do
				{
					do
					{
						// Check if this cell is loaded.
						SCellPos neighbouringRelativeCell = new SCellPos(occupiedRelativeCell.x + x, occupiedRelativeCell.y + y, occupiedRelativeCell.z + z);
						if (RelativeCellWithinProximityOfPoint(neighbouringRelativeCell, gubbinPosition, gubbin.mBoundingRadius))
						{
							if (mCells.ContainsKey(neighbouringRelativeCell + mCentreCell))
							{
								gubbin.mAlternator = mbValidGubbinValue;
								x = y = z = -1;  // Way to break the nested loop.
							}
						}

						++z; if (z > iNeighboursPerDirection) z = -iNeighboursPerDirection;
					} while (z != 0);

					++y; if (y > iNeighboursPerDirection) y = -iNeighboursPerDirection;
				} while (y != 0);

				++x; if (x > iNeighboursPerDirection) x = -iNeighboursPerDirection;
			} while (x != 0);
		}
	}

	private void QueueGubbinsForUnloading()
	{
		// Queue for unloading: Gubbins that are not within proximity to the cells.
		foreach (CRegisteredGubbin gubbin in mGubbins)
		{
			if (!gubbin.mAwaitingCull && gubbin.mAlternator != mbValidGubbinValue)  // If this gubbin needs to be culled, and is not already marked for culling...
			{
				gubbin.mAwaitingCull = true;    // Mark for culling.
				mGubbinsToUnload.Add(gubbin);   // Queue for culling.
			}
			else if (gubbin.mAwaitingCull && gubbin.mAlternator == mbValidGubbinValue)  // If this gubbin does not need to be culled, but is marked for culling...
			{
				gubbin.mAwaitingCull = false;   // Unmark for culling.
				mGubbinsToUnload.Remove(gubbin);    // Unqueue for culling.
			}
		}
	}

	private void UnloadQueuedGubbin()
	{
		if (mGubbinsToUnload.Count > 0)  // If there are gubbins to unload...
		{
			UnloadGubbin(mGubbinsToUnload[0]);
			mGubbinsToUnload.RemoveAt(0);
		}
	}

	private void ServerTestGalaxyShift()
	{
		// Shift the galaxy if the average position of all points is far from the centre of the scene (0,0,0).
		SCellPos relativeCentrePos = RelativePointToRelativeCell(CalculateAverageObserverPosition());

		if (relativeCentrePos.x != 0 || relativeCentrePos.y != 0 || relativeCentrePos.z != 0)
		{
			if (relativeCentrePos.x != 0)
				mCentreCellX.Set(mCentreCell.x + relativeCentrePos.x);
			if (relativeCentrePos.y != 0)
				mCentreCellY.Set(mCentreCell.y + relativeCentrePos.y);
			if (relativeCentrePos.z != 0)
				mCentreCellZ.Set(mCentreCell.z + relativeCentrePos.z);
		}
	}

	private void LoadQueuedCell()
	{
		if (mCellsToLoad.Count > 0)  // If there are cells to load...
		{
			LoadAbsoluteCell(mCellsToLoad[0]);  // Load the cell.
			mCellsToLoad.RemoveAt(0);
		}
	}

	private void LoadQueuedGubbin()
	{
		if (mGubbinsToLoad.Count > 0)    // If there are gubbins to load...
		{
			for (uint uiTry = 0; uiTry < 3; ++uiTry)   // Try a couple times to place the gubbin.
			{
				if (LoadGubbin(mGubbinsToLoad[0]))
					break;
				else
					mGubbinsToLoad[0].mPosition = new Vector3(Random.Range(-cellRadius, +cellRadius), Random.Range(-cellRadius, +cellRadius), Random.Range(-cellRadius, +cellRadius));
			}
			mGubbinsToLoad.RemoveAt(0);
		}
	}

	private void ClientHandleGalaxyShift(INetworkVar centreCellNetworkVar)
	{
		SCellPos deltaCellPos = new SCellPos(0,0,0);
		Vector3 translation;

		if(centreCellNetworkVar == mCentreCellX)
		{
			deltaCellPos.x = mCentreCellX.Get() - mCentreCell.x;
			translation = new Vector3(deltaCellPos.x * -cellDiameter, 0.0f, 0.0f);
		}
		else if(centreCellNetworkVar == mCentreCellY)
		{
			deltaCellPos.y = mCentreCellY.Get() - mCentreCell.y;
			translation = new Vector3(0.0f, deltaCellPos.y * -cellDiameter, 0.0f);
		}
		else	// centreCellNetworkVar == mCentreCellZ
		{
			deltaCellPos.z = mCentreCellZ.Get() - mCentreCell.z;
			translation = new Vector3(0.0f, 0.0f, deltaCellPos.z * -cellDiameter);
		}

		if (eventPreGalaxyShift != null)
			eventPreGalaxyShift(translation);

		foreach (GalaxyShiftable shiftableEntity in mShiftableEntities)
			shiftableEntity.Shift(translation);

		mCentreCell += deltaCellPos;
		mBackdrop.UpdateBackdrop(mCentreCell);

		if (eventPostGalaxyShift != null)
			eventPostGalaxyShift(translation);
	}

	private Vector3 CalculateAverageObserverPosition()
	{
		Vector3 result = new Vector3();
		if (mObservers.Count > 0)
		{
			foreach (CRegisteredObserver observer in mObservers)
				result += observer.mEntity.transform.position;
			result /= mObservers.Count;
		}

		return result;
	}

	public void SyncCentreCell(INetworkVar sender)
	{
		ClientHandleGalaxyShift(sender);
	}
	public void SyncGalaxySize(INetworkVar sender)
	{
		mfGalaxySize = mGalaxySize.Get();
	}
	public void SyncNumCellSubsets(INetworkVar sender)
	{
		muiNumCellSubsets = mNumCellSubsets.Get();
	}

	public void RegisterObserver(GameObject observer, float boundingRadius)
	{
		mObservers.Add(new CRegisteredObserver(observer, boundingRadius));
	}

	public void DeregisterObserver(GameObject observer)
	{
		foreach (CRegisteredObserver elem in mObservers)
		{
			if (elem.mEntity.GetInstanceID() == observer.GetInstanceID())
			{
				mObservers.Remove(elem);
				break;
			}
		}
	}

	public void RegisterShiftableEntity(GalaxyShiftable shiftableEntity)
	{
		mShiftableEntities.Add(shiftableEntity);
	}

	public void DeregisterShiftableEntity(GalaxyShiftable shiftableEntity)
	{
		mShiftableEntities.Remove(shiftableEntity);
	}

	void LoadAbsoluteCell(SCellPos absoluteCell)
	{
		// If the cell was queued for loading, it will already have an entry in the cell dictionary, but unlike Add(); the [] operator allows overwriting existing elements in the dictionary.
		mCells[absoluteCell] = new CCellContent(mbValidCellValue, ECellState.Loaded); // Create cell with updated alternator to indicate cell is within proximity of observer.

		// Load the content for the cell.
		//if (false)   // TODO: If the content for the cell is on file...
		//{
		//    // TODO: Load content from SQL.
		//}
		//else    // This cell is not on file, so it has not been visited...
		{
			// Generate the content in the cell.
			LoadEnemyShips(absoluteCell);
			LoadAsteroidClusters(absoluteCell);
			LoadSparseAsteroids(absoluteCell);
			LoadDebris(absoluteCell);
		}
	}

	void UnloadAbsoluteCell(SCellPos absoluteCell)
	{
		// Todo: Save stuff to file.
		mCells.Remove(absoluteCell); // Unload the cell.
	}

	public void DeregisterGubbin(GalaxyGubbin gubbinToDeregister)
	{
		for (int ui = 0; ui < mGubbins.Count; ++ui)
		{
			if (mGubbins[ui].mEntity == gubbinToDeregister.gameObject)
			{
				if (mGubbins[ui].mAwaitingCull)
					mGubbinsToUnload.Remove(mGubbins[ui]);

				mGubbins.RemoveAt(ui);
				break;
			}
		}

		gubbinToDeregister.registeredWithGalaxy = false;
	}

	public bool LoadGubbin(CGubbinMeta gubbin)
	{
		// Create object.
		GameObject gubbinObject = CNetwork.Factory.CreateGameObject((ushort)gubbin.mPrefabID);

		if (gubbinObject == null)
		{
			return false;
		}

		Vector3 gubbinPosition = RelativeCellToRelativePoint(gubbin.mParentAbsoluteCell - mCentreCell) + gubbin.mPosition;

		// Check if the new gubbin has room to spawn.
		if (Physics.CheckSphere(gubbinPosition, CUtility.GetBoundingRadius(gubbinObject), 1 << LayerMask.NameToLayer("Galaxy")))
		{
			CNetwork.Factory.DestoryGameObject(gubbinObject);
			return false;
		}

		gubbinObject.AddComponent<GalaxyGubbin>();

		// Grab components for future use.
		CNetworkView networkView = gubbinObject.GetComponent<CNetworkView>(); System.Diagnostics.Debug.Assert(networkView != null); // Get network view - the object is assumed to have one.
		NetworkedEntity networkedEntity = gubbin.mHasNetworkedEntityScript ? gubbinObject.GetComponent<NetworkedEntity>() : null;   // Get networked entity script IF it has one.
		Rigidbody rigidBody = gubbin.mHasRigidBody ? gubbinObject.GetComponent<Rigidbody>() : null; // Get rigid body IF it has one.

		// Parent object.
		gubbinObject.GetComponent<CNetworkView>().SetParent(gameObject.GetComponent<CNetworkView>().ViewId);   // Set the object's parent as the galaxy.

		// Position.
		gubbinObject.transform.position = gubbinPosition; // Set position.
		if (!networkedEntity || !networkedEntity.Position)   // If the object does not have a networked entity script, or if the networked entity script does not update position...
			networkView.SyncTransformPosition();    // Sync the position through the network view.

		// Rotation.
		gubbinObject.transform.rotation = gubbin.mRotation; // Set rotation.
		if (!networkedEntity || !networkedEntity.Angle)  // If the object does not have a networked entity script, or if the networked entity script does not update rotation...
			networkView.SyncTransformRotation();// Sync the rotation through the network view.

		// Scale
		gubbinObject.transform.localScale = gubbin.mScale; // Set scale.
		networkView.SyncTransformScale(); // Sync the scale through the network view.

		// Linear velocity.
		if (rigidBody != null/* && gubbin.mLinearVelocity != null*/)
			rigidBody.velocity = gubbin.mLinearVelocity;

		// Angular velocity.
		if (rigidBody != null/* && gubbin.mAngularVelocity != null*/)
			rigidBody.angularVelocity = gubbin.mAngularVelocity;

		// Sync everything the networked entity script handles.
		if (networkedEntity)
			networkedEntity.UpdateNetworkVars();

		mGubbins.Add(new CRegisteredGubbin(gubbinObject, CUtility.GetBoundingRadius(gubbinObject), networkView.ViewId, mbValidGubbinValue));

		return true;
	}

	void UnloadGubbin(CRegisteredGubbin gubbin)
	{
		// Todo: Save gubbin to file.

		gubbin.mEntity.GetComponent<GalaxyGubbin>().registeredWithGalaxy = false;
		mGubbins.Remove(gubbin);
		CNetwork.Factory.DestoryGameObject(gubbin.mNetworkViewID);
	}

	public Vector3 RelativeCellToRelativePoint(SCellPos relativeCell)
	{
		return new Vector3(relativeCell.x * cellDiameter, relativeCell.y * cellDiameter, relativeCell.z * cellDiameter);
	}

	public Vector3 RelativeCellToAbsolutePoint(SCellPos relativeCell)
	{
		return AbsoluteCellToAbsolutePoint(relativeCell + mCentreCell);
	}

	public Vector3 AbsoluteCellToAbsolutePoint(SCellPos absoluteCell)
	{
		return new Vector3(absoluteCell.x * cellDiameter, absoluteCell.y * cellDiameter, absoluteCell.z * cellDiameter);
	}

	public Vector3 AbsoluteCellToRelativePoint(SCellPos absoluteCell)
	{
		return RelativeCellToRelativePoint(absoluteCell - mCentreCell);
	}

	public SCellPos RelativePointToRelativeCell(Vector3 relativePoint)
	{
		relativePoint.x += cellRadius;
		relativePoint.y += cellRadius;
		relativePoint.z += cellRadius;
		relativePoint /= cellDiameter;
		return new SCellPos(Mathf.FloorToInt(relativePoint.x), Mathf.FloorToInt(relativePoint.y), Mathf.FloorToInt(relativePoint.z));
	}

	public SCellPos RelativePointToAbsoluteCell(Vector3 relativePoint)
	{
		relativePoint.x += cellRadius;
		relativePoint.y += cellRadius;
		relativePoint.z += cellRadius;
		relativePoint /= cellDiameter;
		return new SCellPos(Mathf.FloorToInt(relativePoint.x) + mCentreCell.x, Mathf.FloorToInt(relativePoint.y) + mCentreCell.y, Mathf.FloorToInt(relativePoint.z) + mCentreCell.z);
	}

	public SCellPos AbsolutePointToAbsoluteCell(Vector3 absolutePoint)
	{
		absolutePoint.x += cellRadius;
		absolutePoint.y += cellRadius;
		absolutePoint.z += cellRadius;
		absolutePoint /= cellDiameter;
		return new SCellPos(Mathf.FloorToInt(absolutePoint.x), Mathf.FloorToInt(absolutePoint.y), Mathf.FloorToInt(absolutePoint.z));
	}

	public SCellPos AbsolutePointToRelativeCell(Vector3 absolutePoint)
	{
		absolutePoint.x += cellRadius;
		absolutePoint.y += cellRadius;
		absolutePoint.z += cellRadius;
		absolutePoint /= cellDiameter;
		return new SCellPos(Mathf.FloorToInt(absolutePoint.x) - mCentreCell.x, Mathf.FloorToInt(absolutePoint.y) - mCentreCell.y, Mathf.FloorToInt(absolutePoint.z) - mCentreCell.z);
	}

	public Vector3 RelativePointToAbsolutePoint(Vector3 relativePoint) { return relativePoint + AbsoluteCellToAbsolutePoint(mCentreCell); }
	public Vector3 AbsolutePointToRelativePoint(Vector3 absolutePoint) { return absolutePoint - AbsoluteCellToAbsolutePoint(mCentreCell); }

	public bool RelativeCellWithinProximityOfPoint(SCellPos relativeCell, Vector3 point, float pointRadius)
	{
		Vector3 cellCentrePos = new Vector3(relativeCell.x * cellDiameter, relativeCell.y * cellDiameter, relativeCell.z * cellDiameter);
		float cellBoundingSphereRadius = cellDiameter * 0.86602540378443864676372317075294f;
		return (cellCentrePos - point).sqrMagnitude <= cellBoundingSphereRadius * cellBoundingSphereRadius + pointRadius * pointRadius;
	}

	public uint SparseAsteroidCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(4/*maxAsteroids*/ * mNoise.SampleNoise(absoluteCell, CGalaxyNoise.ENoiseLayer.SparseAsteroids)); }
	public uint AsteroidClusterCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(1/*maxClusters*/ * mNoise.SampleNoise(absoluteCell, CGalaxyNoise.ENoiseLayer.AsteroidClustersHF)); }
	public float DebrisDensity(SCellPos absoluteCell) { return mNoise.SampleNoise(absoluteCell, CGalaxyNoise.ENoiseLayer.DebrisDensity); }
	public float FogDensity(SCellPos absoluteCell) { return mNoise.SampleNoise(absoluteCell, CGalaxyNoise.ENoiseLayer.FogDensity); }
	public float ResourceAmount(SCellPos absoluteCell) { return 800 * mNoise.SampleNoise(absoluteCell, CGalaxyNoise.ENoiseLayer.AsteroidResource); }
	public uint EnemyShipCount(SCellPos absoluteCell) { return (uint)Mathf.RoundToInt(1/*maxEnemyShips*/ * mNoise.SampleNoise(absoluteCell, CGalaxyNoise.ENoiseLayer.EnemyShips)); }

	private void LoadSparseAsteroids(SCellPos absoluteCell)
	{
		float fCellRadius = cellRadius;

		uint uiNumAsteroids = SparseAsteroidCount(absoluteCell);
		for (uint ui = 0; ui < uiNumAsteroids; ++ui)
		{
			mGubbinsToLoad.Add(new CGubbinMeta((CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1),    // Random asteroid prefab.
												absoluteCell,   // Parent cell.
												new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius)), // Position within parent cell.
												Random.rotationUniform, // Rotation.
												Vector3.one * Random.Range(5.0f, 10.0f), // Scale
												Vector3.zero, // Linear velocity.Random.onUnitSphere * Random.Range(10.0f, 25.0f)
                                                Vector3.zero, // Angular velocity.Random.onUnitSphere * Random.Range(0.25f, 0.5f)
												true,   // Has NetworkedEntity script.
												true    // Has a rigid body.
												));
		}
	}

	private void LoadAsteroidClusters(SCellPos absoluteCell)
	{
		float fCellRadius = cellRadius;

		uint uiNumAsteroidClusters = AsteroidClusterCount(absoluteCell);
		for (uint uiCluster = 0; uiCluster < uiNumAsteroidClusters; ++uiCluster)
		{
			//Vector3 linearClusterVelocity = Random.onUnitSphere * Random.Range(0.0f, 75.0f);

			uint uiNumAsteroidsInCluster = (uint)Random.Range(6, 21);
			for (uint uiAsteroid = 0; uiAsteroid < uiNumAsteroidsInCluster; ++uiAsteroid)
			{
				Vector3 clusterCentre = new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius));

				mGubbinsToLoad.Add(new CGubbinMeta((CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.Asteroid_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.Asteroid_LAST + 1),    // Random asteroid prefab.
													absoluteCell,   // Parent cell.
													clusterCentre + Random.onUnitSphere * Random.Range(0.0f, fCellRadius * 0.25f), // Position within parent cell.
													Random.rotationUniform, // Rotation.
													Vector3.one * Random.Range(2.0f, 8.0f), // Scale
                                                    Vector3.zero, // Linear velocity.Random.onUnitSphere * Random.Range(10.0f, 25.0f)
                                                    Vector3.zero, // Angular velocity.Random.onUnitSphere * Random.Range(0.25f, 0.5f)
													true,   // Has NetworkedEntity script.
													true    // Has a rigid body.
													));
			}
		}
	}

	private void LoadDebris(SCellPos absoluteCell)
	{

	}

	private void LoadEnemyShips(SCellPos absoluteCell)
	{
		float fCellRadius = cellRadius;
		uint uiNumEnemyShips = EnemyShipCount(absoluteCell);

		for (uint ui = 0; ui < uiNumEnemyShips; ++ui)
		{
			mGubbinsToLoad.Add(new CGubbinMeta(
				(CGameRegistrator.ENetworkPrefab)Random.Range((ushort)CGameRegistrator.ENetworkPrefab.EnemyShip_FIRST, (ushort)CGameRegistrator.ENetworkPrefab.EnemyShip_LAST + 1),
				absoluteCell,   // Parent cell.
				new Vector3(Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius), Random.Range(-fCellRadius, fCellRadius)), // Position within parent cell.
				Random.rotationUniform, // Rotation.
				Vector3.one, // Scale
				Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 75.0f)*/,	// Linear velocity.
				Vector3.zero/*Random.onUnitSphere * Random.Range(0.0f, 2.0f)*/,	// Angular velocity.
				true,   // Has NetworkedEntity script.
				true    // Has a rigid body.
				));
		}
	}

	void OnGUI()
	{
		if (!debug_GalaxyStuff)
			return;

		float boxWidth = 200;
		float boxHeight = 100;
		GUI.Box(new Rect((Screen.width - boxWidth) / 2, (Screen.height - boxHeight) / 2, boxWidth, boxHeight), "Displaying " + (mNoise.debug_UsingNoiseLayer ? "raw noise" : "final noise") + '\n' + mNoise.Debug_SampleNoiseName() + '\n' + mCentreCell.ToString());
	}

	void OnDrawGizmos()/*OnDrawGizmos & OnDrawGizmosSelected*/
	{
		if (!debug_GalaxyStuff)
			return;

		if (CNetwork.IsServer)
		{
			foreach (CRegisteredObserver elem in mObservers)
				Gizmos.DrawWireSphere(elem.mEntity.transform.position, elem.mBoundingRadius);

			foreach (CRegisteredGubbin elem in mGubbins)
				Gizmos.DrawWireSphere(elem.mEntity.transform.position, elem.mBoundingRadius);

			float fCellDiameter = cellDiameter;
			float fCellRadius = fCellDiameter * .5f;

			foreach (System.Collections.Generic.KeyValuePair<SCellPos, CCellContent> pair in mCells)
			{
				SCellPos relativeCell = pair.Key - mCentreCell;

				float x = relativeCell.x * fCellDiameter;
				float y = relativeCell.y * fCellDiameter;
				float z = relativeCell.z * fCellDiameter;

				// Set colour based on whether it is loading, loaded, or unloading.
				GL.Color(pair.Value.mState == ECellState.Loading ? Color.yellow : pair.Value.mState == ECellState.Loaded ? Color.green : /*Else unloading*/Color.red);

				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
				GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
				GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
				GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
				GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
				GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
				GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
				GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
				GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y - fCellRadius, z - fCellRadius);
				GL.Vertex3(x - fCellRadius, y - fCellRadius, z + fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x - fCellRadius, y + fCellRadius, z - fCellRadius);
				GL.Vertex3(x - fCellRadius, y + fCellRadius, z + fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x + fCellRadius, y - fCellRadius, z - fCellRadius);
				GL.Vertex3(x + fCellRadius, y - fCellRadius, z + fCellRadius);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Vertex3(x + fCellRadius, y + fCellRadius, z - fCellRadius);
				GL.Vertex3(x + fCellRadius, y + fCellRadius, z + fCellRadius);
				GL.End();

				float noiseValue = mNoise.Debug_SampleNoise(pair.Key);
				Gizmos.color = new Color(1.0f, 1.0f, 1.0f, noiseValue);
				Gizmos.DrawSphere(new Vector3(x, y, z), cellRadius * 0.5f);
			}
		}
	}
}