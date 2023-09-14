using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OtherUtils : MonoBehaviour
{
    public static void DestroyAllChildrenSafely(Transform parent)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent)
        {
            children.Add(child);
        }
        foreach (Transform child in children)
            Destroy(child.gameObject);
    }

    public static void DestroyChildrenSafely(Transform parent, int countLeft)
    {
        int cCount = parent.childCount;
        for (int i = 0; i < cCount - countLeft; i++)
            Destroy(parent.GetChild(cCount-1 - i).gameObject);
    }

    public static Vector3 GetMousePositionOnXZ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (ray.direction.y != 0f)
        {
            float t = -ray.origin.y / ray.direction.y;
            Vector3 point = ray.origin + t * ray.direction;
            point.y = 0f;
            return point;
        }
        return Vector3.negativeInfinity;
    }
}