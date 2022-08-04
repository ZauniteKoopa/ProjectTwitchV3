using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextPopup : MonoBehaviour
{
    //Serialized variables
    [SerializeField]
    private TMP_Text popupInfo = null;
    [SerializeField]
    private Vector3 initialPos = Vector3.zero;
    [SerializeField]
    private Vector3 endPos = Vector3.zero;
    [SerializeField]
    private Color startColor = Color.white;
    [SerializeField]
    private Color endColor = Color.clear;
    [SerializeField]
    private float fadeDuration = 0.75f;
    [SerializeField]
    private float stayDuration = 0.25f;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(popupSequence());
    }

    //Method to set text up
    public void SetUpPopup(string textInfo)
    {
        popupInfo.text = textInfo;
    }


    // Main Popup sequence
    private IEnumerator popupSequence() {
        // Calculate max distance
        float maxDistanceTime = fadeDuration + stayDuration;
        float distanceTimer = 0f;
        WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

        // Main timer for when it's constant
        popupInfo.color = startColor;
        
        while (distanceTimer < stayDuration) {
            yield return waitFrame;
            distanceTimer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(initialPos, endPos, distanceTimer / maxDistanceTime);
        }

        // Main timer for when it starts to fade
        float fadeTimer = 0f;

        while (distanceTimer < maxDistanceTime) {
            // update timers
            yield return waitFrame;
            distanceTimer += Time.deltaTime;
            fadeTimer += Time.deltaTime;

            // Update properties
            transform.localPosition = Vector3.Lerp(initialPos, endPos, distanceTimer / maxDistanceTime);
            popupInfo.color = Color.Lerp(startColor, endColor, fadeTimer / fadeDuration);
        }

        // Destroy gameobject at the end
        Destroy(gameObject);
    }
}
