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

        bool active = (firstCardMat.selectedCardNumbers > 0 && secondCardMat.selectedCardNumbers > 0);
        float cgAlpha = active ? 1f : 0f;
        canvasGroup.alpha = cgAlpha;
        canvasGroup.interactable = active;
    }

    public void ShowFrames()
    {
        firstFocusFrame.gameObject.SetActive(firstCardMat.selectedCardNumbers > 0 && secondCardMat.selectedCardNumbers > 0);
        secondFocusFrame.gameObject.SetActive(firstCardMat.selectedCardNumbers > 0 && secondCardMat.selectedCardNumbers > 0);
    }

    public void HideFrame()
    {
        firstFocusFrame.gameObject.SetActive(false);
        secondFocusFrame.gameObject.SetActive(false);
    }

    public void MakeSwap()
    {
        //

        if (fateDiscardUpdate)
            firstCardMat.mainDeck.UpdateFateDiscardSize();

        firstCardMat.mainDeck.UpdateMidBorders();

        //call the arranging of the decks
        
        //fateMat.AddCardsToMat(fate);
        //discardMat.AddCardsToMat(discard);
    }
}
