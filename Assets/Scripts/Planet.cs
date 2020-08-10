using System.Collections;
using System.Collections.Generic;
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
		bool foldout_sculpter = true;

		private void OnEnable() {
			script = (Planet)target;
		}

		public override void OnInspectorGUI() {

			if(GUILayout.Button("Update Mesh")) {
				script.GenerateMesh(true);
			}

			base.OnInspectorGUI();

			if(
				script.settings
				&& (foldout_settings = EditorGUILayout.InspectorTitlebar(foldout_settings, script.settings))
			) {
				Editor editor = CreateEditor(script.settings);
				editor.OnInspectorGUI();
			}

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

	public void Start() {
		filter = GetComponent<MeshFilter>();
		collider = GetComponent<MeshCollider>();
		CreateFaces();
		RequestUpdate();
	}

	private void Update() {
		TryUpdateRequest();
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
		GenerateMesh();
	}

	void CreateFaces() {

		Vector3[] directions = new Vector3[6] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

		faces = new PlanetFace[6];
		for(int i = 0; i < 6; i++) {
			faces[i] = new PlanetFace(directions[i]);
		}

	}

	public void GenerateMesh(bool forced = false) {

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

		// Generate then combine meshes.
		Mesh combined = new Mesh();
		CombineInstance[] meshInfo = new CombineInstance[6];
		for(int i = 0; i < 6; i++) {

			GenerationResult result = faces[i].CreateUnitSphereFace(settings);

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
		combined.CombineMeshes(meshInfo, true, true);

		// Apply combined mesh.
		filter.sharedMesh = combined;
		collider.sharedMesh = combined;

	}

}
