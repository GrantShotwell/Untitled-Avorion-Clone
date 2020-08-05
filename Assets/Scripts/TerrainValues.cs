using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainValues : MonoBehaviour {

    [HideInInspector]
    public float[,,] values;

    [Range(0.00f, 1.00f)]
    public float cutoff = 0.10f;

    public Vector3Int size => new Vector3Int(values.GetLength(0), values.GetLength(1), values.GetLength(2));

    void Start() {
        values = new float[4, 4, 4];
        GenerateValues(seed: 1);
    }

    void GenerateValues(int? seed = null) {

        if(seed.HasValue) NoiseS3D.seed = seed.Value;

        int x = values.GetLength(0), y = values.GetLength(1), z = values.GetLength(2);
        for(int ix = 0; ix < x; ix++) {
            for(int iy = 0; iy < y; iy++) {
                for(int iz = 0; iz < z; iz++) {

                    float value = (float)NoiseS3D.Noise(ix, iy, iz);
                    values[ix, iy, iz] = (value + 1) / 2;

                }
            }
        }

    }

    void OnDrawGizmos() {

        if(values == null) return;

        int x = values.GetLength(0), y = values.GetLength(1), z = values.GetLength(2);
        for(int ix = 0; ix < x; ix++) {
            for(int iy = 0; iy < y; iy++) {
                for(int iz = 0; iz < z; iz++) {

                    Vector3 position = new Vector3(ix, iy, iz);
                    float value = values[ix, iy, iz];
                    Color color = new Color { r = value, g = value, b = value, a = 1.0f };

                    if(value <= cutoff) continue;

                    Gizmos.color = color;
                    Gizmos.DrawSphere(position, 0.1f);

                }
            }
        }

    }

}


