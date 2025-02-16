using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

//grabs and displays the cards in an orderly fashion
public class CardMat : MonoBehaviour
{
    [HideInInspector] public PlayerDeck mainDeck;

    [SerializeField] private int cardsPerRow = 10;
    [SerializeField] private int maxNumberOfRows = 1; //this takes precedence. if the cards per row is exceeded but the row number is already at its limit, will try to keep adding cards to the row
    [SerializeField] private bool autoArrange = true;
    public bool AutoArrange => autoArrange;

    [Space(10f)]
    [SerializeField] private RectTransform matRT;
    [SerializeField] private RectTransform matGraphic;
    [SerializeField] private float minCardSpacing;
    [SerializeField] private float maxCardSpacing;

    [Space(10f)]
    [SerializeField] private float minWidthForMat = 100f;
    [SerializeField] private float rowHeightFactorForCards = 0.9f;
    [HideInInspector] public float cardScaledHeight;
    
    [Space(10f)]
    [SerializeField] private float heightForCardAreaMin = 0.1f;
    [SerializeField] private float heightForCardAreaMax = 0.75f;

    public UnityEvent OnCardSelectUpdate;

    [HideInInspector] public int selectedCardNumbers;

    [Space(10f)]
    [SerializeField] private Image hideButtonIcon;
    [SerializeField] private float hiddenIconAlpha = 0.35f;

    public bool IsMatHidden { get; private set; }
    
    private struct CardRow
    {
        public List<FateCard> cards;
    }

    private List<CardRow> currentRows;

    private List<FateCard> toArrange;
    public bool awaitingArrangeFlag { get; private set; }

    public void AddCardToMat(FateCard card, bool asNewCard = false)
    {
        AddCardsToMat(new List<FateCard>() {card}, asNewCard);
    }
    
    public void AddCardsToMat(List<FateCard> cards, bool asNewCards = false)
    {
        if (currentRows == null)
            currentRows = new List<CardRow>();

        if (asNewCards)
        {
            List<FateCard> newCards = new List<FateCard>();
            for (int i = 0; i < cards.Count; i++)
            {
                FateCard nuCard = mainDeck.BuildNewCard(cards[i].cardSuit, cards[i].cardNumber);
                newCards.Add(nuCard);
            }
            
            cards = newCards;
        }
        
        List<FateCard> aux = new List<FateCard>();
        for(int r = 0; r < currentRows.Count; r++)
        {
            aux.AddRange(currentRows[r].cards);
        }

        for(int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.parent = matRT;
            cards[i].SetParentMat(this);
            cards[i].transform.SetAsFirstSibling();
            cards[i].HideCard(IsMatHidden);
        }
        aux.AddRange(cards);

        //set label below
        matRT.GetChild(matRT.childCount - 1).SetAsFirstSibling();

        toArrange = aux;
        awaitingArrangeFlag = true;
        if(autoArrange)
            ArrangeMat();
    }

    public List<FateCard> RemoveCardsFromMat()
    {
        if (currentRows == null)
            currentRows = new List<CardRow>();

        List<FateCard> ToReturn = new List<FateCard>();
        List<FateCard> ToKeep = new List<FateCard>();

        for(int r = 0; r < currentRows.Count; r++)
        {
            for(int c = 0; c < currentRows[r].cards.Count; c++)
            {
                FateCard checking = currentRows[r].cards[c];
                if (checking.IsSelected)
                {
                    checking.transform.parent = null;
                    checking.SetParentMat(null);
                    checking.SelectCard(false); //de select them
                    checking.FocusCard(false);
                    ToReturn.Add(checking);
                }
                else
                    ToKeep.Add(checking);
            }
        }

        toArrange = ToKeep;
        awaitingArrangeFlag = true;
        if (autoArrange)
            ArrangeMat();

        return ToReturn;
    }

    public void ToggleHideCards()
    {
        SetCardsHidden(!IsMatHidden);
    }

    public void SetCardsHidden(bool hidden)
    {
        IsMatHidden = hidden;

        if (hideButtonIcon != null)
        {
            Color iconColor = hideButtonIcon.color; // tintColor;
            iconColor.a = IsMatHidden ? hiddenIconAlpha : 1f;
            hideButtonIcon.color = iconColor;
        }

        if (currentRows == null)
            return;

        for(int i = 0; i < currentRows.Count; i++)
        {
            for(int c = 0; c < currentRows[i].cards.Count; c++)
            {
                currentRows[i].cards[c].HideCard(hidden);
            }
        }
    }

