using AsciiFBXExporter;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField]//unused?
    Material buildingNormal;
    [SerializeField]
    GameObject fbxContainer, startPanel;
    [SerializeField]
    TextMeshProUGUI polygonText, gfzText, grzText, bmzText, thText, floorsText;
    [SerializeField]
    TMP_InputField polygonInput, gfzMinInput, gfzMaxInput, grzMinInput, grzMaxInput, bmzMinInput, bmzMaxInput, thMinInput, thMaxInput, floorsMinInput, floorsMaxInput;

    public void ExportToFBX()
    {
        var fullPath = StandaloneFileBrowser.SaveFilePanel("Save Mesh File", "", "SceneMesh", "fbx");
        if (fullPath == "")
            return;

        // copy and clean meshs to fbxcontainer for export. Must remove deactivated objects.
        // Otherwise they are also exported, just without material
        GameObject copy;
        foreach (Transform floorPart in Floor.GetInstance().transform)
        {
            copy = Instantiate(floorPart.gameObject);
            copy.transform.SetParent(fbxContainer.transform, false);
        }

        foreach (Transform building in BuildingManager.GetInstance().transform)
        {
            //copy = Instantiate(building.gameObject);
            //copy.transform.SetParent(fbxContainer.transform, false);
            foreach (Transform meshContainer in building)
            {
                // do not copy inactive objects (roof or markers)
                if (meshContainer.gameObject.activeSelf)
                {
                    copy = Instantiate(meshContainer.gameObject);
                    copy.transform.SetParent(fbxContainer.transform, true);
                    copy.transform.position = meshContainer.position;
                    copy.transform.rotation = meshContainer.rotation;
                }
            }
        }

        FBXExporter.ExportGameObjAtRuntime(fbxContainer, fullPath);

        // clean fbxcontainer
        OtherUtils.DestroyAllChildrenSafely(fbxContainer.transform);
    }

    public void SaveProjectToFile()
    {
        var fullPath = StandaloneFileBrowser.SaveFilePanel("Save Project File", "", "ProjectFile", "json");
        if (fullPath == "")
            return;

        BuildingData[] buildingsData = BuildingManager.GetInstance().GetBuildingsData();
        Vector2[] polygon = CoreController.GetInstance().GetPolygon();
        var polygonLineColors = CoreController.GetInstance().GetPolygonLineColors();
        float minGFZ = CoreController.GetInstance().GetMinGFZ();
        float maxGFZ = CoreController.GetInstance().GetMaxGFZ();
        float minGRZ = CoreController.GetInstance().GetMinGRZ();
        float maxGRZ = CoreController.GetInstance().GetMaxGRZ();
        float minBMZ = CoreController.GetInstance().GetMinBMZ();
        float maxBMZ = CoreController.GetInstance().GetMaxBMZ();
        float minTH = CoreController.GetInstance().GetMinTH();
        float maxTH = CoreController.GetInstance().GetMaxTH();
        uint minFloors = CoreController.GetInstance().GetMinFloors();
        uint maxFloors = CoreController.GetInstance().GetMaxFloors();

        string jsonProjectData = JsonUtility.ToJson(new SaveFileStructure(buildingsData, polygon, polygonLineColors, 
            minGFZ, maxGFZ, minGRZ, maxGRZ, minBMZ, maxBMZ, minTH, maxTH, minFloors, maxFloors), true);
        File.WriteAllText(fullPath, jsonProjectData);

        BuildingManager.GetInstance().SelectBuilding(null);
    }

    public void LoadProjectFromFile()
    {
        var fullPath = StandaloneFileBrowser.OpenFilePanel("Open Project File", "", "json", false);
        if (fullPath.Length <= 0)
            return;

        string jsonProjectData = File.ReadAllText(fullPath[0]);
        SaveFileStructure projectData = JsonUtility.FromJson<SaveFileStructure>(jsonProjectData);

        BuildingManager.GetInstance().FromBuildingsData(projectData.buildingsData);
        CoreController.GetInstance().LoadSaveData(projectData.polygon, projectData.polygonLineColors, 
            projectData.minGFZ, projectData.maxGFZ, projectData.minGRZ, projectData.maxGRZ, projectData.minBMZ, projectData.maxBMZ, 
            projectData.minTH, projectData.maxTH, projectData.minFloors, projectData.maxFloors);

        startPanel.SetActive(false);
    }

    public void TryCreateNewProject()
    {
        float minGFZ = -1, maxGFZ = -1,
        minGRZ = -1, maxGRZ = -1,
        minBMZ = -1, maxBMZ = -1,
        minTH = -1, maxTH = -1;
        uint minFloors = 0, maxFloors = 0;

        if (gfzMinInput.text == "," || gfzMinInput.text == "")
            minGFZ = -1;
        else minGFZ = float.Parse(gfzMinInput.text);
        if (grzMinInput.text == "," || grzMinInput.text == "")
            minGRZ = -1;
        else minGRZ = float.Parse(grzMinInput.text);
        if (bmzMinInput.text == "," || bmzMinInput.text == "")
            minBMZ = -1;
        else minBMZ = float.Parse(bmzMinInput.text);
        if (thMinInput.text == "," || thMinInput.text == "")
            minTH = -1;
        else minTH = float.Parse(thMinInput.text);
        if (floorsMinInput.text == "")
            minFloors = 0;
        else minFloors = uint.Parse(floorsMinInput.text);

        if (gfzMaxInput.text == "," || gfzMaxInput.text == "")
            maxGFZ = -1;
        else maxGFZ = float.Parse(gfzMaxInput.text);
        if (grzMaxInput.text == "," || grzMaxInput.text == "")
            maxGRZ = -1;
        else maxGRZ = float.Parse(grzMaxInput.text);
        if (bmzMaxInput.text == "," || bmzMaxInput.text == "")
            maxBMZ = -1;
        else maxBMZ = float.Parse(bmzMaxInput.text);
        if (thMaxInput.text == "," || thMaxInput.text == "")
            maxTH = -1;
        else maxTH = float.Parse(thMaxInput.text);
        if (floorsMaxInput.text == "")
            maxFloors = 0;
        else maxFloors = uint.Parse(floorsMaxInput.text);

        bool successfullyRead = true;

        if (TryReadPointListString(polygonInput.text, out Vector2[] polygon, out HashSet<int> polygonLineColors) && polygon.Length >= 3)
            polygonText.color = Color.black;
        else
        {
            successfullyRead = false;
            polygonText.color = Color.red;
        }

        if (minGFZ >= 0 && maxGFZ >= 0 && minGFZ > maxGFZ)
        {
            successfullyRead = false;
            gfzText.color = Color.red;
        }
        else
            gfzText.color = Color.black;
        if (minGRZ >= 0 && maxGRZ >= 0 && minGRZ > maxGRZ)
        {
            successfullyRead = false;
            grzText.color = Color.red;
        }
        else
            grzText.color = Color.black;
        if (minBMZ >= 0 && maxBMZ >= 0 && minBMZ > maxBMZ)
        {
            successfullyRead = false;
            bmzText.color = Color.red;
        }
        else
            bmzText.color = Color.black;
        if (minTH >= 0 && maxTH >= 0 && minTH > maxTH)
        {
            successfullyRead = false;
            thText.color = Color.red;
        }
        else
            thText.color = Color.black;
        if (minFloors > 0 && maxFloors > 0 && minFloors > maxFloors)
        {
            successfullyRead = false;
            floorsText.color = Color.red;
        }
        else
            floorsText.color = Color.black;

        if (!successfullyRead)
            return;

        CoreController.GetInstance().LoadSaveData(polygon, polygonLineColors.ToArray(), 
            minGFZ, maxGFZ, minGRZ, maxGRZ, minBMZ, maxBMZ, minTH, maxTH, minFloors, maxFloors);
        startPanel.SetActive(false);
    }

    public bool TryReadPointListString(string pointList, out Vector2[] polygon, out HashSet<int> polygonLineColors)
    {
        polygon = null; polygonLineColors = null;

        if (pointList == "")
            return false;

        string[] tokens = pointList.Split(new char[] { '(', ')' }, System.StringSplitOptions.RemoveEmptyEntries);
        List<Vector2> points = new List<Vector2>();
        HashSet<int> colorIndexes = new HashSet<int>();
        bool lastPointRed = false;
        foreach (string token in tokens)
        {
            if (string.Equals(token, "r") || string.Equals(token, "R"))
            {
                if (points.Count == 0)
                    lastPointRed = true;
                else
                    colorIndexes.Add(points.Count - 1); // line0 is between p0 and p1
            }
            else
            {
                string[] coordinates = token.Split(',');
                if (coordinates.Length == 2 &&
                    float.TryParse(coordinates[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(coordinates[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y))
                {
                    points.Add(new Vector2(x, y));
                }
                else
                    return false;
            }
            //else return false;
        }
        if (lastPointRed)
        {
            colorIndexes.Add(points.Count - 1);
        }
        polygon = points.ToArray();
        polygonLineColors = colorIndexes;

        // ensure points are listed clockwise
        int goesClockwise = MathUtils.CheckIfPolygonGoesClockwise(polygon);
        if (goesClockwise == -1)
        {
            System.Array.Reverse(polygon);

            HashSet<int> reversedSet = new HashSet<int>();
            foreach (int line in polygonLineColors)
            {
                int reversedLine = polygon.Length - 2 - line;
                if (reversedLine < 0)
                    reversedLine += polygon.Length;
                reversedSet.Add(reversedLine);
            }
            polygonLineColors = reversedSet;
        }

        return true;
    }
}

[Serializable]
public class SaveFileStructure
{
    public float minGFZ, maxGFZ,
        minGRZ, maxGRZ,
        minBMZ, maxBMZ,
        minTH, maxTH;
    public uint minFloors, maxFloors;
    public Vector2[] polygon;
    public int[] polygonLineColors;
    public BuildingData[] buildingsData;
    public SaveFileStructure(BuildingData[] buildingsData, Vector2[] polygon, HashSet<int> polygonLineColors,
        float minGFZ, float maxGFZ, float minGRZ, float maxGRZ, float minBMZ, float maxBMZ, float minTH, float maxTH, uint minFloors, uint maxFloors)
    {
        this.buildingsData = buildingsData;
        this.polygon = polygon;
        this.polygonLineColors = polygonLineColors.ToArray();
        this.minGFZ = minGFZ;
        this.maxGFZ = maxGFZ;
        this.minGRZ = minGRZ;
        this.maxGRZ = maxGRZ;
        this.minBMZ = minBMZ;
        this.maxBMZ = maxBMZ;
        this.minTH = minTH;
        this.maxTH = maxTH;
        this.minFloors = minFloors;
        this.maxFloors = maxFloors;
    }
}