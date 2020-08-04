using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TerrainValues))]
public class MarchingCubes : MonoBehaviour {

    public ComputeShader shader;
    Mesh mesh;

    Vector3[] verticies;
    int[] trianges;
    
    TerrainValues terrain;


    void Start() {
        mesh = GetComponent<MeshFilter>().mesh;
        GenerateMesh();
        ApplyMesh();
    }

    void GenerateMesh() {

        ComputeBuffer points, triangles;
        shader.SetBuffer(0, "points", points);
        shader.SetBuffer(0, "triangles", triangles);
        shader.SetInts("size", terrain.size.x, terrain.size.y, terrain.size.z);


    }

    void ApplyMesh() {

        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = trianges;
        mesh.RecalculateNormals();

    }

}
