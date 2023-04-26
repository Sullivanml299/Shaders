using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vines : MonoBehaviour
{
    public Material vineMaterial;
    public float radius = 0.1f;
    public int numVertices = 10;
    public LineRenderer lineRenderer;
    public bool debugDraw = true;
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();
    Vector3[] pathPoints;


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        pathPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(pathPoints);
        makeCylinderMesh();
        // makeQuadMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void makeQuadMesh()
    {
        vertices.Clear();
        indices.Clear();

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(0, 1, 0));
        vertices.Add(new Vector3(1, 1, 0));
        vertices.Add(new Vector3(1, 0, 0));

        indices.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
    }

    void makeCylinderMesh()
    {
        vertices.Clear();
        indices.Clear();

        Vector3 start, end;
        //segments 0 to n-1
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            start = pathPoints[i];
            end = pathPoints[i + 1];
            makeRing(start, end, true);
        }

        //segment n
        start = pathPoints[pathPoints.Length - 2];
        end = pathPoints[pathPoints.Length - 1];
        makeRing(start, end, false);

        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            for (int j = 0; j < numVertices - 1; j++)
            {
                indices.AddRange(new int[] { i * numVertices + j, (i + 1) * numVertices + j, i * numVertices + j + 1 });
                indices.AddRange(new int[] { (i + 1) * numVertices + j, (i + 1) * numVertices + j + 1, i * numVertices + j + 1 });
            }
            indices.AddRange(new int[] { (i + 1) * numVertices - 1, (i + 2) * numVertices - 1, i * numVertices });
            indices.AddRange(new int[] { (i + 2) * numVertices - 1, (i + 1) * numVertices, i * numVertices });
        }

        print(indices[indices.Count - 3] + " " + indices[indices.Count - 2] + " " + indices[indices.Count - 1]);

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
    }

    void makeRing(Vector3 start, Vector3 end, bool atStart = true)
    {
        Vector3 offset = end - start;
        Vector3 side = Vector3.Cross(offset, Vector3.up).normalized;
        if (side == Vector3.zero) side = new Vector3(1, 0, 0);
        Vector3 forward = Vector3.Cross(side, offset).normalized;
        for (int j = 0; j < numVertices; j++)
        {
            float angle = 2 * Mathf.PI * j / numVertices;
            Vector3 circlePoint = Mathf.Cos(angle) * side + Mathf.Sin(angle) * forward;
            vertices.Add((atStart ? start : end) + circlePoint * radius);
        }
    }

    void OnRenderObject()
    {
        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, vineMaterial, 0);
    }

    void OnDrawGizmos()
    {
        if (debugDraw)
        {
            // Gizmos.DrawLine(transform.position + getCenter(), transform.position + getCenter() + averageNormal);
            // if (vertices.Count() >= 4)
            // {
            //     for (var i = 0; i < vertices.Count; i++)
            //     {
            //         Gizmos.color = debugColors[i % 4];
            //         Gizmos.DrawSphere(transform.position + vertices[i], lineWidth * 0.1f);
            //     }
            // }

            Gizmos.color = Color.cyan;
            foreach (Vector3 vertex in vertices)
            {
                Gizmos.DrawSphere(transform.position + vertex, radius * 0.1f);
            }

            Gizmos.color = Color.red;

            if (vertices.Count > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position + vertices[0], radius * 0.1f);
                Gizmos.DrawSphere(transform.position + vertices[vertices.Count - 1], radius * 0.1f);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + vertices[1], radius * 0.1f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position + vertices[2], radius * 0.1f);
            }

            // Gizmos.color = Color.white;
            // if (pathPoints != null)
            // {
            //     foreach (Vector3 point in pathPoints)
            //     {
            //         Gizmos.DrawSphere(transform.position + point, radius * 0.1f);
            //     }
            // }

            Gizmos.color = Color.green;
            for (var i = 0; i <= indices.Count - 3; i += 3)
            {
                Gizmos.DrawLine(transform.position + vertices[indices[i]],
                                transform.position + vertices[indices[i + 1]]);
                Gizmos.DrawLine(transform.position + vertices[indices[i + 1]],
                                transform.position + vertices[indices[i + 2]]);
                Gizmos.DrawLine(transform.position + vertices[indices[i + 2]],
                                transform.position + vertices[indices[i]]);
            }
            // if (debugV1 != null && debugV2 != null)
            // {
            //     Debug.DrawRay(transform.position + points.Last(), debugV1 * 50, Color.cyan, 0.1f);
            //     Debug.DrawRay(transform.position + points.Last(), debugV2 * 50, Color.magenta, 0.1f);
            // }
        }
    }
}
