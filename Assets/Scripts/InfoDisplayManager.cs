using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoDisplayManager : MonoBehaviour
{
    [SerializeField]
    GameObject infoText;
    [SerializeField]
    Transform contentContainer;

    void Start()
    {
        RefreshInfoDisplay();
    }

    void Update()
    { 
        // instead call this on every affecting user action?
        RefreshInfoDisplay();
    }

    public void RefreshInfoDisplay()
    {
        OtherUtils.DestroyAllChildrenSafely(contentContainer);

        float sumGF = 0, 
            sumGR = 0, 
            sumBM = 0, 
            gr = CoreController.GetInstance().GetGR();
        foreach (Building b in BuildingManager.GetInstance().GetBuildings())
        {
            sumGF += b.GetArea() * b.GetSettingsDict_UInt()["Floors"];
            sumGR += b.GetArea();
            sumBM += b.GetArea() * b.GetSettingsDict_Float()["Floor Height"] * b.GetSettingsDict_UInt()["Floors"];
        }

        // "General"
        var textObj = Instantiate(infoText);
        textObj.transform.SetParent(contentContainer, false);
        textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "<b><color=blue>General:</color></b>";

        // Construction Area
        textObj = Instantiate(infoText);
        textObj.transform.SetParent(contentContainer, false);
        textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "Area = <b>" + gr + "</b> m²";

        // Sum(GF)
        textObj = Instantiate(infoText);
        textObj.transform.SetParent(contentContainer, false);
        textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "Sum(GF) = <b>" + sumGF + "</b> m²";

        // Sum(GR)
        textObj = Instantiate(infoText);
        textObj.transform.SetParent(contentContainer, false);
        textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "Sum(GR) = <b>" + sumGR + "</b> m²";

        // Sum(BM)
        textObj = Instantiate(infoText);
        textObj.transform.SetParent(contentContainer, false);
        textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "Sum(BM) = <b>" + sumBM + "</b> m³";

        Building selectedBuilding = BuildingManager.GetInstance().GetSelectedBuilding();
        if (selectedBuilding is not null)
        {

            // "selected Building"
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "<b><color=#008000>Selected Building:</color></b>";

            // GR
            float selectedGR = selectedBuilding.GetArea();
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "GR = <b>" + selectedGR + "</b> m²";

            // GF
            float selectedGF = selectedGR * selectedBuilding.GetSettingsDict_UInt()["Floors"];
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "GF = <b>" + selectedGF + "</b> m²";

            // BM
            float selectedBM = selectedGR * selectedBuilding.GetSettingsDict_Float()["Floor Height"] * selectedBuilding.GetSettingsDict_UInt()["Floors"];
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "BM = <b>" + selectedBM + "</b> m³";

            // TH
            float selectedTH = selectedBuilding.GetSettingsDict_Float()["Floor Height"] * selectedBuilding.GetSettingsDict_UInt()["Floors"];
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "TH = <b>" + selectedTH + "</b> m";
        }

        // "Restrictions" // make restrictions as one generic function with a comparator for each restriction?
        if (CoreController.GetInstance().GetMinGFZ() >= 0 || CoreController.GetInstance().GetMaxGFZ() >= 0 ||
            CoreController.GetInstance().GetMinGRZ() >= 0 || CoreController.GetInstance().GetMaxGRZ() >= 0 ||
            CoreController.GetInstance().GetMinBMZ() >= 0 || CoreController.GetInstance().GetMaxBMZ() >= 0 ||
            CoreController.GetInstance().GetMinTH() >= 0 || CoreController.GetInstance().GetMaxTH() >= 0 ||
            CoreController.GetInstance().GetMinFloors() > 0 || CoreController.GetInstance().GetMaxFloors() > 0)
        { // Don't show this text, if no restrictions are defined
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "<b><color=purple>Restrictions:</color></b>";
        }

        string text;
        // GFZ
        if (CoreController.GetInstance().GetMinGFZ() >= 0 || CoreController.GetInstance().GetMaxGFZ() >= 0)
        {
            text = "Sum(GF)";
            if (CoreController.GetInstance().GetMinGFZ() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "min GFZ = <b>" + CoreController.GetInstance().GetMinGFZ() + "</b>";

                if (sumGF >= CoreController.GetInstance().GetMinGFZ() * gr)
                    text = "<color=green>min GF ≤</color> " + text;
                else
                    text = "<color=red>min GF ></color> " + text;
            }
            if (CoreController.GetInstance().GetMaxGFZ() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "max GFZ = <b>" + CoreController.GetInstance().GetMaxGFZ() + "</b>";

                if (sumGF <= CoreController.GetInstance().GetMaxGFZ() * gr)
                    text += " <color=green>≤ max GF</color>";
                else
                    text += " <color=red>> max GF</color>";
            }
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        // GRZ
        if (CoreController.GetInstance().GetMinGRZ() >= 0 || CoreController.GetInstance().GetMaxGRZ() >= 0)
        {
            text = "Sum(GR)";
            if (CoreController.GetInstance().GetMinGRZ() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "min GRZ = <b>" + CoreController.GetInstance().GetMinGRZ() + "</b>";

                if (sumGR >= CoreController.GetInstance().GetMinGRZ() * gr)
                    text = "<color=green>min GR ≤</color> " + text;
                else
                    text = "<color=red>min GR ></color> " + text;
            }
            if (CoreController.GetInstance().GetMaxGRZ() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "max GRZ = <b>" + CoreController.GetInstance().GetMaxGRZ() + "</b>";

                if (sumGR <= CoreController.GetInstance().GetMaxGRZ() * gr)
                    text += " <color=green>≤ max GR</color>";
                else
                    text += " <color=red>> max GR</color>";
            }
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        // BMZ
        if (CoreController.GetInstance().GetMinBMZ() >= 0 || CoreController.GetInstance().GetMaxBMZ() >= 0)
        {
            text = "Sum(BM)";
            if (CoreController.GetInstance().GetMinBMZ() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "min BMZ = <b>" + CoreController.GetInstance().GetMinBMZ() + "</b>";

                if (sumBM >= CoreController.GetInstance().GetMinBMZ() * gr)
                    text = "<color=green>min BM ≤</color> " + text;
                else
                    text = "<color=red>min BM ></color> " + text;
            }
            if (CoreController.GetInstance().GetMaxBMZ() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "max BMZ = <b>" + CoreController.GetInstance().GetMaxBMZ() + "</b>";

                if (sumBM <= CoreController.GetInstance().GetMaxBMZ() * gr)
                    text += " <color=green>≤ max BM</color>";
                else
                    text += " <color=red>> max BM</color>";
            }
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        // TH
        if (CoreController.GetInstance().GetMinTH() >= 0 || CoreController.GetInstance().GetMaxTH() >= 0)
        {
            text = "All(TH)";
            if (CoreController.GetInstance().GetMinTH() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "min TH = <b>" + CoreController.GetInstance().GetMinTH() + "</b> m";

                bool foundSmaller = false;
                foreach (Building b in BuildingManager.GetInstance().GetBuildings())
                    if(b.GetSettingsDict_Float()["Floor Height"] * b.GetSettingsDict_UInt()["Floors"] < CoreController.GetInstance().GetMinTH())
                    {
                        foundSmaller = true;
                        break;
                    }
                if (!foundSmaller)
                    text = "<color=green>min TH ≤</color> " + text;
                else
                    text = "<color=red>min TH ></color> " + text;
            }
            if (CoreController.GetInstance().GetMaxTH() >= 0)
            {
                textObj = Instantiate(infoText);
                textObj.transform.SetParent(contentContainer, false);
                textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "max TH = <b>" + CoreController.GetInstance().GetMaxTH() + "</b> m";

                bool foundBigger = false;
                foreach (Building b in BuildingManager.GetInstance().GetBuildings())
                    if (b.GetSettingsDict_Float()["Floor Height"] * b.GetSettingsDict_UInt()["Floors"] > CoreController.GetInstance().GetMaxTH())
                    {
                        foundBigger = true;
                        break;
                    }
                if (!foundBigger)
                    text += " <color=green>≤ max TH</color>";
                else
                    text += " <color=red>> max TH</color>";
            }
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        // Floors // had to split it up, because the text is too long
        text = "All(Floors)";
        if (CoreController.GetInstance().GetMinFloors() > 0)
        {
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "min Floors = <b>" + CoreController.GetInstance().GetMinFloors() + "</b>";

            bool foundSmaller = false;
            foreach (Building b in BuildingManager.GetInstance().GetBuildings())
                if (b.GetSettingsDict_UInt()["Floors"] < CoreController.GetInstance().GetMinFloors())
                {
                    foundSmaller = true;
                    break;
                }
            if (!foundSmaller)
                text = "<color=green>min Floors ≤</color> " + text;
            else
                text = "<color=red>min Floors ></color> " + text;

            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        text = "All(Floor)";
        if (CoreController.GetInstance().GetMaxFloors() > 0)
        {
            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "max Floors = <b>" + CoreController.GetInstance().GetMaxFloors() + "</b>";

            bool foundBigger = false;
            foreach (Building b in BuildingManager.GetInstance().GetBuildings())
                if (b.GetSettingsDict_UInt()["Floors"] > CoreController.GetInstance().GetMaxFloors())
                {
                    foundBigger = true;
                    break;
                }
            if (!foundBigger)
                text += " <color=green>≤ max Floors</color>";
            else
                text += " <color=red>> max Floors</color>";

            textObj = Instantiate(infoText);
            textObj.transform.SetParent(contentContainer, false);
            textObj.GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        // for better spacing:
        //textObj = Instantiate(infoText);
        //textObj.transform.SetParent(contentContainer, false);
        //textObj.GetComponent<TMPro.TextMeshProUGUI>().text = "";
    }
}
