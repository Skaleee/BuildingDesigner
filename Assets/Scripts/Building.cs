using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class Building : MonoBehaviour
{
    public enum BuildingType
    {
        I,
        L,
        U
    }

    public enum SettingType
    {
        String,
        Float,
        Bool,
        UInt
    }

    [SerializeField]
    Material buildingNormal, buildingSelected, buildingSelectedUnplaceable;
    [SerializeField]
    Transform buildingPartsContainer, roofPartsContainer, markerContainer;

    //only used for initialisation. Are otherwise ignored.
    [SerializeField]
    string[] settingsKeys_String = new string[] { "Name" };
    [SerializeField]
    string[] settingsValues_String = new string[] { "" };

    [SerializeField]
    string[] settingsKeys_UInt = new string[] { "Floors", "Floor Height", "Rotation" };
    [SerializeField]
    uint[] settingsValues_UInt = new uint[] { 1, 1, 0 };

    [SerializeField]
    string[] settingsKeys_Float = new string[] {};
    [SerializeField]
    float[] settingsValues_Float = new float[] {};

    [SerializeField]
    string[] settingsKeys_Bool = new string[] { "Flat Roof" };
    [SerializeField]
    bool[] settingsValues_Bool = new bool[] { true };

    [SerializeField]
    BuildingType buildingType;

    float markerExtraSize = 0.1f;

    Dictionary<string, string> settingsDict_String = new Dictionary<string, string>();
    Dictionary<string, uint> settingsDict_UInt = new Dictionary<string, uint>();
    Dictionary<string, float> settingsDict_Float = new Dictionary<string, float>();
    Dictionary<string, bool> settingsDict_Bool = new Dictionary<string, bool>();

    bool isBeingDragged = false;
    bool isBeingPlaced = false;
    bool isSelected = false;
    new Rigidbody rigidbody;
    List<MeshRenderer> childrenMr = new List<MeshRenderer>();
    List<BoxCollider> childrenCol = new List<BoxCollider>();
    BuildingManager buildingManager;


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        foreach (Transform child in buildingPartsContainer)
        {
            childrenMr.Add(child.GetComponent<MeshRenderer>());
            childrenCol.Add(child.GetComponent<BoxCollider>());
        }

        // Initialize Settings
        for (int i = 0; i < settingsKeys_String.Length; i++)
            settingsDict_String.Add(settingsKeys_String[i], settingsValues_String[i]);
        for (int i = 0; i < settingsKeys_UInt.Length; i++)
            settingsDict_UInt.Add(settingsKeys_UInt[i], settingsValues_UInt[i]);
        for (int i = 0; i < settingsKeys_Float.Length; i++)
            settingsDict_Float.Add(settingsKeys_Float[i], settingsValues_Float[i]);
        for (int i = 0; i < settingsKeys_Bool.Length; i++)
            settingsDict_Bool.Add(settingsKeys_Bool[i], settingsValues_Bool[i]);
    }

    void Start()
    {
        UpdateFloors(settingsDict_UInt["Floors"], settingsDict_Float["Floor Height"]);
        switch (buildingType)
        {
            case BuildingType.I: UpdateGroundSpaceI();  break;
            case BuildingType.L: UpdateGroundSpaceL(); break;
            case BuildingType.U: UpdateGroundSpaceU(); break;
        }
        UpdateMaterial();
    }

    void OnMouseDown()
    { // maybe IMPLEMENT extra offset when clicked so building does not teleport to mouse-position. Make position relative to clickpoint
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (Input.GetButton("CameraEnabled"))
            return;

        if (isBeingPlaced)
        {
            // check if placeable and place
            if (!IsCurrentPositionLegal())
                buildingManager.DestroyBuilding(this);
            isBeingPlaced = false;
            // is already selected by BuildingManager
        }
        else if (!isBeingDragged) 
        {
            if (buildingManager.GetSelectedBuilding() != null) // is another object being dragged/placed?
                if (buildingManager.GetSelectedBuilding().IsBeingPlaced())
                    return;
            isBeingDragged = true; // start dragging this object

            buildingManager.SelectBuilding(this);
        }
    }

    void OnMouseUp()
    {
        isBeingDragged = false;
    }

    void Update()
    {
        if (isBeingPlaced || (Input.GetMouseButton(0) && isBeingDragged))
        {
            Vector3 point = OtherUtils.GetMousePositionOnXZ();
            //point.y = transform.position.y; // height is now automatically handled in UpdateFloors()

            if (!IsCurrentPositionLegal() || isBeingPlaced)// no collision if position is already invalid or when being placed
            {
                rigidbody.position = point;
                transform.position = point;
            }
            else
            {
                // check position after moving. If invalid, move back.
                Vector3 previousPosition = transform.position;

                rigidbody.position = point;
                transform.position = point;
                if (!IsCurrentPositionLegal() && !isBeingPlaced)
                {
                    rigidbody.position = previousPosition;
                    transform.position = previousPosition;
                }
            }

            settingsDict_Float["X-Position"] = transform.position.x;
            settingsDict_Float["Y-Position"] = transform.position.z;
            BuildingEditor.GetInstance().UpdateSettingInputField("X-Position", transform.position.x);
            BuildingEditor.GetInstance().UpdateSettingInputField("Y-Position", transform.position.z);

            UpdateMaterial();
        }
    }

    public Vector2[] AsPolygon()
    {
        // Building position is the middle-point of a cube exactly surrounding the polygon
        Vector2[] polygon = null;
        Vector3[] tmp = null;
        switch (buildingType)
        {
            case BuildingType.I:
                polygon = new Vector2[4];
                tmp = new Vector3[4];
                // cube: Breadth x Length
                tmp[0] = transform.rotation * new Vector3(settingsDict_Float["Breadth"] / 2, 0, settingsDict_Float["Length"] / 2) + transform.position;
                tmp[1] = transform.rotation * new Vector3(-settingsDict_Float["Breadth"] / 2, 0, settingsDict_Float["Length"] / 2) + transform.position;
                tmp[2] = transform.rotation * new Vector3(-settingsDict_Float["Breadth"] / 2, 0, -settingsDict_Float["Length"] / 2) + transform.position;
                tmp[3] = transform.rotation * new Vector3(settingsDict_Float["Breadth"] / 2, 0, -settingsDict_Float["Length"] / 2) + transform.position;
                
                break;
            case BuildingType.L:
                polygon = new Vector2[6];
                tmp = new Vector3[6];
                // cube: Length1 x Length2
                // L1 >= B2 && L2 >= B1
                tmp[0] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2, 0, settingsDict_Float["Length1"] / 2) + transform.position;
                tmp[1] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length1"] / 2) + transform.position;
                tmp[2] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length1"] / 2) + transform.position;
                tmp[3] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Breadth2"]) + transform.position;
                tmp[4] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"], 0, -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Breadth2"]) + transform.position;
                tmp[5] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"], 0, settingsDict_Float["Length1"] / 2) + transform.position;

                break;
            case BuildingType.U:
                polygon = new Vector2[8];
                tmp = new Vector3[8];
                // cube: L1>L3 ?  Length1 x Length2  :  Length3 x Length2
                // L1 >= B2 && L3 >= B2 && L2 >= B1+B3
                if (settingsDict_Float["Length1"] > settingsDict_Float["Length3"]) // longer length decides length of cube
                {
                    tmp[0] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2, 0, settingsDict_Float["Length1"] / 2) + transform.position;
                    tmp[1] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length1"] / 2) + transform.position;
                    tmp[2] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length1"] / 2) + transform.position;
                    tmp[3] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Length3"]) + transform.position;
                    tmp[4] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2 - settingsDict_Float["Breadth3"], 0, -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Length3"]) + transform.position;
                    tmp[5] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2 - settingsDict_Float["Breadth3"], 0, -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Breadth2"]) + transform.position;
                    tmp[6] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"], 0, -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Breadth2"]) + transform.position;
                    tmp[7] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"], 0, settingsDict_Float["Length1"] / 2) + transform.position;
                }
                else
                {
                    tmp[0] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length3"] / 2 + settingsDict_Float["Length1"]) + transform.position;
                    tmp[1] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length3"] / 2) + transform.position;
                    tmp[2] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2, 0, -settingsDict_Float["Length3"] / 2) + transform.position;
                    tmp[3] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2, 0, settingsDict_Float["Length3"] / 2) + transform.position;
                    tmp[4] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2 - settingsDict_Float["Breadth3"], 0, settingsDict_Float["Length3"] / 2) + transform.position;
                    tmp[5] = transform.rotation * new Vector3(settingsDict_Float["Length2"] / 2 - settingsDict_Float["Breadth3"], 0, -settingsDict_Float["Length3"] / 2 + settingsDict_Float["Breadth2"]) + transform.position;
                    tmp[6] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"], 0, -settingsDict_Float["Length3"] / 2 + settingsDict_Float["Breadth2"]) + transform.position;
                    tmp[7] = transform.rotation * new Vector3(-settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"], 0, -settingsDict_Float["Length3"] / 2 + settingsDict_Float["Length1"]) + transform.position;
                }
                break;
        }
        for (int i = 0; i < polygon.Length; i++)
            polygon[i] = new Vector2(tmp[i].x, tmp[i].z);

        return polygon;
    }

    public bool IsCurrentPositionLegal() //INSTEAD make this "doesCollideWithOtherBuildings" and make another method for polygon, because of placing mechanism? // Maybe not needed?
    {
        // check for collision with other buildings
        /* check using bounds of collider-component-> works not correct, because bounds is not rotatable cube
        List<Building> buildings = buildingManager.GetBuildings();
        foreach (Building building in buildings)
        {
            if (building == this)
                continue;

            foreach (Collider buildingCol in building.GetChildrenColliders())
                foreach(Collider childCol in childrenCol)
                    if (buildingCol.bounds.Intersects(childCol.bounds))
                        return false;
            /*if (MathUtils.DoPolygonsIntersect(AsPolygon(), building.AsPolygon()))
                return false;
            if (MathUtils.IsPolygonInsidePolygon(AsPolygon(), building.AsPolygon()))
                return false;
            if (MathUtils.IsPolygonInsidePolygon(building.AsPolygon(), AsPolygon()))
                return false;*/
        //}

        // check using OverlapBox
        foreach (Transform child in buildingPartsContainer)
        {
            var colliders = Physics.OverlapBox(child.position, child.lossyScale / 2, child.rotation);
            // OverlapBox is its own collider
            // => check if colliders contained in own childrenColliders
            // => if not found, must be foreign Collider
            foreach (Collider col in colliders)
            {
                bool found = false;
                foreach (Collider childCol in childrenCol)
                    if (col.GetInstanceID() == childCol.GetInstanceID())
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    return false;
            }
        }
        

        // check if in bounds of polygon
        if (!MathUtils.IsPolygonInsidePolygon(this.AsPolygon(), CoreController.GetInstance().GetPolygon()))
            return false;

        return true;
    }

    public string ChangeSetting_String(string key, string value) // returns updated value (incase value was not accepted)
    {
        if (!settingsDict_String.ContainsKey(key))
            Debug.Log("Unknown settings key \"" + key + "\" passed." + settingsDict_String["Name"]); // ??
        settingsDict_String[key] = string.Copy(value);
        return value;
    }

    public uint ChangeSetting_UInt(string key, uint value)
    {
        if (!settingsDict_UInt.ContainsKey(key))
            Debug.Log("Unknown settings key \"" + key + "\" passed." + settingsDict_String["Name"]); // ??
        settingsDict_UInt[key] = value;

        switch (key)
        {
            case "Floors":
                if (value < 1)
                    value = 1;
                settingsDict_UInt[key] = value;
                UpdateFloors(value, settingsDict_Float["Floor Height"]);
                break;
        }
        return value;
    }

    public float ChangeSetting_Float(string key, float value)
    {
        if (!settingsDict_Float.ContainsKey(key))
            Debug.Log("Unknown settings key \"" + key + "\" passed." + settingsDict_String["Name"]); // ??
        settingsDict_Float[key] = value;

        Vector3 tmp;
        switch (key)
        {
            // I,L,U <3
            case "Floor Height":
                if (value < 1)
                    value = 1;
                settingsDict_Float[key] = value;
                UpdateFloors(settingsDict_UInt["Floors"], value);
                break;
            case "Rotation":
                Quaternion rotation = Quaternion.AngleAxis(value, Vector3.up);
                rigidbody.rotation = rotation;
                transform.rotation = rotation;
                UpdateMaterial();
                break;
            case "X-Position":
                tmp = transform.position;
                tmp.x = value;
                rigidbody.position = tmp;
                transform.position = tmp;
                UpdateMaterial();
                break;
            case "Y-Position":
                tmp = transform.position;
                tmp.z = value;
                rigidbody.position = tmp;
                transform.position = tmp;
                UpdateMaterial();
                break;
            //I
            case "Length":
                if (value < 1)
                    value = 1;
                settingsDict_Float[key] = value;

                UpdateGroundSpaceI();
                UpdateMaterial();
                break;
            case "Breadth":
                if (value < 1)
                    value = 1;
                settingsDict_Float[key] = value;

                UpdateGroundSpaceI();
                UpdateMaterial();
                break;
            //L,U
            case "Length1":
                if (value < 1)
                    value = 1;
                if(value < settingsDict_Float["Breadth2"])
                    value = settingsDict_Float["Breadth2"];
                settingsDict_Float[key] = value;

                if (buildingType == BuildingType.L) UpdateGroundSpaceL();
                else UpdateGroundSpaceU();
                UpdateMaterial();
                break;
            case "Breadth1":
                if (value < 1)
                    value = 1;
                if (buildingType == BuildingType.L) 
                {
                    if (value > settingsDict_Float["Length2"])
                        value = settingsDict_Float["Length2"];
                }else 
                    if(value > settingsDict_Float["Length2"] - settingsDict_Float["Breadth3"])
                        value = settingsDict_Float["Length2"] - settingsDict_Float["Breadth3"];
                settingsDict_Float[key] = value;

                if (buildingType == BuildingType.L) UpdateGroundSpaceL();
                else UpdateGroundSpaceU();
                UpdateMaterial();
                break;
            case "Length2":
                if (value < 1)
                    value = 1;
                if (buildingType == BuildingType.L)
                {
                    if (value < settingsDict_Float["Breadth1"])
                        value = settingsDict_Float["Breadth1"];
                }
                else 
                    if (value < settingsDict_Float["Breadth1"] + settingsDict_Float["Breadth3"])
                        value = settingsDict_Float["Breadth1"] + settingsDict_Float["Breadth3"];
                settingsDict_Float[key] = value;

                if (buildingType == BuildingType.L) UpdateGroundSpaceL();
                else UpdateGroundSpaceU();
                UpdateMaterial();
                break;
            case "Breadth2":
                if (value < 1)
                    value = 1;
                if (buildingType == BuildingType.L)
                {
                    if (value > settingsDict_Float["Length1"])
                        value = settingsDict_Float["Length1"];
                }
                else
                    if (value > settingsDict_Float["Length1"] || value > settingsDict_Float["Length3"])
                        value = settingsDict_Float["Length1"] > settingsDict_Float["Length3"] ? settingsDict_Float["Length3"] : settingsDict_Float["Length1"];
                settingsDict_Float[key] = value;

                if (buildingType == BuildingType.L) UpdateGroundSpaceL();
                else UpdateGroundSpaceU();
                UpdateMaterial();
                break;
            //U
            case "Length3":
                if (value < 1)
                    value = 1;
                if(value < settingsDict_Float["Breadth2"])
                    value = settingsDict_Float["Breadth2"];
                settingsDict_Float[key] = value;

                if (buildingType == BuildingType.L) UpdateGroundSpaceL();
                else UpdateGroundSpaceU();
                UpdateMaterial();
                break;
            case "Breadth3":
                if (value < 1)
                    value = 1;
                if (value > settingsDict_Float["Length2"] - settingsDict_Float["Breadth1"])
                    value = settingsDict_Float["Length2"] - settingsDict_Float["Breadth1"];
                settingsDict_Float[key] = value;

                UpdateGroundSpaceU();
                UpdateMaterial();
                break;


        }
        return value;
    }

    public bool ChangeSetting_Bool(string key, bool value)
    {
        if (!settingsDict_Bool.ContainsKey(key))
            Debug.Log("Unknown settings key \"" + key + "\" passed." + settingsDict_String["Name"]); // ??
        settingsDict_Bool[key] = value;

        switch (key)
        {
            case "Flat Roof":
                roofPartsContainer.gameObject.SetActive(!value);
                break;
        }
        return value;
    }

    public void UpdateGroundSpaceI()
    {
        Vector3 tmpScale;

        tmpScale.x = settingsDict_Float["Breadth"];
        tmpScale.z = settingsDict_Float["Length"];

        tmpScale.y = buildingPartsContainer.GetChild(0).localScale.y;
        buildingPartsContainer.GetChild(0).localScale = tmpScale;

        tmpScale.y = roofPartsContainer.GetChild(0).localScale.y;
        roofPartsContainer.GetChild(0).localScale = tmpScale;

        tmpScale.y = markerContainer.GetChild(0).localScale.y;
        tmpScale.x += markerExtraSize;
        tmpScale.z += markerExtraSize;
        markerContainer.GetChild(0).localScale = tmpScale;

        RefreshMarkers();
    }

    public void UpdateGroundSpaceL()
    {
        Vector3 tmpScale, tmpPos;
        // Block 1
        tmpScale.x = settingsDict_Float["Breadth1"];
        tmpScale.z = settingsDict_Float["Length1"];

        tmpPos.x = -settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"] / 2;
        tmpPos.z = 0;

        tmpScale.y = buildingPartsContainer.GetChild(0).localScale.y;
        buildingPartsContainer.GetChild(0).localScale = tmpScale;
        tmpPos.y = buildingPartsContainer.GetChild(0).localPosition.y;
        buildingPartsContainer.GetChild(0).localPosition = tmpPos;

        tmpScale.y = roofPartsContainer.GetChild(0).localScale.y;
        roofPartsContainer.GetChild(0).localScale = tmpScale;
        tmpPos.y = roofPartsContainer.GetChild(0).localPosition.y;
        roofPartsContainer.GetChild(0).localPosition = tmpPos;

        // marker
        tmpScale.y = markerContainer.GetChild(0).GetChild(0).localScale.y;
        tmpScale.x += markerExtraSize;
        tmpScale.z += markerExtraSize;
        markerContainer.GetChild(0).GetChild(0).localScale = tmpScale;
        tmpPos.y = markerContainer.GetChild(0).GetChild(0).localPosition.y;
        markerContainer.GetChild(0).GetChild(0).localPosition = tmpPos;

        // Block 2
        tmpScale.x = settingsDict_Float["Length2"];
        tmpScale.z = settingsDict_Float["Breadth2"];

        tmpPos.x = 0;
        tmpPos.z = -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Breadth2"] / 2;
        
        tmpScale.y = buildingPartsContainer.GetChild(1).localScale.y;
        buildingPartsContainer.GetChild(1).localScale = tmpScale;
        tmpPos.y = buildingPartsContainer.GetChild(1).localPosition.y;
        buildingPartsContainer.GetChild(1).localPosition = tmpPos;

        tmpScale.y = roofPartsContainer.GetChild(1).localScale.y;
        roofPartsContainer.GetChild(1).localScale = tmpScale;
        tmpPos.y = roofPartsContainer.GetChild(1).localPosition.y;
        roofPartsContainer.GetChild(1).localPosition = tmpPos;

        // marker
        tmpScale.y = markerContainer.GetChild(0).GetChild(1).localScale.y;
        tmpScale.x += markerExtraSize;  
        tmpScale.z += markerExtraSize;
        markerContainer.GetChild(0).GetChild(1).localScale = tmpScale;
        tmpPos.y = markerContainer.GetChild(0).GetChild(1).localPosition.y;
        markerContainer.GetChild(0).GetChild(1).localPosition = tmpPos;

        RefreshMarkers();
    }

    public void UpdateGroundSpaceU()
    {
        Vector3 tmpScale, tmpPos;
        // Block 1
        tmpScale.x = settingsDict_Float["Breadth1"];
        tmpScale.z = settingsDict_Float["Length1"];

        tmpPos.x = -settingsDict_Float["Length2"] / 2 + settingsDict_Float["Breadth1"] / 2;
        if (settingsDict_Float["Length1"] > settingsDict_Float["Length3"])
            tmpPos.z = 0;
        else tmpPos.z = -settingsDict_Float["Length3"] / 2 + settingsDict_Float["Length1"] / 2;

        tmpScale.y = buildingPartsContainer.GetChild(0).localScale.y;
        buildingPartsContainer.GetChild(0).localScale = tmpScale;
        tmpPos.y = buildingPartsContainer.GetChild(0).localPosition.y;
        buildingPartsContainer.GetChild(0).localPosition = tmpPos;

        tmpScale.y = roofPartsContainer.GetChild(0).localScale.y;
        roofPartsContainer.GetChild(0).localScale = tmpScale;
        tmpPos.y = roofPartsContainer.GetChild(0).localPosition.y;
        roofPartsContainer.GetChild(0).localPosition = tmpPos;

        // marker
        tmpScale.y = markerContainer.GetChild(0).GetChild(0).localScale.y;
        tmpScale.x += markerExtraSize;
        tmpScale.z += markerExtraSize;
        markerContainer.GetChild(0).GetChild(0).localScale = tmpScale;
        tmpPos.y = markerContainer.GetChild(0).GetChild(0).localPosition.y;
        markerContainer.GetChild(0).GetChild(0).localPosition = tmpPos;

        // Block 2
        tmpScale.x = settingsDict_Float["Length2"];
        tmpScale.z = settingsDict_Float["Breadth2"];

        tmpPos.x = 0;
        if (settingsDict_Float["Length1"] > settingsDict_Float["Length3"])
            tmpPos.z = -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Breadth2"] / 2;
        else tmpPos.z = -settingsDict_Float["Length3"] / 2 + settingsDict_Float["Breadth2"] / 2;
        
        tmpScale.y = buildingPartsContainer.GetChild(1).localScale.y;
        buildingPartsContainer.GetChild(1).localScale = tmpScale;
        tmpPos.y = buildingPartsContainer.GetChild(1).localPosition.y;
        buildingPartsContainer.GetChild(1).localPosition = tmpPos;

        tmpScale.y = roofPartsContainer.GetChild(1).localScale.y;
        roofPartsContainer.GetChild(1).localScale = tmpScale;
        tmpPos.y = roofPartsContainer.GetChild(1).localPosition.y;
        roofPartsContainer.GetChild(1).localPosition = tmpPos;

        // marker
        tmpScale.y = markerContainer.GetChild(0).GetChild(1).localScale.y;
        tmpScale.x += markerExtraSize;
        tmpScale.z += markerExtraSize;
        markerContainer.GetChild(0).GetChild(1).localScale = tmpScale;
        tmpPos.y = markerContainer.GetChild(0).GetChild(1).localPosition.y;
        markerContainer.GetChild(0).GetChild(1).localPosition = tmpPos;

        // Block 3
        tmpScale.x = settingsDict_Float["Breadth3"];
        tmpScale.z = settingsDict_Float["Length3"];

        tmpPos.x = settingsDict_Float["Length2"] / 2 - settingsDict_Float["Breadth3"] / 2;
        if (settingsDict_Float["Length3"] > settingsDict_Float["Length1"])
            tmpPos.z = 0;
        else tmpPos.z = -settingsDict_Float["Length1"] / 2 + settingsDict_Float["Length3"] / 2;

        tmpScale.y = buildingPartsContainer.GetChild(2).localScale.y;
        buildingPartsContainer.GetChild(2).localScale = tmpScale;
        tmpPos.y = buildingPartsContainer.GetChild(2).localPosition.y;
        buildingPartsContainer.GetChild(2).localPosition = tmpPos;

        tmpScale.y = roofPartsContainer.GetChild(2).localScale.y;
        roofPartsContainer.GetChild(2).localScale = tmpScale;
        tmpPos.y = roofPartsContainer.GetChild(2).localPosition.y;
        roofPartsContainer.GetChild(2).localPosition = tmpPos;

        // marker
        tmpScale.y = markerContainer.GetChild(0).GetChild(2).localScale.y;
        tmpScale.x += markerExtraSize;
        tmpScale.z += markerExtraSize;
        markerContainer.GetChild(0).GetChild(2).localScale = tmpScale;
        tmpPos.y = markerContainer.GetChild(0).GetChild(2).localPosition.y;
        markerContainer.GetChild(0).GetChild(2).localPosition = tmpPos;

        RefreshMarkers();
    }

    public void UpdateFloors(uint floors, float floorHeight)
    {
        float size = floors * floorHeight;
        
        Vector3 tmp = buildingPartsContainer.localScale;
        tmp.y = size;
        buildingPartsContainer.localScale = tmp;

        tmp = buildingPartsContainer.position;
        tmp.y = size / 2;
        buildingPartsContainer.position = tmp;

        tmp = roofPartsContainer.position;
        tmp.y = size;
        roofPartsContainer.position = tmp;

        RefreshMarkers();
    }

    public void RefreshMarkers()
    {
        OtherUtils.DestroyChildrenSafely(markerContainer, 1);

        if (settingsDict_UInt["Floors"] <= 1)
        {
            markerContainer.gameObject.SetActive(false);
            return;
        }
        else markerContainer.gameObject.SetActive(true);

        for (int i = 0; i < settingsDict_UInt["Floors"] - 1; i++)
        {
            GameObject newMarker;
            if (i == 0) newMarker = markerContainer.GetChild(0).gameObject;
            else newMarker = Instantiate(markerContainer.GetChild(0).gameObject, markerContainer);

            newMarker.transform.localPosition = Vector3.up * settingsDict_Float["Floor Height"] * (i + 1);
        }
    }

    public void UpdateMaterial()
    {
        if (IsCurrentPositionLegal())
        {
            if (isSelected)
                SetMaterial(buildingSelected);
            else SetMaterial(buildingNormal);
        }
        else
            SetMaterial(buildingSelectedUnplaceable);
    }

    void SetMaterial(Material material)
    {
        foreach (MeshRenderer childMr in childrenMr)
            childMr.material = material;
    }
    public void SetIsSelected(bool isSelected) 
    { 
        this.isSelected = isSelected; 
        UpdateMaterial();
    }

    public void FromBuildingData(BuildingData buildingData)
    {
        // buildingType is already set in prefab

        for(int i = 0; i < buildingData.settingsKeys_String.Length; i++)
            settingsDict_String[buildingData.settingsKeys_String[i]] = buildingData.settingsValues_String[i];
        for (int i = 0; i < buildingData.settingsKeys_UInt.Length; i++)
            settingsDict_UInt[buildingData.settingsKeys_UInt[i]] = buildingData.settingsValues_UInt[i];
        for (int i = 0; i < buildingData.settingsKeys_Float.Length; i++)
            settingsDict_Float[buildingData.settingsKeys_Float[i]] = buildingData.settingsValues_Float[i];
        for (int i = 0; i < buildingData.settingsKeys_Bool.Length; i++)
            settingsDict_Bool[buildingData.settingsKeys_Bool[i]] = buildingData.settingsValues_Bool[i];

        transform.position = new Vector3(settingsDict_Float["X-Position"], 0, settingsDict_Float["Y-Position"]);
        transform.rotation = Quaternion.AngleAxis(settingsDict_Float["Rotation"], Vector3.up);
        roofPartsContainer.gameObject.SetActive(!settingsDict_Bool["Flat Roof"]);
    }

    public BuildingData ToBuildingData() { return new BuildingData(buildingType, settingsDict_String, settingsDict_UInt, settingsDict_Float, settingsDict_Bool); }
    public List<BoxCollider> GetChildrenColliders() { return childrenCol; }
    public void SetIsBeingPlaced(bool isBeingPlaced) { this.isBeingPlaced = isBeingPlaced; }
    public bool IsBeingPlaced() { return isBeingPlaced; }
    public void SetBuildingManager(BuildingManager buildingManager) { this.buildingManager = buildingManager; }
    public Dictionary<string, string> GetSettingsDict_String() { return settingsDict_String; }
    public Dictionary<string, float> GetSettingsDict_Float() { return settingsDict_Float; }
    public Dictionary<string, uint> GetSettingsDict_UInt() { return settingsDict_UInt; }
    public Dictionary<string, bool> GetSettingsDict_Bool() { return settingsDict_Bool; }
    public float GetArea() { return MathUtils.GetAreaOfPolygon(AsPolygon()); }
}

