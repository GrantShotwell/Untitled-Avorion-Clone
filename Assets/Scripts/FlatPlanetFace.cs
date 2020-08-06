using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatPlanetFace {

	public Mesh mesh { get; private set; }
	int resolution;
	Vector3 up, forward, right;

	public FlatPlanetFace(Mesh mesh, int resolution, Vector3 up) {
		this.mesh = mesh;
		this.resolution = resolution;
		this.up = up;
		forward = new Vector3(up.y, up.z, up.x);
		right = Vector3.Cross(up, forward);
	}

	public void GenerateMesh() {

		int maxIndex = resolution - 1;
		Vector3[] vertices = new Vector3[resolution * resolution];
		int[] triangles = new int[maxIndex * maxIndex * 6];

		for(int x = 0, t = 0; x < resolution; x++) {
			for(int y = 0; y < resolution; y++) {

				int i = x + resolution * y;
				Vector2Int index = new Vector2Int(x, y);

				Vector2 percent = (Vector2)index / maxIndex;
				Vector3 point = up + (percent.x - 0.5f) * 2 * right
								   + (percent.y - 0.5f) * 2 * forward;
				vertices[i] = point.normalized;

				if(x != maxIndex && y != maxIndex) {
					// Triangle 1
					triangles[t++] = i + resolution;
					triangles[t++] = i + resolution + 1;
					triangles[t++] = i;
					// Triangle 2
					triangles[t++] = i + resolution + 1;
					triangles[t++] = i + 1;
					triangles[t++] = i;
				}

			}
		}

		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();

	}

}
