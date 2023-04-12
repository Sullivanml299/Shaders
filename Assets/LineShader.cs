using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineShader : MonoBehaviour
{
    public float fillTime = 10f;

    LineRenderer lineRenderer;
    Material lineMaterial;
    Vector3[] linePositions;
    int currentIndex = 0;
    float currentTime = 0f;
    float lerpValue = 0f;
    float period; //time between each vertex
    int positionPropertyID;


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineMaterial = lineRenderer.material;
        linePositions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(linePositions);
        positionPropertyID = Shader.PropertyToID("_fillVertex");
        period = fillTime / linePositions.Length;
        print(period);
    }

    // Update is called once per frame
    void Update()
    {
        var periodTime = currentTime % period;
        var index = (int)(currentTime / period);
        if (index >= linePositions.Length - 1)
        {
            lineMaterial.SetVector(positionPropertyID, linePositions[index]);
            this.enabled = false;
            return;
        }
        lerpValue = Mathf.Min(periodTime / period, 1f);
        Vector3 pos = Vector3.Lerp(linePositions[index], linePositions[index + 1], lerpValue);
        lineMaterial.SetVector(positionPropertyID, pos);
        currentTime += Time.deltaTime;
    }
}
