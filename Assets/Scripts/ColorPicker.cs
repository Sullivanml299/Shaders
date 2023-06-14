using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//IMPORTANT: Object must have a mesh collider. Box collider will not work.

public class ColorPicker : MonoBehaviour
{
    Vector3[] vertices;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        printVertices();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            printVertices();
        }

        if (!Input.GetMouseButton(0))
            return;

        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        print("Index: " + hit.triangleIndex + " Barycentric: " + hit.barycentricCoordinate + " UV: " + hit.textureCoord);

    }

    void printVertices()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            print(vertices[i]);
        }
    }
}
