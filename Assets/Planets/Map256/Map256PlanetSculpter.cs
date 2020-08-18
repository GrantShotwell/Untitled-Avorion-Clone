using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Planets/Sculpter Assets/Mapped 256")]
public class Map256PlanetSculpter : PlanetSculpter {

	[Range(1, 1000)]
	public float radius;
	[Range(0.00f, 2.00f)]
	public float magnitude;

	public RenderTexture[] maps = new RenderTexture[6];

	protected override void SetSpecificParameters() {

		int kernel = shader.FindKernel("Generate");

		shader.SetFloat("radius", radius);
		shader.SetFloat("magnitude", magnitude);
		for(int i = 0; i < 6; i++) {
			shader.SetTexture(kernel, $"map{i}", maps[i]);
		}

	}

}
