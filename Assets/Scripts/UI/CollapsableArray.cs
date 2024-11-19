using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollapsableArray : MonoBehaviour
{
    [SerializeField] private GameObject baseEntryPrefab;
    [SerializeField] private float collapsedWidth = 20f;
    [SerializeField] private float displayedWidth = 120f;

    [SerializeField] private float morphTime = 1f;
    private bool morphing = false;
    private float startMorphTime = 0f;
    public int currentIndex { get; private set; }
    public int totalEntries { get { return currentStateArray.Length; } }

    [Space(20f)]
    [SerializeField] private List<string> textEntries; //for now its just text
    [SerializeField] private bool startWithDefaultValues = false;

    private int[] currentStateArray;

    public UnityEvent arrayWidthChange;

    [Serializable]
    public struct ArrayEntryData
    {
        public GameObject entryObject;
        public bool opened;

        public RectTransform entryRect;
        public Vector3 startPos;
        public Vector3 endPos;
        public Vector2 startScale;
        public Vector2 endScale;
        public CanvasGroup textAlpha;
        public float startGroupAlpha;
        public float endGroupAlpha;

        public Image frameImage;
    }

    private ArrayEntryData[] entriesArray;
    private bool arrayBuilt = false;

    private void Awake()
    {
        if(startWithDefaultValues)
            BuildArray();
    }

    private void BuildArray()
    {
        int[] defaultEntryState = new int[textEntries.Count];
        for (int i = 0; i < defaultEntryState.Length; i++)
            defaultEntryState[i] = 0;

        entriesArray = new ArrayEntryData[textEntries.Count];
        Vector3[] startPositions = GetPositionArrayInThisConfig(defaultEntryState);

        for(int i = 0; i < textEntries.Count; i++)
        {
            GameObject nuEntry = Instantiate<GameObject>(baseEntryPrefab, transform);
            nuEntry.SetActive(true);
            ArrayEntryData toStore = new ArrayEntryData();
            toStore.entryObject = nuEntry;
            toStore.opened = false;
            toStore.textAlpha = nuEntry.transform.GetChild(0).GetChild(0).GetComponent<CanvasGroup>();
            nuEntry.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = textEntries[i];
            toStore.entryRect = nuEntry.GetComponent<RectTransform>();
            Vector2 sizeDelta = toStore.entryRect.sizeDelta;
            sizeDelta.x = collapsedWidth;
            nuEntry.GetComponent<RectTransform>().sizeDelta = sizeDelta;
            toStore.textAlpha.alpha = 0f;
            toStore.frameImage = nuEntry.transform.GetChild(1).GetComponent<Image>();
            entriesArray[i] = toStore;

            toStore.entryRect.anchoredPosition = startPositions[i];
        }

        currentIndex = 0;
        DisplayIndex(currentIndex);

        arrayBuilt = true;

        if (arrayWidthChange != null)
            arrayWidthChange.Invoke();
    }

    public void GiveTextArray(List<string> textEntries)
    {
        this.textEntries = textEntries;

        for(int i = transform.childCount - 1; i >= 1; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        BuildArray();
    }

    public void GiveColorArray(List<Color> colorEntries)
    {
        if (colorEntries.Count != textEntries.Count)
            return;

        for(int i = 0; i < colorEntries.Count; i++)
        {
            ApplyColorFrame(i, colorEntries[i]);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!arrayBuilt)
            return;

        /*
        if (Input.GetKeyDown(KeyCode.A))
            ShowPrevious();
        else if (Input.GetKeyDown(KeyCode.D))
            ShowNext();
        */

        if (morphing)
        {
            float t = (Time.time - startMorphTime) / morphTime;
            if(t >= 1)
            {
                t = 1;
                morphing = false;
            }

            for(int i = 0; i < entriesArray.Length; i++)
            {
                RectTransform toRect = entriesArray[i].entryRect;

                Vector3 tPos = Vector3.Lerp(entriesArray[i].startPos, entriesArray[i].endPos, t);
                Vector2 tSize = Vector2.Lerp(entriesArray[i].startScale, entriesArray[i].endScale, t);
                float tAlpha = Mathf.Lerp(entriesArray[i].startGroupAlpha, entriesArray[i].endGroupAlpha, t);

                toRect.anchoredPosition = tPos;
                toRect.sizeDelta = tSize;
                entriesArray[i].textAlpha.alpha = tAlpha;
            }
        }
    }

    public void ShowPrevious(bool circle = true)
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            if (circle)
                currentIndex = entriesArray.Length - 1;
            else
            {
                currentIndex = 0;
                return;
            }
        }

        DisplayIndex(currentIndex);
    }

    public void ShowNext(bool circle = true)
    {
        currentIndex++;
        if (currentIndex >= entriesArray.Length)
        {
            if (circle)
                currentIndex = 0;
            else
            {
                currentIndex = entriesArray.Length;
                return;
            }
        }

        DisplayIndex(currentIndex);
    }

    //use wisely
    public void ForceCurrentIndexUpdate(int value)
    {
        currentIndex = value;
        DisplayIndex(currentIndex);
    }

    public void DisplayIndex(int index)
    {
        int[] stateArray = new int[entriesArray.Length];
        stateArray[index] = 1;
        ApplyStateArray(stateArray);
    }

    public void ApplyStateArray(int[] openStateArray)
    {
        Vector3[] startPositions = GetPositionArrayInThisConfig(openStateArray);

        morphing = true;
        startMorphTime = Time.time;

        //for now, directly apply
        for(int i = 0; i < entriesArray.Length; i++)
        {
            RectTransform entryRect = entriesArray[i].entryRect;

            //position
            entriesArray[i].startPos = entryRect.anchoredPosition;
            entriesArray[i].endPos = startPositions[i];

            //scale
            entriesArray[i].startScale = entryRect.sizeDelta;
            Vector2 sizeDelta = entryRect.sizeDelta;
            sizeDelta.x = openStateArray[i] == 1 ? displayedWidth : collapsedWidth;
            entriesArray[i].endScale = sizeDelta;

            //alpha
            entriesArray[i].startGroupAlpha = entriesArray[i].textAlpha.alpha;
            entriesArray[i].endGroupAlpha = openStateArray[i] == 1 ? 1f : 0f;


            /*
            entryRect.localPosition = startPositions[i];
            Vector2 sizeDelta = entryRect.sizeDelta;
            sizeDelta.x = openStateArray[i] == 0 ? collapsedWidth : displayedWidth;
            entryRect.sizeDelta = sizeDelta;
            entriesArray[i].opened = openStateArray[i] != 0;
            entriesArray[i].textAlpha.alpha = entriesArray[i].opened ? 1f : 0f;
            */
        }

        currentStateArray = openStateArray;

        /*
        if (arrayWidthChange != null)
            arrayWidthChange.Invoke();
        */
    }

    //gets the correct positions according if the pieces would be opened or closed in the array
    private Vector3[] GetPositionArrayInThisConfig(int[] openStateArray, bool horizontal = true)
    {
        if(openStateArray.Length != entriesArray.Length)
        {
            Debug.LogError("[CollapsableArray] " + "Open config array and the array of entries don't match in size.");
            return null;
        }

        Vector3[] configPositions = new Vector3[openStateArray.Length];

        bool countIsEven = (openStateArray.Length % 2) == 0;
        float halfValue = (float)openStateArray.Length / 2f;

        float[] pointValues = new float[openStateArray.Length];

        float startValue = 0f;

        if (!countIsEven)
        {
            int midIndex = Mathf.FloorToInt(halfValue);
            startValue = collapsedWidth / 2f;
            if (openStateArray[midIndex] == 1)
                startValue = displayedWidth / 2f;

            pointValues[midIndex] = 0f;
        }

        //lower half
        for (int i = Mathf.FloorToInt(halfValue) - 1; i >= 0; i--)
        {
            float widthValue = (openStateArray[i] == 0) ? collapsedWidth : displayedWidth;
            float posValue = (-0.5f * widthValue) - startValue;
            pointValues[i] = posValue;
            startValue = -1f * (posValue - (0.5f * widthValue));
        }

        startValue = 0f;
        int higherHalfStartIndex = Mathf.FloorToInt(halfValue);
        if (!countIsEven)
        {
            higherHalfStartIndex++;
            int midIndex = Mathf.FloorToInt(halfValue);
            startValue = collapsedWidth / 2f;
            if (openStateArray[midIndex] == 1)
                startValue = displayedWidth / 2f;
        }

        //higher half
        for (int i = higherHalfStartIndex; i < openStateArray.Length; i++)
        {
            float widthValue = (openStateArray[i] == 0) ? collapsedWidth : displayedWidth;
            float posValue = (0.5f * widthValue) + startValue;
            pointValues[i] = posValue;
            startValue = posValue + (0.5f * widthValue);
        }

        //recenter
        float rightMost = pointValues[pointValues.Length - 1];
        rightMost += (0.5f * (openStateArray[pointValues.Length - 1] == 1 ? displayedWidth : collapsedWidth));
        float leftMost = pointValues[0];
        leftMost -= (0.5f * (openStateArray[0] == 1 ? displayedWidth : collapsedWidth));

        float recenterFactor = -0.5f * (rightMost + leftMost);
        for (int i = 0; i < pointValues.Length; i++)
        {
            pointValues[i] = pointValues[i] + recenterFactor;
        }

        for (int i = 0; i < configPositions.Length; i++)
        {
            if (horizontal)
                configPositions[i] = new Vector3(pointValues[i], 0f, 0f);
            else
                configPositions[i] = new Vector3(0f, pointValues[i], 0f);
        }

        return configPositions;
    }

    public int[] GetCurrentStateArray()
    {
        return currentStateArray;
    }

    public bool IsMorphing()
    {
        return morphing;
    }

    public void ApplyColorFrame(int index, Color frameColor)
    {
        if (entriesArray == null)
            return;

        entriesArray[index].frameImage.color = frameColor;
    }

    public ArrayEntryData GetCurrentIndexRect()
    {
        return entriesArray[currentIndex];
    }

    public float GetCurrentWidth()
    {
        float widthSum = 0f;
        for(int i = 0; i < currentStateArray.Length; i++)
        {
            if (currentStateArray[i] == 1)
                widthSum += displayedWidth;
            else
                widthSum += collapsedWidth;
        }

        return widthSum;
    }
}
