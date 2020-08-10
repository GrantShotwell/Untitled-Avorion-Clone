using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Planets/Sculpter Assets/Default")]
public class DefaultPlanetSculpter : PlanetSculpter {

	[Range(1, 1000)]
	public float radius;
	[Range(0, 100)]
	public float magnitude;

	protected override void SetSpecificParameters() {

		shader.SetFloat("radius", radius);
		shader.SetFloat("magnitude", magnitude);

	}
}
