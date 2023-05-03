using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionTest : MonoBehaviour
{
    public float time = 10f;
    [Range(0f, 1f)]
    public float t = 0f;
    public bool manualControl = false;
    public bool toggle = true;
    public Vector3 p1 = new Vector3(0, 0, 0);
    public Vector3 p2 = new Vector3(0, 0.75f, 0.25f);
    public Vector3 p3 = new Vector3(0, 1, 1);
    public Vector3 p = new Vector3(0, 0, -1);
    public Vector3 pModel = new Vector3(0, 0, -1);
    Vector3 v1, v2;
    Quaternion q1, q2, q;
    float segmentInterval;
    float ellapsedTime = 0f;
    float t_last = 0f;
    bool lastToggle = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        v1 = p2 - p1;
        v2 = p3 - p2;
        q1 = Quaternion.FromToRotation(Vector3.up, v1);
        q2 = Quaternion.FromToRotation(Vector3.up, v2);
        segmentInterval = time / 2f;
        // if (manualControl) manualTest();
        // else if (ellapsedTime < time) test();
        if (toggle != lastToggle) matrixTest();
    }

    void matrixTest()
    {
        print("toggle");
        q = Quaternion.FromToRotation(v1, v2);
        // Matrix4x4 mO = Matrix4x4.Translate(p);
        Matrix4x4 mT = !toggle ? Matrix4x4.Translate(v1 + v2) : Matrix4x4.Translate(v1 + v2).inverse;
        Matrix4x4 mR = !toggle ? Matrix4x4.Rotate(q) : Matrix4x4.Rotate(q).inverse;

        Matrix4x4 m = !toggle ? mT * mR : mR * mT;

        p = m.MultiplyPoint3x4(p);

        lastToggle = toggle;
    }



    void test()
    {
        t = ellapsedTime / segmentInterval;
        int segment = Mathf.FloorToInt(ellapsedTime / segmentInterval);
        Vector3 start = segment > 0 ? p2 : p1;
        Vector3 end = segment > 0 ? p3 : p2;
        Vector3 path = end - start;

        Vector3 positionOffset = path / segmentInterval * Time.deltaTime;
        float angle = Quaternion.Angle(q1, q2);
        q = Quaternion.RotateTowards(q1, q2, angle / segmentInterval);

        Matrix4x4 mT = Matrix4x4.Translate(positionOffset);
        Matrix4x4 mO = Matrix4x4.Translate(p);
        Matrix4x4 mR = Matrix4x4.Rotate(q);

        // q = Quaternion.Slerp(q1, q2, t);
        p = mO.inverse.MultiplyPoint3x4(p);
        p = mR.MultiplyPoint3x4(p);
        p = mO.MultiplyPoint3x4(p);
        p = mT.MultiplyPoint3x4(p);

        ellapsedTime += Time.deltaTime;
    }

    void manualTest()
    {
        int segment = Mathf.FloorToInt(ellapsedTime / segmentInterval);
        Vector3 start = segment > 0 ? p2 : p1;
        Vector3 end = segment > 0 ? p3 : p2;
        Vector3 path = end - start;

        // Vector3 positionOffset = path / segmentInterval * (t - t_last);
        // Matrix4x4 m = Matrix4x4.Translate(positionOffset);
        // q = Quaternion.Slerp(q1, q2, t);
        // p = m.MultiplyPoint3x4(p);

        t_last = t;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(p1, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(p2, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(p3, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(p1, p1 + v1);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(p2, p2 + v2);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(p, p + q1 * Vector3.up);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(p, p + q2 * Vector3.up);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(p, 0.1f);
        Gizmos.DrawLine(p, p + q * Vector3.up);
    }
}
