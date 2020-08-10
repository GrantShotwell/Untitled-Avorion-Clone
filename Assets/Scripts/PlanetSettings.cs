using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Planets/Settings Asset")]
public class PlanetSettings : UpdatableScriptableObject {

	public ComputeShader sphere;
	private PlanetSculpter _sculpter = null;
	public PlanetSculpter sculpter;

	[Tooltip("Material to apply to terrain mesh.")]
	public Material material;
	[Range(2, 100)]
	public int resolution;

	private void OnValidate() {
		RequestUpdateNow();
	}

	private void OnDestroy() {
		if(_sculpter) _sculpter.update -= RequestUpdateNow;
		if(sculpter) sculpter.update -= RequestUpdateNow;
		_sculpter = sculpter;
	}

	protected override void OnUpdateRequest() {
		if(_sculpter) _sculpter.update -= RequestUpdateNow;
		if(sculpter) sculpter.update += RequestUpdateNow;
		_sculpter = sculpter;
	}

}
