using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PointerEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{

    [Header("Maximium Distance")]
    [Tooltip("Maximum Distance this object can be from the UIPointer to be considered valid and receive events")]
    public float MaxDistance = 100f;

    [Header("Enable Events")]
    [Tooltip("If True then the Unity Events below will be sent. Set to False if you need to disable sending pointer events.")]
    public bool Enabled = true;

    [Header("Unity Events : ")]
    public PointerEventDataEvent OnPointerClickEvent;
    public PointerEventDataEvent OnPointerEnterEvent;
    public PointerEventDataEvent OnPointerExitEvent;
    public PointerEventDataEvent OnPointerDownEvent;
    public PointerEventDataEvent OnPointerUpEvent;

    [Header("Only register Exit if pointer completely out")]
    public bool exitOnTotalOut = false;
    private RectTransform rt;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // Don't call events if exceeded distance
        if (DistanceExceeded(eventData))
        {
            return;
        }

        OnPointerClickEvent?.Invoke(eventData);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // Don't call events if exceeded distance
        if (DistanceExceeded(eventData))
        {
            return;
        }

        OnPointerEnterEvent?.Invoke(eventData);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (exitOnTotalOut)
        {
            if (rt == null)
                rt = GetComponent<RectTransform>();

            if(!TooltipManager.CheckMouseInArea(rt))
                OnPointerExitEvent?.Invoke(eventData);

            return;
        }

        // Can call OnPointerExit events even if exceeded distance
        OnPointerExitEvent?.Invoke(eventData);
    }


    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // Don't call events if exceeded distance
        if (DistanceExceeded(eventData))
        {
            return;
        }
        
        OnPointerDownEvent?.Invoke(eventData);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        // Can call OnPointerUp events even if exceeded distance
        OnPointerUpEvent?.Invoke(eventData);
    }

    public virtual bool DistanceExceeded(PointerEventData eventData)
    {

        if (eventData == null)
        {
            return false;
        }

        if (eventData.pointerCurrentRaycast.distance > MaxDistance)
        {
            return true;
        }

        return false;
    }
}

/// <summary>
/// A UnityEvent with a Vector3 as a parameter
/// </summary>
[System.Serializable]
public class PointerEventDataEvent : UnityEvent<UnityEngine.EventSystems.PointerEventData> { }

