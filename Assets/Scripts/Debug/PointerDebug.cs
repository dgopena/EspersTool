using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class PointerDebug : StandaloneInputModule
{
    public bool show = false;

    // Update is called once per frame
    void LateUpdate()
    {
        if (show)
        {
            GameObject pointedAt = GameObjectUnderPointer();

            if (pointedAt != null) {
                Debug.Log("[PointerDebug]: Pointing at " + pointedAt.name);
            }
        }
    }

    public GameObject GameObjectUnderPointer(int pointerId)
    {
        var lastPointer = GetLastPointerEventData(pointerId);
        if (lastPointer != null)
            return lastPointer.pointerCurrentRaycast.gameObject;
        return null;
    }

    public GameObject GameObjectUnderPointer()
    {
        return GameObjectUnderPointer(kMouseLeftId);
    }
}
