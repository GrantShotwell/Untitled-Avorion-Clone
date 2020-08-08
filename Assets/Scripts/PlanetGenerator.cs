using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[SelectionBase]
public class PlanetGenerator : MonoBehaviour {

	[HideInInspector]
	public (PlanetFace face, MeshFilter filter)[] faces;
	[Tooltip("Material to apply to terrain mesh.")]
	public Material material;
	[Range(2, 256)]
	public int resolution = 10;

	private bool needsUpdate = false;
	public delegate void UpdateRequest();
	public event UpdateRequest update = () => { };


	private void Start() {
		RequestUpdate();
	}

	private void Update() {
		if(needsUpdate) {
			CreateFaces();
			GenerateMesh();
			needsUpdate = false;
			update.Invoke();
		}
	}

	private void OnValidate() {
		RequestUpdate();
	}

	private void OnDestroy() {
		ClearFaces();
	}


	public void RequestUpdate() {
		needsUpdate = true;
	}

	void CreateFaces() {

		Vector3[] directions = new Vector3[6] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
		ClearFaces();

		for(int i = 0; i < 6; i++) {

			// Create GameObject.
			string name = $"Terrain Face {i + 1}";
			GameObject go = new GameObject(name);
			go.transform.parent = transform;

			// Create MeshFilter.
			MeshFilter filter = go.AddComponent<MeshFilter>();
			filter.sharedMesh = new Mesh();

			// Create MeshRenderer.
			MeshRenderer renderer = go.AddComponent<MeshRenderer>();
			renderer.material = material;

			// Create MeshCollider.
			MeshCollider collider = go.AddComponent<MeshCollider>();
			collider.sharedMesh = filter.sharedMesh;
			collider.convex = false;

			// Save this terrain face into the array.
			faces[i] = (new PlanetFace(filter.sharedMesh, resolution, directions[i]), filter);

		}

	}

	/// <summary>Creates <see cref="faces"/> and populates it with any terrain face objects that might already exist.</summary>
	void FindFaces() {
		// Create 'faces' array if necessary.
		if(faces == null) faces = new (PlanetFace face, MeshFilter filter)[6];
		// Use Transform.Fild([child name]) to find terrain faces that already exist.
		for(int i = 0; i < 6; i++) {
			// Find face 'i'.
			Transform child = transform.Find($"Terrain Face {i + 1}");
			MeshFilter filter = child ? child.GetComponent<MeshFilter>() : null;
			// Make sure to not leave any faces behind.
			if(faces[i].filter) DestroyFace(faces[i].filter.gameObject);
			// Assign new face.
			faces[i] = (null, filter);
		}
	}

	/// <summary>Destroys any terrain face objects that might already exist.</summary>
	void ClearFaces() {
		FindFaces();
		foreach((_, MeshFilter filter) in faces) {
			if(filter == null) continue;
			DestroyFace(filter.gameObject);
		}
	}

	void DestroyFace(GameObject face) {
		if(Application.isEditor) DestroyImmediate(face);
		else Destroy(face);
	}

	public void GenerateMesh() {
		if(faces == null) return;
		foreach((PlanetFace face, _) in faces) {
			face.GenerateMesh();
		}
	}

}
