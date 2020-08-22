using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Planets/Sculpter Assets/Mapped 256")]
public class Map256PlanetSculpter : PlanetSculpter {

	#region Editor
#if UNITY_EDITOR

	public override Type GetEditorType() => typeof(Editor);

	[CustomEditor(typeof(Editor))]
	public class Editor : AbstractEditor {

		public Map256PlanetSculpter asset;
		private readonly GUIContent label_radius = new GUIContent { text = "Radius", tooltip = "The base radius for the planet." };
		private readonly GUIContent label_magnitude = new GUIContent { text = "Magnitude", tooltip = "The variance in height from the radius." };
		private readonly GUIContent label_cubemap = new GUIContent { text = "Cubemap", tooltip = "" };

		private void OnEnable() {
			abstractAsset = asset = (Map256PlanetSculpter)target;
		}

		public override void OnInspectorGUI() {

			BaseInspectorGUI(out bool updateMesh);

			var radius = EditorGUILayout.Slider(label_radius, asset.radius, 1, 1000);
			if(radius != asset.radius) {
				updateMesh = true;
				asset.radius = radius;
			}

			var magnitude = EditorGUILayout.Slider(label_magnitude, asset.magnitude, 0, 100);
			if(magnitude != asset.magnitude) {
				updateMesh = true;
				asset.magnitude = magnitude;
			}

			var cubemap = (Cubemap)EditorGUILayout.ObjectField(label_cubemap, asset.cubemap, typeof(Cubemap), false);
			if(cubemap != asset.cubemap) {
				updateMesh = true;
				asset.cubemap = cubemap;
			}

			if(updateMesh) UpdateMesh();

		}

	}

#endif
	#endregion

	//public float radius;
	public float magnitude;

	public Cubemap cubemap;

	protected override void SetSpecificParameters() {

		int kernel = shader.FindKernel("Generate");

		shader.SetFloat("radius", radius);
		shader.SetFloat("magnitude", magnitude);
		shader.SetTexture(kernel, "cubemap", cubemap);

	}

}
