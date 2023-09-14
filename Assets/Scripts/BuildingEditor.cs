using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class BuildingEditor : MonoBehaviour
{
    static BuildingEditor instance = null;

    [SerializeField]
    GameObject inputField_String, inputField_UInt, inputField_Float, inputField_Bool, deleteButton;
    [SerializeField]
    Transform contentContainer;

    Dictionary<string, SettingsInputField> inputFieldDict_String = new Dictionary<string, SettingsInputField>();
    Dictionary<string, SettingsInputField> inputFieldDict_UInt = new Dictionary<string, SettingsInputField>();
    Dictionary<string, SettingsInputField> inputFieldDict_Float = new Dictionary<string, SettingsInputField>();
    Dictionary<string, SettingsInputField> inputFieldDict_Bool = new Dictionary<string, SettingsInputField>();

    //ScrollRect scrollRect; // unused? or set scroll to the start?

    void Awake()
    {
        instance = this;
    }

    public void SetSettingsList(Building newBuilding)
    {
        inputFieldDict_String.Clear();
        inputFieldDict_UInt.Clear();
        inputFieldDict_Float.Clear();
        inputFieldDict_Bool.Clear();
        OtherUtils.DestroyAllChildrenSafely(contentContainer);

        if (newBuilding == null)
            return;

        var dict_String = newBuilding.GetSettingsDict_String();
        var dict_UInt = newBuilding.GetSettingsDict_UInt();
        var dict_Float = newBuilding.GetSettingsDict_Float();
        var dict_Bool = newBuilding.GetSettingsDict_Bool();

        foreach(KeyValuePair<string, string> entry in dict_String.ToArray())
        {
            GameObject newSettingItem = Instantiate(inputField_String);
            newSettingItem.transform.SetParent(contentContainer, false);
            SettingsInputField sif = newSettingItem.GetComponent<SettingsInputField>();
            sif.SetSetting(newBuilding, entry.Key, entry.Value);
            inputFieldDict_String.Add(entry.Key, sif);
        }

        foreach (KeyValuePair<string, uint> entry in dict_UInt.ToArray())
        {
            GameObject newSettingItem = Instantiate(inputField_UInt);
            newSettingItem.transform.SetParent(contentContainer, false);
            SettingsInputField sif = newSettingItem.GetComponent<SettingsInputField>();
            sif.SetSetting(newBuilding, entry.Key, entry.Value);
            inputFieldDict_UInt.Add(entry.Key, sif);
        }

        foreach (KeyValuePair<string, float> entry in dict_Float.ToArray())
        {
            GameObject newSettingItem = Instantiate(inputField_Float);
            newSettingItem.transform.SetParent(contentContainer, false);
            SettingsInputField sif = newSettingItem.GetComponent<SettingsInputField>();
            sif.SetSetting(newBuilding, entry.Key, entry.Value);
            inputFieldDict_Float.Add(entry.Key, sif);
        }

        foreach (KeyValuePair<string, bool> entry in dict_Bool.ToArray()) // somehow modified?? added to array to all others too as precaution, though no exceptions appeared yet
        {
            GameObject newSettingItem = Instantiate(inputField_Bool);
            newSettingItem.transform.SetParent(contentContainer, false);
            SettingsInputField sif = newSettingItem.GetComponent<SettingsInputField>();
            sif.SetSetting(newBuilding, entry.Key, entry.Value);
            inputFieldDict_Bool.Add(entry.Key, sif);
        }

        GameObject newDeleteButton = Instantiate(deleteButton);
        newDeleteButton.transform.SetParent(contentContainer, false);
        newDeleteButton.GetComponent<SettingDeleteButton>().SetCurrentBuilding(newBuilding);
    }

    public void UpdateSettingInputField(string key, string value)
    {
        inputFieldDict_String[key].UpdateSetting(value);
    }

    public void UpdateSettingInputField(string key, uint value)
    {
        inputFieldDict_UInt[key].UpdateSetting(value);
    }

    public void UpdateSettingInputField(string key, float value)
    {
        inputFieldDict_Float[key].UpdateSetting(value);
    }

    public void UpdateSettingInputField(string key, bool value)
    {
        inputFieldDict_Bool[key].UpdateSetting(value);
    }

    public static BuildingEditor GetInstance() { return instance; }
}
