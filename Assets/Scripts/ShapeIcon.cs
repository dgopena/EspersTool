using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using UnityEngine.UI;

public class ShapeIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public PointerEventDataEvent OnPointerClickEvent;
    public PointerEventDataEvent OnPointerEnterEvent;
    public PointerEventDataEvent OnPointerExitEvent;
    public PointerEventDataEvent OnPointerDownEvent;
    public PointerEventDataEvent OnPointerUpEvent;

    public void OnPointerClick(PointerEventData eventData)
    {

        OnPointerClickEvent?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        OnPointerEnterEvent?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Can call OnPointerExit events even if exceeded distance
        OnPointerExitEvent?.Invoke(eventData);
    }


    public void OnPointerDown(PointerEventData eventData)
    {

        OnPointerDownEvent?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Can call OnPointerUp events even if exceeded distance
        OnPointerUpEvent?.Invoke(eventData);
    }

    /// <summary>
    /// A UnityEvent with a Vector3 as a parameter
    /// </summary>
    [System.Serializable]
    public class PointerEventDataEvent : UnityEvent<UnityEngine.EventSystems.PointerEventData> { }
}
