using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Planets/Settings Asset")]
public class PlanetSettings : UpdatableScriptableObject {

	public const int minRes = 2, maxRes = 256;

	public ComputeShader sphere;
	private PlanetSculpter _sculpter = null;
	public PlanetSculpter sculpter;

	[Tooltip("Material to apply to terrain mesh.")]
	public Material material;
	[Range(minRes, maxRes)]
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
