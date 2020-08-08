using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlanetSettings : UpdatableScriptableObject {

	public ComputeShader sphereGenerator;
	public ComputeShader shapeGenerator;

	[Tooltip("Material to apply to terrain mesh.")]
	public Material material;
	[Range(2, 256)]
	public int resolution = 10;
	[Range(1.0f, 1000.0f)]
	public float radius;


	private void OnValidate() {
		RequestUpdate();
		TryUpdateRequest();
	}


	public void ModifyUnitSphere(ComputeBuffer vertices) {

		// Set parameters for the compute shader.
		shapeGenerator.SetBuffer(0, "vertices", vertices);
		shapeGenerator.SetInts("resolution", resolution, resolution);
		shapeGenerator.SetFloat("radius", radius);

		// Set parameters for NoiseS3D.
		NoiseS3D.SetShaderVars(shapeGenerator, new Vector2(0, 0), true, 1.0f, 0);
		using(ComputeBuffer permBuffer = new ComputeBuffer(NoiseS3D.perm.Length, 4)) {
			permBuffer.SetData(NoiseS3D.perm);
			shapeGenerator.SetBuffer(0, "perm", permBuffer);
		}

		// Get the compute shader's thread group sizes.
		shapeGenerator.GetKernelThreadGroupSizes(0, out uint threadX, out uint threadY, out _);
		// Run the compute shader.
		shapeGenerator.Dispatch(0, resolution / (int)threadX, resolution / (int)threadY, 1);

	}

}
