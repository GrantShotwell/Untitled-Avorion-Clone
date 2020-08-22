using Boo.Lang;
using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

[CreateAssetMenu(menuName = "Planets/Settings Asset")]
public class PlanetSettings : ScriptableObject {

	#region Editor
#if UNITY_EDITOR

	[CustomEditor(typeof(PlanetSettings))]
	public class Editor : UnityEditor.Editor {

		public PlanetSettings asset { get; private set; }

		public event Action onMeshChange;
		public event Action onMaterialChange;

		private void OnEnable() {
			asset = (PlanetSettings)target;
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			bool updateMesh = false;
			bool updateMaterial = false;

			// "Unit Sphere Generator"
			var sphere = (ComputeShader)EditorGUILayout.ObjectField("Unit Sphere Generator", asset.sphereCompute, typeof(ComputeShader), false);
			if(sphere != asset.sphereCompute) {
				updateMesh = true;
				asset.sphereCompute = sphere;
			}

			// "Resolution"
			var resolution = EditorGUILayout.IntSlider("Resolution", asset.resolution, minRes, maxRes);
			if(resolution != asset.resolution) {
				updateMesh = true;
				asset.resolution = resolution;
			}

			// "Approximate Resolutions"
			float[] approximations = new float[asset.approximations.Length];
			for(int i = 0; i < approximations.Length; i++) {
				GUIContent label_approximation = new GUIContent { text = $"Approximation {i + 1}", tooltip = "The percentage of detail to have on the mesh. 'Approximation 1' is the closest." };
				var approximation = EditorGUILayout.Slider(label_approximation, asset.approximations[i], 0.00f, 1.00f);
				if(approximation != asset.approximations[i]) {
					updateMesh = true;
					asset.approximations[i] = approximation;
				}
			}

			// "Sculpter"
			var sculpter = (PlanetSculpter)EditorGUILayout.ObjectField("Sculpter", asset.sculpter, typeof(PlanetSculpter), false);
			if(sculpter != asset.sculpter) {
				updateMesh = true;
				asset.sculpter = sculpter;
			}

			// "Material"
			var material = (Material)EditorGUILayout.ObjectField("Material", asset.material, typeof(Material), false);
			if(material != asset.material) {
				updateMaterial = true;
				asset.material = material;
			}

			// Trigger events.
			if(updateMesh) UpdateMesh();
			if(updateMaterial) UpdateMaterial();

			serializedObject.ApplyModifiedProperties();
		}

		void UpdateMesh() {
			asset.GenerateModels();
			onMeshChange?.Invoke();
		}

		void UpdateMaterial() {
			onMaterialChange?.Invoke();
		}

	}

#endif
	#endregion

	public const int minRes = 2, maxRes = 256;
	
	[HideInInspector]
	public ComputeShader sphereCompute;
	[HideInInspector]
	public PlanetSculpter sculpter;
	[HideInInspector]
	public Material material;
	[HideInInspector]
	public int resolution;
	[HideInInspector]
	public float[] approximations = new float[5];

	[NonSerialized]
	public readonly PlanetModel[] models = new PlanetModel[5];


	private void OnDisable() {
		if(models is null) return;
		foreach(PlanetModel model in models) {
			if(model != null) ((IDisposable)model).Dispose();
		}
	}


	public int GetDetailIndex(float detail) {
		int count = models.Length;
		int lod = Mathf.CeilToInt((1 - detail) * count - 1);
		if(lod >= count) lod = count - 1;
		else if(lod <= 0) lod = 0;
		return lod;
	}

	public int GetApproximateResolution(float detail) {
		float res = this.resolution * detail;
		int resolution = Mathf.CeilToInt(res);
		if(resolution > this.resolution) resolution = this.resolution;
		else if(resolution < 2) resolution = 2;
		return resolution;
	}

	public void GenerateModels() {
		CreateModels();
		SculptModels();
	}

	void CreateModels() {

		Profiler.BeginSample($"{nameof(PlanetSettings)}.{nameof(CreateModels)}", this);

		for(int i = 0; i < models.Length; i++) {

			if(models[i] is null) {
				UnitSphere sphere = new UnitSphere(sphereCompute, GetApproximateResolution(approximations[i]));
				models[i] = new PlanetModel(sphere);
			} else {
				models[i].sphere.resolution = GetApproximateResolution(approximations[i]);
			}

		}

		Profiler.EndSample();

	}

	void SculptModels() {

		Profiler.BeginSample($"{nameof(PlanetSettings)}.{nameof(SculptModels)}", this);
		List<Thread> threads = new List<Thread>(6);

		for(int i = 0; i < models.Length; i++) {

			Profiler.BeginSample($"Unit Sphere {i + 1}", this);

			// Sculpt unit sphere.
			var model = models[i];
			bool sculpted = sculpter.ModifyUnitSphere(model.sphere.resolution, model.sphere.vectors, model.vertices, model.normals, model.heights);
			if(sculpted) model.meshNeedsUpdate = true;
			else Debug.LogWarning($"Was unable to sculpt model {i + 1}.");

			Profiler.EndSample();

		}

		Profiler.EndSample();

	}

}
