using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class meshgenTest : MonoBehaviour
{
    Vector3[] vertices = new Vector3[4];
    Vector2[] uvs = new Vector2[4];
    int[] triangles = new int[6];
    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        vertices[0] = new Vector3(0f, 0f, 0f);
        vertices[1] = new Vector3(1.0f, 0f, 0f);
        vertices[2] = new Vector3(1.0f, 1.0f, 0f);
        vertices[3] = new Vector3(0f, 1.0f, 0f);
        uvs[0] = new Vector2(0f,1f);
        uvs[1] = new Vector2(1.0f,1f);
        uvs[2] = new Vector2(1.0f,0f);
        uvs[3] = new Vector2(0f,0f);
        triangles[0]=0;
        triangles[1]=3;
        triangles[2]=2;
        triangles[3]=0;
        triangles[4]=2;
        triangles[5]=1;
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
