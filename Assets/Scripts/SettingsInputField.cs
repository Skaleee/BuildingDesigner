using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsInputField : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI keyText;
    [SerializeField]
    TMP_InputField inputField;
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    Building.SettingType type;

    Building currentBuilding;

    void Awake()
    {
        //if (type == Building.SettingType.UInt)
        //    inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    public void SetSetting(Building currentBuilding, string key, string value)
    {
        type = Building.SettingType.String;
        this.currentBuilding = currentBuilding;
        keyText.text = string.Copy(key);
        inputField.text = string.Copy(value);
    }

    public void SetSetting(Building currentBuilding, string key, float value)
    {
        type = Building.SettingType.Float;
        this.currentBuilding = currentBuilding;
        keyText.text = string.Copy(key);
        inputField.text = value.ToString();
    }

    public void SetSetting(Building currentBuilding, string key, uint value)
    {
        type = Building.SettingType.UInt;
        this.currentBuilding = currentBuilding;
        keyText.text = string.Copy(key);
        inputField.text = value.ToString();
    }

    public void SetSetting(Building currentBuilding, string key, bool value)
    {
        type = Building.SettingType.Bool;
        this.currentBuilding = currentBuilding;
        keyText.text = string.Copy(key);
        toggle.isOn = value;
    }

    public void UpdateSetting(string value)
    {
        inputField.text = string.Copy(value);
    }

    public void UpdateSetting(uint value)
    {
        inputField.text = value.ToString();
    }

    public void UpdateSetting(float value)
    {
        inputField.text = value.ToString();
    }

    public void UpdateSetting(bool value)
    {
        toggle.isOn = value;
    }

    public void OnValueEdited()
    {
        switch (type)
        {
            case Building.SettingType.String:
                string newString = currentBuilding.ChangeSetting_String(keyText.text, inputField.text);
                inputField.text = string.Copy(newString);
                break;
            case Building.SettingType.UInt:
                if (inputField.text.Equals(""))
                    inputField.text = ""+0;
                uint newUInt = currentBuilding.ChangeSetting_UInt(keyText.text, uint.Parse(inputField.text));
                inputField.text = newUInt.ToString();
                break;
            case Building.SettingType.Float:
                if (inputField.text.Equals("") || inputField.text.Equals("-") || inputField.text.Equals(",") || inputField.text.Equals("."))
                    inputField.text = "" + 0;
                float newFloat = currentBuilding.ChangeSetting_Float(keyText.text, float.Parse(inputField.text));
                inputField.text = newFloat.ToString();
                break;
            case Building.SettingType.Bool:
                bool newBool = currentBuilding.ChangeSetting_Bool(keyText.text, toggle.isOn);
                toggle.isOn = newBool;
                break;
        }
    }
}
