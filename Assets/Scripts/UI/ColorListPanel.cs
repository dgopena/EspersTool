using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ColorListPanel : ListPanel
{
    public new delegate void EntryAction(int entryIndex);
    public new event EntryAction OnEntryClick;

    public void BuildPanel(Vector3 origin, ColorManager.ColorSet[] entries, bool lockToCanvas = true)
    {
        canvasRT = transform.parent.GetComponent<RectTransform>();

        Color panelColor = listColor;
        panelColor.a = 0.6f;
        GetComponent<Image>().color = panelColor;

        Vector3 deltaPos = Vector3.zero;

        panelWidth = screenProportionSize.x * canvasRT.rect.width;
        entryHeight = entryScreenHeightProportion * canvasRT.rect.height;

        float initialHeight = (entries.Length * (entryHeight + entrySeparation)) + entrySeparation;

        panelHeight = screenProportionSize.y * canvasRT.rect.height;
        if (initialHeight < panelHeight)
            panelHeight = initialHeight;

        if (lockToCanvas)
        {
            Vector2 localPoint = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, origin, cam, out localPoint);

            if (localPoint.x < ((1f - widthLimits.x) * -0.5f * canvasRT.sizeDelta.x))
            {
                ShowPanel(false);
                return;
            }
            else if (localPoint.x > (widthLimits.y * 0.5f * canvasRT.sizeDelta.x))
            {
                ShowPanel(false);
                return;
            }
            else if (localPoint.y < ((1f - heightLimits.x) * -0.5f * canvasRT.sizeDelta.y))
            {
                ShowPanel(false);
                return;
            }
            else if (localPoint.y > (heightLimits.y * 0.5f * canvasRT.sizeDelta.y))
            {
                ShowPanel(false);
                return;
            }

            //we correct positioning

            //right
            if ((panelWidth + localPoint.x) > (widthLimits.y * 0.5f * canvasRT.sizeDelta.x))
                deltaPos.x = -panelWidth;

            //down
            if ((localPoint.y - panelHeight) < ((1f - heightLimits.x) * -0.5f * canvasRT.sizeDelta.y))
                deltaPos.y = ((1f - heightLimits.x) * -0.5f * canvasRT.sizeDelta.y) - (localPoint.y - panelHeight);
        }

        transform.position = origin + deltaPos;

        panelRT = GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(panelWidth, panelHeight);

        BuildList(entries);
    }

    public void BuildList(ColorManager.ColorSet[] entries)
    {
        //clean list
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        float posY = 0.5f * entrySeparation;
        for (int i = 0; i < entries.Length; i++)
        {
            GameObject nuEntry = Instantiate<GameObject>(entryPrefab, contentParent);
            RectTransform entryRT = nuEntry.GetComponent<RectTransform>();
            //entryRT.anchoredPosition = Vector2.zero;
            nuEntry.GetComponent<Image>().color = listColor;

            Vector3 pos = entryRT.anchoredPosition;
            pos.y = -posY;
            entryRT.anchoredPosition = pos;

            entryRT.GetChild(1).GetComponent<Image>().color = entries[i].color;
            entryRT.GetChild(2).GetComponent<TextMeshProUGUI>().text = entries[i].name;
            int entryIndex = i;
            entryRT.GetComponent<HoldButton>().onRelease.AddListener(delegate
            {
                if (OnEntryClick != null)
                {
                    OnEntryClick(entryIndex);
                }
            });

            Vector2 sd = entryRT.sizeDelta;
            sd.y = entryHeight;
            entryRT.sizeDelta = sd;

            posY += entryHeight + ((i == (entries.Length - 1) ? 0.5f : 1f) * entrySeparation);
            nuEntry.SetActive(true);
        }

        Vector2 csd = contentParent.sizeDelta;
        csd.y = posY;
        contentParent.sizeDelta = csd;
    }
}
