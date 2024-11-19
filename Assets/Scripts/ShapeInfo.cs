using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeInfo : MonoBehaviour
{
    public Transform boundPoints;

    private RectTransform decorMarker;

    public bool isDecor { get; private set; }

    public int shapeID { get; private set; }

    public Color shapeColor { get; private set; }

    private void LateUpdate()
    {
        if (!isDecor && decorMarker != null)
        {
            decorMarker.gameObject.SetActive(false);
        }
        else if (isDecor && decorMarker == null)
        {
            GameObject nuDecMarker = Instantiate<GameObject>(ShapesManager._instance.decorMarkerPrefab);
            nuDecMarker.transform.parent = ShapesManager._instance.decorMarkerPrefab.transform.parent;
            nuDecMarker.transform.localPosition = Vector3.zero;

            RectTransform ndmRT = nuDecMarker.GetComponent<RectTransform>();
            ndmRT.sizeDelta = ShapesManager._instance.decorMarkerSize * Screen.height * Vector2.one;

            decorMarker = ndmRT;
            nuDecMarker.SetActive(true);
        }
        else if(isDecor && decorMarker != null)
        {
            if (MapManager._instance.toolMode == MapManager.ToolMode.ObjectEditor)
            {
                if (!decorMarker.gameObject.activeInHierarchy)
                    decorMarker.gameObject.SetActive(true);

                Vector3 pos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(transform.position + (0.65f * new Vector3(0f, 2f, 0f)));
                decorMarker.position = pos;
            }
            else
            {
                if (decorMarker.gameObject.activeInHierarchy)
                    decorMarker.gameObject.SetActive(false);
            }
        }
    }

    public bool TryPosition(Vector3 displacement, bool apply = true)
    {
        Vector3 prevPos = transform.position;
        transform.position += displacement;
        if (AtLeastOneInBounds())
        {
            if (!apply)
                transform.position = prevPos;
            return true;
        }
        transform.position = prevPos;
        return false;
    }

    public bool TryRotation(Vector3 axis, float angle, bool global, bool apply = true)
    {
        Quaternion prevRot = transform.rotation;
        transform.Rotate(axis, angle, global ? Space.World : Space.Self);

        if (AtLeastOneInBounds())
        {
            if (!apply)
                transform.rotation = prevRot;
            return true;
        }

        transform.rotation = prevRot;
        return false;
    }

    public bool TryScaling(Vector3 nuScale, bool apply = true)
    {
        Vector3 prevScl = transform.localScale;
        transform.localScale = nuScale;

        if (AtLeastOneInBounds())
        {
            if (!apply)
                transform.localScale = prevScl;
            return true;
        }

        transform.localScale = prevScl;
        return false;
    }

    private bool AtLeastOneInBounds()
    {
        for(int i = 0; i < boundPoints.childCount; i++)
        {
            if (InBounds(boundPoints.GetChild(i).position))
            {
                return true;
            }
        }

        return false;
    }

    private static bool InBounds(Vector3 point)
    {

        BattleMap.BoundSet bounds = MapManager._instance.mapTarget.bounds;
        if (point.x < bounds.minX)
            return false;
        else if (point.x > bounds.maxX)
            return false;
        else if (point.z < bounds.minZ)
            return false;
        else if (point.z > bounds.maxZ)
            return false;
        else if (point.y < bounds.floorY)
            return false;
        else if (point.y > bounds.ceilY)
            return false;

        return true;
    }

    public float GetRightMostValue()
    {
        float bestX = float.MinValue;
        for(int i = 0; i < boundPoints.childCount; i++)
        {
            float xValue = boundPoints.GetChild(i).position.x;
            if (xValue > bestX)
                bestX = xValue;
        }
        return bestX;
    }

    public float GetLeftMostValue()
    {
        float bestX = float.MaxValue;
        for (int i = 0; i < boundPoints.childCount; i++)
        {
            float xValue = boundPoints.GetChild(i).position.x;
            if (xValue < bestX)
                bestX = xValue;
        }
        return bestX;
    }

    public float GetUpMostValue()
    {
        float bestY = float.MinValue;
        for (int i = 0; i < boundPoints.childCount; i++)
        {
            float yValue = boundPoints.GetChild(i).position.y;
            if (yValue > bestY)
                bestY = yValue;
        }
        return bestY;
    }

    public float GetDownMostValue()
    {
        float bestY = float.MaxValue;
        for (int i = 0; i < boundPoints.childCount; i++)
        {
            float yValue = boundPoints.GetChild(i).position.y;
            if (yValue < bestY)
                bestY = yValue;
        }
        return bestY;
    }

    public float GetForwardMostValue()
    {
        float bestZ = float.MinValue;
        for (int i = 0; i < boundPoints.childCount; i++)
        {
            float zValue = boundPoints.GetChild(i).position.z;
            if (zValue > bestZ)
                bestZ = zValue;
        }
        return bestZ;
    }

    public float GetBackMostValue()
    {
        float bestZ = float.MaxValue;
        for (int i = 0; i < boundPoints.childCount; i++)
        {
            float zValue = boundPoints.GetChild(i).position.z;
            if (zValue < bestZ)
                bestZ = zValue;
        }
        return bestZ;
    }

    public void SetDecor(bool value)
    {
        isDecor = value;
    }

    public void SetShapeID(int id)
    {
        shapeID = id;
    }

    public void SetColor(Color c)
    {
        shapeColor = c;
    }

    public void DestroyMarker()
    {
        if (decorMarker != null)
            Destroy(decorMarker.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (boundPoints == null)
            return;

        Gizmos.color = Color.green;
        for(int i = 0; i < boundPoints.childCount; i++)
        {
            Gizmos.DrawSphere(boundPoints.GetChild(i).position, 0.1f);
        }
    }
}
