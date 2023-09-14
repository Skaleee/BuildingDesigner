using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    bool isEnabled = true;

    public void toggleObject(GameObject toggleableObject)
    {
        isEnabled = !isEnabled;
        toggleableObject.SetActive(isEnabled);
    }
}
