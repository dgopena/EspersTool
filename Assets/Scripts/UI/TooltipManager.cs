using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager _instance;

    [HideInInspector] public Camera cam;

    //list panel needs to be a direct child of the main canvas
    [Space(20f)]
    private RectTransform canvasRT;
    private RectTransform panelRT;

    public TextMeshProUGUI tipText;
    public CanvasGroup canvasGroup;

    [Header("Panel Settings")]
    public Vector2 screenProportionSize = new Vector2(0.1f, 0.8f);
    public float entryScreenHeightProportion = 0.08f;
    public Vector2 widthLimits = new Vector2(0.1f, 0.9f);
    public Vector2 heightLimits = new Vector2(0.1f, 0.9f);

    [Range(1f, 2f)]
    public float outerPanelMultiplier = 1.1f;

    private Vector2 panelDeltaPos;

    private float panelWidth;
    private float panelHeight;

    protected float entryHeight;

    [Header("Registered Tips")]
    public List<ButtonTip> tips;

    [Space(20f)]

    public float toolTipWait = 1f;
    private bool tipWaiting = false;
    private int entryID;
    private bool tipShown = false;
    private float entryTime;
    public float pointerDistanceMinDelta = 20f;
    private Vector3 lastPointerPosition;

    [System.Serializable]
    public struct ButtonTip
    {
        public RectTransform rectComponent;
        public string tooltip;
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    int auxCounter = 0;

    private void LateUpdate()
    {
        /*
        string[] testStrings = new string[3] {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
            "Tiny",
            "Sphinx of Black Quartz Judge my Vow" };



        if (Input.GetMouseButtonDown(0))
        {
            ShowTip(testStrings[auxCounter]);
            auxCounter = (auxCounter + 1) % 3;
        }
        */

        bool inButton = false;
        for (int i = 0; i < tips.Count; i++)
        {
            if (!tips[i].rectComponent.gameObject.activeInHierarchy)
                continue;

            if (CheckMouseInArea(tips[i].rectComponent))
            {
                if (!tipShown)
                {
                    if (!tipWaiting)
                    {
                        tipWaiting = true;
                        entryTime = Time.time;
                        entryID = i;
                    }
                    else if (i != entryID)
                    {
                        entryID = i;
                        entryTime = Time.time;
                    }
                }

                inButton = true;
                break;
            }
        }

        if (inButton)
        {
            if (Vector3.Distance(Input.mousePosition, lastPointerPosition) > pointerDistanceMinDelta)
            {
                lastPointerPosition = Input.mousePosition;
                entryTime = Time.time;

                if (tipShown)
                {
                    HideTip();
                    tipShown = false;
                }
            }

            if (tipWaiting)
            {
                if ((Time.time - entryTime) > toolTipWait)
                {
                    tipWaiting = false;
                    ShowTip(tips[entryID].tooltip);
                    tipShown = true;
                }
            }
        }
        else
        {
            if (tipShown)
            {
                HideTip();
                tipShown = false;
            }
        }
    }

    public void ShowTip(string tooltip)
    {
        ShowTip(Input.mousePosition, tooltip);
    }

    //origin must be in screen size
    public void ShowTip(Vector3 origin, string toolTip)
    {
        canvasRT = transform.parent.GetComponent<RectTransform>();

        RectTransform textRT = tipText.GetComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(screenProportionSize.x * Screen.width, screenProportionSize.y * Screen.height);

        Vector3 deltaPos = Vector3.zero;

        tipText.text = toolTip;
        tipText.ForceMeshUpdate();

        float baseTextWidth = float.MinValue;
        float baseTextHeight = 0f;
        for (int i = 0; i < tipText.textInfo.lineCount; i++)
        {
            baseTextWidth = Mathf.Max(baseTextWidth, tipText.textInfo.lineInfo[i].length);
            baseTextHeight += tipText.textInfo.lineInfo[i].lineHeight;
        }

        panelWidth = screenProportionSize.x * canvasRT.rect.width;

        float textSize = baseTextWidth; // correctiveFactor.x * tipText.fontSize * toolTip.Length;

        int lineCount = tipText.textInfo.lineCount; // Mathf.FloorToInt(textSize / panelWidth) + 1;

        float panelHeight = 0f;

        panelWidth = baseTextWidth;

        panelHeight = baseTextHeight; // lineCount * correctiveHeightFactor * tipText.fontSize;

        Vector2 localPoint = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, origin, cam, out localPoint);

        //we correct positioning

        //right
        if ((panelWidth + localPoint.x) > (widthLimits.y * 0.5f * canvasRT.sizeDelta.x))
            deltaPos.x = -panelWidth;

        //down
        if ((localPoint.y - panelHeight) < ((1f - heightLimits.x) * -0.5f * canvasRT.sizeDelta.y))
            deltaPos.y = ((1f - heightLimits.x) * -0.5f * canvasRT.sizeDelta.y) - (localPoint.y - panelHeight);

        transform.position = origin + deltaPos;

        panelRT = GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(outerPanelMultiplier * panelWidth, outerPanelMultiplier * panelHeight);

        textRT = tipText.GetComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(panelWidth, panelHeight);

        ShowPanel(true);
    }

    private void ShowPanel(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
    }

    public void HideTip()
    {
        ShowPanel(false);
    }

    public void AddTip(RectTransform rect, string tooltip)
    {
        ButtonTip nuTip = new ButtonTip();
        nuTip.rectComponent = rect;
        nuTip.tooltip = tooltip;

        tips.Add(nuTip);
    }

    public static bool CheckMouseInArea(RectTransform area)
    {
        bool ret = false;

        Vector2 mouseAreaLocalPosition = area.InverseTransformPoint(Input.mousePosition);
        if (area.rect.Contains(mouseAreaLocalPosition))
        {
            ret = true;
        }

        return ret;
    }
}
