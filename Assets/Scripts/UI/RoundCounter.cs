using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static PieceCamera;

public class RoundCounter : MonoBehaviour
{
    [SerializeField] private CollapsableArray array;

    private int roundCount = 1;
    private int currentPieceCount;
    private int currentPieceIndex = 0;
    private UnitPiece focusedUnitPiece;

    private List<RoundListEntry> orderedRoundList;
    
    private struct RoundListEntry
    {
        public UnitPiece refPiece;
        public bool isSlowed;
        public int baseOrderValue;
    }

    [SerializeField] private float markerHeightDelta = 200f;

    [Space(20f)]
    [SerializeField] private TextMeshProUGUI roundLabel;
    [SerializeField] private RectTransform roundPlayerMarker;
    [SerializeField] private RectTransform roundLeftButton;
    [SerializeField] private RectTransform roundRightButton;

    [Space(10f)]
    [SerializeField] private RectTransform arrayContainer;
    [SerializeField] private GameObject containerScrollbarHorizontal;
    private ScrollRect scrollView;
    [SerializeField] private GameObject leftMargin;
    [SerializeField] private GameObject rightMargin;
    private bool marginsActive = false;
    [SerializeField] private float recenterSpeed = 0.5f;
    private float recenterCountdown;
    private bool recentering = false;

    private float recenterStartValue;
    private float recenterTargetValue;
    private Vector3 rectPosition;

    private float frameWidth;

    private bool optionListOpen = false;
    private int pieceOptionIndex = 0;
    private int[] auxStateArray;
    private RectTransform listRT;

    [Space(20f)]
    [SerializeField] private Color unitColor = Color.white;
    [SerializeField] private Color slowedUnitColor = Color.yellow;
    [Space(20f)]
    [SerializeField] private Color foeColor = Color.red;
    [SerializeField] private Color slowedFoeColor = Color.yellow;

    void Awake()
    {
        /*
        PieceManager._instance.OnPieceAdded += UpdateRoundArray;
        PieceManager._instance.OnPieceRemoved += UpdateRoundArray;

        UpdateRoundArray();
        */

        array.arrayWidthChange.AddListener(AdaptContainerToWidth);
    }

    private void OnEnable()
    {
        frameWidth = containerScrollbarHorizontal.GetComponent<RectTransform>().rect.width;
        scrollView = containerScrollbarHorizontal.transform.parent.GetComponent<ScrollRect>();

        UpdateRoundArray();
    }

    private void LateUpdate()
    {
        if (focusedUnitPiece == null)
            return;

        if (optionListOpen && Input.GetMouseButtonDown(0))
        {
            if (!TooltipManager.CheckMouseInArea(listRT))
            {
                optionListOpen = false;
                UnitManager._instance.listPanel.ShowPanel(false);
                UnitManager._instance.listPanel.OnEntryClick -= RoundOptionListClick;

                array.ApplyStateArray(auxStateArray);
            }
        }

        Vector3 piecePos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(focusedUnitPiece.GetModelPosition());
        roundPlayerMarker.position = piecePos + (markerHeightDelta * Vector3.up);

        if (recentering)
        {
            recenterCountdown -= Time.deltaTime;
            float frac = 1 - (recenterCountdown / recenterSpeed);

            if(frac >= 1f)
            {
                frac = 1f;
                recentering = false;
            }

            Vector3 contAbs = rectPosition;
            contAbs.x = Mathf.Lerp(recenterStartValue, recenterTargetValue, frac);
            arrayContainer.anchoredPosition = contAbs;

            frameWidth = containerScrollbarHorizontal.GetComponent<RectTransform>().rect.width;
        }
    }

    public void UpdateRoundArray()
    {
        List<UnitPiece> currentPieces = PieceManager._instance.GetPieceList();
        List<RoundListEntry> updatedRoundList = new List<RoundListEntry>();

        int orderCounter = 0;
        for(int i = 0; i < currentPieces.Count; i++)
        {
            if(currentPieces[i] is CharacterPiece)
            {
                CharacterPiece piece = (CharacterPiece)currentPieces[i];
                RoundListEntry nuEntry = new RoundListEntry();
                nuEntry.refPiece = piece;
                nuEntry.isSlowed = false;
                nuEntry.baseOrderValue = orderCounter;

                updatedRoundList.Add(nuEntry);

                orderCounter++;
            }
        }
        for(int i = 0; i < currentPieces.Count; i++)
        {
            if (currentPieces[i] is FoePiece)
            {
                FoePiece piece = (FoePiece)currentPieces[i];
                RoundListEntry nuEntry = new RoundListEntry();
                nuEntry.refPiece = piece;
                nuEntry.isSlowed = false;
                nuEntry.baseOrderValue = orderCounter;

                updatedRoundList.Add(nuEntry);

                orderCounter++;
            }
        }

        if (orderCounter == 0)
        {
            HideRoundCounter(true);
            return;
        }
        else
            HideRoundCounter(false);

        OrderRoundList(updatedRoundList, false);

        List<string> pieceNames = new List<string>();
        List<Color> pieceColors = new List<Color>();
        for(int i = 0; i < orderedRoundList.Count; i++)
        {
            pieceNames.Add(orderedRoundList[i].refPiece.unitName);
            bool isFoe = orderedRoundList[i].refPiece is FoePiece;
            if (isFoe)
                pieceColors.Add(foeColor);
            else
                pieceColors.Add(unitColor);
        }

        array.GiveTextArray(pieceNames);
        array.GiveColorArray(pieceColors);

        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(ListUpdateFrameSkip());
    }

