using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ElixirList : MonoBehaviour
{
    private GameObject elixirIconPrefab;

    [SerializeField] private Sprite filledBottle;
    [SerializeField] private Sprite emptyBottle;

    private RectTransform listRect;

    private bool listSetup = false;

    [Range(0.05f, 3f)]
    public float iconSeparation = 0.1f;

    public int maximumElixirDisplay = 8;

    private List<ElixirPair> elixirIcons;

    public UnityEvent OnListChange;

    private struct ElixirPair
    {
        public RectTransform elixirObj;
        public bool elixirState;
    }

    private void LateUpdate()
    {
        if (elixirIcons == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < elixirIcons.Count; i++)
            {
                if (TooltipManager.CheckMouseInArea(elixirIcons[i].elixirObj))
                {
                    ElixirPair chgPair = elixirIcons[i];
                    chgPair.elixirState = !chgPair.elixirState;
                    elixirIcons[i] = chgPair;

                    elixirIcons[i].elixirObj.GetComponent<Image>().sprite = elixirIcons[i].elixirState ? filledBottle : emptyBottle;

                    if (OnListChange != null)
                        OnListChange.Invoke();

                    break;
                }
            }
        }
    }

    public void SetupList()
    {
        if (listSetup)
            return;

        listRect = transform.GetChild(0).GetComponent<RectTransform>();
        elixirIconPrefab = listRect.GetChild(0).gameObject;
        listSetup = true;
    }

    public int GetIconStateCount(bool active)
    {
        int sum = 0;
        for(int i = 0; i < elixirIcons.Count; i++)
        {
            if (active && elixirIcons[i].elixirState)
                sum++;
            else if (!active && !elixirIcons[i].elixirState)
                sum++;
        }

        return sum;
    }

    public void SetIconSlotCount(int amount)
    {
        if (amount > maximumElixirDisplay)
            amount = maximumElixirDisplay;

        SetupList();

        if (elixirIcons != null)
        {
            for (int i = 0; i < elixirIcons.Count; i++)
            {
                Destroy(elixirIcons[i].elixirObj.gameObject);
            }
        }

        elixirIcons = new List<ElixirPair>();

        AddIcon(amount);
    }

    public void AddIcon(int amount)
    {
        if (amount > maximumElixirDisplay)
            amount = maximumElixirDisplay;

        for (int i = 0; i < amount; i++)
            AddIcon();
    }

    public void AddIcon()
    {
        if (elixirIcons.Count >= maximumElixirDisplay)
            return;

        SetupList();

        if (elixirIcons == null)
            elixirIcons = new List<ElixirPair>();

        GameObject nuIcon = Instantiate<GameObject>(elixirIconPrefab, listRect);
        RectTransform iconRT = nuIcon.GetComponent<RectTransform>();

        float size = listRect.rect.height;
        Vector3 positioning = new Vector3(elixirIcons.Count * (size + (iconSeparation * size)), 0f, 0f);
        iconRT.anchoredPosition = positioning;

        Vector2 sd = iconRT.sizeDelta;
        sd.x = size;
        iconRT.sizeDelta = sd;

        iconRT.GetComponent<Image>().sprite = filledBottle;

        ElixirPair nuPair = new ElixirPair();
        nuPair.elixirObj = iconRT;
        nuPair.elixirState = true;

        elixirIcons.Add(nuPair);

        nuIcon.SetActive(true);
    }

    public void RemoveIcon(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (elixirIcons.Count <= 0)
                break;

            RemoveIcon();
        }
    }

    public void RemoveIcon()
    {
        SetupList();

        GameObject iconToRemove = elixirIcons[elixirIcons.Count - 1].elixirObj.gameObject;
        elixirIcons.RemoveAt(elixirIcons.Count - 1);

        Destroy(iconToRemove);
    }

    public bool[] GetStateArray()
    {
        bool[] stateArray = new bool[elixirIcons.Count];
        for(int i = 0; i < stateArray.Length; i++)
        {
            stateArray[i] = elixirIcons[i].elixirState;
        }

        return stateArray;
    }

    public void SetFillCount(int count)
    {
        count = Mathf.Clamp(count, 0, elixirIcons.Count);

        bool[] stateBuilt = new bool[elixirIcons.Count];

        for (int i = 0; i < elixirIcons.Count; i++)
        {
            stateBuilt[i] = i < count;
        }

        SetStateArray(stateBuilt);
    }

    public void SetStateArray(bool[] stateArray)
    {
        if (stateArray.Length != elixirIcons.Count)
            return;

        for(int i = 0; i < stateArray.Length; i++)
        {
            ElixirPair targetPair = elixirIcons[i];
            targetPair.elixirState = stateArray[i];
            elixirIcons[i] = targetPair;

            elixirIcons[i].elixirObj.GetComponent<Image>().sprite = elixirIcons[i].elixirState ? filledBottle : emptyBottle;
        }
    }
}
