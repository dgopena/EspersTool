using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FateHandWidget : MonoBehaviour
{
    public PlayerDeck playerDeck;

    public CardMat handDisplay;

    public CardMat handSource;

    private void OnEnable()
    {
        playerDeck.SetDeckUp();
        handDisplay.mainDeck = playerDeck;
        FullUpdate();
    }

    public void FullUpdate()
    {
        handDisplay.ReplaceAllCards(handSource.GetAllCards());
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
        
    }
}
