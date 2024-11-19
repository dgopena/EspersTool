using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiscTools
{
    public struct Segment
    {
        public Vector2 Start;
        public Vector2 End;
    }

    public static Vector2 LineIntersect(Segment AB, Segment CD)
    {
        float dACy = AB.Start.y - CD.Start.y;
        float dDCx = CD.End.x - CD.Start.x;
        float dACx = AB.Start.x - CD.Start.x;
        float dDCy = CD.End.y - CD.Start.y;
        float dBAx = AB.End.x - AB.Start.x;
        float dBAy = AB.End.y - AB.Start.y;

        float denominator = (dBAx * dDCy) - (dBAy * dDCx);
        float numerator = (dACy * dDCx) - (dACx * dDCy);

        if (denominator == 0)
        {
            if (numerator == 0)
            {
                //collinear
                if (AB.Start.x >= CD.Start.x && AB.Start.x <= CD.End.x) { return AB.Start; }
                else if (CD.Start.x >= AB.Start.x && CD.Start.x <= AB.End.x) { return CD.Start; }
                else { return Vector2.negativeInfinity; }
            }
            else
            {
                //parallel
                return Vector2.negativeInfinity;
            }
        }

        float r = numerator / denominator;
        if (r < 0 || r > 1) { return Vector2.negativeInfinity; }

        float s = ((dACy * dBAx) - (dACx * dBAy)) / denominator;
        if (s < 0 || s > 1) { return Vector2.negativeInfinity; }

        return new Vector2(AB.Start.x + (r * dBAx), AB.Start.y + (r * dBAy));
    }

    public static bool AreInstersecting(Segment AB, Segment CD)
    {
        Vector2 res = LineIntersect(AB, CD);
        return !float.IsNegativeInfinity(res.x);
    }

    public static bool RayTriangleIntersection(Vector3 v0, Vector3 v1, Vector3 v2, Ray ray, out Vector3 IntersectionPoint)
    {
        IntersectionPoint = Vector3.zero;

        Vector3 rayOrigin = ray.origin;
        Vector3 rayVector = ray.direction;

        const float EPSILON = 0.0000001f;
        Vector3 edge1, edge2, h, s, q;
        float a, f, u, v;

        edge1 = v1 - v0;
        edge2 = v2 - v0;
        h = Vector3.Cross(rayVector, edge2);
        a = Vector3.Dot(edge1, h);
        if (a > -EPSILON && a < EPSILON)
            return false;    // This ray is parallel to this triangle.
        f = 1.0f / a;
        s = rayOrigin - v0;
        u = f * Vector3.Dot(s, h);
        if (u < 0.0f || u > 1.0f)
            return false;
        q = Vector3.Cross(s, edge1);
        v = f * Vector3.Dot(rayVector, q);
        if (v < 0.0f || u + v > 1.0f)
            return false;
        // At this stage we can compute t to find out where the intersection point is on the line.
        float t = f * Vector3.Dot(edge2, q);
        if (t > EPSILON) // ray intersection
        {
            IntersectionPoint = rayOrigin + rayVector * t;
            return true;
        }
        else // This means that there is a line intersection but not a ray intersection.
            return false;
    }

    public static bool RayMeshIntersection(Mesh mesh, Ray ray)
    {
        for(int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 v0 = mesh.vertices[mesh.triangles[i]];
            Vector3 v1 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 v2 = mesh.vertices[mesh.triangles[i + 2]];

            Vector3 iPoint = Vector3.zero;
            if (RayTriangleIntersection(v0, v1, v2, ray, out iPoint))
                return true;
        }

        return false;
    }

    public static bool RayMeshClosestIntersection(Mesh mesh, Ray ray, out Vector3 IntersectionPoint)
    {
        IntersectionPoint = Vector3.negativeInfinity;
        float distanceToOrigin = float.MaxValue;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 v0 = mesh.vertices[mesh.triangles[i]];
            Vector3 v1 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 v2 = mesh.vertices[mesh.triangles[i + 2]];

            Vector3 iPoint = Vector3.zero;
            if (RayTriangleIntersection(v0, v1, v2, ray, out iPoint))
            {
                float dist = Vector3.Distance(iPoint, ray.origin);
                if (dist < distanceToOrigin)
                {
                    distanceToOrigin = dist;
                    IntersectionPoint = iPoint;
                }
            }
        }

        if(float.IsNegativeInfinity(IntersectionPoint.x))
            return false;
        else
        {
            return true;
        }
    }

    public static string GetSpacedForm(string entry)
    {
        string ret = "";

        for (int i = 0; i < entry.Length - 1; i++)
        {
            if (i > 0)
                ret += entry[i];
            else
                ret += Char.ToUpper(entry[i]);

            if (Char.IsLower(entry[i]))
            {
                if (Char.IsUpper(entry[i + 1]))
                    ret += ' ';
            }
        }

        ret += entry[entry.Length - 1];

        return ret;
    }

    public static string GetLineJumpedForm(string entry)
    {
        string auxText = entry;

        for (int c = 0; c < auxText.Length; c++)
        {
            if (((int)auxText[c] == 8226) || ((int)auxText[c] == 183))
            {
                string sub = auxText.Substring(0, c) + "\n ";
                auxText = auxText.Substring(c);
                auxText = sub + auxText;
                c += 3;
            }
        }

        return auxText;
    }

    public static void Shuffle<T>(this System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static string GetCellCoordinateForm(int i, int j)
    {
        int asciiAdd = 65;

        string letterPart = "";

        int xCoor = i;

        while (true)
        {
            int div = xCoor / 26;
            int res = xCoor % 26;

            letterPart = ((char)(res + asciiAdd)) + letterPart;

            if (div == 0)
                break;
            else
                xCoor = (div - 1);
        }

        return letterPart + j;
    }

    public static int CompareUnitsByName(IconUnit unit1, IconUnit unit2)
    {
        if (unit1 == null)
        {
            if (unit2 == null)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (unit2 == null)
            {
                return 1;
            }
            else
            {
                int retval = string.Compare(unit1.unitName, unit2.unitName, StringComparison.OrdinalIgnoreCase);

                return retval;
            }
        }
    }

    public static int CompareUnitsByNewerDate(IconUnit unit1, IconUnit unit2)
    {
        if (unit1 == null)
        {
            if (unit2 == null)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (unit2 == null)
            {
                return 1;
            }
            else
            {
                int retval = unit1.lastModified.CompareTo(unit2.lastModified);
                retval *= -1;

                return retval;
            }
        }
    }
}
