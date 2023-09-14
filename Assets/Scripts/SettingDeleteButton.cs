using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingDeleteButton : MonoBehaviour
{
    Building currentBuilding;
    public void SetCurrentBuilding(Building currentBuilding)
    {
        this.currentBuilding = currentBuilding;
    }

    public void OnClick()
    {
        BuildingManager.GetInstance().DestroyBuilding(currentBuilding);
    }
}
