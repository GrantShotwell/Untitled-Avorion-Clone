using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainValues : MonoBehaviour {

    public ComputeShader shader;
    public ComputeBuffer pointsBuffer;
    public Vector3Int size;

    public Vector3 center;
    [Range(1, 100)]
    public float radius;

    public delegate void SettingsUpdate();
    public event SettingsUpdate settingsUpdate;

    [HideInInspector]
    public bool needsUpdate = true;

    void Update() {
        if(needsUpdate) {
            GenerateValues();
            settingsUpdate.Invoke();
            needsUpdate = false;
        }
    }

    void OnValidate() {
        int min = 4;
        if(size.x < min) size.x = min;
        if(size.y < min) size.y = min;
        if(size.z < min) size.z = min;
        needsUpdate = true;
    }

    void OnDestroy() {
        DisposeBuffers();
    }

    struct Point {
#pragma warning disable 649
        public Vector4 point;
        public Vector3Int index;
#pragma warning restore 649
    }

    void GenerateValues() {

        CreateBuffers();

        // Set parameters for the compute shader.
        shader.SetFloats("center", center.x, center.y, center.z);
        shader.SetFloat("radius", radius);
        shader.SetInts("size", size.x, size.y, size.z);
        shader.SetBuffer(0, "points", pointsBuffer);

        // Run the compute shader.
        shader.Dispatch(0, size.x, size.y, size.z);

    }

    void CreateBuffers() {

        DisposeBuffers();

        int numPoints = size.x * size.y * size.z;

        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);

    }

    void DisposeBuffers() {

        pointsBuffer?.Dispose();

    }

}
