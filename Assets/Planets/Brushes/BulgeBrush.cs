using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Planets/Brushes/Bulge")]
public class BulgeBrush : PlanetBrush {

	[Range(0, 50)]
	public float radius = 10;
	[Range(0, 1)]
	public float strength = 1;
	public AnimationCurve curve = new AnimationCurve();

	protected override Color Paint(Mode mode, Color value, CubemapFace face, object data) {

		float strength = this.strength * (float)data;
		if(mode == Mode.Primary) value.r += strength;
		else value.r -= strength;
		return value;

	}

	protected override ICollection<(Vector2Int coord, object data)> GetCoordsToPaint(Vector2 center, float delta, Mode mode) {
		
		int diameter = Mathf.RoundToInt(radius / delta * 2);
		List<(Vector2Int coord, object data)> coords = new List<(Vector2Int coord, object data)>(diameter * diameter);

		for(int x = 0; x < diameter; x++) {
			for(int y = 0; y < diameter; y++) {

				int X = x - diameter / 2;
				int Y = y - diameter / 2;

				Vector2 coord = center + new Vector2(X, Y);
				float distance = Vector2.Distance(coord, center) * delta;
				if(distance <= radius) {
					coords.Add((new Vector2Int(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y)), curve.Evaluate(1 - distance / radius)));
				}

			}
		}

		return coords;

	}

	public override void DrawHandles(Planet planet, Vector3 local, Mode mode) {
		Vector3 world = local + planet.transform.position;
		Handles.color = mode == Mode.Primary ? Color.green : Color.red;
		Handles.DrawWireDisc(world, local.normalized, radius);
	}

}
