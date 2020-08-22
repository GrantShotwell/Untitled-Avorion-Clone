using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Planets/Sculpter Assets/Default")]
public class DefaultPlanetSculpter : PlanetSculpter {

	#region Editor
#if UNITY_EDITOR

	public override Type GetEditorType() => typeof(Editor);

	[CustomEditor(typeof(Editor))]
	public class Editor : AbstractEditor {

		public DefaultPlanetSculpter asset;
		private readonly GUIContent label_radius = new GUIContent { text = "Radius", tooltip = "The base radius for the planet." };
		private readonly GUIContent label_magnitude = new GUIContent { text = "Magnitude", tooltip = "The variance in height from the radius." };

		private void OnEnable() {
			abstractAsset = asset = (DefaultPlanetSculpter)target;
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

			if(updateMesh) UpdateMesh();

		}

	}

#endif
	#endregion

	//public float radius;
	public float magnitude;

	protected override void SetSpecificParameters() {

		shader.SetFloat("radius", radius);
		shader.SetFloat("magnitude", magnitude);

	}

}
