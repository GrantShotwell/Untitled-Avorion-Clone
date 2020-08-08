using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


#region Editor
#if UNITY_EDITOR

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor {

	PlanetGenerator script;
	bool foldout_settings = true;

	private void OnEnable() {
		script = (PlanetGenerator)target;
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		Object target;

		target = script.settings;
		if(foldout_settings = EditorGUILayout.InspectorTitlebar(foldout_settings, target)) {
			Editor editor = CreateEditor(target);
			editor.OnInspectorGUI();
		}

	}

}

#endif
#endregion


[ExecuteInEditMode]
[SelectionBase]
public class PlanetGenerator : UpdatableMonoBehaviour {

	private PlanetSettings _settings = null;
	public PlanetSettings settings;
	[HideInInspector]
	public (PlanetFace face, MeshFilter filter)[] faces;


	public void Start() {
		RequestUpdate();
	}

	private void Update() {
		TryUpdateRequest();
	}

	private void OnValidate() {
		if(_settings) _settings.update -= RequestUpdate;
		if(settings) settings.update += RequestUpdate;
		RequestUpdate();
	}

	private void OnDestroy() {
		ClearFaces();
	}


	protected override void OnUpdateRequest() {
		CreateFaces();
		GenerateMesh();
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
			renderer.material = settings.material;

			// Create MeshCollider.
			MeshCollider collider = go.AddComponent<MeshCollider>();
			collider.sharedMesh = filter.sharedMesh;
			collider.convex = false;

			// Save this terrain face into the array.
			faces[i] = (new PlanetFace(filter.sharedMesh, directions[i]), filter);

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
			if(faces[i].filter) DestroyFace(faces[i]);
			// Assign new face.
			faces[i] = (null, filter);
		}
	}

	/// <summary>Destroys any terrain face objects that might already exist.</summary>
	void ClearFaces() {
		FindFaces();
		foreach((PlanetFace, MeshFilter) face in faces) {
			DestroyFace(face);
		}
	}

	void DestroyFace((PlanetFace, MeshFilter) face) {
		if(face.Item1 != null) {
			face.Item1.Dispose();
		}
		if(face.Item2) {
			if(Application.isEditor) DestroyImmediate(face.Item2.gameObject);
			else Destroy(face.Item2.gameObject);
		}
	}

	public void GenerateMesh() {
		if(faces == null) return;
		foreach((PlanetFace face, _) in faces) {
			face.GenerateMesh(settings);
		}
	}

}
