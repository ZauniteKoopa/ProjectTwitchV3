using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsingHalo : MonoBehaviour
{
    [SerializeField]
    private float radius = 2f;
    [SerializeField]
    private Transform character = null;
    [SerializeField]
    private float circleStepSize = 0.02f;
    [SerializeField]
    private LineRenderer circleBorderRender = null;

    private const float CIRCLE_RADS = 2f * Mathf.PI;
    private const float COLLAPSING_TIME = 0.1f;


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(collapsingHaloSequence(3f));
    }


    // Main Sequence to do collapsing circle
    private IEnumerator collapsingHaloSequence(float duration) {
        // Enable renderers
        circleBorderRender.enabled = true;

        // Main loop for drawing the circle
        float timer = 0f;
        float drawDuration = duration - COLLAPSING_TIME;

        while (timer < drawDuration) {
            yield return null;

            timer += Time.deltaTime;
            float progress = Mathf.Min(timer / drawDuration, 1f);
            drawCircle(radius, circleBorderRender, progress);
        }

        // Main loop for collapsing the circle
        timer = 0f;
        while (timer < COLLAPSING_TIME) {
            yield return null;

            timer += Time.deltaTime;
            float curRadius = Mathf.Lerp(radius, 0f, timer / COLLAPSING_TIME);
            drawCircle(curRadius, circleBorderRender, 1f);
        }

        // Disable renderers
        circleBorderRender.enabled = false;
    }

    // Main function to draw a circle
    private void drawCircle(float r, LineRenderer circleRender, float progress) {
        Vector3 circleCenter = character.position;
        float radsProgressed = Mathf.Lerp(0f, CIRCLE_RADS, progress);
        int numSteps = (int)Mathf.Ceil(radsProgressed / circleStepSize);
        Vector3[] circlePositions = new Vector3[numSteps];

        for (int i = 0; i < numSteps; i++) {
            // Calculate what rads we're on
            float curRads = Mathf.Min(circleStepSize * i, CIRCLE_RADS);

            // Get the X and Z distance vectors individual
            float zDistance = r * Mathf.Cos(curRads);
            float xDistance = r * Mathf.Sin(curRads);
            Vector3 distVector = new Vector3(xDistance, 0f, zDistance);

            // Add the vector to the array
            circlePositions[i] = circleCenter + distVector;
        }

        // Set lineRenderer positions to circle positions
        circleRender.positionCount = numSteps;
        circleRender.SetPositions(circlePositions);
    }

}
