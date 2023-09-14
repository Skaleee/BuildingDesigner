using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CoreController : MonoBehaviour
{
    static CoreController instance = null;

    Vector2[] polygon = new Vector2[0];

    // if hashset contains index of line, then the line must be red
    HashSet<int> polygonLineColors = new HashSet<int>();

    // use Dictionaries instead?
    // Completely move them to SaveManager? => CoreManager would not be needed anymore then.
    // -1, if uninitialised/unused
    float gr = -1, // Grundfläche
        minGFZ = -1, maxGFZ = -1,
        minGRZ = -1, maxGRZ = -1,
        minBMZ = -1, maxBMZ = -1,
        minTH = -1, maxTH = -1;
    uint minFloors = 0, maxFloors = 0; // 0, if uninitialised

    void Awake()
    {
        if (instance == null)
            instance = this;
        else 
            return;

        DontDestroyOnLoad(gameObject); // unnecessary?

        // check if normal concave polygon (no intersecting lines)?
        /*TryReadPointListString("r(1.7,6)(1.7,3)(4.7,3)r(2.9,6)(4.4,8.9)(1.7,8.9)");
        TryReadPointListString("r(0,0)r(4,0)(4,4)r(0,4)");
        TryReadPointListString("(22.4,13)(22.4,0)(13.5,0)(13.5,2.1)(6.6,2.1)(6.6,4.7)(3.4,4.7)(3.4,7.4)(0,7.4)(0,17.8)");*/
        //gf = MathUtils.GetAreaOfPolygon(polygon); // only calculate once
    }

    public void LoadSaveData(Vector2[] polygon, int[] polygonLineColors, float minGFZ, float maxGFZ, float minGRZ, float maxGRZ,
        float minBMZ, float maxBMZ, float minTH, float maxTH, uint minFloors, uint maxFloors)
    {
        this.polygon = polygon;
        this.polygonLineColors = new HashSet<int>(polygonLineColors);
        gr = MathUtils.GetAreaOfPolygon(polygon);
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
        
        Floor.GetInstance().RefreshMesh();
        CameraController.GetInstance().ResetCamera();
        // currently restrictions update every frame by themselves
    }

    public static CoreController GetInstance() { return instance; }
    public Vector2[] GetPolygon() { return polygon; }
    public HashSet<int> GetPolygonLineColors() { return polygonLineColors; }
    public float GetGR() { return gr; }
    public float GetMinGFZ() { return minGFZ; }
    public float GetMaxGFZ() { return maxGFZ; }
    public float GetMinGRZ() { return minGRZ; }
    public float GetMaxGRZ() { return maxGRZ; }
    public float GetMinBMZ() { return minBMZ; }
    public float GetMaxBMZ() { return maxBMZ; }
    public float GetMinTH() { return minTH; }
    public float GetMaxTH() { return maxTH; }
    public uint GetMinFloors() { return minFloors; }
    public uint GetMaxFloors() { return maxFloors; }
}
