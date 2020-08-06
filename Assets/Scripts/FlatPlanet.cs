using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[SelectionBase]
public class FlatPlanet : MonoBehaviour {

	public Material material;
	[Range(2, 256)]
	public int resolution = 10;

	(FlatPlanetFace face, MeshFilter filter)[] faces;

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
		if(faces == null) {
			faces = new (FlatPlanetFace face, MeshFilter filter)[6];
		} else {
			ClearFaces();
		}

		for(int i = 0; i < 6; i++) {

			GameObject go = new GameObject($"Terrain Face {i + 1}");
			go.transform.parent = transform;

			MeshFilter filter = go.AddComponent<MeshFilter>();
			MeshRenderer renderer = go.AddComponent<MeshRenderer>();
			renderer.material = material;

			filter.sharedMesh = new Mesh();
			faces[i] = (new FlatPlanetFace(filter.sharedMesh, resolution, directions[i]), filter);

		}

	}

	void ClearFaces() {
		if(faces == null) return;
		foreach((_, MeshFilter filter) in faces) {
			if(filter == null) continue;
			if(Application.isEditor) DestroyImmediate(filter.gameObject);
			else Destroy(filter.gameObject);
		}
	}

	public void GenerateMesh() {
		if(faces == null) return;
		foreach((FlatPlanetFace face, MeshFilter filter) in faces) {
			face.GenerateMesh();
		}
	}

}
