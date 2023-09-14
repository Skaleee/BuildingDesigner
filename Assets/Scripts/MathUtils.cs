using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class MathUtils
{
    public static int CheckTurnType(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 v12 = p2 - p1;
        Vector2 v13 = p3 - p1;

        float crossMagnitude = v12.x * v13.y - v12.y * v13.x;
        if (Mathf.Approximately(crossMagnitude, 0))
            return 0; // no turn, same direction
        else if (crossMagnitude > 0)
            return -1; // left turn
        else
            return 1; // right turn
    }

    public static float GetAreaOfPolygon(Vector2[] polygon, bool signed = false)
    {
        // shoelace formula/Gauﬂsche Trapezformel
        float orientedArea = (polygon[polygon.Length - 1].x * polygon[0].y) - (polygon[0].x * polygon[polygon.Length - 1].y); // last point to first point

        for (int i = 0; i < polygon.Length - 1; i++)
        {
            float x1 = polygon[i].x;
            float y1 = polygon[i].y;
            float x2 = polygon[i + 1].x;
            float y2 = polygon[i + 1].y;

            orientedArea += (x1 * y2) - (x2 * y1);
        }

        if (signed)
            return orientedArea/2;
        else 
            return Mathf.Abs(orientedArea/2);
    }

    public static int CheckIfPolygonGoesClockwise(Vector2[] polygon)
    {
        /* check number of right/left turns
        int rightTurns = 0,
            leftTurns = 0;

        for (int i = 2; i < polygon.Length; i++)
        {
            int currentTurnType = CheckTurnType(polygon[i - 2], polygon[i - 1], polygon[i]);
            if (currentTurnType == -1) leftTurns++;
            else if (currentTurnType == 1) rightTurns++;
        }

        int extraTurnType = CheckTurnType(polygon[polygon.Length - 2], polygon[polygon.Length - 1], polygon[0]);
        if (extraTurnType == -1) leftTurns++;
        else if (extraTurnType == 1) rightTurns++;

        extraTurnType = CheckTurnType(polygon[polygon.Length - 1], polygon[0], polygon[1]);
        if (extraTurnType == -1) leftTurns++;
        else if (extraTurnType == 1) rightTurns++;

        if (rightTurns == leftTurns)
            return 0; // invalid polygon (might contain crossings)
        else if (rightTurns > leftTurns)
            return 1; // clockwise
        else return -1; // counter-clockwise*/

        float orientedArea = GetAreaOfPolygon(polygon, true);
        if (orientedArea > 0f)
            return -1; // counter-clockwise
        else //if (orientedArea < 0f)
            return 1;
        //else return 0;
    }

    public static Vector2 GetIntersectionPoint(Vector2 point1, Vector2 direction1, Vector2 point2, Vector2 direction2, bool firstLineInfinite = true)
    {
        float denominator = (direction2.y * direction1.x) - (direction2.x * direction1.y);

        if (Mathf.Approximately(denominator, 0f))
        {
            //Debug.Log("parallel: " + point1 + "" + direction1 + "" + point2 + "" + direction2);
            return Vector2.positiveInfinity; // parallel
        }

        float t = ((point2.x - point1.x) * direction2.y - (point2.y - point1.y) * direction2.x) / denominator;

        if (!firstLineInfinite)
            if (t <= 0 || t >= 1)
            {
                //Debug.Log("t<0||t>1: " + point1 + "" + direction1 + "" + point2 + "" + direction2);
                return Vector2.negativeInfinity; // no intersection
            }
        //Debug.Log("intersected: " + point1 + "" + direction1 + "" + point2 + "" + direction2);
        float intersectionX = point1.x + (t * direction1.x);
        float intersectionY = point1.y + (t * direction1.y);

        return new Vector2(intersectionX, intersectionY);
    }

    public static Vector2[] GeneratePolygonAround(Vector2[] polygon, float distance)
    {
        Vector2[] around = new Vector2[polygon.Length];

        int goesClockwise = 1;//CheckIfPolygonGoesClockwise(polygon); // always clockwise
        //if (goesClockwise == 0) { } // invalid polygon //not anymore?

        /* // distances taken from corners
        for(int i = 2; i < polygon.Length; i++) 
        {
            Vector2 middle = (polygon[i-2] + polygon[i]) / 2;

            Vector2 directionFromMiddle = polygon[i-1] - middle;

            int currentTurnType = CheckTurnType(polygon[i - 2], polygon[i - 1], polygon[i]);
            if (currentTurnType == 0) continue; // need to generate point next to line orthogonal to p1 to p3?

            around[i-1] = middle + directionFromMiddle * (directionFromMiddle.magnitude + distance * goesClockwise*currentTurnType) / directionFromMiddle.magnitude;
        }*/

        // distances taken from edges. Points are the intersections between the distanced lines.
        Vector2 lastDirection = polygon[0] - polygon[polygon.Length - 1];
        Vector2 lastDirectionOrthogonal = new Vector2(-1 * goesClockwise * lastDirection.y, goesClockwise * lastDirection.x);
        Vector2 lastPoint = polygon[polygon.Length - 1] + (lastDirectionOrthogonal.normalized * distance);

        Vector2 veryLastPoint = lastPoint;
        Vector2 veryLastDirection = lastDirection;

        for (int i = 1; i < polygon.Length; i++)
        {
            Vector2 nextDirection = polygon[i] - polygon[i - 1];
            Vector2 nextDirectionOrthogonal = new Vector2(-1 * goesClockwise * nextDirection.y, goesClockwise * nextDirection.x);
            Vector2 nextPoint = polygon[i - 1] + (nextDirectionOrthogonal.normalized * distance);

            Vector2 vertex = GetIntersectionPoint(lastPoint, lastDirection.normalized, nextPoint, nextDirection.normalized);
            if (float.IsInfinity(vertex.x)) // must be parallel => just juse nextPoint as new Vertex
                around[i - 1] = nextPoint;
            else 
                around[i - 1] = vertex;

            lastPoint = nextPoint;
            lastDirection = nextDirection;
        }

        // do last point manually
        Vector2 lastVertex = GetIntersectionPoint(lastPoint, lastDirection.normalized, veryLastPoint, veryLastDirection.normalized);
        if (float.IsInfinity(lastVertex.x))
            around[polygon.Length - 1] = veryLastPoint;
        else
            around[polygon.Length - 1] = lastVertex;

        return around;
    }

    public static bool IsPointOnLine(Vector2 point, Vector2 lineStartPoint, Vector2 lineEndPoint)
    {
        float distanceToStart = Vector2.Distance(lineStartPoint, point);
        float distanceToEnd = Vector2.Distance(lineEndPoint, point);
        float length = Vector2.Distance(lineStartPoint, lineEndPoint);

        return Mathf.Approximately(distanceToStart + distanceToEnd, length);
    }

    public static bool IsPointInsidePolygon(Vector2 point, Vector2[] polygon)
    { // Raycast. Also allows the point to lie on the edges of polygon.
        bool inside = false;

        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            // check if point is left or right of lines
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
            if (IsPointOnLine(point, polygon[j], polygon[i]))
                return true;
        }

        return inside;
    }

    public static bool DoPolygonsIntersect(Vector2[] inner, Vector2[] outer) 
    { // checks intersection for every line in inner with every line in outer
        for (int i1 = 0, i0 = inner.Length - 1; i1 < inner.Length; i0 = i1++)
        {
            for (int o1 = 0, o0 = outer.Length - 1; o1 < outer.Length; o0 = o1++)
            {
                Vector2 s0 = GetIntersectionPoint(inner[i0], inner[i1] - inner[i0], outer[o0], outer[o1] - outer[o0], false);
                Vector2 s1 = GetIntersectionPoint(outer[o0], outer[o1] - outer[o0], inner[i0], inner[i1] - inner[i0], false);
                //if posInfinity then might be same exact line. Might need to check (togglable with a bool?), if the lines are shifted multiples.
                if (!float.IsInfinity(s0.x) &&
                    !float.IsInfinity(s1.x))
                    return true;
            }
        }
        return false;
    }

    public static bool IsPolygonInsidePolygon(Vector2[] inner, Vector2[] outer)
    { // also allows inner to touch the edges of outer. For example: if inner==outer then returns true
        // check if all points are inside
        foreach (Vector2 p in inner)
            if (!IsPointInsidePolygon(p, outer))
                return false;

        // check for line intersection
        return !DoPolygonsIntersect(inner, outer);
    }
}