    private IEnumerator ListUpdateFrameSkip()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 1; i < array.transform.childCount; i++)
        {
            int entryIndex = i;
            array.transform.GetChild(i).GetComponent<HoldButton>().onRelease.AddListener(delegate { RoundListEntryClick(entryIndex); });
        }

        currentPieceCount = array.transform.childCount - 1;

        if (currentPieceIndex >= currentPieceCount)
            currentPieceIndex--;

        if (focusedUnitPiece != null)
            focusedUnitPiece.SetSelected(false);
        focusedUnitPiece = orderedRoundList[currentPieceIndex].refPiece;
        focusedUnitPiece.SetSelected(true);

        array.ForceCurrentIndexUpdate(currentPieceIndex);

        for (int i = 0; i < orderedRoundList.Count; i++)
        {
            bool isFoe = orderedRoundList[i].refPiece is FoePiece;
            if(!isFoe)
                array.ApplyColorFrame(i, orderedRoundList[i].isSlowed ? slowedUnitColor : unitColor);
            else
                array.ApplyColorFrame(i, orderedRoundList[i].isSlowed ? slowedFoeColor : foeColor);
        }
    }

    private void OrderRoundList(List<RoundListEntry> entries, bool doFirstOrder = true)
    {
        if(orderedRoundList == null)
        {
            orderedRoundList = entries;
        }
        else
        {
            if (doFirstOrder)
            {
                //first order
                List<RoundListEntry> aux = new List<RoundListEntry>(entries);
                for (int i = 0; i < entries.Count; i++)
                {
                    aux[entries[i].baseOrderValue] = entries[i];
                }
                entries = new List<RoundListEntry>(aux);
            }

            //slow order
            List<RoundListEntry> reOrder = new List<RoundListEntry>();
            List<RoundListEntry> slowedOrder = new List<RoundListEntry>();
            for(int i = 0; i < entries.Count; i++)
            {
                RoundListEntry match = orderedRoundList.Find(x => x.refPiece.GetInstanceID() == entries[i].refPiece.GetInstanceID());
                if (match.refPiece == null)
                    reOrder.Add(entries[i]);
                else if (match.isSlowed)
                {
                    match.baseOrderValue = entries[i].baseOrderValue;
                    slowedOrder.Add(match);
                }
                else
                {
                    match.baseOrderValue = entries[i].baseOrderValue;
                    reOrder.Add(match);
                }
            }

            for(int i = 0; i < slowedOrder.Count; i++)
            {
                reOrder.Add(slowedOrder[i]);
            }

            orderedRoundList = reOrder;
        }
    }

    public void MoveCounter(bool right)
    {
        if (right)
            array.ShowNext();
        else if (currentPieceIndex != 0 || roundCount != 1)
            array.ShowPrevious();

        currentPieceIndex += right ? 1 : -1;
        if (currentPieceIndex >= currentPieceCount)
        {
            currentPieceIndex = 0;
            roundCount++;
        }
        else if (currentPieceIndex < 0)
        {
            if (roundCount == 1)
                currentPieceIndex = 0;
            else
            {
                currentPieceIndex = currentPieceCount - 1;
                roundCount--;
            }
        }

        if (focusedUnitPiece != null)
            focusedUnitPiece.SetSelected(false);
        focusedUnitPiece = orderedRoundList[currentPieceIndex].refPiece;
        focusedUnitPiece.SetSelected(true);

        roundLabel.text = "Round " + roundCount;

        SetContainerCentered();
    }

    public void RoundOptionListClick(int choiceIndex)
    {
        if(choiceIndex == 0)
        {
            //apply or remove
            RoundListEntry chosenEntry = orderedRoundList[pieceOptionIndex];
            chosenEntry.isSlowed = !chosenEntry.isSlowed;

            orderedRoundList[pieceOptionIndex] = chosenEntry;

            List<RoundListEntry> auxList = new List<RoundListEntry>(orderedRoundList);
            OrderRoundList(auxList);

            List<string> pieceNames = new List<string>();
            List<Color> pieceColors = new List<Color>();
            for (int i = 0; i < orderedRoundList.Count; i++)
            {
                pieceNames.Add(orderedRoundList[i].refPiece.unitName);
                bool isFoe = orderedRoundList[i].refPiece is FoePiece;
                if (isFoe)
                    pieceColors.Add(foeColor);
                else
                    pieceColors.Add(unitColor);
            }

            array.GiveTextArray(pieceNames);
            array.GiveColorArray(pieceColors);

            StartCoroutine(ListUpdateFrameSkip());
        }

        optionListOpen = false;
        UnitManager._instance.listPanel.ShowPanel(false);
        UnitManager._instance.listPanel.OnEntryClick -= RoundOptionListClick;

        array.ApplyStateArray(auxStateArray);
    }

    public void RoundListEntryClick(int listIndex)
    {
        if (optionListOpen || array.IsMorphing())
            return;

        pieceOptionIndex = listIndex - 1;
        auxStateArray = array.GetCurrentStateArray();

        if (auxStateArray[pieceOptionIndex] == 0)
        {
            int[] tempStateArray = new int[auxStateArray.Length]; 
            Array.Copy(auxStateArray, tempStateArray, auxStateArray.Length);
            tempStateArray[pieceOptionIndex] = 1;
            array.ApplyStateArray(tempStateArray);
        }

        UnitManager._instance.listPanel.screenProportionSize = 0.6f * UnitManager._instance.bondClassJobPanelProportions;
        UnitManager._instance.listPanel.listColor = 0.9f * Color.black;

        Vector3 listOrigin = Input.mousePosition;
        List<string> optionLabels = new List<string>();
        if (orderedRoundList[listIndex - 1].isSlowed)
            optionLabels.Add("Remove Slow Turn");
        else
            optionLabels.Add("Apply Slow Turn");

        optionLabels.Add("Cancel");

        UnitManager._instance.listPanel.ShowPanel(listOrigin, optionLabels, true);
        optionListOpen = true;
        UnitManager._instance.listPanel.OnEntryClick += RoundOptionListClick;

        listRT = UnitManager._instance.listPanel.GetComponent<RectTransform>();
    }

    private void HideRoundCounter(bool hide)
    {
        array.gameObject.SetActive(!hide);
        roundLabel.gameObject.SetActive(!hide);
        roundLeftButton.gameObject.SetActive(!hide);
        roundRightButton.gameObject.SetActive(!hide);
        //roundPlayerMarker.gameObject.SetActive(!hide);

        if (focusedUnitPiece != null)
            focusedUnitPiece.SetSelected(!hide);
    }

    private void AdaptContainerToWidth()
    {
        Vector2 sd = arrayContainer.sizeDelta;
        sd.x = array.GetCurrentWidth();
        arrayContainer.sizeDelta = sd;

        bool prevMarg = marginsActive;
        marginsActive = frameWidth < arrayContainer.rect.width;

        leftMargin.SetActive(marginsActive);
        rightMargin.SetActive(marginsActive);

        if(prevMarg != marginsActive)
            UpdateWidthMode();

        SetContainerCentered();
    }

    private void UpdateWidthMode()
    {
        if (marginsActive)
        {
            Vector3 aPos = arrayContainer.anchoredPosition;
            aPos.x = -0.5f * frameWidth;
            arrayContainer.anchoredPosition = aPos;
            scrollView.enabled = true;
            containerScrollbarHorizontal.SetActive(true);
        }
        else
        {
            Vector3 aPos = arrayContainer.anchoredPosition;
            aPos.x = -0.5f * arrayContainer.rect.width;
            arrayContainer.anchoredPosition = aPos;
            scrollView.enabled = false;
            containerScrollbarHorizontal.SetActive(false);
        }
    }

    private void SetContainerCentered()
    {
        if (!marginsActive)
            return;

        float width = array.GetCurrentWidth();

        float moveDiff = width * 0.5f;

        int currentIndex = array.currentIndex;
        int totalIndex = array.totalEntries;
        float correction = moveDiff / (float)totalIndex;

        float centerPos = -355f;

        centerPos -= correction * (float) currentIndex;

        if (Mathf.Abs(centerPos) > frameWidth)
            centerPos = -1f * frameWidth;

        //make it lerpy
        recenterTargetValue = centerPos;
        recenterStartValue = arrayContainer.anchoredPosition.x;

        recenterCountdown = recenterSpeed;

        rectPosition = arrayContainer.anchoredPosition;

        recentering = true;
    }
}
