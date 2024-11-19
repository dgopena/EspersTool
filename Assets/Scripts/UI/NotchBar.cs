using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class NotchBar : MonoBehaviour
{
    public Image fillBar;
    public GameObject notchObj;

    public float fillSpeed = 50f;
    public bool immediateFill = false;
    private float fillGoal;
    private float currentFill;
    private int filling = 0; // 0- not filling, 1 - filling up, 2 - filling down

    public int notchShowLimit = 30;
    private int baseNotchCount = 6;

    public int notchCount { get; private set; }

    public int currentNotch { get; private set; }

    private bool barSet = false;

    void Start()
    {
        //SetBar(6);
    }

    private void LateUpdate()
    {
        if (!barSet)
            return;
        
        if (filling == 1)
        {
            currentFill += Time.unscaledDeltaTime * fillSpeed;
            if(currentFill > fillGoal)
            {
                currentFill = fillGoal;
                filling = 0;
            }

            fillBar.fillAmount = currentFill;
        }
        else if(filling == 2)
        {
            currentFill -= Time.unscaledDeltaTime * fillSpeed;
            if(currentFill < fillGoal)
            {
                currentFill = fillGoal;
                filling = 0;
            }

            fillBar.fillAmount = currentFill;
        }
    }

    public void SetBar(int notches)
    {
        RectTransform notchParent = notchObj.transform.parent.GetComponent<RectTransform>();

        for(int i = notchParent.childCount -1; i >= 1; i--)
        {
            Destroy(notchParent.GetChild(i).gameObject);
        }

        if (notches <= notchShowLimit)
        {
            float notchWidth = (notches < baseNotchCount) ? 5f : 6f - ((float)notches / (float)baseNotchCount);

            for (int i = 0; i < (notches - 1); i++)
            {
                GameObject nuNotch = Instantiate<GameObject>(notchObj, notchParent);
                RectTransform notchRect = nuNotch.GetComponent<RectTransform>();

                float normPosX = (float)(i + 1) / (float)(notches);

                Vector2 localPos = Rect.NormalizedToPoint(notchParent.rect, new Vector2(normPosX, 0.5f));

                notchRect.localPosition = localPos;
                Vector2 sd = notchRect.sizeDelta;
                sd.x = notchWidth;
                notchRect.sizeDelta = sd;
                nuNotch.SetActive(true);
            }
        }

        fillBar.fillAmount = 1f;
        notchCount = notches;
        currentNotch = notchCount;

        barSet = true;
    }

    public void AddToBar(int amount)
    {
        currentNotch += amount;
        if (currentNotch < 0)
            currentNotch = 0;
        else if (currentNotch > notchCount)
            currentNotch = notchCount;

        if (notchCount != 0)
            fillGoal = (float)currentNotch / (float)notchCount;
        else
            fillGoal = 1;

        if (immediateFill)
        {
            fillBar.fillAmount = fillGoal;
            currentFill = fillGoal;
        }
        else
        {
            currentFill = fillBar.fillAmount;

            if (currentFill < fillGoal)
                filling = 1;
            else if (currentFill > fillGoal)
                filling = 2;
        }
    }

    public void ApplyValue(int value)
    {
        int delta = value - currentNotch;

        AddToBar(delta);
    }

    public void ForceValue(int value)
    {
        currentNotch = value;
        if (currentNotch < 0)
            currentNotch = 0;
        else if (currentNotch > notchCount)
            currentNotch = notchCount;

        if (notchCount != 0)
            fillGoal = (float)currentNotch / (float)notchCount;
        else
            fillGoal = 1;

        fillBar.fillAmount = fillGoal;
        currentFill = fillGoal;
    }
}
