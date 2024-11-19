using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityEngine.UI;

public class TabbedLabels : MonoBehaviour
{
    private RectTransform containerRT;

    public RectTransform[] tabsRT;

    public int startIndex = 0;
    public int chosenIndex { get; private set; }

    private RectTransform returningRT;
    private Image returningImage;
    private RectTransform displayingRT;
    private Image displayingImage;

    [Range(0f,1f)] public float tabSpacing = 0.1f;

    private float moveAmount;

    [Space(10f)]
    public float switchingTime;
    public float moveFactor = 0.2f;
    public float startAnchorDifferential = -0.15f;
    [SerializeField] private TabMoveDirections moveDirectionType;

    private Vector2 moveDir;

    private float tabSwitchStamp;
    private bool tabSwitching = false;

    private enum TabMoveDirections
    {
        Up,
        Down,
        Left,
        Right
    }

    [Space(10f)]
    public Color selectedColor = Color.white;
    public Color deselectedColor = Color.gray;

    [Space(10f)]
    public UnityEvent OnTabChange;

    private void Awake()
    {
        containerRT = GetComponent<RectTransform>();

        if (moveDirectionType == TabMoveDirections.Down) 
        {
            moveAmount = moveFactor * containerRT.rect.height;
            moveDir = Vector2.down;
        }
        else if (moveDirectionType == TabMoveDirections.Up)
        {
            moveAmount = moveFactor * containerRT.rect.height;
            moveDir = Vector2.up;
        }
        else if(moveDirectionType == TabMoveDirections.Left)
        {
            moveAmount = moveFactor * containerRT.rect.width;
            moveDir = Vector2.left;
        }
        else if (moveDirectionType == TabMoveDirections.Right)
        {
            moveAmount = moveFactor * containerRT.rect.width;
            moveDir = Vector2.right;
        }

        //modify anchors to adapt to spacing
        if (moveDirectionType == TabMoveDirections.Down || moveDirectionType == TabMoveDirections.Up)
        {

            float tabWidthPctg = (1f - (tabSpacing * (float)(tabsRT.Length - 1))) / ((float)tabsRT.Length);
            if (tabWidthPctg < 0f)
            {
                Debug.Log("[TabbedLabels] Tab width is negative. Try assigning a lower spacing number. The default 0.1f value was assigned instead)");
                tabWidthPctg = (1f - (0.1f * (float)(tabsRT.Length - 1))) / ((float)tabsRT.Length);
            }

            for (int i = 0; i < tabsRT.Length; i++)
            {
                RectTransform tabRT = tabsRT[i];

                Vector2 ankh = tabRT.anchorMin;
                ankh.x = i * (tabWidthPctg + tabSpacing);
                tabRT.anchorMin = ankh;
                ankh = tabRT.anchorMax;
                ankh.x = ((i + 1) * tabWidthPctg) + (i * tabSpacing);
                tabRT.anchorMax = ankh;
            }
        }
        else
        {
            float tabHeightPctg = (1f - (tabSpacing * (float)(tabsRT.Length - 1))) / ((float)tabsRT.Length);
            if (tabHeightPctg < 0f)
            {
                Debug.Log("[TabbedLabels] Tab width is negative. Try assigning a lower spacing number. The default 0.1f value was assigned instead)");
                tabHeightPctg = (1f - (0.1f * (float)(tabsRT.Length - 1))) / ((float)tabsRT.Length);
            }

            for (int i = 0; i < tabsRT.Length; i++)
            {
                RectTransform tabRT = tabsRT[i];

                int posIdx = tabsRT.Length - i - 1;

                Vector2 ankh = tabRT.anchorMin;
                ankh.x = startAnchorDifferential;
                ankh.y = posIdx * (tabHeightPctg + tabSpacing);
                tabRT.anchorMin = ankh;
                ankh = tabRT.anchorMax;
                ankh.y = ((posIdx + 1) * tabHeightPctg) + (posIdx * tabSpacing);
                tabRT.anchorMax = ankh;
            }
        }

        for (int i = 0; i < tabsRT.Length; i++)
        {
            if (i == startIndex)
            {
                tabsRT[i].anchoredPosition = (moveDir * moveAmount);
                Image tabIMG = tabsRT[i].GetComponent<Image>();
                tabIMG.color = selectedColor;
            }
            else
            {
                tabsRT[i].anchoredPosition = Vector2.zero;
                Image tabIMG = tabsRT[i].GetComponent<Image>();
                tabIMG.color = deselectedColor;
            }
        }

        chosenIndex = startIndex;
    }

    private void LateUpdate()
    {
        if (!tabSwitching)
            return;

        tabSwitchStamp += Time.deltaTime;
        float t = tabSwitchStamp / switchingTime;

        returningImage.color = Color.Lerp(selectedColor, deselectedColor, t);
        returningRT.anchoredPosition = Vector2.Lerp((moveDir * moveAmount), Vector2.zero, t);
        displayingImage.color = Color.Lerp(deselectedColor, selectedColor, t);
        displayingRT.anchoredPosition = Vector2.Lerp(Vector2.zero, (moveDir * moveAmount), t);

        if(t >= 1f)
        {
            returningRT.anchoredPosition = Vector2.zero;
            returningImage.color = deselectedColor;
            displayingRT.anchoredPosition = (moveDir * moveAmount);
            displayingImage.color = selectedColor;

            tabSwitching = false;
        }
    }

    public void TabClick(int index)
    {
        if (index == chosenIndex)
            return;

        if (tabSwitching)
            return;

        returningRT = tabsRT[chosenIndex];
        returningImage = returningRT.GetComponent<Image>();
        displayingRT = tabsRT[index];
        displayingImage = displayingRT.GetComponent<Image>();

        chosenIndex = index;

        tabSwitchStamp = 0f;

        if (OnTabChange != null)
            OnTabChange.Invoke();

        tabSwitching = true;
    }
}
