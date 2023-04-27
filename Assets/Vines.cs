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

    [Range(-1, 1)]
    public float angle = 0f;

    float lastAngle = 0f;

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
        pathPoints = Spline.generateCatmullrom(controlPoints, 10).ToArray(); //FIXME: inefficient
        print("pathPoints " + pathPoints.Length);
        makeCylinderMesh();
        // makeQuadMesh();
    }

    // Update is called once per frame
    void Update()
    {
        // if (angle != lastAngle)
        // {
        //     lastAngle = angle;
        //     pathPoints = new Vector3[lineRenderer.positionCount];
        //     lineRenderer.GetPositions(pathPoints);
        //     makeCylinderMesh();
        // }
        if (drawMesh) Graphics.DrawMesh(mesh, transform.localToWorldMatrix, vineMaterial, LayerMask.NameToLayer("Default"));
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
        float angleAdjustment = 0;
        //TODO: fix angle adjustment. Need to adjust the rotation of the circle points based on the angle between the two segments
        //segments 0 to n-1
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            start = pathPoints[i];
            end = pathPoints[i + 1];
            if (i > 0)
            {
                Vector3 prev = pathPoints[i - 1];
                angleAdjustment = Vector3.Angle(start - prev, end - start);
                angleAdjustment = Mathf.Deg2Rad * angleAdjustment;
                // print("angleAdjustment " + angleAdjustment + " percent " + angleAdjustment / (2 * Mathf.PI));
                // angleAdjustment = Vector3.Dot(offset, averageNormal) > 0 ? angleAdjustment : -angleAdjustment;
                // angleAdjustment = Mathf.Rad2Deg * angleAdjustment;
                makeRing(start, end, 2 * Mathf.PI * angle, true); //TODO: replace with angleAdjustment
            }
            else
            {
                makeRing(start, end, 0, true);
            }
        }

        //segment n
        start = pathPoints[pathPoints.Length - 2];
        end = pathPoints[pathPoints.Length - 1];
        makeRing(start, end, 2 * Mathf.PI * angle, false); //TODO: replace with angleAdjustment

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

    void makeRing(Vector3 start, Vector3 end, float angleAdjustment, bool atStart = true)
    {
        Vector3 offset = end - start;
        Vector3 side = Vector3.Cross(offset, Vector3.up).normalized;
        if (side == Vector3.zero) side = new Vector3(1, 0, 0);
        // else side = new Vector3(Mathf.Abs(side.x), Mathf.Abs(side.y), Mathf.Abs(side.z));
        Vector3 forward = Vector3.Cross(side, offset).normalized;
        // forward = new Vector3(Mathf.Abs(forward.x), Mathf.Abs(forward.y), Mathf.Abs(forward.z));
        print("side " + side + " forward " + forward + " offset " + offset + " dot " + Vector3.Dot(Vector3.up, forward));
        for (int j = 0; j < numVertices; j++)
        {
            float angle = 2 * Mathf.PI * j / numVertices + angleAdjustment;
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

            // Gizmos.color = Color.white;
            // if (pathPoints != null)
            // {
            //     foreach (Vector3 point in pathPoints)
            //     {
            //         Gizmos.DrawSphere(transform.position + point, radius * 0.1f);
            //     }
            // }

            // Gizmos.color = Color.green;
            // for (var i = 0; i <= indices.Count - 3; i += 3)
            // {
            //     Gizmos.DrawLine(transform.position + vertices[indices[i]],
            //                     transform.position + vertices[indices[i + 1]]);
            //     Gizmos.DrawLine(transform.position + vertices[indices[i + 1]],
            //                     transform.position + vertices[indices[i + 2]]);
            //     Gizmos.DrawLine(transform.position + vertices[indices[i + 2]],
            //                     transform.position + vertices[indices[i]]);
            // }

        }
    }
}
