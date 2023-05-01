using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vines : MonoBehaviour
{
    public Material vineMaterial;
    public float radius = 0.1f;
    public int numVertices = 10; // number of vertices that make up each ring
    public LineRenderer lineRenderer;
    public bool debugDraw = true;
    public bool drawMesh = true;

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();
    Vector3[] pathPoints;


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        var controlPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(controlPoints);
        pathPoints = Spline.generateCatmullrom(controlPoints, 10).ToArray(); //FIXME: .toArray is inefficient
        makeCylinderMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (drawMesh) Graphics.DrawMesh(mesh, transform.localToWorldMatrix, vineMaterial, LayerMask.NameToLayer("Default"));
    }

    /// <summary>
    /// creates a cylinder mesh along the path defined by pathPoints
    /// </summary>
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

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// creates a ring of vertices around the axis defined by start and end. 
    /// Using Quaternions is important to avoid gimbal lock side effects.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="atStart">if true, generate the points at the start position</param>
    void makeRing(Vector3 start, Vector3 end, bool atStart = true)
    {
        Vector3 offset = end - start;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, offset);
        Vector3 side = rotation * Vector3.right;
        Vector3 forward = rotation * Vector3.forward;
        for (int j = 0; j < numVertices; j++)
        {
            float angle = 2 * Mathf.PI * j / numVertices;
            Vector3 circlePoint = Mathf.Cos(angle) * side + Mathf.Sin(angle) * forward;
            vertices.Add((atStart ? start : end) + circlePoint * radius);
        }
    }

    void OnDrawGizmos()
    {
        if (debugDraw)
        {

            if (pathPoints != null)
            {
                for (var i = 0; i < pathPoints.Length; i++)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(transform.position + pathPoints[i], Vector3.one * radius * 0.1f);
                    for (int j = 0; j < numVertices; j++)
                    {
                        Vector3 vertex = vertices[i * numVertices + j];
                        Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)j / numVertices);
                        Gizmos.DrawSphere(transform.position + vertex, radius * 0.1f);
                    }

                }
            }

        }
    }
}
