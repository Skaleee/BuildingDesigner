using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    static BuildingManager instance;

    [SerializeField]
    GameObject prefabBuildingI, prefabBuildingL, prefabBuildingU;

    List<Building> buildings = new List<Building>();
    Building selectedBuilding = null;

    void Awake()
    {
        instance = this;
    }

    public void SelectBuilding(Building building)
    {// building can be selected under certain conditions.
        if (selectedBuilding == building)
            return;
        if (selectedBuilding != null)
        {
            selectedBuilding.SetIsSelected(false);
        }
        selectedBuilding = building;

        if(building is not null)
            building.SetIsSelected(true);

        BuildingEditor.GetInstance().SetSettingsList(building);
    }

    public void Build_Building(GameObject buildingObject)
    {
        if(selectedBuilding != null && selectedBuilding.IsBeingPlaced())
            return;

        GameObject newObject = Instantiate(buildingObject, Vector3.right * 177013, Quaternion.identity, transform);
        Building building = newObject.GetComponent<Building>();
        building.SetBuildingManager(this);
        buildings.Add(building);

        SelectBuilding(building);
        building.SetIsBeingPlaced(true);
    }

    public void DestroyBuilding(Building building)
    {
        if (selectedBuilding == building)
        {
            selectedBuilding = null;
            BuildingEditor.GetInstance().SetSettingsList(null);
        }
        buildings.Remove(building);
        Destroy(building.gameObject);
    }

    public BuildingData[] GetBuildingsData()
    {
        List<BuildingData> bd = new List<BuildingData>();
        foreach (Building building in buildings)
            bd.Add(building.ToBuildingData());
        return bd.ToArray();
    }

    public void FromBuildingsData(BuildingData[] buildingsData)
    {
        foreach(BuildingData bd in buildingsData)
        {
            GameObject newObject = null;
            switch (bd.buildingType)
            {
                case Building.BuildingType.I: newObject = Instantiate(prefabBuildingI); break;
                case Building.BuildingType.L: newObject = Instantiate(prefabBuildingL); break;
                case Building.BuildingType.U: newObject = Instantiate(prefabBuildingU); break;
            }
            if (newObject is null)
                continue;

            newObject.transform.SetParent(transform, false);
            Building building = newObject.GetComponent<Building>();
            building.FromBuildingData(bd);

            building.SetBuildingManager(this);
            buildings.Add(building);
        }

        BuildingEditor.GetInstance().SetSettingsList(null);
    }

    public static BuildingManager GetInstance() { return instance; }
    public List<Building> GetBuildings() { return buildings; }
    public Building GetSelectedBuilding() { return selectedBuilding; }
}