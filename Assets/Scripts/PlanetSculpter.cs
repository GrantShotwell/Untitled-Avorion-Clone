using System;
using UnityEditor;
using UnityEngine;

public abstract class PlanetSculpter : ScriptableObject {

	#region Editor
#if UNITY_EDITOR

	public abstract Type GetEditorType();

	public abstract class AbstractEditor : Editor {

		public PlanetSculpter abstractAsset { get; protected set; }
		protected readonly GUIContent label_shader = new GUIContent { text = "Compute Shader", tooltip = "Compute shader used to sculpt a unit sphere." };
		protected readonly GUIContent label_seed = new GUIContent { text = "Noise Seed", tooltip = "Seed used for coherent noise in the shader." };

		public event Action onMeshChange;

		protected void BaseInspectorGUI(out bool updateMesh) {

			updateMesh = false;

			var shader = (ComputeShader)EditorGUILayout.ObjectField(label_shader, abstractAsset.shader, typeof(ComputeShader), false);
			if(shader != abstractAsset.shader) {
				updateMesh = true;
				abstractAsset.shader = shader;
			}

			var seed = EditorGUILayout.IntField(label_seed, abstractAsset.seed);
			if(seed != abstractAsset.seed) {
				updateMesh = true;
				abstractAsset.seed = seed;
			}

		}

		protected void UpdateMesh() {
			onMeshChange?.Invoke();
		}

	}

#endif
	#endregion

	[HideInInspector]
	public ComputeShader shader;
	[HideInInspector]
	public int seed;
	[HideInInspector]
	public float radius;

	public bool ModifyUnitSphere(int resolution, ComputeBuffer vectors, ComputeBuffer vertices, ComputeBuffer normals, ComputeBuffer heights) {

		if(shader == null) return false;

		// Pass arguments for the compute shader.
		int kernel = shader.FindKernel("Generate");
		shader.SetBuffer(kernel, "vertices", vertices);
		shader.SetBuffer(kernel, "vectors", vectors);
		shader.SetBuffer(kernel, "normals", normals);
		shader.SetBuffer(kernel, "heights", heights);
		shader.SetInts("resolution", resolution, resolution);
		shader.SetFloat("s", 0.01f);
		ShaderNoise.SetupNoise(shader, seed);
		SetSpecificParameters();

		// Get the compute shader's thread group sizes.
		shader.GetKernelThreadGroupSizes(kernel, out uint threadX, out uint threadY, out uint threadZ);
		// Run the compute shader.
		shader.Dispatch(kernel, resolution / (int)threadX, resolution / (int)threadY, 6 / (int)threadZ);

		return true;

	}

	protected abstract void SetSpecificParameters();

}
