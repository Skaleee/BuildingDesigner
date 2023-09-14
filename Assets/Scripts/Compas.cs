using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compas : MonoBehaviour
{
    void LateUpdate() // using LateUpdate(), because Camera is adjusted in Update()
    {
        float screenWidth = Camera.main.pixelWidth;
        float screenHeight = Camera.main.pixelHeight;
        Vector3 screenPos = new Vector3(screenWidth * 0.50f, screenHeight * 0.95f, 80f);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        transform.position = worldPos;
    }
}
