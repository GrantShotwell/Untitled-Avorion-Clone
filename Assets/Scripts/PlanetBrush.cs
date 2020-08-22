using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public abstract class PlanetBrush : ScriptableObject {

	public enum Mode { Primary, Alternate }

	public void Apply(Planet planet, Vector3 local, Mode mode) {
		if(!TryGetSculpter(planet, out Map256PlanetSculpter sculpter)) Debug.LogError("Unable to get the planet sculpter.", this);
		else {

			Vector3 vector = local.normalized;
			Cubemap cubemap = sculpter.cubemap;

			float absX = Mathf.Abs(vector.x);
			bool posX = vector.x > 0;
			float absY = Mathf.Abs(vector.y);
			bool posY = vector.y > 0;
			float absZ = Mathf.Abs(vector.z);
			bool posZ = vector.z > 0;

			Vector3 uv;
			CubemapFace face;
			Vector2Int resolution = new Vector2Int(256, 256);
			float max;

			if(absX >= absY && absX >= absZ) {
				max = absX;
				if(posX) {
					face = CubemapFace.PositiveX;
					uv = new Vector2(
						-vector.z,
						+vector.y
					);
				} else {
					face = CubemapFace.NegativeX;
					uv = new Vector2(
						+vector.z,
						+vector.y
					);
				}
			} else if(absY >= absX && absY >= absZ) {
				max = absY;
				if(posY) {
					face = CubemapFace.PositiveY;
					uv = new Vector2(
						+vector.x,
						-vector.z
					);
				} else {
					face = CubemapFace.NegativeY;
					uv = new Vector2(
						+vector.x,
						+vector.z
					);
				}
			} else {
				max = absZ;
				if(posZ) {
					face = CubemapFace.PositiveZ;
					uv = new Vector2(
						+vector.x,
						+vector.y
					);
				} else {
					face = CubemapFace.NegativeZ;
					uv = new Vector2(
						-vector.x,
						+vector.y
					);
				}
			}

			Vector2Int index = new Vector2Int(
				Mathf.RoundToInt(0.5f * (uv.x / max + 1f) * resolution.x),
				resolution.y - Mathf.RoundToInt(0.5f * (uv.y / max + 1f) * resolution.y)
			);


			float delta = 0.5f * Mathf.PI * planet.settings.sculpter.radius / planet.settings.resolution;
			foreach((Vector2Int coord, object data) in GetCoordsToPaint(index, delta, mode)) {
				Color value = cubemap.GetPixel(face, coord.x, coord.y);
				cubemap.SetPixel(face, coord.x, coord.y, Paint(mode, value, face, data));
			}

			cubemap.Apply();
			planet.GenerateMeshes();
			planet.SelectMesh();

		}
	}

	protected abstract Color Paint(Mode mode, Color value, CubemapFace face, object data);
	protected abstract ICollection<(Vector2Int coord, object data)> GetCoordsToPaint(Vector2 center, float delta, Mode mode);

	public virtual void DrawHandles(Planet planet, Vector3 local, Mode mode) {
		Vector3 world = local + planet.transform.position;
		Handles.color = mode == Mode.Primary ? Color.green : Color.red;
		Handles.DrawWireDisc(world, local.normalized, 10f);
	}

	protected bool TryGetSculpter<TSculpter>(Planet planet, out TSculpter sculpter) where TSculpter : PlanetSculpter {
		return (sculpter = (TSculpter)planet.settings.sculpter) != null;
	}

	static Vector2Int GetIndex(Vector3 vector, FaceAxes axes) {

		// value between 0.5 and 1.0
		float dot = Vector3.Dot(vector, axes.up);

		Vector2 percent = Vector2.one * dot;

		Vector2Int index = new Vector2Int(Mathf.RoundToInt(percent.x * 256), Mathf.RoundToInt(percent.x * 256));

		return index;

	}

}
