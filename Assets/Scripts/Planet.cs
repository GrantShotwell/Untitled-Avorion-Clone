using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using static PlanetFace;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class Planet : UpdatableMonoBehaviour {

	#region Editor
	#if UNITY_EDITOR

	[CustomEditor(typeof(Planet))]
	public class PlanetGeneratorEditor : Editor {

		Planet script;
		bool foldout_settings = true;
		Editor editor_settings;
		bool foldout_sculpter = true;
		Editor editor_sculpter;

		private void OnEnable() {
			script = (Planet)target;
			editor_settings = CreateEditor(script.settings);
			editor_sculpter = CreateEditor(script.settings.sculpter);
		}

		public override void OnInspectorGUI() {

			if(GUILayout.Button("Update Mesh")) {
				script.GenerateMeshes(true);
			}

			float detail = EditorGUILayout.Slider("Detail", script.detail, 0.00f, 1.00f);
			if(script.detail != detail) {
				script.detail = detail;
				script.SelectMesh();
			}

			base.OnInspectorGUI();

			// "Settings"
			if(
				script.settings
				&& (foldout_settings = EditorGUILayout.InspectorTitlebar(foldout_settings, script.settings))
			) {
				Editor editor = CreateEditor(script.settings);
				editor.OnInspectorGUI();
			}

			// "Sculpter"
			if(
				script.settings && script.settings.sculpter
				&& (foldout_sculpter = EditorGUILayout.InspectorTitlebar(foldout_sculpter, script.settings.sculpter))
			) {
				Editor editor = CreateEditor(script.settings.sculpter);
				editor.OnInspectorGUI();
			}

		}

	}

	#endif
	#endregion

	private PlanetSettings _settings = null;
	public PlanetSettings settings;

	[HideInInspector]
	public PlanetFace[] faces;

	MeshFilter filter;
	new MeshCollider collider;

	[HideInInspector]
	public float detail = 0;
	readonly Mesh[] meshes = new Mesh[4];

	public void Start() {
		ValidateFields();
		CreateFaces();
		RequestUpdate();
	}

	private void Update() {
		TryUpdateRequest();
		SelectMesh();
	}

	private void OnValidate() {
		if(_settings) _settings.update -= RequestUpdate;
		if(settings) settings.update += RequestUpdate;
		_settings = settings;
		RequestUpdate();
	}

	private void OnDestroy() {
		if(_settings) _settings.update -= RequestUpdate;
		if(settings) settings.update -= RequestUpdate;
		_settings = settings;
	}


	protected override void OnUpdateRequest() {
		ValidateFields();
		GenerateMeshes();
		SelectMesh();
	}

	void ValidateFields() {

		if(!filter) filter = GetComponent<MeshFilter>();

		if(!collider) collider = GetComponent<MeshCollider>();

		for(int i = 0; i < meshes.Length; i++) {
			if(!meshes[i]) meshes[i] = new Mesh() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
		}

	}

	void SelectMesh() {

		int lod = Mathf.CeilToInt((1 - detail) * meshes.Length - 1);
		if(lod >= meshes.Length) lod = meshes.Length - 1;
		else if(lod <= 0) lod = 0;

		filter.sharedMesh = meshes[lod];
		collider.sharedMesh = meshes[0];

	}

	void CreateFaces() {

		Vector3[] directions = new Vector3[6] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

		faces = new PlanetFace[6];
		for(int i = 0; i < 6; i++) {
			faces[i] = new PlanetFace(directions[i]);
		}

	}

	public void GenerateMeshes(bool forced = false) {

		// Get mesh filter.
		if(!filter) filter = GetComponent<MeshFilter>();

		// Get mesh collider.
		if(!collider) collider = GetComponent<MeshCollider>();

		// Populate 'faces' array.
		if(faces == null) CreateFaces();

		// Get planet settings.
		if(!settings) {
			if(forced) Debug.LogWarning($"Cannot generate mesh because {nameof(settings)} was not assigned.");
			return;
		}

		// Get planet sculpter.
		if(!settings.sculpter) {
			if(forced) Debug.LogWarning($"Cannot generate mesh because {nameof(settings.sculpter)} was not assigned.");
			return;
		}

		// Generate meshes.
		GenerateMesh(ref meshes[0], 1.00f, forced);
		GenerateMesh(ref meshes[1], 0.90f, forced);
		GenerateMesh(ref meshes[2], 0.60f, forced);
		GenerateMesh(ref meshes[3], 0.40f, forced);

	}

	void GenerateMesh(ref Mesh mesh, float detail, bool forced) {

		// Generate meshes.
		CombineInstance[] meshInfo = new CombineInstance[6];
		for(int i = 0; i < 6; i++) {

			GenerationResult result = faces[i].GenerateFace(settings, detail);

			if((result & GenerationResult.Success) != 0) {
				meshInfo[i].mesh = faces[i].mesh;
				meshInfo[i].transform = transform.localToWorldMatrix;
			}

			if(!forced) continue;

			if(result == GenerationResult.NotSculpted) {
				Debug.LogWarning($"Mesh face {i + 1} was unable to be sculpted.");
				continue;
			}

			if(result == GenerationResult.Failure) {
				Debug.LogWarning("Mesh was unable to be created.");
				return;
			}

		}

		// Combine meshes.
		mesh.Clear();
		mesh.CombineMeshes(meshInfo);

	}

}
