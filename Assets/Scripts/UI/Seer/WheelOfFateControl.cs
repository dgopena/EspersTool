using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class WheelOfFateControl : MonoBehaviour
{
    public SeerDeck wheelDeck;
    public RectTransform discardPile;
    public TextMeshProUGUI discardButtonLabel;
    private int undoIndex = -1;

    [Space(20f)]
    public RectTransform[] handCardsPosition;
    private SeerCard[] handCards;
    private int handCardCount = 0;

    private int selectedIndex = -1;

    [Space(20f)]
    public float cardMoveTime = 1f;
    public float cardHandScaleUp = 1.3f; //base scale will always be 1

    private bool cardMovingFlag = false;
    private bool cardDiscarding = false;
    private RectTransform movableCard;
    private Vector3 startCardPos;
    private Vector3 finalCardPos;
    private float startMoveTime;
    private float endMoveTime;

    [Space(20f)]
    public TextMeshProUGUI chosenCardTitleLabel;
    public TextMeshProUGUI chosenCardDescriptionLabel;


    void LateUpdate()
    {
        if (cardMovingFlag)
        {
            float frac = (Time.time - startMoveTime) / cardMoveTime;
            movableCard.position = Vector3.Lerp(startCardPos, finalCardPos, frac);
            if (!cardDiscarding)
                movableCard.localScale = Mathf.Lerp(1f, cardHandScaleUp, frac) * Vector3.one;
            else
                movableCard.localScale = Mathf.Lerp(cardHandScaleUp, 1f, frac) * Vector3.one;

            if(frac >= 1f)
            {
                cardMovingFlag = false;
            }
        }

        if (handCards == null)
            return;

        if (Input.GetMouseButtonDown(0) && !cardMovingFlag)
        {
            bool positionMatch = false;
            for(int i = 0; i < handCardsPosition.Length; i++)
            {
                if (TooltipManager.CheckMouseInArea(handCardsPosition[i]))
                {
                    if(selectedIndex != i)
                    {
                        if (selectedIndex >= 0)
                        {
                            handCardsPosition[selectedIndex].GetChild(1).gameObject.SetActive(false);
                            selectedIndex = -1;
                        }

                        if (handCards[i] != null)
                        {
                            selectedIndex = i;
                            handCardsPosition[selectedIndex].GetChild(1).gameObject.SetActive(true);
                        }
                    }
                    else if(selectedIndex == i)
                    {
                        handCardsPosition[selectedIndex].GetChild(1).gameObject.SetActive(false);
                        selectedIndex = -1;
                    }

                    positionMatch = true;
                    break;
                }
            }

            /*
            if (!positionMatch && selectedIndex >= 0)
            {
                handCardsPosition[selectedIndex].GetChild(1).gameObject.SetActive(false);
                selectedIndex = -1;
            }
            */

            if(selectedIndex >= 0 && handCards[selectedIndex] != null)
            {
                chosenCardTitleLabel.text = handCards[selectedIndex].cardTitle;
                chosenCardDescriptionLabel.text = handCards[selectedIndex].cardDescription;
                discardButtonLabel.text = "Discard";
            }
            else
            {
                chosenCardTitleLabel.text = "";
                chosenCardDescriptionLabel.text = "";
                discardButtonLabel.text = "-";

                if (undoIndex >= 0)
                    discardButtonLabel.text = "Undo Last Discard";
            }
        }
    }

    public void PullCard()
    {
        if (cardMovingFlag)
            return;

        if (handCards != null && handCardCount == handCardsPosition.Length)
            return;

        SeerCard newPull = wheelDeck.GetTopCard();

        if (newPull == null)
            return;

        if(handCards == null)
            handCards = new SeerCard[handCardsPosition.Length];

        int targetSlot = 0;
        
        for(int i = 0; i < handCards.Length; i++)
        {
            if (handCards[i] == null)
            {
                targetSlot = i;
                break;
            }
        }

        finalCardPos = handCardsPosition[targetSlot].position;
        movableCard = newPull.GetComponent<RectTransform>();
        startCardPos = movableCard.position;
        startMoveTime = Time.time;

        handCards[targetSlot] = newPull;

        cardMovingFlag = true;
        cardDiscarding = false;
        handCardCount++;

        undoIndex = -1;
        discardButtonLabel.text = "-";

        if(selectedIndex >= 0)
            discardButtonLabel.text = "Discard";
    }

    public void DiscardCard()
    {
        if (cardMovingFlag)
            return;

        //if none selected, is "undo discard"
        if(selectedIndex < 0 && undoIndex >= 0)
        {
            //undo discard
            handCards[undoIndex] = movableCard.GetComponent<SeerCard>();
            wheelDeck.FlipCard(handCards[undoIndex]);

            finalCardPos = handCardsPosition[undoIndex].position;
            startCardPos = discardPile.position;
            startMoveTime = Time.time;

            cardMovingFlag = true;
            cardDiscarding = false;

            handCardCount++;
            undoIndex = -1;
            discardButtonLabel.text = "-";
        }
        else if(selectedIndex >= 0)
        {
            //discard selected card
            undoIndex = selectedIndex;
            handCardsPosition[selectedIndex].GetChild(1).gameObject.SetActive(false);

            finalCardPos = discardPile.position;
            movableCard = handCards[selectedIndex].GetComponent<RectTransform>();
            startCardPos = movableCard.position;
            startMoveTime = Time.time;

            wheelDeck.FlipCard(handCards[selectedIndex]);
            handCards[selectedIndex] = null;

            cardMovingFlag = true;
            cardDiscarding = true;
            handCardCount--;

            selectedIndex = -1;

            chosenCardTitleLabel.text = "";
            chosenCardDescriptionLabel.text = "";
            discardButtonLabel.text = "Undo Last Discard";
        }
    }

    public void ShuffleDeck()
    {
        if (cardMovingFlag)
            return;

        handCards = new SeerCard[handCardsPosition.Length];
        handCardCount = 0;
        chosenCardTitleLabel.text = "";
        chosenCardDescriptionLabel.text = "";

        if(selectedIndex >= 0)
            handCardsPosition[selectedIndex].GetChild(1).gameObject.SetActive(false);

        selectedIndex = -1;
        undoIndex = -1;
        wheelDeck.ShuffleCards();
    }
}
