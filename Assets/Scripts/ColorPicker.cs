using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//IMPORTANT: Object must have a mesh collider. Box collider will not work.

public class ColorPicker : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Color pickedColor = Color.white;

    Vector3[] vertices;
    Material material;

    Vector2 offset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        print("Vertices: " + vertices.Length);

        material = meshRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        // print("Index: " + hit.triangleIndex + " Barycentric: " + hit.barycentricCoordinate + " UV: " + hit.textureCoord);

        if (offset != hit.textureCoord)
        {
            material.SetVector("_MousePos", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
            offset = hit.textureCoord;
            var colorVector = (transform.InverseTransformPoint(hit.point) + Vector3.one * 0.5f);
            pickedColor = new(colorVector.x, colorVector.y, colorVector.z);
            material.SetVector("_MouseColor", pickedColor);
            material.SetInteger("_TriangleIndex", hit.triangleIndex);
        }

    }



    // void OnDrawGizmos()
    // {
    //     if (vertices == null)
    //         return;

    //     for (int i = 0; i < vertices.Length; i++)
    //     {
    //         Gizmos.color = Color.Lerp(Color.white, Color.red, (float)i / vertices.Length);
    //         Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.1f);
    //     }
    // }
}
