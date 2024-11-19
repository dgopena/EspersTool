using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class TabbedContent : MonoBehaviour
{
    private RectTransform containerRT;

    [SerializeField] private TabLocation tabType = TabLocation.UpSide;

    public List<TabElement> tabs;

    private CanvasGroup currentFocusedTabGroup;
    private int currentTab = 0;

    [Range(0.1f,0.95f)]
    public float tabSizeFactor = 0.9f;
    [Range(0.05f, 1.95f)]
    public float tabFrameColorDarken = 0.9f;

    [System.Serializable]
    public class TabElement
    {
        public RectTransform pageRect;
        public RectTransform tabRect;
        public RectTransform pageContent;
        public RectTransform tabContent;
        public Color tabColor = Color.gray;

        public Button.ButtonClickedEvent OnTabClick;
    }

    public enum TabLocation
    {
        UpSide,
        DownSide,
        LeftSide,
        RightSide
    }

    public void Awake()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.LogError("TabbedContent component needs an eventsystem element on scene to work properly.");
        }

        BuildTabs();
    }

    public void BuildTabs()
    {
        containerRT = GetComponent<RectTransform>();
        Rect containerRect = containerRT.rect;

        float containerWidth = containerRect.width;
        float containerHeight = containerRect.height;

        float tabWidth = containerWidth / (float)tabs.Count;
        float tabHeight = containerHeight / (float)tabs.Count;

        for (int i = 0; i < tabs.Count; i++)
        {
            RectTransform tabRT = tabs[i].tabRect;
            Color frameColor = tabFrameColorDarken * tabs[i].tabColor;
            frameColor.a = 1f;
            tabRT.GetComponent<Image>().color = frameColor;
            tabRT.GetChild(0).GetComponent<Image>().color = tabs[i].tabColor;

            if (tabType == TabLocation.UpSide || tabType == TabLocation.DownSide)
            {
                Vector2 sd = tabRT.sizeDelta;
                sd.x = tabSizeFactor * tabWidth;
                tabRT.sizeDelta = sd;

                Vector2 tabPos = tabRT.anchoredPosition;
                tabPos.x = (0.5f * (1 - tabSizeFactor) * tabWidth) + (i * tabWidth);
                tabRT.anchoredPosition = tabPos;
            }
            else if(tabType == TabLocation.RightSide || tabType == TabLocation.LeftSide)
            {
                Vector2 sd = tabRT.sizeDelta;
                sd.y = tabSizeFactor * tabHeight;
                tabRT.sizeDelta = sd;

                Vector2 tabPos = tabRT.anchoredPosition;
                tabPos.y = ((0.5f * (tabSizeFactor - 1) * tabHeight) - (i * tabHeight)) - sd.y;
                tabRT.anchoredPosition = tabPos;
            }

            RectTransform pageRT = tabs[i].pageRect;
            pageRT.GetComponent<Image>().color = frameColor;
            pageRT.GetChild(1).GetComponent<Image>().color = tabs[i].tabColor;

            HoldButton tabButton = tabRT.GetComponent<HoldButton>();
            int tabIndex = i;
            tabButton.onRelease.AddListener(delegate { TabClick(tabIndex); });

            pageRT.SetAsFirstSibling();

            CanvasGroup cg = tabs[i].pageContent.GetComponent<CanvasGroup>();
            cg.interactable = (i == 0);

            if (i == 0)
            {
                currentFocusedTabGroup = cg;
            }

            if(i == 0)
                tabs[i].pageContent.gameObject.SetActive(true);
            else
                tabs[i].pageContent.gameObject.SetActive(false);
        }
    }

    public void TabClick(int index)
    {
        currentFocusedTabGroup.interactable = false;
        if(currentTab >= 0)
        {
            tabs[currentTab].pageContent.gameObject.SetActive(false);
        }
        tabs[index].pageRect.SetAsLastSibling();
        currentFocusedTabGroup = tabs[index].pageContent.GetComponent<CanvasGroup>();
        currentFocusedTabGroup.interactable = true;

        currentTab = index;
        tabs[currentTab].pageContent.gameObject.SetActive(true);

        if (tabs[currentTab].OnTabClick != null)
            tabs[currentTab].OnTabClick.Invoke();
    }

    public void SetContentRectActive(bool value)
    {

        currentFocusedTabGroup.GetComponent<RectTransform>().gameObject.SetActive(value);
    }
}