[Serializable]
public class BuildingData
{
    // must instantiate prefab according to type.
    public Building.BuildingType buildingType;

    // can't use Dictionaries, because it's not serializable
    public string[] settingsKeys_String;
    public string[] settingsValues_String;

    public string[] settingsKeys_UInt;
    public uint[] settingsValues_UInt;

    public string[] settingsKeys_Float;
    public float[] settingsValues_Float;

    public string[] settingsKeys_Bool;
    public bool[] settingsValues_Bool;

    public BuildingData(Building.BuildingType buildingType, Dictionary<string, string> settingsDict_String, Dictionary<string, uint> settingsDict_UInt,
        Dictionary<string, float> settingsDict_Float, Dictionary<string, bool> settingsDict_Bool)
    {
        this.buildingType = buildingType;

        settingsKeys_String = new string[settingsDict_String.Count];
        settingsValues_String = new string[settingsDict_String.Count];
        int i = 0;
        foreach (var setting in settingsDict_String) {
            settingsKeys_String[i] = setting.Key;
            settingsValues_String[i] = setting.Value;
            i++;
        }

        settingsKeys_UInt = new string[settingsDict_UInt.Count];
        settingsValues_UInt = new uint[settingsDict_UInt.Count];
        i = 0;
        foreach (var setting in settingsDict_UInt)
        {
            settingsKeys_UInt[i] = setting.Key;
            settingsValues_UInt[i] = setting.Value;
            i++;
        }

        settingsKeys_Float = new string[settingsDict_Float.Count];
        settingsValues_Float = new float[settingsDict_Float.Count];
        i = 0;
        foreach (var setting in settingsDict_Float)
        {
            settingsKeys_Float[i] = setting.Key;
            settingsValues_Float[i] = setting.Value;
            i++;
        }

        settingsKeys_Bool = new string[settingsDict_Bool.Count];
        settingsValues_Bool = new bool[settingsDict_Bool.Count];
        i = 0;
        foreach (var setting in settingsDict_Bool)
        {
            settingsKeys_Bool[i] = setting.Key;
            settingsValues_Bool[i] = setting.Value;
            i++;
        }
    }
}
