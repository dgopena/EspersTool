using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ListPanel : MonoBehaviour
{
    protected Camera cam;

    public Color listColor = Color.gray;

    //list panel needs to be a direct child of the main canvas
    [Space(20f)]
    protected RectTransform canvasRT;
    protected RectTransform panelRT;
    public RectTransform contentParent;
    public GameObject entryPrefab;

    public CanvasGroup canvasGroup;

    [Header("List Settings")]
    public Vector2 screenProportionSize = new Vector2(0.1f, 0.8f);
    public float entryScreenHeightProportion = 0.08f;
    public Vector2 widthLimits = new Vector2(0.1f, 0.9f);
    public Vector2 heightLimits = new Vector2(0.1f, 0.9f);

    public float entrySeparation = 10f;

    private Vector2 panelDeltaPos;

    protected float panelWidth;
    protected float panelHeight;

    protected float entryHeight;

    public delegate void EntryAction(int entryIndex);
    public event EntryAction OnEntryClick;

    //origin must be in screen size
    public void ShowPanel(Vector3 origin, List<string> entries, bool lockToCanvas = true)
    {
        BuildPanel(origin, entries, lockToCanvas);

        ShowPanel(true);
    }

    public void ShowPanel(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void BuildPanel(Vector3 origin, List<string> entries, bool lockToCanvas = true)
    {
        canvasRT = transform.parent.GetComponent<RectTransform>();

        Color panelColor = listColor;
        panelColor.a = 0.6f;
        GetComponent<Image>().color = panelColor;

        Vector3 deltaPos = Vector3.zero;

        panelWidth = screenProportionSize.x * canvasRT.rect.width;
        entryHeight = entryScreenHeightProportion * canvasRT.rect.height;

        float initialHeight = (entries.Count * (entryHeight + entrySeparation)) + entrySeparation;

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

    public virtual void BuildList(List<string> entries)
    {
        //clean list
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        float posY = 0.5f * entrySeparation;
        for (int i = 0; i < entries.Count; i++)
        {
            GameObject nuEntry = Instantiate<GameObject>(entryPrefab, contentParent);
            RectTransform entryRT = nuEntry.GetComponent<RectTransform>();
            //entryRT.anchoredPosition = Vector2.zero;
            nuEntry.GetComponent<Image>().color = listColor;

            Vector3 pos = entryRT.anchoredPosition;
            pos.y = -posY;
            entryRT.anchoredPosition = pos;

            entryRT.GetChild(1).GetComponent<TextMeshProUGUI>().text = entries[i];
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

            posY += entryHeight + ((i == (entries.Count - 1) ? 0.5f : 1f) * entrySeparation);
            nuEntry.SetActive(true);
        }

        Vector2 csd = contentParent.sizeDelta;
        csd.y = posY;
        contentParent.sizeDelta = csd;
    }
}
