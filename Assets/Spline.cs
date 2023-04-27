using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spline : MonoBehaviour
{
    public bool test = false;
    List<Vector3> controlPoints = new List<Vector3>(new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(2, 1, 0),
            new Vector3(3, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(5, 1, 0),
            new Vector3(6, 1, 0),

        });
    List<Vector3> curvePoints = new List<Vector3>();
    static Matrix4x4 m = new Matrix4x4(
        new Vector4(-1f, 2f, -1f, 0f) * 0.5f,
        new Vector4(3f, -5f, 0f, 2f) * 0.5f,
        new Vector4(-3f, 4f, 1f, 0f) * 0.5f,
        new Vector4(1f, -1f, 0f, 0f) * 0.5f
    );

    // Start is called before the first frame update
    void Start()
    {
        if (test) curvePoints = generateCatmullrom(controlPoints);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static List<Vector3> generateCatmullrom(List<Vector3> controlPoints)
    {
        List<Vector3> curvePoints = new List<Vector3>();
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ?
                        controlPoints[i - 1] :
                        controlPoints[0] - (controlPoints[1] - controlPoints[0]);
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = i < controlPoints.Count - 2 ?
                        controlPoints[i + 2] :
                        controlPoints[controlPoints.Count - 1] - (controlPoints[controlPoints.Count - 2] - controlPoints[controlPoints.Count - 1]);


            for (int j = 0; j < 10; j++)
            {
                float t = j / 10f;
                // Vector3 newPos = CatmullRom(p0, p1, m0, m1, t);
                Vector3 newPos = CatmullRomMatrix(p0, p1, p2, p3, t);
                curvePoints.Add(newPos);
            }
        }
        return curvePoints;
    }

    static Vector3 CatmullRomMatrix(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector4 tVec = new Vector4(t * t * t, t * t, t, 1);
        Vector4 pVec = new Vector4(p0.x, p1.x, p2.x, p3.x);
        float x = Vector4.Dot(tVec, m * pVec);
        pVec = new Vector4(p0.y, p1.y, p2.y, p3.y);
        float y = Vector4.Dot(tVec, m * pVec);
        pVec = new Vector4(p0.z, p1.z, p2.z, p3.z);
        float z = Vector4.Dot(tVec, m * pVec);
        return new Vector3(x, y, z);
    }

    void OnDrawGizmos()
    {
        if (test)
        {
            Gizmos.color = Color.red;
            foreach (Vector3 point in controlPoints)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }

            Gizmos.color = Color.blue;
            foreach (Vector3 point in curvePoints)
            {
                Gizmos.DrawSphere(point, 0.05f);
            }

            Gizmos.color = Color.green;
            var m0 = controlPoints[0] - (controlPoints[1] - controlPoints[0]);
            var m1 = controlPoints[controlPoints.Count - 1] - (controlPoints[controlPoints.Count - 2] - controlPoints[controlPoints.Count - 1]);
            Gizmos.DrawSphere(m0, 0.1f);
            Gizmos.DrawSphere(m1, 0.1f);
        }
    }
}

