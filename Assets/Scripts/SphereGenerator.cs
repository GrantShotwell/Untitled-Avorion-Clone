using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(DensityField))]
public class SphereGenerator : MonoBehaviour {

	public ComputeShader shader;
	DensityField field;

	public Vector3 center;
	public float radius;

	private bool needsUpdate = false;
	public delegate void UpdateRequest();
	public event UpdateRequest update = () => { };


	void Awake() {
		field = GetComponent<DensityField>();
		RequestUpdate();
	}

	void Update() {
		if(needsUpdate) {
			UpdateDensityField();
			needsUpdate = false;
			update.Invoke();
		}
	}
	
	void OnValidate() {

		if(field != null) {

			// Size cannot be zero.
			if(field.size.x < 1) field.size.x = 1;
			if(field.size.y < 1) field.size.y = 1;
			if(field.size.z < 1) field.size.z = 1;

		}

		if(shader != null && field != null) {

			// Size must be a multiple of thread counts.
			shader.GetKernelThreadGroupSizes(0, out uint threadX, out uint threadY, out uint threadZ);
			int x = (int)threadX, y = (int)threadY, z = (int)threadZ;
			int dx = x - (field.size.x % x), dy = y - (field.size.y % y), dz = z - (field.size.z % z);
			if(dx < x) field.size.x += dx;
			if(dy < y) field.size.y += dy;
			if(dz < z) field.size.z += dz;

		}

		// Values have been updated.
		RequestUpdate();

	}


	public void RequestUpdate() {
		needsUpdate = true;
	}

	void UpdateDensityField() {

		// Create buffers for the compute shader.
		field.CreateBuffers();

		// Set parameters for the compute shader.
		shader.SetBuffer(0, "points", field.points);
		shader.SetInts("size", field.size.x, field.size.y, field.size.y);
		shader.SetFloats("center", center.x, center.y, center.z);
		shader.SetFloat("radius", radius);

		// Get thread sizes.
		shader.GetKernelThreadGroupSizes(0, out uint threadX, out uint threadY, out uint threadZ);
		// Run the compute shader.
		shader.Dispatch(0, field.size.x / (int)threadX, field.size.y / (int)threadY, field.size.z / (int)threadZ);

		// Notify 'field' that it has been updated.
		field.RequestUpdate();

	}

}
