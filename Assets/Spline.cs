using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spline : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        generateCatmullrom();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void generateCatmullrom()
    {
        curvePoints.Clear();
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 p0 = controlPoints[i];
            Vector3 p1 = controlPoints[i + 1];
            Vector3 m0 = Vector3.zero;
            Vector3 m1 = Vector3.zero;

            if (i > 0)
            {
                m0 = (controlPoints[i + 1] - controlPoints[i - 1]) * 0.5f;
            }
            //make fake first point
            else
            {
                m0 = controlPoints[0] - (controlPoints[1] - controlPoints[0]) * 0.5f;
            }

            if (i < controlPoints.Count - 2)
            {
                m1 = (controlPoints[i + 2] - controlPoints[i]) * 0.5f;
            }
            //make fake last point
            //FIXME: last curve is not generated properly
            else
            {
                m1 = controlPoints[controlPoints.Count - 1] - (controlPoints[controlPoints.Count - 2] - controlPoints[controlPoints.Count - 1]) * 0.5f;
            }

            for (int j = 0; j < 10; j++)
            {
                float t = j / 10f;
                Vector3 newPos = CatmullRom(p0, p1, m0, m1, t);
                curvePoints.Add(newPos);
            }
        }
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float h00 = 2 * t3 - 3 * t2 + 1;
        float h01 = -2 * t3 + 3 * t2;
        float h10 = t3 - 2 * t2 + t;
        float h11 = t3 - t2;

        return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
    }

    void OnDrawGizmos()
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

