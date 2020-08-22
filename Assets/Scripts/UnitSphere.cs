using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class UnitSphere : IDisposable {

	public static Matrix4x4 transform = new Matrix4x4(
		new Vector4(1, 0, 0, 0),
		new Vector4(0, 1, 0, 0),
		new Vector4(0, 0, 1, 0),
		new Vector4(0, 0, 0, 1));

	private int _resolution;
	public static readonly FaceAxes[] axes = new FaceAxes[] {
		new FaceAxes(0), new FaceAxes(1), new FaceAxes(2), new FaceAxes(3), new FaceAxes(4), new FaceAxes(5)
	};

	public ComputeShader shader;

	public int resolution {
		get => _resolution;
		set {
			if(_resolution != value) {
				_resolution = value;
				UpdateMesh();
			}
		}
	}

	public int vertexCount => resolution * resolution;
	public int triangleCount => (resolution - 1) * (resolution - 1) * 6;

	public ComputeBuffer vectors;
	public int[] triangles;


	public UnitSphere(ComputeShader shader, int resolution) {
		this.shader = shader;
		this.resolution = resolution;
	}


	public void UpdateMesh() {

		Profiler.BeginSample("UnitSphere.UpdateMesh()");

		int vectCount = vertexCount;
		int trigCount = triangleCount;
		CombineInstance[] faces = new CombineInstance[6];

		for(int i = 0; i < 6; i++) {

			// Get face axes.
			Vector3 up = axes[i].up;
			Vector3 right = axes[i].right;
			Vector3 forward = axes[i].forward;

			// Create compute buffers.
			ComputeBuffer vectBuffer = new ComputeBuffer(vectCount, sizeof(float) * 3, ComputeBufferType.Structured);
			ComputeBuffer trigBuffer = new ComputeBuffer(trigCount, sizeof(int), ComputeBufferType.Structured);

			// Set compute shader parameters.
			int kernel = shader.FindKernel("Generate");
			shader.SetBuffer(kernel, "vectors", vectBuffer);
			shader.SetBuffer(kernel, "triangles", trigBuffer);
			shader.SetInts("resolution", resolution, resolution);
			shader.SetFloats("up", up.x, up.y, up.z);
			shader.SetFloats("right", right.x, right.y, right.z);
			shader.SetFloats("forward", forward.x, forward.y, forward.z);

			// Run compute shader.
			shader.GetKernelThreadGroupSizes(kernel, out uint threadX, out uint threadY, out uint threadZ);
			shader.Dispatch(kernel, resolution / (int)threadX, resolution / (int)threadY, 6 / (int)threadZ);

			// Create mesh from buffers.
			faces[i].mesh = faces[i].mesh ? faces[i].mesh : new Mesh();
			faces[i].mesh.Clear();
			Vector3[] vertices = new Vector3[vectCount];
			int[] triangles = new int[trigCount];
			vectBuffer.GetData(vertices, 0, 0, vectCount);
			trigBuffer.GetData(triangles, 0, 0, trigCount);
			faces[i].mesh.vertices = vertices;
			faces[i].mesh.triangles = triangles;
			faces[i].transform = transform;

			// Dispose compute buffers.
			vectBuffer.Dispose();
			trigBuffer.Dispose();

		}

		// Create sphere mesh.
		Mesh sphere = new Mesh() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
		sphere.CombineMeshes(faces);

		// Save results.
		DisposeBuffers();
		vectors = new ComputeBuffer(sphere.vertices.Length * 6, sizeof(float) * 3, ComputeBufferType.Structured);
		vectors.SetData(sphere.vertices);
		triangles = sphere.triangles;

		Profiler.EndSample();

	}

	void IDisposable.Dispose() => DisposeBuffers();
	public void DisposeBuffers() {
		vectors?.Dispose();
	}

}
