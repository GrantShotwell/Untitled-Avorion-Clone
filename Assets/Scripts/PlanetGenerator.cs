using System;
using UnityEngine;

[SelectionBase]
public class PlanetGenerator : MonoBehaviour {

	public ComputeShader sphereCompute;
	public GameObject prefab;

	/// <summary>The radius of the planet.</summary>
	public float radius = 100;
	/// <summary>The minimum distance from the radius to include in chunks</summary>
	public float buffer = 100;

	private Vector3Int gridSize = new Vector3Int(0, 0, 0);
	public Vector3Int chunkSize;
	public GameObject[,,] chunks = new GameObject[0, 0, 0];

	private bool needsUpdate = false;
	public delegate void UpdateRequest();
	public event UpdateRequest update = () => { };


	void Start() {
		RequestUpdate();
	}

	void Update() {
		if(needsUpdate) {
			UpdateChunksArray();
			UpdateChunkTerrain();
			needsUpdate = false;
			update.Invoke();
		}
	}

	void OnValidate() {

		// Chunk size cannot be zero.
		if(chunkSize.x < 1) chunkSize.x = 1;
		if(chunkSize.y < 1) chunkSize.y = 1;
		if(chunkSize.z < 1) chunkSize.z = 1;

		if(sphereCompute != null) {

			// Chunk sizes must be a multiple of thread counts.
			sphereCompute.GetKernelThreadGroupSizes(0, out uint threadX, out uint threadY, out uint threadZ);
			int x = (int)threadX, y = (int)threadY, z = (int)threadZ;
			int dx = x - (chunkSize.x % x), dy = y - (chunkSize.y % y), dz = z - (chunkSize.z % z);
			if(dx < x) chunkSize.x += dx;
			if(dy < y) chunkSize.y += dy;
			if(dz < z) chunkSize.z += dz;

		}

		// Values have been updated.
		RequestUpdate();

	}


	public void RequestUpdate() {
		needsUpdate = true;
	}

	void UpdateChunksArray() {

		// How many chunks do we need to fill at least one (radius + buffer)?
		float buffered = radius + buffer;
		Vector3Int newSize = new Vector3Int(
			Mathf.CeilToInt(buffered / chunkSize.x * 2),
			Mathf.CeilToInt(buffered / chunkSize.y * 2),
			Mathf.CeilToInt(buffered / chunkSize.z * 2));
		Vector3Int halfSize = newSize / 2;
		GameObject[,,] newChunks = new GameObject[newSize.x, newSize.y, newSize.z];

		int maxX = Math.Max(gridSize.x, newSize.x);
		int maxY = Math.Max(gridSize.y, newSize.y);
		int maxZ = Math.Max(gridSize.z, newSize.z);
		for(int x = 0; x < maxX; x++) {
			for(int y = 0; y < maxY; y++) {
				for(int z = 0; z < maxZ; z++) {

					if(x >= newSize.x || y >= newSize.z || z >= newSize.z) {
						Destroy(chunks[x, y, z]);
					} else {
						GameObject oldChunk;
						if(x < gridSize.x && y < gridSize.y && z < gridSize.z) {
							oldChunk = chunks[x, y, z];
						} else {
							oldChunk = Instantiate(prefab, transform);
							Vector3Int indexPos = new Vector3Int(
								x * newSize.x - halfSize.x,
								y * newSize.y - halfSize.y,
								z * newSize.z - halfSize.z);
							if(indexPos.x > 0) indexPos.x -= 1;
							if(indexPos.y > 0) indexPos.y -= 1;
							if(indexPos.z > 0) indexPos.z -= 1;
							oldChunk.transform.localPosition = new Vector3(
								indexPos.x * chunkSize.x,
								indexPos.y * chunkSize.y,
								indexPos.z * chunkSize.z);
						}
						newChunks[x, y, z] = oldChunk;
					}

				}
			}
		}

		// Apply new array.
		chunks = newChunks;
		gridSize = newSize;

	}

	void UpdateChunkTerrain() {

		foreach(GameObject chunk in chunks) {

			// Set values for 'field'.
			DensityField field = chunk.GetComponent<DensityField>();
			field.size = chunkSize;
			field.CreateBuffers();

			// Center is 0, 0, 0: so local center is the local position.
			Vector3 localCenter = -field.transform.localPosition;

			// Set parameters for the compute shader.
			sphereCompute.SetBuffer(0, "points", field.points);
			sphereCompute.SetInts("size", chunkSize.x, chunkSize.y, chunkSize.z);
			sphereCompute.SetFloats("center", localCenter.x, localCenter.y, localCenter.z);
			sphereCompute.SetFloat("radius", radius);

			// Get thread sizes.
			sphereCompute.GetKernelThreadGroupSizes(0, out uint threadX, out uint threadY, out uint threadZ);
			// Run the compute shader.
			sphereCompute.Dispatch(0, chunkSize.x / (int)threadX, chunkSize.y / (int)threadY, chunkSize.z / (int)threadZ);

			// Notify 'field' that it has been updated.
			field.RequestUpdate();

		}

	}

}
