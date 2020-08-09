using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlanetSettings : UpdatableScriptableObject {

	public ComputeShader sphereGenerator;
	public ComputeShader shapeGenerator;

	[Tooltip("Material to apply to terrain mesh.")]
	public Material material;
	[Range(2, 255)]
	public int resolution = 10;
	[Range(1, 100)]
	public float radius;
	[Range(0, 100)]
	public float magnitude;

	public Vector3 offset;
	[Range(0, 10)]
	public float scale;
	[Range(0, 100)]
	public int seed;
	[Range(0.0001f, 1.0000f)]
	public float s;


	private void OnValidate() {
		RequestUpdate();
		TryUpdateRequest();
	}


	public void ModifyUnitSphere(ComputeBuffer vertices, ComputeBuffer normals) {

		// Set parameters for the compute shader.
		shapeGenerator.SetBuffer(0, "vertices", vertices);
		shapeGenerator.SetBuffer(0, "normals", normals);
		shapeGenerator.SetInts("resolution", resolution, resolution);
		shapeGenerator.SetFloat("radius", radius);
		shapeGenerator.SetFloats("offset", offset.x, offset.y, offset.z);
		shapeGenerator.SetFloat("scale", scale);
		shapeGenerator.SetFloat("magnitude", magnitude);
		shapeGenerator.SetFloat("s", s);
		ShaderNoise.SetupNoise(shapeGenerator, seed);

		// Get the compute shader's thread group sizes.
		shapeGenerator.GetKernelThreadGroupSizes(0, out uint threadX, out uint threadY, out _);
		// Run the compute shader.
		shapeGenerator.Dispatch(0, resolution / (int)threadX, resolution / (int)threadY, 1);

	}

}
