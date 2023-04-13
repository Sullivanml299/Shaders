using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E2 : MonoBehaviour
{
    public float fillTime = 2f;
    public bool active = false;

    LineRenderer lineRenderer;
    Material lineMaterial;
    Vector4 lerpDefault;
    Vector4 lerpValue;
    int lerpPropertyID;


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineMaterial = lineRenderer.material;

        lerpPropertyID = Shader.PropertyToID("_lerp");
        lineMaterial.SetVector(lerpPropertyID, lerpValue);

        lerpDefault = new Vector4(0, -0.1f, -0.2f, -0.3f);
        lerpValue = lerpDefault;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            active = !active;
            lerpValue = lerpDefault;
            lineMaterial.SetVector(lerpPropertyID, lerpValue);

        }

        if (!active)
        {
            return;
        }


        for (var i = 0; i < 4; i++)
        {
            lerpValue[i] += Time.deltaTime / fillTime;
        }

        lineMaterial.SetVector(lerpPropertyID, lerpValue);

        if (lerpValue[3] >= 1f)
        {
            active = false;
            return;
        }
    }

}
