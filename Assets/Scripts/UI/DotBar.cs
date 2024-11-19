using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DotBar : MonoBehaviour, IPointerClickHandler
{
    private RectTransform clickArea;

    public int value { get; private set; }

    public delegate void BarEvents(int barChildIndex);
    public event BarEvents BarUpdate;

    //bar can't go under this value
    public int baseValue { get; private set; }

    public int barID { get; private set; }

    public bool interactable = true;

    private void Awake()
    {
        //SetAtZero();
    }

    public void OnPointerClick(PointerEventData pointerData)
    {
        if (!gameObject.activeInHierarchy || !interactable)
            return;

        if (clickArea == null)
            clickArea = transform.GetComponent<RectTransform>();

        Vector2 mouseAreaLocalPosition = clickArea.InverseTransformPoint(Input.mousePosition);
        if (clickArea.rect.Contains(mouseAreaLocalPosition))
        {
            Vector2 size = clickArea.rect.size;

            float wFrac = (mouseAreaLocalPosition.x + (0.5f * size.x)) / size.x;
            //float hFrac = mouseAreaLocalPosition.y / size.y;

            for (int i = 0; i < transform.childCount; i++)
            {
                if(wFrac < ((float)(i + 1) / (float)transform.childCount))
                {
                    ClickAtIndex(i);
                    break;
                }
            }
        }
    }

    private void ClickAtIndex(int index)
    {
        int auxValue = value;
        if (index >= value)
            auxValue++;
        else if (index <= value)
            auxValue--;

        SetBarValue(auxValue);
    }

    private void UpdateDots()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetChild(0).gameObject.SetActive(i < value);
            Color dotColor = transform.GetChild(i).GetChild(0).GetComponent<Image>().color;
            dotColor.a = (i < baseValue) ? 0.3f : 0.6f;
            transform.GetChild(i).GetChild(0).GetComponent<Image>().color = dotColor;
        }

        if (BarUpdate != null)
            BarUpdate(barID);
    }

    public void SetBarStartValues(int value, int baseValue)
    {
        this.baseValue = baseValue;
        this.baseValue = Mathf.Clamp(baseValue, 0, transform.childCount);

        value = Mathf.Clamp(value, baseValue, transform.childCount);
        this.value = value;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetChild(0).gameObject.SetActive(i < value);
            Color dotColor = transform.GetChild(i).GetChild(0).GetComponent<Image>().color;
            dotColor.a = (i < baseValue) ? 0.3f : 0.6f;
            transform.GetChild(i).GetChild(0).GetComponent<Image>().color = dotColor;
        }
    }

    public void SetBarValue(int value)
    {
        value = Mathf.Clamp(value, baseValue, transform.childCount);

        this.value = value;
        UpdateDots();
    }

    public void SetAtZero()
    {
        SetBarValue(0);
    }

    public void SetBaseValue(int val)
    {
        baseValue = val;
        baseValue = Mathf.Clamp(baseValue, 0, transform.childCount);

        SetBarValue(value);
    }

    public void SetBarID(int ID)
    {
        barID = ID;
    }
}
