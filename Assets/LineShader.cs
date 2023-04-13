using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineShader : MonoBehaviour
{
    public float fillTime = 2f;
    public bool active = false;
    LineRenderer lineRenderer;
    Material lineMaterial;
    // Vector3[] linePositions;
    int currentIndex = 0;
    float currentTime = 0f;
    float lerpValue = 0f;
    int lerpPropertyID;
    // float period; //time between each vertex
    // int positionPropertyID;



    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineMaterial = lineRenderer.material;

        lerpPropertyID = Shader.PropertyToID("_lerp");
        lineMaterial.SetFloat(lerpPropertyID, lerpValue);

        // linePositions = new Vector3[lineRenderer.positionCount];
        // lineRenderer.GetPositions(linePositions);
        // positionPropertyID = Shader.PropertyToID("_fillVertex");
        // period = fillTime / linePositions.Length;
    }

    // Update is called once per frame
    void Update()
    {
        // // Position-based coloring
        // var periodTime = currentTime % period;
        // var index = (int)(currentTime / period);
        // if (index >= linePositions.Length - 1)
        // {
        //     lineMaterial.SetVector(positionPropertyID, linePositions[index]);
        //     this.enabled = false;
        //     return;
        // }
        // lerpValue = Mathf.Min(periodTime / period, 1f);
        // Vector3 pos = Vector3.Lerp(linePositions[index], linePositions[index + 1], lerpValue);
        // lineMaterial.SetVector(positionPropertyID, pos);
        // currentTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            active = !active;
            lerpValue = 0f;
            lineMaterial.SetFloat(lerpPropertyID, lerpValue);

        }

        if (lerpValue >= 1f)
        {
            active = false;
            return;
        }

        if (!active)
        {
            return;
        }

        // if (lerpValue >= 1f)
        // {
        //     this.enabled = false;
        //     return;
        // }

        lerpValue += Time.deltaTime / fillTime;
        lineMaterial.SetFloat(lerpPropertyID, lerpValue);
    }

}
