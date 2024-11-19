using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeerDeck : MonoBehaviour
{
    private List<SeerCard> cards;

    [SerializeField] private Vector2 cardDimensions = new Vector2(3f, 2f);
    private RectTransform deckRect;

    [Space(20f)]
    [SerializeField] private float flipSpeed = 20f;
    [SerializeField] private FlipStyle flipStyle = FlipStyle.Horizontal;

    [Space(20f)]
    [SerializeField] private float cardSeparation = 0.1f;

    private List<SeerCard> flippedDeck;
    private List<SeerCard> unflippedDeck;

    private enum FlipStyle
    {
        Horizontal,
        Vertical
    }

    private void Awake()
    {
        cards = new List<SeerCard>();
        for(int i = 0; i < transform.childCount; i++)
        {
            SeerCard trgCard = transform.GetChild(i).GetComponent<SeerCard>();
            if (trgCard  != null)
            {
                cards.Add(trgCard);
            }
        }

        deckRect = GetComponent<RectTransform>();
        float HWratio = cardDimensions.x / cardDimensions.y;

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetCardDimensions(deckRect.rect.height, deckRect.rect.width, HWratio);
        }

        flippedDeck = new List<SeerCard>();
        unflippedDeck = new List<SeerCard>();

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].startBackFacing)
            {
                cards[i].SetCardFlipped(false, flipStyle == FlipStyle.Horizontal);
                unflippedDeck.Add(cards[i]);
            }
            else
            {
                cards[i].SetCardFlipped(true, flipStyle == FlipStyle.Horizontal);
                flippedDeck.Add(cards[i]);
            }
        }

        ShuffleCards();
    }

    public void ShuffleCards()
    {
        //shuffle

        int[] indexArray = new int[cards.Count];
        for (int i = 0; i < indexArray.Length; i++)
            indexArray[i] = i;

        var rng = new System.Random();
        rng.Shuffle(indexArray);

        flippedDeck.Clear();
        unflippedDeck.Clear();

        for (int i = 0; i < cards.Count; i++)
        {
            cards[indexArray[i]].SetCardFlipped(false, flipStyle == FlipStyle.Horizontal);
            unflippedDeck.Add(cards[indexArray[i]]);
            cards[indexArray[i]].transform.SetAsLastSibling();
        }

        unflippedDeck.Reverse();

        ArrangeCardPosition();
    }

    private void ArrangeCardPosition(bool onlyUnflipped = false)
    {
        int cardCounter = 0;

        if (!onlyUnflipped)
        {
            for (int i = 0; i < flippedDeck.Count; i++)
            {
                flippedDeck[i].transform.localPosition = new Vector3(0f, -1f * cardCounter * (Screen.height * cardSeparation), 0f);
                cardCounter++;
            }
        }

        for (int i = 0; i < unflippedDeck.Count; i++)
        {
            unflippedDeck[i].transform.localPosition = new Vector3(0f, -1f * cardCounter * (Screen.height * cardSeparation), 0f);
            cardCounter++;
        }
    }

    public void FlipCard(SeerCard targetCard)
    {
        targetCard.FlipCard(flipSpeed, flipStyle == FlipStyle.Horizontal);
    }

    public void FlipTopCard()
    {
        if (unflippedDeck.Count == 0)
            return;

        List<SeerCard> auxFlipped = new List<SeerCard>();
        auxFlipped.Add(unflippedDeck[0]);
        for (int i = 0; i < flippedDeck.Count; i++)
        {
            auxFlipped.Add(flippedDeck[i]);
        }

        flippedDeck = new List<SeerCard>(auxFlipped);
        auxFlipped.Clear();

        for (int i = 1; i < unflippedDeck.Count; i++)
        {
            auxFlipped.Add(unflippedDeck[i]);
        }

        unflippedDeck = new List<SeerCard>(auxFlipped);

        for (int i = 0; i < flippedDeck.Count; i++)
            flippedDeck[i].transform.SetAsFirstSibling();
        for (int i = 0; i < unflippedDeck.Count; i++)
            unflippedDeck[i].transform.SetAsFirstSibling();

        flippedDeck[0].FlipCard(flipSpeed, flipStyle == FlipStyle.Horizontal);

        ArrangeCardPosition();
    }

    public SeerCard GetTopCard()
    {
        if(cards==null || unflippedDeck.Count == 0) 
            return null;

        List<SeerCard> auxFlipped = new List<SeerCard>();
        auxFlipped.Add(unflippedDeck[0]);
        for (int i = 0; i < flippedDeck.Count; i++)
        {
            auxFlipped.Add(flippedDeck[i]);
        }

        flippedDeck = new List<SeerCard>(auxFlipped);
        auxFlipped.Clear();

        for (int i = 1; i < unflippedDeck.Count; i++)
        {
            auxFlipped.Add(unflippedDeck[i]);
        }

        unflippedDeck = new List<SeerCard>(auxFlipped);

        for (int i = 0; i < flippedDeck.Count; i++)
            flippedDeck[i].transform.SetAsFirstSibling();
        for (int i = 0; i < unflippedDeck.Count; i++)
            unflippedDeck[i].transform.SetAsFirstSibling();

        flippedDeck[0].FlipCard(flipSpeed, flipStyle == FlipStyle.Horizontal);

        ArrangeCardPosition(true);

        return flippedDeck[0];
    }
}
