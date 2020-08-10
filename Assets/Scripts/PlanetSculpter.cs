using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetSculpter : UpdatableScriptableObject {

	public ComputeShader shader;
	public int seed;


	private void OnValidate() {
		if(shader) RequestUpdateNow();
	}

	protected override void OnUpdateRequest() { }

	public bool ModifyUnitSphere(int resolution, ref ComputeBuffer vertices, ref ComputeBuffer normals) {

		if(shader == null) return false;

		// Pass arguments for the compute shader.
		int kernel = shader.FindKernel("Generate");
		shader.SetBuffer(kernel, "vertices", vertices);
		shader.SetBuffer(kernel, "normals", normals);
		shader.SetInts("resolution", resolution, resolution);
		shader.SetFloat("s", 0.01f);
		ShaderNoise.SetupNoise(shader, seed);
		SetSpecificParameters();

		// Get the compute shader's thread group sizes.
		shader.GetKernelThreadGroupSizes(kernel, out uint threadX, out uint threadY, out _);
		// Run the compute shader.
		shader.Dispatch(kernel, resolution / (int)threadX, resolution / (int)threadY, 1);

		return true;

	}

	protected abstract void SetSpecificParameters();

}
