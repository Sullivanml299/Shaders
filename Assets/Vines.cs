using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vines : MonoBehaviour
{
    
    public Material vineMaterial;
    public float radius = 0.1f;
    public int splineSegmentResolution = 10; // number of points to generate along the spline for each segment
    public int numVertices = 10; // number of vertices that make up each ring
    public float growthTime = 2f; // time it takes for the vine to grow from start to end
    public LineRenderer lineRenderer;
    public bool debugDraw = true;
    public bool drawMesh = true;
    [Range(0f, 59.999f)]
    public float t = 0f; // time along the spline

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();
    Vector3[] pathPoints;
    float growthInterval; // time between each segment growth
    [Range(0f, 0.5f)] //TODO: remove this
    public float ellapsedTime = 0f;
    List<Vector3> model = new List<Vector3>();
    float t_last = 0f; //used for testing only



    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        var controlPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(controlPoints);
        pathPoints = Spline.generateCatmullrom(controlPoints, splineSegmentResolution).ToArray(); //FIXME: .toArray is inefficient

        growthInterval = growthTime / pathPoints.Length;
        // makeCylinderMesh();
        makeRingModel();
        vertices.AddRange(model);
    }

    // Update is called once per frame
    void Update()
    {
        if (t != t_last)
        {
            t_last = t;
            updateVineMesh();
        }
        if (drawMesh) Graphics.DrawMesh(mesh, transform.localToWorldMatrix, vineMaterial, LayerMask.NameToLayer("Default"));
    }

    /// <summary>
    /// creates a growing mesh that extend along the path defined by pathPoints
    /// over time.
    /// </summary>
    void updateVineMesh()
    {
        vertices.Clear();
        indices.Clear();
        int numSegments = Mathf.FloorToInt(t);
        float segmentT = (t % 1);

        // add vertices at the start so that there is always a "base"
        Matrix4x4 m_base = getTransformationMatrix(0, 0);
        for (var j = 0; j < model.Count; j++)
        {
            vertices.Add(m_base.MultiplyPoint3x4(model[j]));
        }
        if (numSegments > 0) tessellate(0);

        // add vertices for each segment
        for (var i = 0; i < numSegments; i++)
        {
            Matrix4x4 m = getTransformationMatrix(i, segmentT);
            for (var j = 0; j < model.Count; j++)
            {
                vertices.Add(m.MultiplyPoint3x4(model[j]));
            }
            if (i > 0) tessellate(i);
        }

        // add vertices for the tip
        vertices.Add(Vector3.Lerp(pathPoints[numSegments], pathPoints[numSegments + 1], segmentT));
        tessellateTip();

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
    }

    Matrix4x4 getTransformationMatrix(int segment, float segmentT)
    {
        Vector3 prev = segment == 0 ?
                            pathPoints[0] - (pathPoints[1] - pathPoints[0]) :
                            pathPoints[segment - 1];
        Vector3 start = pathPoints[segment];
        Vector3 end = segment == pathPoints.Length - 1 ?
                        pathPoints[pathPoints.Length - 1] + (pathPoints[pathPoints.Length - 1] - pathPoints[pathPoints.Length - 2]) :
                        pathPoints[segment + 1];
        Vector3 next = segment == pathPoints.Length - 2 ?
                        pathPoints[pathPoints.Length - 1] + (pathPoints[pathPoints.Length - 1] - pathPoints[pathPoints.Length - 2]) * 2 :
                        pathPoints[segment + 2];

        Vector3 forward1 = (end - start).normalized;
        Vector3 forward2 = (next - end).normalized;

        Quaternion q1 = Quaternion.LookRotation(forward1, Vector3.Cross(Vector3.up, forward1));
        Quaternion q2 = Quaternion.LookRotation(forward2, Vector3.Cross(Vector3.up, forward2));
        Quaternion q = Quaternion.Slerp(q1, q2, segmentT);

        Matrix4x4 mT = Matrix4x4.Translate(Vector3.Lerp(start, end, segmentT));
        Matrix4x4 mR = Matrix4x4.Rotate(q);
        Matrix4x4 m = mT * mR;

        return m;
    }

    void tessellate(int i)
    {
        //i = segment number
        for (int j = 0; j < numVertices - 1; j++)
        {
            indices.AddRange(new int[] { i * numVertices + j, (i + 1) * numVertices + j, i * numVertices + j + 1 });
            indices.AddRange(new int[] { (i + 1) * numVertices + j, (i + 1) * numVertices + j + 1, i * numVertices + j + 1 });
        }
        indices.AddRange(new int[] { (i + 1) * numVertices - 1, (i + 2) * numVertices - 1, i * numVertices });
        indices.AddRange(new int[] { (i + 2) * numVertices - 1, (i + 1) * numVertices, i * numVertices });
    }

    void tessellateTip()
    {
        //create triangle fan at tip
        for (int i = 0; i < numVertices - 1; i++)
        {
            indices.AddRange(new int[] { vertices.Count - 1, vertices.Count - 2 - i, vertices.Count - 3 - i });
        }

        indices.AddRange(new int[] { vertices.Count - 1, vertices.Count - 1 - numVertices, vertices.Count - 2 });
    }

    /// <summary>
    /// creates a cylinder mesh along the entire path defined by pathPoints
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

    /// <summary>
    /// creates a ring model
    /// </summary>
    void makeRingModel()
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        Vector3 side = rotation * Vector3.right;
        Vector3 forward = rotation * Vector3.forward;
        for (int j = 0; j < numVertices; j++)
        {
            float angle = 2 * Mathf.PI * j / numVertices;
            Vector3 circlePoint = Mathf.Cos(angle) * side + Mathf.Sin(angle) * forward;
            model.Add(circlePoint * radius);
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
                    // for (int j = 0; j < numVertices; j++)
                    // {
                    //     Vector3 vertex = vertices[i * numVertices + j];
                    //     Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)j / numVertices);
                    //     Gizmos.DrawSphere(transform.position + vertex, radius * 0.1f);
                    // }

                }
            }

            if (vertices.Count > 0)
            {
                for (int j = 0; j < vertices.Count; j++)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.cyan, (float)j / vertices.Count);
                    Vector3 vertex = vertices[j];
                    Gizmos.DrawSphere(transform.position + vertex, radius * 0.1f);
                }
            }
        }
    }
}
