using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Planet : MonoBehaviour {

	#region Editor
#if UNITY_EDITOR

	[CustomEditor(typeof(Planet))]
	public class Editor : UnityEditor.Editor {

		public Planet script { get; private set; }
		private static readonly GUIContent label_useSphereCollider = new GUIContent { text = "Use sphere collider in edit mode?" };
		private bool foldout_settings = true;
		private PlanetSettings.Editor editor_settings;
		private bool foldout_sculpter = true;
		private PlanetSculpter.AbstractEditor editor_sculpter;

		private void OnEnable() {
			script = (Planet)target;
			UpdateMesh();
			UpdateMaterial();
		}

		private void OnDisable() {
			DetachEvents(editor_settings);
			DetachEvents(editor_sculpter);
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			// "Update Mesh"
			if(GUILayout.Button("Update Mesh")) {
				script.GenerateMeshes(true);
			}

			// "Use sphere collider in edit mode?"
			var useSphereCollider = EditorGUILayout.ToggleLeft(label_useSphereCollider, script.useSphereColliderInSceneView);
			if(script.useSphereColliderInSceneView != useSphereCollider) {
				script.useSphereColliderInSceneView = useSphereCollider;
				script.SelectMesh();
			}

			// "Detail"
			float detail = EditorGUILayout.Slider("Detail", script.detail, 0.00f, 1.00f);
			if(script.detail != detail) {
				script.detail = detail;
				script.SelectMesh();
			}

			// "Settings"
			var settings = (PlanetSettings)EditorGUILayout.ObjectField("Settings", script.settings, typeof(PlanetSettings), false);
			if(settings != script.settings) {
				script.settings = settings;
				script.GenerateMeshes();
			}
			if(
				script.settings
				&& (foldout_settings = EditorGUILayout.InspectorTitlebar(foldout_settings, script.settings))
			) {
				if(!editor_settings) {
					AttachEvents(editor_settings = CreateEditor(script.settings, typeof(PlanetSettings.Editor)) as PlanetSettings.Editor);
				} else if(editor_settings.target != script.settings) {
					DetachEvents(editor_settings);
					AttachEvents(editor_settings = CreateEditor(script.settings, typeof(PlanetSettings.Editor)) as PlanetSettings.Editor);
				}
				editor_settings.OnInspectorGUI();
			}

			// "Sculpter"
			if(
				script.settings && script.settings.sculpter
				&& (foldout_sculpter = EditorGUILayout.InspectorTitlebar(foldout_sculpter, script.settings.sculpter))
			) {
				if(!editor_sculpter) {
					AttachEvents(editor_sculpter = CreateEditor(script.settings.sculpter, script.settings.sculpter.GetEditorType()) as PlanetSculpter.AbstractEditor);
				} else if(editor_sculpter.target != script.settings.sculpter) {
					DetachEvents(editor_sculpter);
					AttachEvents(editor_sculpter = CreateEditor(script.settings.sculpter, script.settings.sculpter.GetEditorType()) as PlanetSculpter.AbstractEditor);
				}
				editor_sculpter.OnInspectorGUI();
			}

			serializedObject.ApplyModifiedProperties();
		}

		void AttachEvents(PlanetSettings.Editor editor) {
			if(!editor) return;
			editor.onMeshChange += UpdateMesh;
			editor.onMaterialChange += UpdateMaterial;
		}

		void DetachEvents(PlanetSettings.Editor editor) {
			if(!editor) return;
			editor.onMeshChange -= UpdateMesh;
			editor.onMaterialChange -= UpdateMaterial;
		}

		void AttachEvents(PlanetSculpter.AbstractEditor editor) {
			if(!editor) return;
			editor.onMeshChange += UpdateMesh;
		}

		void DetachEvents(PlanetSculpter.AbstractEditor editor) {
			if(!editor) return;
			editor.onMeshChange -= UpdateMesh;
		}

		void UpdateMesh() {
			script.GenerateMeshes();
			script.SelectMesh();
		}

		void UpdateMaterial() {
			script.UpdateMaterial();
		}

	}

#endif
	#endregion

	[HideInInspector]
	public PlanetSettings settings;

	public MeshFilter filter;
	public new MeshRenderer renderer;
	public new MeshCollider collider;
	private SphereCollider sphereCollider;

	[HideInInspector]
	public bool useSphereColliderInSceneView = true;
	[HideInInspector]
	public float detail = 0;

	private void Start() {
		ValidateFields();
		GenerateMeshes();
		SelectMesh();
	}

	public void UpdateMaterial() {
		if(!renderer) renderer = GetComponent<MeshRenderer>();
		renderer.sharedMaterial = settings.material;
	}

	public void ValidateFields() {
		if(!filter) filter = GetComponent<MeshFilter>();
		if(!collider) collider = GetComponent<MeshCollider>();
	}

	public void SelectMesh() {
		int lod = settings.GetDetailIndex(detail);

		filter.sharedMesh = settings.models[lod].mesh;


		if(useSphereColliderInSceneView && !Application.isPlaying) {
			collider.enabled = false;
			if(!sphereCollider && !TryGetComponent(out sphereCollider)) sphereCollider = gameObject.AddComponent<SphereCollider>();
			sphereCollider.enabled = true;
		} else {
			if(sphereCollider) sphereCollider.enabled = false;
			collider.enabled = true;
			collider.sharedMesh = settings.models[0].mesh;
		}

	}

	public Collider GetEditCollider() {
		if(collider.enabled) return collider;
		else {
			if(!sphereCollider && !TryGetComponent(out sphereCollider)) sphereCollider = gameObject.AddComponent<SphereCollider>();
			return sphereCollider;
		}
	}

	public void GenerateMeshes(bool forced = false) {

		ValidateFields();

		if(!settings) {
			if(forced) Debug.LogWarning($"Cannot generate mesh because {nameof(settings)} was not assigned.");
			return;
		}

		if(!settings.sculpter) {
			if(forced) Debug.LogWarning($"Cannot generate mesh because {nameof(settings.sculpter)} was not assigned.");
			return;
		}

		settings.GenerateModels();

	}

}