    public void ShuffleCards()
    {
        List<FateCard> aux = new List<FateCard>();
        for (int r = 0; r < currentRows.Count; r++)
        {
            aux.AddRange(currentRows[r].cards);
        }

        FateCard[] shuffDeck = aux.ToArray();

        System.Random rng = new System.Random();
        rng.Shuffle<FateCard>(shuffDeck);
        aux = new List<FateCard>(shuffDeck);

        currentRows.Clear();
        AddCardsToMat(aux);

        if (mainDeck.ForceMinScale)
        {
            float minHeight = mainDeck.GetMinScale();
            ApplyHeightToCards(minHeight);
        }
    }

    public int GetCardCount()
    {
        int cardTotal = 0;
        for (int i = 0; i < currentRows.Count; i++)
        {
            cardTotal += currentRows[i].cards.Count;
        }

        return cardTotal;
    }

    public int GetFocusedCardCount()
    {
        int count = 0;
        for(int r = 0; r < currentRows.Count; r++)
        {
            for(int c = 0; c < currentRows[r].cards.Count; c++)
            {
                FateCard checking = currentRows[r].cards[c];
                if (checking.IsSelected)
                {
                    count++;
                }
            }
        }

        return count;
    }
    
    public Vector4 GetAnchorPoints()
    {
        Vector4 anchorValues = new Vector4();

        /*
        anchorValues.x = matRT.rect.xMin;
        anchorValues.y = matRT.rect.xMax;
        anchorValues.z = matRT.rect.yMin;
        anchorValues.w = matRT.rect.yMax;
        */

        float canvasScale = 1f;
        if (mainDeck.baseCanvas)
            canvasScale = mainDeck.baseCanvas.scaleFactor;
        
        anchorValues.x = matRT.position.x;
        anchorValues.x -= canvasScale * matRT.pivot.x * matRT.rect.width;

        anchorValues.y = matRT.position.x;
        anchorValues.y += canvasScale * (1f - matRT.pivot.x) * matRT.rect.width;

        anchorValues.z = matRT.position.y;
        anchorValues.z -= canvasScale * matRT.pivot.y * matRT.rect.height;

        anchorValues.w = matRT.position.y;
        anchorValues.w += canvasScale * (1f - matRT.pivot.y) * matRT.rect.height;
        
        return anchorValues;
    }

    public List<FateCard> GetSelectedCards()
    {
        List<FateCard> selectedCards = new List<FateCard>();
        
        for(int r = 0; r < currentRows.Count; r++)
        {
            for(int c = 0; c < currentRows[r].cards.Count; c++)
            {
                FateCard checking = currentRows[r].cards[c];
                if (checking.IsSelected)
                {
                    selectedCards.Add(checking);
                }
            }
        }

        return selectedCards;
    }

    public void SetCardsSelected(List<FateCard> selectedCards)
    {
        for (int r = 0; r < currentRows.Count; r++)
        {
            for (int c = 0; c < currentRows[r].cards.Count; c++)
            {
                FateCard checking = currentRows[r].cards[c];
                checking.SelectCard(false, false);
                
                for (int i = 0; i < selectedCards.Count; i++)
                {
                    bool setSelected = checking.Equals(selectedCards[i]);
                    if(setSelected)
                        checking.SelectCard(true);
                }
            }
        }
    }
    
    public void ResetSelectedCardCounter()
    {
        selectedCardNumbers = 0;
    }

    public void ClearMat()
    {
        if (currentRows == null)
            return;
        
        for (int r = 0; r < currentRows.Count; r++)
        {
            for (int c = 0; c < currentRows[r].cards.Count; c++)
            {
                Destroy(currentRows[r].cards[c].gameObject);
            }
        }

        currentRows = new List<CardRow>();
    }

    public List<FateCard> GetAllCards()
    {
        List<FateCard> allCards = new List<FateCard>();

        for (int r = 0; r < currentRows.Count; r++)
        {
            for (int c = 0; c < currentRows[r].cards.Count; c++)
            {
                allCards.Add(currentRows[r].cards[c]);
            }
        }

        return allCards;
    }

    public void ReplaceAllCards(List<FateCard> newCards)
    {
        ClearMat();
        
        AddCardsToMat(newCards, true);
        if(!autoArrange)
            ArrangeMat();
    }
    
