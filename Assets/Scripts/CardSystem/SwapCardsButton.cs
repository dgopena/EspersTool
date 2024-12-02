using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapCardsButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform buttonRT;

    [Space(10f)]
    [SerializeField] private RectTransform firstFocusFrame;
    [SerializeField] private CardMat firstCardMat;

    [Space(10f)]
    [SerializeField] private RectTransform secondFocusFrame;
    [SerializeField] private CardMat secondCardMat;

    [Space(10f)]
    [SerializeField] private bool fateDiscardUpdate = false;

    private void Awake()
    {
        buttonRT = GetComponent<RectTransform>();

        firstCardMat.OnCardSelectUpdate.AddListener(delegate { LookUpdate(); });
        secondCardMat.OnCardSelectUpdate.AddListener(delegate { LookUpdate(); });
    }

    private void LookUpdate()
    {
        //adjust position according to mat shapes and position
        firstCardMat.mainDeck.UpdateMidBorders();

        firstFocusFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, firstCardMat.GetMatWidth());
        secondFocusFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, secondCardMat.GetMatWidth());

        bool active = (firstCardMat.selectedCardNumbers > 0 || secondCardMat.selectedCardNumbers > 0);
        float cgAlpha = active ? 1f : 0f;
        canvasGroup.alpha = cgAlpha;
        canvasGroup.interactable = active;
    }

    public void ShowFrames()
    {
        bool active = (firstCardMat.selectedCardNumbers > 0 || secondCardMat.selectedCardNumbers > 0);
        firstFocusFrame.gameObject.SetActive(active);
        secondFocusFrame.gameObject.SetActive(active);
    }

    public void HideFrame()
    {
        firstFocusFrame.gameObject.SetActive(false);
        secondFocusFrame.gameObject.SetActive(false);
    }

    public void MakeSwap()
    {
        List<FateCard> firstMatRemoved = firstCardMat.RemoveCardsFromMat();
        List<FateCard> secondMatRemoved = secondCardMat.RemoveCardsFromMat();

        firstCardMat.AddCardsToMat(secondMatRemoved);
        secondCardMat.AddCardsToMat(firstMatRemoved);

        if (fateDiscardUpdate)
            firstCardMat.mainDeck.UpdateFateDiscardSize();

        //call the arranging of the decks
        firstCardMat.ArrangeMat();
        secondCardMat.ArrangeMat();

        firstCardMat.mainDeck.UpdateMidBorders();

        float minScale = firstCardMat.mainDeck.GetMinScale();
        firstCardMat.ApplyHeightToCards(minScale);
        secondCardMat.ApplyHeightToCards(minScale);

        firstCardMat.ResetSelectedCardCounter();
        secondCardMat.ResetSelectedCardCounter();

        firstCardMat.OnCardSelectUpdate.Invoke();
        secondCardMat.OnCardSelectUpdate.Invoke();
    }
}
