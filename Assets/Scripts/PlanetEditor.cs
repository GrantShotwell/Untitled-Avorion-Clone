using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode]
public class PlanetEditor : EditorWindow {

	public bool editing = false;
	private static readonly GUIContent label_editing = new GUIContent { text = "Enable editing." };
	public bool requireDrag = true;
	private static readonly GUIContent label_requireDrag = new GUIContent { text = "Require mouse dragging to apply brush." };

	public PlanetBrush brush;
	private static readonly GUIContent label_brush = new GUIContent { text = "Selected Brush" };
	private Editor editor_brush;
	private bool foldout_brush;
	public Planet target;
	private static readonly GUIContent label_target = new GUIContent { text = "Target Planet" };

	bool m1Down = false;

	[MenuItem("Window/Planet Editor")]
	private static void Init() {
		PlanetEditor window = (PlanetEditor)GetWindow(typeof(PlanetEditor));
		window.Show();
	}

	private void OnEnable() {
		SceneView.duringSceneGui += OnSceneGUI;
		m1Down = false;
	}

	private void OnDisable() {
		SceneView.duringSceneGui -= OnSceneGUI;
	}

	private void OnGUI() {

		bool canEdit = target && brush;

		if(canEdit) {
			editing = EditorGUILayout.ToggleLeft(label_editing, editing);
		} else {
			GUI.enabled = false;
			EditorGUILayout.ToggleLeft(label_editing, false);
			editing = false;
			GUI.enabled = true;
		}

		requireDrag = EditorGUILayout.ToggleLeft(label_requireDrag, requireDrag);

		target = (Planet)EditorGUILayout.ObjectField(label_target, target, typeof(Planet), true);

		brush = (PlanetBrush)EditorGUILayout.ObjectField(label_brush, brush, typeof(PlanetBrush), false);
		if(brush && (foldout_brush = EditorGUILayout.InspectorTitlebar(foldout_brush, brush))) {
			if(!editor_brush || editor_brush.target != brush) editor_brush = Editor.CreateEditor(brush);
			editor_brush.OnInspectorGUI();
		}

	}

	void OnSceneGUI(SceneView scene) {

		if(editing) {

			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

			Collider collider = target.GetEditCollider();

			Event current = Event.current;
			Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
			if(collider.Raycast(ray, out RaycastHit hit, 9999f)) {

				PlanetBrush.Mode mode = current.control ? PlanetBrush.Mode.Alternate : PlanetBrush.Mode.Primary;

				Vector3 local = hit.point - target.transform.position;
				brush.DrawHandles(target, local, mode);

				bool drag = current.type == EventType.MouseDrag;

				if(current.type == EventType.MouseDown && current.button == 0) {
					m1Down = drag = true;
					current.Use();
				} else if(current.type == EventType.MouseUp && current.button == 0) {
					m1Down = false;
					current.Use();
				}

				if(m1Down && (drag || !requireDrag)) {
					brush.Apply(target, local, mode);
				}

			}

			Selection.activeObject = null;
			SceneView.RepaintAll();

		}

	}

}
#endif
