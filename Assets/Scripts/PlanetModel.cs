using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetModel : IDisposable {

	public bool meshNeedsUpdate = true;

	public readonly UnitSphere sphere;
	public readonly ComputeBuffer vertices, normals, heights;
	
	public Mesh mesh => meshNeedsUpdate || !_mesh ? ApplyToMesh() : _mesh;
	private Mesh _mesh;

	public PlanetModel(UnitSphere sphere) {
		
		this.sphere = sphere ?? throw new ArgumentNullException(nameof(sphere));

		int count = sphere.vectors.count;
		vertices = new ComputeBuffer(count, sizeof(float) * 3);
		normals = new ComputeBuffer(count, sizeof(float) * 3);
		heights = new ComputeBuffer(count, sizeof(float));

	}

	public Mesh ApplyToMesh() {

		Mesh mesh = _mesh;
		if(!mesh) mesh = new Mesh() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };

		mesh.Clear();
		Vector3[] vertices = new Vector3[this.vertices.count];
		Vector3[] normals = new Vector3[this.normals.count];
		this.vertices.GetData(vertices, 0, 0, vertices.Length);
		this.normals.GetData(normals, 0, 0, normals.Length);
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = sphere.triangles;

		meshNeedsUpdate = false;
		return _mesh = mesh;

	}

	public void DisposeBuffers() {
		vertices?.Dispose();
		normals?.Dispose();
		heights?.Dispose();
	}

	void IDisposable.Dispose() {
		DisposeBuffers();
		sphere.DisposeBuffers();
	}

}
