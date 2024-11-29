using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

//grabs and displays the cards in an orderly fashion
public class CardMat : MonoBehaviour
{
    [SerializeField] private int cardsPerRow = 10;
    [SerializeField] private int maxNumberOfRows = 1; //this takes precedence. if the cards per row is exceeded but the row number is already at its limit, will try to keep adding cards to the row
    [SerializeField] private bool autoArrange = true;
    public bool AutoArrange => autoArrange;

    [Space(10f)]
    [SerializeField] private RectTransform matRT;
    [SerializeField] private float minCardSpacing;
    [SerializeField] private float maxCardSpacing;

    [Space(10f)]
    [SerializeField] private float minWidthForMat = 100f;
    [SerializeField] private float rowHeightFactorForCards = 0.9f;
    public float cardScaledHeight;
    
    [Space(10f)]
    [SerializeField] private float heightForCardAreaMin = 0.1f;
    [SerializeField] private float heightForCardAreaMax = 0.75f;
    
    private struct CardRow
    {
        public List<FateCard> cards;
    }

    private List<CardRow> currentRows;

    private List<FateCard> toArrange;
    public bool awaitingArrangeFlag { get; private set; }

    public void AddCardsToMat(List<FateCard> cards)
    {
        if (currentRows == null)
            currentRows = new List<CardRow>();

        List<FateCard> aux = new List<FateCard>();
        for(int r = 0; r < currentRows.Count; r++)
        {
            aux.AddRange(currentRows[r].cards);
        }

        for(int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.parent = matRT;
            cards[i].transform.SetAsFirstSibling();
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

    //orders the cards for proper display
    public void ArrangeMat()
    {
        if (toArrange.Count == 0)
            return;

        Debug.Log("arranging");

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

                currentRows[i].cards[c].ChangeScaleByFactor(cardScaledHeight);
            }
        }

        awaitingArrangeFlag = false;
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
    }
}
