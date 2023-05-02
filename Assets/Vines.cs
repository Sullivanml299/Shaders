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

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();
    Vector3[] pathPoints;
    float growthInterval; // time between each segment growth
    [Range(0f, 0.5f)] //TODO: remove this
    public float ellapsedTime = 0f;
    [Range(0f, 1f)]
    public float lerpTest = 0f;



    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        var controlPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(controlPoints);
        pathPoints = Spline.generateCatmullrom(controlPoints, splineSegmentResolution).ToArray(); //FIXME: .toArray is inefficient

        growthInterval = growthTime / pathPoints.Length;
        print(growthInterval);
        // makeCylinderMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (ellapsedTime < growthTime)
        {
            updateVineMeshPush();
        }
        if (drawMesh) Graphics.DrawMesh(mesh, transform.localToWorldMatrix, vineMaterial, LayerMask.NameToLayer("Default"));
    }

    /// <summary>
    /// creates a growing mesh that extend along the path defined by pathPoints
    /// over time.
    /// Adds vertices to the base of the vine. Pushes old vertices along the vine. 
    /// </summary>
    void updateVineMeshPush()
    {
        // use ellapsedTime to determine number of segments
        int numSegments = Mathf.FloorToInt(ellapsedTime / growthInterval);

        for (var i = 0; i < numSegments; i++)
        {
            if (vertices.Count < numSegments * numVertices)
            {
                // add new vertices to the base of the vine
                Vector3 start = pathPoints[0];
                Vector3 end = pathPoints[1];
                makeRing(start, end, true);
            }
            else
            {
                // push old vertices along the vine
                Vector3 start = pathPoints[numSegments - i - 1];
                Vector3 end = pathPoints[numSegments - i];
                Vector3 offset = (end - start) / growthInterval * Time.deltaTime;
                for (var j = 0; j < numVertices; j++)
                {
                    vertices[i * numVertices + j] += offset;
                }
            }
        }
        ellapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// creates a growing mesh that extend along the path defined by pathPoints
    /// over time.
    /// Tip is the leading edge. New vertices are added to the tip
    /// </summary>
    void updateVineMeshPull()
    {
        vertices.Clear();
        indices.Clear();

        Vector3 start, end;

        // use ellapsedTime to determine which segment to grow
        int segment = Mathf.FloorToInt(ellapsedTime / growthInterval);
        float t = ellapsedTime % growthInterval / growthInterval;
        // print("ratio: " + ellapsedTime / growthInterval + " segment: " + segment + " t: " + t);

        //FIXME: if t=0 or 1, the ring direction is wrong

        //define tip position
        start = pathPoints[segment];
        end = pathPoints[segment + 1];
        // Vector3 tip = Vector3.Lerp(start, end, t);
        Vector3 tip = Vector3.Lerp(start, end, t);

        makeRing(start, tip, true);
        makeRing(tip, end, true);

        // //segment n
        // start = pathPoints[pathPoints.Length - 2];
        // end = pathPoints[pathPoints.Length - 1];
        // makeRing(start, end, false);

        // for (int i = 0; i < pathPoints.Length - 1; i++)
        // {
        //     for (int j = 0; j < numVertices - 1; j++)
        //     {
        //         indices.AddRange(new int[] { i * numVertices + j, (i + 1) * numVertices + j, i * numVertices + j + 1 });
        //         indices.AddRange(new int[] { (i + 1) * numVertices + j, (i + 1) * numVertices + j + 1, i * numVertices + j + 1 });
        //     }
        //     indices.AddRange(new int[] { (i + 1) * numVertices - 1, (i + 2) * numVertices - 1, i * numVertices });
        //     indices.AddRange(new int[] { (i + 2) * numVertices - 1, (i + 1) * numVertices, i * numVertices });
        // }

        // mesh.Clear();
        // mesh.vertices = vertices.ToArray();
        // mesh.triangles = indices.ToArray();
        // mesh.RecalculateNormals();

        // ellapsedTime += Time.deltaTime;
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
                Gizmos.color = Color.cyan;
                for (int j = 0; j < vertices.Count; j++)
                {
                    Vector3 vertex = vertices[j];
                    Gizmos.DrawSphere(transform.position + vertex, radius * 0.1f);
                }
            }
        }
    }
}