    public Tuple<int,int> SelectRandomCard()
    {
        int cardNum = GetCardCount();
        int cardSelection = UnityEngine.Random.Range(0, cardNum);
        
        int count = 0;
        for(int r = 0; r < currentRows.Count; r++)
        {
            for(int c = 0; c < currentRows[r].cards.Count; c++)
            {
                if (count == cardSelection)
                {
                    currentRows[r].cards[c].SelectCard(true);
                    return new Tuple<int, int>(r,c);
                }

                count++;
            }
        }

        return new Tuple<int, int>(0, 0);
    }

    public void SelectCard(int rowIndex, int cardIndex)
    {
        currentRows[rowIndex].cards[cardIndex].SelectCard(true);
    }
    
    public void DeselectCard(int rowIndex, int cardIndex)
    {
        currentRows[rowIndex].cards[cardIndex].SelectCard(false);
    }
    
    #region Mat Look
    //orders the cards for proper display
    public void ArrangeMat()
    {
        if (toArrange.Count == 0)
            return;

        Rect matRect = matRT.rect;
        float matWidth = matRect.width;
        float matHeight = (heightForCardAreaMax * matRect.height) - (heightForCardAreaMin * matRect.height);

        int align = 1; //0 - left, 1 - middle, 2 - right
        if (matRT.pivot.x == 0)
            align = 0;
        else if (matRT.pivot.x == 1)
            align = 2;

        int neededRows = Mathf.CeilToInt((float)toArrange.Count / (float)cardsPerRow);
        if (neededRows > maxNumberOfRows)
            neededRows = maxNumberOfRows;

        int rowCardNumber = Mathf.CeilToInt((float)toArrange.Count / (float)neededRows);

        if (currentRows != null)
            currentRows.Clear();

        int cardCount = 0;
        CardRow nuRow = new CardRow();
        nuRow.cards = new List<FateCard>();
        while(cardCount < toArrange.Count)
        {
            nuRow.cards.Add(toArrange[cardCount]);
            cardCount++;
            if(cardCount == rowCardNumber)
            {
                currentRows.Add(nuRow);
                nuRow = new CardRow();
                nuRow.cards = new List<FateCard>();
            }
        }

        if (nuRow.cards.Count > 0)
            currentRows.Add(nuRow);

        //we dispose of the cards depending on the row
        //for uniformity, we check a common spacing for all rows
        float tiniestSpace = float.MaxValue;
        for(int i = 0; i < currentRows.Count; i++)
        {
            float spacing = matWidth / (float)currentRows[i].cards.Count;
            if (spacing > maxCardSpacing)
                spacing = maxCardSpacing;
            else if (spacing < minCardSpacing)
                spacing = minCardSpacing;

            if (spacing < tiniestSpace)
                tiniestSpace = spacing;
        }

        float rowHeight = matHeight / (float)currentRows.Count;
        float startHeight = (0.5f * rowHeight) + (heightForCardAreaMin * matRect.height);

        cardScaledHeight = rowHeightFactorForCards * rowHeight;

        //with this spacing, we work
        for(int i = 0; i < currentRows.Count; i++)
        {
            float rowWidth = tiniestSpace * currentRows[i].cards.Count;

            float startWidth = (-0.5f * rowWidth);

            for (int c = 0; c < currentRows[i].cards.Count; c++)
            {
                currentRows[i].cards[c].name = "c_" + c + "_" + i;

                float xPos = startWidth + (c * tiniestSpace) + (0.5f * tiniestSpace);
                if (align == 0)
                    xPos += 0.5f * matRect.xMax;
                else if (align == 2)
                    xPos += 0.5f * matRect.xMin;

                currentRows[i].cards[c].cardRT.localPosition = new Vector3(xPos, startHeight + (i * rowHeight), 0f);
                currentRows[i].cards[c].cardRT.SetAsFirstSibling();

                currentRows[i].cards[c].ChangeScaleByFactor(cardScaledHeight);
            }
        }

        awaitingArrangeFlag = false;

        //Debug.Log(transform.name + ": " + GetCardCount());
    }

    public void ApplyHeightToCards(float forcedHeight)
    {
        for (int i = 0; i < currentRows.Count; i++)
        {
            for (int c = 0; c < currentRows[i].cards.Count; c++)
            {
                currentRows[i].cards[c].ChangeScaleByFactor(forcedHeight);
            }
        }
    }

    public float GetMatWidth()
    {
        return matRT.rect.width;
    }

    public void SetMatWidth(float width)
    {
        matRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        matGraphic.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
    #endregion
}
