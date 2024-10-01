using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaoticPerspective : MonoBehaviour
{
    [SerializeField] float maxDistance = 100f;
    const float constantFactor = 60f;
    [SerializeField] float originalScale = 1f; // Set this to the original scale of the object

    void Start()
    {
        originalScale = transform.localScale.x;

        // Calculate the direction from the camera to the object
        Vector3 viewDir = (transform.position - Camera.main.transform.position).normalized;

        // Choose a random distance within the range
        float chosenDist = UnityEngine.Random.Range(0, maxDistance);

        // Move the object to a new position based on the chosen distance
        transform.position = Camera.main.transform.position + viewDir * chosenDist;

        // Adjust the scale to maintain the same apparent size
        float scaleFactor = chosenDist / constantFactor;
        transform.localScale = Vector3.one * originalScale * scaleFactor;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
