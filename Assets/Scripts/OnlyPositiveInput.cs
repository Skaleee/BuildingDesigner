using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class OnlyPositiveInput : MonoBehaviour
{
    TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    public void OnInputFieldValueChanged(string newValue)
    { // Do not allow minus sign.
        if(newValue.Length > 0)
        if (newValue[0] == '-')
            inputField.text = newValue.Remove(0, 1);
    }
}
