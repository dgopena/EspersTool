using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class PageStepper : MonoBehaviour
{
    [Header("Page Settings")]
    public PageStep[] pages;

    [Space(20f)]
    public float unDisplayedScaleFactor = 0.1f;

    public int currentPage { get; private set; }
    private int flipping = 0; //-1 - left, 0 - none, 1 - right
    [HideInInspector]public int eventFlipFlag = 0;
    private int fading = 0; //-1 - fade out, 0 - none, 1 - fade in

    private float startSize;
    private float endSize;
    private float currentFlipTime;
    private float currentFadeTime;

    private bool stepperSetup = false;

    private RectTransform parentRect;

    public float flipSpeed = 1f;
    [Range(0.1f, 0.9f)]
    public float fadeFraction = 0.1f;

    [System.Serializable]
    public struct PageStep
    {
        public string pageTabName;
        public string pageTabText;

        public RectTransform pageRect;
        public Color pageColor;
        public GameObject leftButton;
        public GameObject rightButton;

        public CanvasGroup displayedGroup;
        public CanvasGroup undisplayedTab;

        public Button.ButtonClickedEvent onPageStartPass;
        public Button.ButtonClickedEvent onPageFinishPass;
    }

    private void Start()
    {
        if(!stepperSetup)
            StepperSetup();
    }

    private void LateUpdate()
    {
        if(fading != 0)
        {
            currentFadeTime += Time.deltaTime;
            float t = currentFadeTime / (fadeFraction * flipSpeed);

            bool fadeDone = false;
            if(t > 1)
            {
                t = 1f;
                fadeDone = true;
            }

            if (fading < 0)
                pages[currentPage].displayedGroup.alpha = Mathf.Lerp(1f, 0f, t);
            else if (fading > 0)
                pages[currentPage].displayedGroup.alpha = Mathf.Lerp(0f, 1f, t);

            if (fadeDone)
            {
                if(fading < 0)
                {
                    pages[currentPage].displayedGroup.interactable = false;
                    pages[currentPage].displayedGroup.blocksRaycasts = false;
                    FlipPage(true);
                }
                else if(fading > 0)
                {
                    pages[currentPage].displayedGroup.interactable = true;
                    pages[currentPage].displayedGroup.blocksRaycasts = true;
                }
                fading = 0;
            }
        }

        if(flipping != 0)
        {
            if(parentRect == null)
                parentRect = GetComponent<RectTransform>();

            currentFlipTime += Time.deltaTime;
            float t = currentFlipTime / ((1f - fadeFraction) * flipSpeed);

            bool flipDone = false;
            if(t > 1)
            {
                t = 1f;
                flipDone = true;
            }

            int changingPage = currentPage;
            if (flipping < 0)
                changingPage -= 1;

            RectTransform pageRect = pages[changingPage].pageRect;
            Vector2 sd = pageRect.sizeDelta;
            sd.x = Mathf.Lerp(startSize, endSize, t);

            pageRect.sizeDelta = sd;

            if(flipping > 0)
            {
                pages[changingPage].undisplayedTab.alpha = t;
                pages[changingPage + 1].undisplayedTab.alpha = (1 - t);
            }
            else if(flipping < 0)
            {
                pages[changingPage].undisplayedTab.alpha = (1 - t);
                pages[changingPage + 1].undisplayedTab.alpha = t;
            }

            if (flipDone)
            {
                if (pages[changingPage].onPageFinishPass != null)
                    pages[changingPage].onPageFinishPass.Invoke();
                eventFlipFlag = 0;

                currentPage += flipping;
                pages[currentPage].displayedGroup.interactable = true;
                pages[currentPage].displayedGroup.blocksRaycasts = true;

                if (flipping < 0)
                {
                    fading = 1;
                    currentFadeTime = 0;
                }
                flipping = 0;
            }
        }
    }

    public void StepperSetup()
    {
        if (stepperSetup)
            return;

        parentRect = GetComponent<RectTransform>();

        float pageWidth = parentRect.rect.width * (1f - (unDisplayedScaleFactor));
        float pageHeight = parentRect.rect.height;

        for(int i = 0; i < pages.Length; i++)
        {
            //scales
            RectTransform pageRect = pages[i].pageRect;
            Vector2 sd = pageRect.sizeDelta;
            sd.x = pageWidth;
            pageRect.sizeDelta = sd;
            RectTransform tabRight = pages[i].undisplayedTab.GetComponent<RectTransform>();
            sd = tabRight.sizeDelta;
            sd.x = ((parentRect.rect.width) * unDisplayedScaleFactor) / (float)(pages.Length - 1);
            tabRight.sizeDelta = sd;

            //position
            pageRect.anchoredPosition = ((i) * sd.x) * Vector2.right;

            //color
            pageRect.GetComponent<Image>().color = pages[i].pageColor;

            pages[i].undisplayedTab.alpha = i > currentPage ? 1 : 0;

            pages[i].undisplayedTab.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pages[i].pageTabName;
            pages[i].undisplayedTab.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";

            //content
            pages[i].displayedGroup.interactable = i == currentPage;
            pages[i].displayedGroup.blocksRaycasts = i == currentPage;
            pages[i].displayedGroup.alpha = i >= currentPage ? 1 : 0;
            pages[i].leftButton.SetActive(i > 0);
            pages[i].rightButton.SetActive(i < (pages.Length - 1));

            pageRect.SetAsFirstSibling();
        }

        stepperSetup = true;
    }

    public void CallFlipPage(bool right)
    {
        if (!stepperSetup)
            return;

        if (flipping != 0)
            return;

        if (currentPage == 0 && !right)
            return;
        else if ((currentPage == (pages.Length - 1)) && right)
            return;

        int eventPageIdx = !right ? currentPage - 1 : currentPage;

        eventFlipFlag = right ? 1 : -1;
        if (pages[eventPageIdx].onPageStartPass != null)
            pages[eventPageIdx].onPageStartPass.Invoke();

        if (right)
        {
            fading = -1;
            currentFadeTime = 0;
        }
        else
            FlipPage(right);
    }

    private void FlipPage(bool right)
    {
        if (!stepperSetup)
            return;

        if (flipping != 0)
            return;

        if (currentPage == 0 && !right)
            return;
        else if ((currentPage == (pages.Length - 1)) && right)
            return;

        flipping = right ? 1 : -1;

        if (flipping > 0)
        {
            startSize = parentRect.rect.width * (1f - (unDisplayedScaleFactor));
            endSize = ((parentRect.rect.width) * unDisplayedScaleFactor) / (float)(pages.Length - 1);
        }
        else if (flipping < 0)
        {
            endSize = parentRect.rect.width * (1f - (unDisplayedScaleFactor));
            startSize = ((parentRect.rect.width) * unDisplayedScaleFactor) / (float)(pages.Length - 1);
        }

        currentFlipTime = 0f;
    }

    public void SetPageAt(int pageNum)
    {
        if (!stepperSetup)
            return;

        if (flipping != 0)
            return;

        pageNum = Mathf.Clamp(pageNum, 0, pages.Length - 1);

        for (int i = 0; i < pages.Length; i++)
        {
            //scales
            RectTransform pageRect = pages[i].pageRect;
            Vector2 sd = pageRect.sizeDelta;
            if (i < pageNum)
                sd.x = ((parentRect.rect.width) * unDisplayedScaleFactor) / (float)(pages.Length - 1);
            else
                sd.x = parentRect.rect.width * (1f - (unDisplayedScaleFactor));
            pageRect.sizeDelta = sd;

            pages[i].undisplayedTab.alpha = (i != pageNum) ? 1 : 0;

            pages[i].undisplayedTab.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pages[i].pageTabName;
            pages[i].undisplayedTab.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";

            //content
            pages[i].displayedGroup.interactable = i == currentPage;
            pages[i].displayedGroup.blocksRaycasts = i == currentPage;
            pages[i].displayedGroup.alpha = (i >= pageNum) ? 1 : 0;
        }

        currentPage = pageNum;
    }

    public void ResetCurrentPage()
    {
        currentPage = 0;

        SetPageAt(currentPage);
    }

    public void TestStartFlip(int number)
    {
        Debug.Log($"Page {pages[number].pageTabName} started flipping");
    }

    public void TestFinishFlip(int number)
    {
        Debug.Log($"Page {pages[number].pageTabName} ended flipping");
    }
}
