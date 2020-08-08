using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlanetSettings : UpdatableScriptableObject {

	[Tooltip("Material to apply to terrain mesh.")]
	public Material material;
	[Range(2, 256)]
	public int resolution = 10;
	[Range(1.0f, 1000.0f)]
	public float radius;


	private void OnValidate() {
		RequestUpdate();
		TryUpdateRequest();
	}

}
