using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FateHandWidget : MonoBehaviour
{
    public PlayerDeck playerDeck;

    public CardMat handDisplay;

    public CardMat handSource;

    private bool setup = false;

    [Space(10.0f)] 
    [SerializeField] private GameObject playCardButton;
    [SerializeField] private GameObject discardCardButton;
    
    [Space(10.0f)]
    [SerializeField] private bool autoSetup = false;

    public UnityEvent<FateCard> OnCardPlayed;
    
    private void OnEnable()
    {
        if (!setup && autoSetup)
            SetUpDisplay();
        else if(setup)
        {
            FullUpdate();
        }
    }

    public void SetUpDisplay()
    {
        if (setup)
            return;
        
        //playerDeck.SetDeckUp();
        handDisplay.mainDeck = playerDeck;
        handDisplay.OnCardSelectUpdate.AddListener(CheckCardsInSelection);
        
        playerDeck.OnPanelClose.AddListener(OnPlayerDeckClose);
        
        FullUpdate();
        
        setup = true;
    }

    public void FullUpdate()
    {
        handDisplay.ReplaceAllCards(handSource.GetAllCards());
        
        CheckCardsInSelection();
    }

    public void DrawCard()
    {
        FateCard draw = playerDeck.DrawCardToHand();
        
        handDisplay.AddCardToMat(draw, true);
        handDisplay.ArrangeMat();
    }

    public void DiscardCard()
    {
        List<FateCard> selectedCards = handDisplay.GetSelectedCards();
        handSource.SetCardsSelected(selectedCards);

        playerDeck.DiscardSelectedCards();

        handDisplay.RemoveCardsFromMat();
        if(!handDisplay.AutoArrange)
            handDisplay.ArrangeMat();
    }

    public void ShowPlayerDeck()
    {
        playerDeck.transform.parent.gameObject.SetActive(true);
        playerDeck.GeneralSizeUpdate();
        gameObject.SetActive(false);
    }

    public void OnPlayerDeckClose(bool saved)
    {
        if(saved)
            FullUpdate();
        
        gameObject.SetActive(true);
    }
    
    public void CheckCardsInSelection()
    {
        int selectedCount = handDisplay.GetFocusedCardCount();
        
        playCardButton.SetActive(selectedCount == 1);
        discardCardButton.SetActive(selectedCount > 0);
    }
    
    public void PlayCard()
    {
        List<FateCard> selectedCards = handDisplay.GetSelectedCards();
        handSource.SetCardsSelected(selectedCards);
        
        Debug.Log(selectedCards.Count);
        
        if (selectedCards.Count != 1)
            return;

        FateCard toPlay = selectedCards[0];
        
        if(OnCardPlayed != null)
            OnCardPlayed.Invoke(toPlay);
    }
}
