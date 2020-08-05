using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DensityField))]
[RequireComponent(typeof(MeshFilter))]
public class MarchingCubes : MonoBehaviour {

    public ComputeShader shader;
    ComputeBuffer triBuffer, triCount, valBuffer;

    Mesh mesh;
    Vector3[] verticies;
    int[] triangles;

    DensityField field;

    [HideInInspector]
    public bool needsUpdate = false;

    void Start() {
        field = GetComponent<DensityField>();
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        if(TryGetComponent(out MeshCollider collider)) collider.sharedMesh = mesh;
        field.settingsUpdate += new DensityField.SettingsUpdate(() => { needsUpdate = true; });
    }

    void Update() {
        if(needsUpdate) {
            UpdateMesh();
            needsUpdate = false;
        }
    }

    void OnValidate() {
        needsUpdate = true;
    }

    void OnDestroy() {
        DisposeBuffers();
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

        // Set parameters for the compute shader.
        shader.SetBuffer(0, "points", valBuffer);
        shader.SetBuffer(0, "triangles", triBuffer);
        shader.SetInts("size", field.size.x, field.size.y, field.size.z);
        shader.SetFloat("cutoff", 0);

        // Run rhe compute shader.
        shader.Dispatch(0, field.size.x, field.size.y, field.size.z);

        // Get results from the compute shader.
        ComputeBuffer.CopyCount(triBuffer, triCount, 0);
        int[] count = { 0 };
        triCount.GetData(count);
        Triangle[] tris = new Triangle[count[0]];
        triBuffer.GetData(tris, 0, 0, count[0]);

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

        DisposeBuffers();

        Vector3Int voxels = field.size - Vector3Int.one;
        int numVoxels = voxels.x * voxels.y * voxels.z;

        valBuffer = field.pointsBuffer;

        triBuffer = new ComputeBuffer(numVoxels * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triBuffer.SetCounterValue(0);

        triCount = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

    }

    void DisposeBuffers() {

        triBuffer?.Dispose();
        triCount?.Dispose();

    }

    void ApplyMesh() {

        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

    }

}
