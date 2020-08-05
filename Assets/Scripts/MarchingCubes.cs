using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(TerrainValues))]
[RequireComponent(typeof(MeshFilter))]
public class MarchingCubes : MonoBehaviour {

    public ComputeShader shader;
    ComputeBuffer triBuffer, triCount, valBuffer;

    Mesh mesh;
    Vector3[] verticies;
    int[] triangles;

    TerrainValues terrain;


    void Start() {
        terrain = GetComponent<TerrainValues>();
        mesh = GetComponent<MeshFilter>().mesh;
        UpdateMesh();
    }

    void UpdateMesh() {
        GenerateMesh();
        ApplyMesh();
    }

    struct Triangle {
#pragma warning disable 649
        public Vector3 a, b, c;
#pragma warning restore 649
    }

    void GenerateMesh() {

        CreateBuffers();

        // Set parameters for compute shader.
        shader.SetBuffer(0, "values", valBuffer);
        shader.SetBuffer(0, "triangles", triBuffer);
        shader.SetInts("size", terrain.size.x, terrain.size.y, terrain.size.z);
        shader.SetFloat("cutoff", terrain.cutoff);

        // Run compute shader.
        shader.Dispatch(0, terrain.size.x, terrain.size.y, terrain.size.z);

        // Get results from compute shader.
        ComputeBuffer.CopyCount(triBuffer, triCount, 0);
        int[] count = { 0 };
        triCount.GetData(count);
        Triangle[] tris = new Triangle[count[0]];
        triBuffer.GetData(tris, 0, 0, count[0]); // TODO:  sometimes gives an error for trying to access too much information.

        // Create a list of verticies and indicies
        List<Vector3> points = new List<Vector3>(tris.Length * 3);
        List<int> indicies = new List<int>(tris.Length * 3);
        foreach(Triangle triangle in tris) {
            indicies.Add(points.Count);
            points.Add(triangle.a);
            indicies.Add(points.Count);
            points.Add(triangle.b);
            indicies.Add(points.Count);
            points.Add(triangle.c);
        }

        // Convert the lists to the verticies and triangles arrays.
        verticies = points.ToArray();
        triangles = indicies.ToArray();

        DisposeBuffers();

    }

    void CreateBuffers() {

        Vector3Int points = terrain.size;
        int numPoints = points.x * points.y * points.z;
        int numVoxels = (points.x - 1) * (points.y - 1) * (points.z - 1) * 5;

        valBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
        valBuffer.SetData(terrain.values);

        triBuffer = new ComputeBuffer(numVoxels, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triBuffer.SetCounterValue(0);

        triCount = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

    }

    void DisposeBuffers() {

        valBuffer.Dispose();
        triBuffer.Dispose();
        triCount.Dispose();

    }

    void ApplyMesh() {

        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

    }

}
