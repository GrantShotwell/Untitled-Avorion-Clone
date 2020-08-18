﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFace {

	public enum GenerationResult { Success = 0b0011, NotSculpted = 0b1001, Failure = 0b1100 }

	public Mesh mesh { get; private set; }
	Vector3 up, right, forward;

	public PlanetFace(Vector3 up) {
		mesh = new Mesh();
		this.up = up;
		forward = new Vector3(up.y, up.z, up.x);
		right = Vector3.Cross(up, forward);
	}

	/// <summary>
	/// Attempts to modify <see cref="mesh"/> .
	/// </summary>
	/// <param name="settings">The <see cref="PlanetSettings"/> to generate the face from.</param>
	/// <param name="lod">A percentage of detail.</param>
	/// <returns>Returns if the attempt was a success or failure.</returns>
	public GenerationResult GenerateFace(PlanetSettings settings, float lod) {

		ComputeShader sphere = settings.sphere;
		if(!sphere) return GenerationResult.Failure;

		// Create buffers for the compute shaders.
		int resolution = Mathf.Clamp(Mathf.CeilToInt(settings.resolution * lod), PlanetSettings.minRes, PlanetSettings.maxRes);
		int maxIndex = resolution - 1;
		int vertCount = resolution * resolution;
		int trigCount = maxIndex * maxIndex * 6;
		ComputeBuffer vertBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
		ComputeBuffer normBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
		ComputeBuffer trigBuffer = new ComputeBuffer(trigCount, sizeof(int));

		// Set parameters for the compute shaders.
		int kernel = sphere.FindKernel("Generate");
		sphere.SetInts("resolution", resolution, resolution);
		sphere.SetBuffer(kernel, "vertices", vertBuffer);
		sphere.SetBuffer(kernel, "triangles", trigBuffer);
		sphere.SetFloats("up", up.x, up.y, up.z);
		sphere.SetFloats("right", right.x, right.y, right.z);
		sphere.SetFloats("forward", forward.x, forward.y, forward.z);

		// Get thread sizes.
		sphere.GetKernelThreadGroupSizes(kernel, out uint threadX, out uint threadY, out _);
		// Run the compute shaders.
		sphere.Dispatch(0, resolution / (int)threadX, resolution / (int)threadY, 1);
		bool sculpted = settings.sculpter.ModifyUnitSphere(resolution, ref vertBuffer, ref normBuffer);

		// Get the results from the compute shaders.
		Vector3[] vertices = new Vector3[vertCount];
		vertBuffer.GetData(vertices, 0, 0, vertCount);
		Vector3[] normals = new Vector3[vertCount];
		normBuffer.GetData(normals, 0, 0, vertCount);
		int[] triangles = new int[trigCount];
		trigBuffer.GetData(triangles, 0, 0, trigCount);

		// Apply the results to the mesh.
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = triangles;

		// Dispose the compute buffers.
		vertBuffer.Dispose();
		normBuffer.Dispose();
		trigBuffer.Dispose();

		return sculpted ? GenerationResult.Success : GenerationResult.NotSculpted;

	}

}
