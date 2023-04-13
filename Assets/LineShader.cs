using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineShader : MonoBehaviour
{
    public float fillTime = 2f;
    public bool active = false;

    LineRenderer lineRenderer;
    Material lineMaterial;

    float lerpValue = 0f;
    int lerpPropertyID;


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineMaterial = lineRenderer.material;

        lerpPropertyID = Shader.PropertyToID("_lerp");
        lineMaterial.SetFloat(lerpPropertyID, lerpValue);
    }

    // Update is called once per frame
    void Update()
    {
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

        lerpValue += Time.deltaTime / fillTime;
        lineMaterial.SetFloat(lerpPropertyID, lerpValue);
    }

}
