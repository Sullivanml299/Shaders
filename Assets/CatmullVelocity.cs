using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullVelocity : MonoBehaviour
{
    public int verticesPerSegment = 10;
    public List<Vector3> controlPoints = new List<Vector3>{
        new Vector3(0, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 2, 0),
        new Vector3(1, 1, 1),
        new Vector3(0, 0, 1),
    };
    [Range(0f, 51f)]
    public float t = 0f;
    public bool auto = false;
    [Range(0.5f, 5f)]
    public float speed = 1f;

    public GameObject model; // model to move along the spline. Not related to modelPosition

    List<Vector3> pathPoints;
    List<Vector3> tangents;
    Vector3 currentVelocity = Vector3.zero;
    Vector3 currentPosition = Vector3.zero; // position on the spline
    Vector3 TestPosition = new Vector3(-0.5f, 0, 0); // position of test point on the spline
    Vector3 modelPosition = new Vector3(-0.5f, 0, 0); // position of test point in an imaginary model
    Vector3 up = Vector3.up;
    Quaternion correctionRotation;
    bool isSet = false;


    // Start is called before the first frame update
    void Start()
    {
        pathPoints = Spline.generateCatmullrom(controlPoints, verticesPerSegment);
        calculateTangents();
    }

    // Update is called once per frame
    void Update()
    {
        if (auto && t < 51) t += Time.deltaTime * speed;
        int segment = Mathf.FloorToInt(t);
        float segmentT = t - segment;
        updateCurrentPosition(segment, segmentT);
        updateVelocityVector(segment, segmentT);
        updateTestPosition(segment, segmentT);
        moveModelAlongPath();
    }

    void moveModelAlongPath()
    {
        //Account for current rotation so that I can keep the desired part of the model facing forward
        if (!isSet)
        {
            correctionRotation = Quaternion.Inverse(model.transform.rotation) * Quaternion.LookRotation(currentVelocity, Vector3.up);
            print(correctionRotation);
            isSet = true;
        }
        model.transform.position = currentPosition;
        model.transform.rotation = Quaternion.LookRotation(currentVelocity, Vector3.up) * correctionRotation;
    }

    void updateTestPosition(int segment, float segmentT)
    {

        Vector3 TestPositionBase;

        if (segment >= pathPoints.Count - 1)
        {
            TestPositionBase = pathPoints[pathPoints.Count - 1];
        }
        else
        {
            Vector3 prev = segment == 0 ?
                            pathPoints[0] - (pathPoints[1] - pathPoints[0]) :
                            pathPoints[segment - 1];
            Vector3 start = pathPoints[segment];
            Vector3 end = segment == pathPoints.Count - 1 ?
                            pathPoints[pathPoints.Count - 1] + (pathPoints[pathPoints.Count - 1] - pathPoints[pathPoints.Count - 2]) :
                            pathPoints[segment + 1];
            Vector3 next = segment == pathPoints.Count - 2 ?
                            pathPoints[pathPoints.Count - 1] + (pathPoints[pathPoints.Count - 1] - pathPoints[pathPoints.Count - 2]) * 2 :
                            pathPoints[segment + 2];

            //// The correct way
            // Vector3 tangent1 = (end - prev).normalized;
            // Vector3 tangent2 = (next - start).normalized;

            // Quaternion q1 = Quaternion.LookRotation(tangent1, Vector3.Cross(Vector3.up, tangent1));
            // Quaternion q2 = Quaternion.LookRotation(tangent2, Vector3.Cross(Vector3.up, tangent2));
            // Quaternion q = Quaternion.Slerp(q1, q2, segmentT);
            // up = q.eulerAngles.normalized;

            // the good enough way
            Vector3 forward1 = (end - start).normalized;
            Vector3 forward2 = (next - end).normalized;

            Quaternion q1 = Quaternion.LookRotation(forward1, Vector3.Cross(Vector3.up, forward1));
            Quaternion q2 = Quaternion.LookRotation(forward2, Vector3.Cross(Vector3.up, forward2));
            Quaternion q = Quaternion.Slerp(q1, q2, segmentT);
            up = q.eulerAngles.normalized;

            TestPositionBase = Vector3.Lerp(start, end, segmentT);

            Matrix4x4 mT = Matrix4x4.Translate(TestPositionBase);
            Matrix4x4 mR = Matrix4x4.Rotate(q);
            Matrix4x4 m = mT * mR;
            TestPosition = m.MultiplyPoint3x4(modelPosition);
        }

    }

    void updateCurrentPosition(int segment, float segmentT)
    {
        if (segment >= pathPoints.Count - 1)
        {
            currentPosition = pathPoints[pathPoints.Count - 1];
            return;
        }

        Vector3 start = pathPoints[segment];
        Vector3 end = pathPoints[segment + 1];


        currentPosition = Vector3.Lerp(start, end, segmentT);
    }

    void updateVelocityVector(int segment, float segmentT)
    {
        if (segment >= pathPoints.Count - 1)
        {
            currentVelocity = Vector3.zero;
            return;
        }

        Vector3 start = tangents[segment];
        Vector3 end = segment != tangents.Count - 1 ?
                      tangents[segment + 1] :
                      tangents[segment] + (tangents[segment] - tangents[segment - 1]);

        currentVelocity = Vector3.Lerp(start, end, segmentT);
    }

    void calculateTangents()
    {
        tangents = new List<Vector3>();
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 tangent = Vector3.zero;
            if (i == 0)
            {
                tangent = pathPoints[i + 1] - pathPoints[i];
            }
            else if (i == pathPoints.Count - 1)
            {
                tangent = pathPoints[i] - pathPoints[i - 1];
            }
            else
            {
                tangent = pathPoints[i + 1] - pathPoints[i - 1];
            }
            tangents.Add(tangent);
        }
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < controlPoints.Count; i++)
        {
            Gizmos.color = Color.Lerp(Color.green, Color.red, (float)i / (float)controlPoints.Count);
            Gizmos.DrawSphere(controlPoints[i], 0.1f);
        }

        if (pathPoints != null)
        {
            for (int i = 0; i < pathPoints.Count; i++)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.blue, (float)i / (float)pathPoints.Count);
                Gizmos.DrawSphere(pathPoints[i], 0.05f);
            }
        }

        //draw tangents
        if (tangents != null)
        {
            for (int i = 0; i < tangents.Count; i++)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(pathPoints[i], pathPoints[i] + tangents[i] * 1.5f);
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(currentPosition, currentPosition + currentVelocity * 2f);

        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(currentPosition, 0.1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(TestPosition, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(currentPosition, currentPosition + up);

    }
}
