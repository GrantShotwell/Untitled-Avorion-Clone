using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TerrainValues))]
[RequireComponent(typeof(MeshFilter))]
public class MarchingCubes : MonoBehaviour {

    public ComputeShader shader;

    Mesh mesh;
    Vector3[] verticies;
    int[] trianges;
    
    TerrainValues terrain;


    void Start() {
        terrain = GetComponent<TerrainValues>();
        mesh = GetComponent<MeshFilter>().mesh;
        GenerateMesh();
        ApplyMesh();
    }

    void GenerateMesh() {

        CreateBuffers(out ComputeBuffer values, out ComputeBuffer triangles);

        shader.SetBuffer(0, "values", values);
        shader.SetBuffer(0, "triangles", triangles);
        shader.SetInts("size", terrain.size.x, terrain.size.y, terrain.size.z);
        shader.SetFloat("cutoff", terrain.cutoff);

        shader.Dispatch(0, terrain.size.x, terrain.size.y, terrain.size.z);



    }

    void CreateBuffers(out ComputeBuffer values, out ComputeBuffer triangles) {

        var size = terrain.size;
        
        values = new ComputeBuffer(size.x * size.y * size.z, sizeof(float));
        triangles = new ComputeBuffer(0, sizeof(float) * 3 * 3);
        values.SetData(terrain.values);

    }

    void ApplyMesh() {

        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = trianges;
        mesh.RecalculateNormals();

    }

}
