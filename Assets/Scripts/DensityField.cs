using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityField : MonoBehaviour {

    public const int min = 2;
    public const int max = 128;

    public ComputeShader shader;
    public ComputeBuffer points;
    public Vector3Int size;

    public Vector3 center;
    public float radius;

    public delegate void NeedsUpdate();
    public event NeedsUpdate update;

    [HideInInspector]
    public bool needsUpdate = true;

    void Update() {
        if(needsUpdate) {
            GenerateValues();
            update.Invoke();
            needsUpdate = false;
        }
    }

    void OnValidate() {

        if(size.x < min) size.x = min;
        if(size.y < min) size.y = min;
        if(size.z < min) size.z = min;

        if(size.x > max) size.x = max;
        if(size.y > max) size.y = max;
        if(size.z > max) size.z = max;

        if(radius < 1) radius = 1;
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
        shader.SetBuffer(0, "points", points);

        // Run the compute shader.
        shader.Dispatch(0, size.x, size.y, size.z);

    }

    void CreateBuffers() {

        DisposeBuffers();

        int numPoints = size.x * size.y * size.z;

        points = new ComputeBuffer(numPoints, sizeof(float) * 4);

    }

    void DisposeBuffers() {

        points?.Dispose();

    }

}
