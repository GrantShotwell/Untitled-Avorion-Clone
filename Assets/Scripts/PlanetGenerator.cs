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

		// Remove old array.
		foreach(GameObject chunk in chunks) {
			Destroy(chunk);
		}

		// Create new array.
		gridSize = new Vector3Int(
			Mathf.CeilToInt((radius + buffer) / chunkSize.x * 2),
			Mathf.CeilToInt((radius + buffer) / chunkSize.y * 2),
			Mathf.CeilToInt((radius + buffer) / chunkSize.z * 2));
		chunks = new GameObject[gridSize.x, gridSize.y, gridSize.z];

		// Fill new array.
		for(int x = 0; x < gridSize.x; x++) {
			for(int y = 0; y < gridSize.x; y++) {
				for(int z = 0; z < gridSize.x; z++) {

					GameObject chunk = Instantiate(prefab, transform);
					chunk.name = prefab.name;
					chunk.transform.localPosition = new Vector3Int(x, y, z) * chunkSize;
					chunk.transform.localPosition -= gridSize * chunkSize / 2;
					chunks[x, y, z] = chunk;

				}
			}
		}

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
