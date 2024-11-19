using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.Events;

using TMPro;

public class StatusIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string iconName { get; private set; }

    public CanvasGroup closeIcon;

    public int typeIndex { get; private set; }
    public int statusIndex { get; private set; }
    public int childIndex { get; private set; }
    public string description { get; private set; }

    public PointerEventDataEvent OnPointerEnterEvent;
    public PointerEventDataEvent OnPointerExitEvent;
    public PointerEventDataEvent OnPointerClickEvent;

    private StatusList parentList;

    private int clickCounter;

    private bool withPointer = false;

    private void Awake()
    {

    }

    public void SetData(string name, int typeIndex, int statusIndex, int childIndex, string description, StatusList parentList)
    {
        iconName = name;
        this.typeIndex = typeIndex;
        this.statusIndex = statusIndex;
        this.childIndex = childIndex;
        this.parentList = parentList;
        this.description = description;
    }

    public void SetChildIndex(int childIndex)
    {
        this.childIndex = childIndex;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        /*
        //closeIcon.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = iconName;
        closeIcon.alpha = 0.8f;

        string buildDesc = "<size=120%><b>" + MiscTools.GetSpacedForm(iconName) + "</b>\n\n<size=100%>" + description;

        parentList.ChangeDescription(buildDesc);

        OnPointerEnterEvent?.Invoke(eventData);
        */

        withPointer = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        /*
        closeIcon.alpha = 0f;

        parentList.ChangeDescription("");

        // Can call OnPointerExit events even if exceeded distance
        OnPointerExitEvent?.Invoke(eventData);
        */

        withPointer = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(clickCounter == 0)
        {
            parentList.ClearIconsClickCounter();
            closeIcon.alpha = 0.8f;

            string buildDesc = "<size=120%><b>" + MiscTools.GetSpacedForm(iconName) + "</b>";
            if(!parentList.titleOnly)
                buildDesc += "\n\n<size=100%>" + description;

            parentList.ChangeDescription(buildDesc);
            clickCounter++;
        }
        else if (clickCounter == 1)
        {
            clickCounter = 0;
            closeIcon.alpha = 0f;

            parentList.ChangeDescription("");

            parentList.RemoveIcon(childIndex);
        }
    }

    public void TryClearing()
    {
        if (!withPointer)
        {
            ClearClickCounter();

            parentList.ChangeDescription("");
        }
    }

    public void ClearClickCounter()
    {
        clickCounter = 0;
        closeIcon.alpha = 0f;
    }

    [System.Serializable]
    public class PointerEventDataEvent : UnityEvent<UnityEngine.EventSystems.PointerEventData> { }
}
