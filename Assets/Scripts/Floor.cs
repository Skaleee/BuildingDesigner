using Sebastian.Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Floor : MonoBehaviour
{
    static Floor instance = null;

    [SerializeField]
    GameObject outerLine,
        innerFloor;
    [SerializeField]
    GameObject outerLinePiece;
    //[SerializeField]
    //MeshCollider innerFloorCollider;
    MeshFilter innerFloorMf;

    [SerializeField]
    Material redFloorMat, blueFloorMat;

    void Awake()
    {
        instance = this;

        //OuterLineMf = OuterLine.GetComponent<MeshFilter>();
        innerFloorMf = innerFloor.GetComponent<MeshFilter>();
        //OuterLineMc = OuterLine.GetComponent<MeshCollider>();
        //InnerFloorMc = InnerFloor.GetComponent<MeshCollider>();
    }

    void Start()
    {
        RefreshMesh();
    }

    public void RefreshMesh()
    {
        //if (CoreController.GetInstance().GetPolygon().Length == 0)
        //{
        //OuterLineMf.mesh.Clear();
        //while (OuterLine.transform.childCount > 0)
        //        Destroy(OuterLine.transform.GetChild(0).gameObject);
        OtherUtils.DestroyAllChildrenSafely(outerLine.transform);

        innerFloorMf.mesh.Clear();
        //return;
        //}
        if (CoreController.GetInstance().GetPolygon().Length > 0)
        {
            SetOuterLineMesh(CoreController.GetInstance().GetPolygon(), .1f, -0.25f, 0.3f, CoreController.GetInstance().GetPolygonLineColors(), true);
            SetInnerFloorMesh(CoreController.GetInstance().GetPolygon(), 0, -0.25f);
        }
    }

    /***
     * polygon is a circular array of 2d points (last point != first point).
     */
    void SetOuterLineMesh(Vector2[] polygon, float topY, float bottomY, float lineThickness, HashSet<int> polygonLineColors, bool createInnerWalls = false)
    {
        /* simple lines
         * for(int i = 1; i < polygon.Length; i++)
        {
            GameObject lineObj = Instantiate(OutlinePiece);
            lineObj.transform.SetParent(transform);

            Vector2 currentVector = polygon[i] - polygon[i-1];
            lineObj.transform.localScale = new Vector3(currentVector.magnitude, lineObj.transform.localScale.y, lineObj.transform.localScale.z);
            Vector3 newPosition = new Vector3(polygon[i-1].x + currentVector.x / 2, 0, polygon[i-1].y + currentVector.y / 2);
            lineObj.transform.position = newPosition;

            lineObj.transform.rotation = Quaternion.Euler(0, -Mathf.Atan(currentVector.y/currentVector.x) * Mathf.Rad2Deg, 0);
        }
        GameObject lastLineObj = Instantiate(OutlinePiece);
        lastLineObj.transform.parent = transform;
        Vector2 lastVector = polygon[0] - polygon[polygon.Length - 1];
        lastLineObj.transform.localScale = new Vector3(lastVector.magnitude+lastLineObj.transform.localScale.y, lastLineObj.transform.localScale.y, lastLineObj.transform.localScale.z);
        Vector3 lastPosition = new Vector3(polygon[polygon.Length - 1].x + lastVector.x / 2, 0, polygon[polygon.Length - 1].y + lastVector.y / 2);
        lastLineObj.transform.position = lastPosition;

        lastLineObj.transform.rotation = Quaternion.Euler(0, -Mathf.Atan(lastVector.y / lastVector.x) * Mathf.Rad2Deg, 0);*/

         
        // build only one mesh
        /*
        Vector2[] outerPolygon = MathUtils.GeneratePolygonAround(polygon, lineThickness);

        Vector3[] vertices = new Vector3[polygon.Length * 2 * 2]; // pointcount * 2 rounds of points * 2 layers
        //top vertices
        for (int i = 0; i < polygon.Length; i++) vertices[i] = new Vector3(polygon[i].x, topY, polygon[i].y);
        for (int i = 0; i < polygon.Length; i++) vertices[i + polygon.Length] = new Vector3(outerPolygon[i].x, topY, outerPolygon[i].y);
        //bottom vertices
        for (int i = 0; i < polygon.Length; i++) vertices[i + polygon.Length * 2] = new Vector3(polygon[i].x, bottomY, polygon[i].y);
        for (int i = 0; i < polygon.Length; i++) vertices[i + polygon.Length * 3] = new Vector3(outerPolygon[i].x, bottomY, outerPolygon[i].y);

        int[] triangles = new int[polygon.Length * 2 * 3 * (createInnerWalls ? 4 : 3)]; // linecount * 2 trigs per line * 3 points per trig * (3 or 4) sides per line
        int tmp = 0;
        for (int i = 1; i < polygon.Length; i++)
        {
            //top
            triangles[tmp++] = i - 1;
            triangles[tmp++] = i - 1 + polygon.Length;
            triangles[tmp++] = i;

            triangles[tmp++] = i - 1 + polygon.Length;
            triangles[tmp++] = i + polygon.Length;
            triangles[tmp++] = i;
            //bottom
            triangles[tmp++] = i - 1 + polygon.Length * 3;
            triangles[tmp++] = i - 1 + polygon.Length*2;
            triangles[tmp++] = i + polygon.Length * 2;

            triangles[tmp++] = i + polygon.Length * 2;
            triangles[tmp++] = i + polygon.Length * 3;
            triangles[tmp++] = i - 1 + polygon.Length * 3;
            //outer
            triangles[tmp++] = i - 1 + polygon.Length;
            triangles[tmp++] = i - 1 + polygon.Length * 3;
            triangles[tmp++] = i + polygon.Length;

            triangles[tmp++] = i - 1 + polygon.Length * 3;
            triangles[tmp++] = i + polygon.Length * 3;
            triangles[tmp++] = i + polygon.Length;
            //inner
            if (createInnerWalls)
            {
                triangles[tmp++] = i - 1 + polygon.Length * 2;
                triangles[tmp++] = i - 1;
                triangles[tmp++] = i;

                triangles[tmp++] = i;
                triangles[tmp++] = i + polygon.Length * 2;
                triangles[tmp++] = i - 1 + polygon.Length * 2;
            }
        }
        // last iteration
        //top
        triangles[tmp++] = polygon.Length - 1;
        triangles[tmp++] = polygon.Length - 1 + polygon.Length;
        triangles[tmp++] = 0;

        triangles[tmp++] = polygon.Length-1 + polygon.Length;
        triangles[tmp++] = 0 + polygon.Length;
        triangles[tmp++] = 0;
        //bottom
        triangles[tmp++] = polygon.Length - 1 + polygon.Length * 3;
        triangles[tmp++] = polygon.Length - 1 + polygon.Length * 2;
        triangles[tmp++] = 0 + polygon.Length * 2;

        triangles[tmp++] = 0 + polygon.Length * 2;
        triangles[tmp++] = 0 + polygon.Length * 3;
        triangles[tmp++] = polygon.Length - 1 + polygon.Length * 3;
        //outer
        triangles[tmp++] = polygon.Length - 1 + polygon.Length;
        triangles[tmp++] = polygon.Length - 1 + polygon.Length * 3;
        triangles[tmp++] = 0 + polygon.Length;

        triangles[tmp++] = polygon.Length - 1 + polygon.Length * 3;
        triangles[tmp++] = 0 + polygon.Length * 3;
        triangles[tmp++] = 0 + polygon.Length;
        //inner
        if (createInnerWalls)
        {
            triangles[tmp++] = polygon.Length - 1 + polygon.Length * 2;
            triangles[tmp++] = polygon.Length - 1;
            triangles[tmp++] = 0;

            triangles[tmp++] = 0;
            triangles[tmp++] = 0 + polygon.Length * 2;
            triangles[tmp++] = polygon.Length - 1 + polygon.Length * 2;
        }

        OuterLineMf.mesh.Clear();
        OuterLineMf.mesh.vertices = vertices;
        OuterLineMf.mesh.triangles = triangles;
        OuterLineMf.mesh.RecalculateNormals();
        OuterLineMf.mesh.RecalculateBounds();

        OuterLineMc.sharedMesh = OuterLineMf.mesh;
        */
        //build a mesh for every line
        Vector2[] outerPolygon = MathUtils.GeneratePolygonAround(polygon, lineThickness);

        Vector3[] vertices;
        int[] triangles;
        int tmp = 0;
        GameObject piece;
        MeshFilter pieceMf;
        for (int i = 1; i < polygon.Length; i++)
        {
            // build line piece
            vertices = new Vector3[8];
            // top vertices
            vertices[0] = new Vector3(polygon[i - 1].x, topY, polygon[i - 1].y);
            vertices[1] = new Vector3(polygon[i].x, topY, polygon[i].y);
            vertices[2] = new Vector3(outerPolygon[i - 1].x, topY, outerPolygon[i - 1].y);
            vertices[3] = new Vector3(outerPolygon[i].x, topY, outerPolygon[i].y);
            // bottom vertices
            vertices[4] = new Vector3(polygon[i - 1].x, bottomY, polygon[i - 1].y);
            vertices[5] = new Vector3(polygon[i].x, bottomY, polygon[i].y);
            vertices[6] = new Vector3(outerPolygon[i - 1].x, bottomY, outerPolygon[i - 1].y);
            vertices[7] = new Vector3(outerPolygon[i].x, bottomY, outerPolygon[i].y);

            triangles = new int[3 * (2 * 3 + (createInnerWalls ? 2 : 0))]; // 3 points per triangle * (2 tiangles * 4 sides (pieces are connected on 2 sides, inner side may be disabled))
            tmp = 0;

            //top
            triangles[tmp++] = 1;
            triangles[tmp++] = 0;
            triangles[tmp++] = 2;

            triangles[tmp++] = 3;
            triangles[tmp++] = 1;
            triangles[tmp++] = 2;
            //bottom
            triangles[tmp++] = 2 + 4;
            triangles[tmp++] = 0 + 4;
            triangles[tmp++] = 1 + 4;

            triangles[tmp++] = 2 + 4;
            triangles[tmp++] = 1 + 4;
            triangles[tmp++] = 3 + 4;
            //outer
            triangles[tmp++] = 3;
            triangles[tmp++] = 2;
            triangles[tmp++] = 7;

            triangles[tmp++] = 2;
            triangles[tmp++] = 6;
            triangles[tmp++] = 7;
            //inner
            if (createInnerWalls)
            {
                triangles[tmp++] = 5;
                triangles[tmp++] = 0;
                triangles[tmp++] = 1;

                triangles[tmp++] = 4;
                triangles[tmp++] = 0;
                triangles[tmp++] = 5;
            }

            piece = Instantiate(outerLinePiece, outerLine.transform);
            pieceMf = piece.GetComponent<MeshFilter>();
            pieceMf.mesh.Clear();
            pieceMf.mesh.vertices = vertices;
            pieceMf.mesh.triangles = triangles;
            pieceMf.mesh.RecalculateNormals();
            pieceMf.mesh.RecalculateBounds();
            if(polygonLineColors.Contains(i-1))
            {
                piece.GetComponent<MeshRenderer>().material = redFloorMat;
            }
            piece.GetComponent<MeshCollider>().sharedMesh = pieceMf.mesh;
        }

        // last iteration
        vertices = new Vector3[8];
        // top vertices
        vertices[0] = new Vector3(polygon[polygon.Length - 1].x, topY, polygon[polygon.Length - 1].y);
        vertices[1] = new Vector3(polygon[0].x, topY, polygon[0].y);
        vertices[2] = new Vector3(outerPolygon[polygon.Length - 1].x, topY, outerPolygon[polygon.Length - 1].y);
        vertices[3] = new Vector3(outerPolygon[0].x, topY, outerPolygon[0].y);
        // bottom vertices
        vertices[4] = new Vector3(polygon[polygon.Length - 1].x, bottomY, polygon[polygon.Length - 1].y);
        vertices[5] = new Vector3(polygon[0].x, bottomY, polygon[0].y);
        vertices[6] = new Vector3(outerPolygon[polygon.Length - 1].x, bottomY, outerPolygon[polygon.Length - 1].y);
        vertices[7] = new Vector3(outerPolygon[0].x, bottomY, outerPolygon[0].y);

        triangles = new int[3 * (2 * 3 + (createInnerWalls ? 2 : 0))]; // 3 points per triangle * (2 tiangles * 4 sides (pieces are connected on 2 sides, inner side may be disabled))
        tmp = 0;

        //top
        triangles[tmp++] = 1;
        triangles[tmp++] = 0;
        triangles[tmp++] = 2;

        triangles[tmp++] = 3;
        triangles[tmp++] = 1;
        triangles[tmp++] = 2;
        //bottom
        triangles[tmp++] = 2 + 4;
        triangles[tmp++] = 0 + 4;
        triangles[tmp++] = 1 + 4;

        triangles[tmp++] = 2 + 4;
        triangles[tmp++] = 1 + 4;
        triangles[tmp++] = 3 + 4;
        //outer
        triangles[tmp++] = 3;
        triangles[tmp++] = 2;
        triangles[tmp++] = 7;

        triangles[tmp++] = 2;
        triangles[tmp++] = 6;
        triangles[tmp++] = 7;
        //inner
        if (createInnerWalls)
        {
            triangles[tmp++] = 5;
            triangles[tmp++] = 0;
            triangles[tmp++] = 1;

            triangles[tmp++] = 4;
            triangles[tmp++] = 0;
            triangles[tmp++] = 5;
        }

        piece = Instantiate(outerLinePiece, outerLine.transform);
        pieceMf = piece.GetComponent<MeshFilter>();
        pieceMf.mesh.Clear();
        pieceMf.mesh.vertices = vertices;
        pieceMf.mesh.triangles = triangles;
        pieceMf.mesh.RecalculateNormals();
        pieceMf.mesh.RecalculateBounds();
        if (polygonLineColors.Contains(polygon.Length-1))
        {
            piece.GetComponent<MeshRenderer>().material = redFloorMat;
        }
        piece.GetComponent<MeshCollider>().sharedMesh = pieceMf.mesh;
    }

    void SetInnerFloorMesh(Vector2[] polygon, float topY, float bottomY)
    {
        Vector3[] vertices = new Vector3[polygon.Length * 2];
        for (int i = 0; i < polygon.Length; i++) vertices[i] = new Vector3(polygon[i].x, topY, polygon[i].y);
        for (int i = 0; i < polygon.Length; i++) vertices[i + polygon.Length] = new Vector3(polygon[i].x, bottomY, polygon[i].y);
        int[] trianglesTop = new Triangulator(new Polygon(polygon)).Triangulate();
        int[] trianglesBottom = trianglesTop.ToArray();

        for (int i = 0; i < trianglesTop.Length / 3; i++) 
        {
            // make the top triangles clockwise, so they render from the top. 
            int tmp = trianglesTop[i * 3];
            trianglesTop[i * 3] = trianglesTop[i * 3 + 1];
            trianglesTop[i * 3 + 1] = tmp;

            // make bottom triangles use the correct vertices
            trianglesBottom[i * 3] += polygon.Length;
            trianglesBottom[i * 3 + 1] += polygon.Length;
            trianglesBottom[i * 3 + 2] += polygon.Length;
        }

        innerFloorMf.mesh.Clear();
        innerFloorMf.mesh.vertices = vertices;
        innerFloorMf.mesh.triangles = trianglesTop.Concat(trianglesBottom).ToArray();
        innerFloorMf.mesh.RecalculateNormals();
        innerFloorMf.mesh.RecalculateBounds();

        //innerFloorCollider.sharedMesh = innerFloorMf.mesh;
    }

    public static Floor GetInstance() { return instance; }
}
