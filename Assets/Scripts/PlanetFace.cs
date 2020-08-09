using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFace {

	public Mesh mesh { get; private set; }
	Vector3 up, right, forward;

	public PlanetFace(Mesh mesh, Vector3 up) {
		this.mesh = mesh;
		this.up = up;
		forward = new Vector3(up.y, up.z, up.x);
		right = Vector3.Cross(up, forward);
	}

	public void GenerateMesh(PlanetSettings settings) {

		ComputeShader sphere = settings.sphereGenerator;

		// Create buffers for the compute shaders.
		int maxIndex = settings.resolution - 1;
		int vertCount = settings.resolution * settings.resolution;
		int trigCount = maxIndex * maxIndex * 6;
		ComputeBuffer vertBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
		ComputeBuffer normBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
		ComputeBuffer trigBuffer = new ComputeBuffer(trigCount, sizeof(int));

		// Set parameters for the compute shaders.
		int kernel = sphere.FindKernel("Generate");
		sphere.SetInts("resolution", settings.resolution, settings.resolution);
		sphere.SetBuffer(kernel, "vertices", vertBuffer);
		sphere.SetBuffer(kernel, "triangles", trigBuffer);
		sphere.SetFloats("up", up.x, up.y, up.z);
		sphere.SetFloats("right", right.x, right.y, right.z);
		sphere.SetFloats("forward", forward.x, forward.y, forward.z);

		// Get thread sizes.
		settings.sphereGenerator.GetKernelThreadGroupSizes(kernel, out uint threadX, out uint threadY, out _);
		// Run the compute shaders.
		settings.sphereGenerator.Dispatch(0, settings.resolution / (int)threadX, settings.resolution / (int)threadY, 1);
		settings.ModifyUnitSphere(vertBuffer, normBuffer);

		// Get the results from the compute shaders.
		Vector3[] vertices = new Vector3[vertCount];
		vertBuffer.GetData(vertices, 0, 0, vertCount);
		Vector3[] normals = new Vector3[vertCount];
		normBuffer.GetData(normals, 0, 0, vertCount);
		int[] triangles = new int[trigCount];
		trigBuffer.GetData(triangles, 0, 0, trigCount);

		// Apply data to the mesh.
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = triangles;

		// Dispose compute buffers.
		vertBuffer.Dispose();
		normBuffer.Dispose();
		trigBuffer.Dispose();

	}

}
