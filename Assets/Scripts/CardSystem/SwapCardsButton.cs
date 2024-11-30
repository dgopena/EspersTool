using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapCardsButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Space(10f)]
    [SerializeField] private GameObject firstFocusFrame;
    [SerializeField] private CardMat firstCardMat;

    [Space(10f)]
    [SerializeField] private GameObject secondFocusFrame;
    [SerializeField] private CardMat secondCardMat;

    private void Awake()
    {
        firstCardMat.OnCardSelectUpdate.AddListener(delegate { LookUpdate(); });
        secondCardMat.OnCardSelectUpdate.AddListener(delegate { LookUpdate(); });
    }

    private void LookUpdate()
    {
        //adjust position according to mat shapes and position

        bool active = (firstCardMat.selectedCardNumbers > 0 && secondCardMat.selectedCardNumbers > 0);
        float cgAlpha = active ? 1f : 0f;
        canvasGroup.alpha = cgAlpha;
        canvasGroup.interactable = active;
    }

    public void ShowFrames()
    {
        firstFocusFrame.SetActive(firstCardMat.selectedCardNumbers > 0 && secondCardMat.selectedCardNumbers > 0);
        secondFocusFrame.SetActive(firstCardMat.selectedCardNumbers > 0 && secondCardMat.selectedCardNumbers > 0);
    }

    public void HideFrame()
    {
        firstFocusFrame.SetActive(false);
        secondFocusFrame.SetActive(false);
    }

    public void MakeSwap()
    {
        //
    }
}
