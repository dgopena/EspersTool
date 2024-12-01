using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    [System.Serializable]
    public struct FateSuitDef
    {
        public int suitID;
        public Sprite suitIcon;
        public Color suitColor;
        public string suitAction;
    }

    public FateSuitDef[] suitDefs;

    [Space(10f)]
    public GameObject fateCardPrefab;

    [Header("Default Sizes")]
    [SerializeField] private int handSize = 5;
    [SerializeField] private bool handStartHidden = false;
    [SerializeField] private int fateSize = 15;
    [SerializeField] private bool fateStartHidden = true;
    [SerializeField] private int discardSize = 0;
    [SerializeField] private bool discardStartHidden = true;
    [SerializeField] private int aetherSize = 32;
    [SerializeField] private bool aetherStartHidden = true;

    [Header("Mats")]
    [SerializeField] private CardMat handMat;

    [Space(10f)] // keep both of these their widths proportional to each other. THEN call the arrange function of each mat
    [SerializeField] private CardMat fateMat;
    [SerializeField] private CardMat discardMat;
    [SerializeField] private float minWidthForMat = 100f;
    [SerializeField] private bool forceMinScale = true;
    public bool ForceMinScale => forceMinScale;

    [Space(10f)]
    [SerializeField] private CardMat aetherMat;

    [Header("BorderPoints")]
    [SerializeField] private RectTransform handFateMidBorder;
    [SerializeField] private RectTransform fateAetherMidBorder;
    [SerializeField] private RectTransform handDiscardMidBorder;
    [SerializeField] private RectTransform discardAetherMidBorder;
    [SerializeField] private RectTransform handAetherMidBorder;
    [SerializeField] private RectTransform fateDiscardMidBorder;

    private void Awake()
    {
        SetDeckUp();
    }

    public void SetDeckUp()
    {
        handMat.mainDeck = this;
        fateMat.mainDeck = this;
        discardMat.mainDeck = this;
        aetherMat.mainDeck = this;

        List<FateCard> allDeck = new List<FateCard>();

        for (int s = 0; s < 4; s++)
        {
            for (int i = 0; i < 13; i++)
            {
                GameObject nuCard = Instantiate<GameObject>(fateCardPrefab, transform);
                FateCard auxFate = nuCard.GetComponent<FateCard>();
                auxFate.SetUpCard(i + 1, s + 1, suitDefs[s].suitAction, suitDefs[s].suitIcon, suitDefs[s].suitColor);
                nuCard.SetActive(true);

                allDeck.Add(auxFate);
            }
        }

        FateCard[] shuffDeck = allDeck.ToArray();

        System.Random rng = new System.Random();
        rng.Shuffle<FateCard>(shuffDeck);

        int cardIndex = 0;

        //set hand
        List<FateCard> hand = new List<FateCard>();
        for (int i = 0; i < handSize; i++)
        {
            hand.Add(shuffDeck[cardIndex]);
            cardIndex++;
        }

        //set fate
        List<FateCard> fate = new List<FateCard>();
        for(int i = 0; i < fateSize; i++)
        {
            fate.Add(shuffDeck[cardIndex]);
            cardIndex++;
        }

        //set discard
        List<FateCard> discard = new List<FateCard>();
        for (int i = 0; i < discardSize; i++)
        {
            discard.Add(shuffDeck[cardIndex]);
            cardIndex++;
        }

        //set aether
        List<FateCard> aether = new List<FateCard>();
        for (int i = 0; i < aetherSize; i++)
        {
            aether.Add(shuffDeck[cardIndex]);
            cardIndex++;
        }

        handMat.SetCardsHidden(handStartHidden);
        handMat.AddCardsToMat(hand);

        aetherMat.SetCardsHidden(aetherStartHidden);
        aetherMat.AddCardsToMat(aether);

        //we first adjust the needed size for the mats
        float maxWidth = handMat.GetMatWidth();
        if (fate.Count == 0)
        {
            fateMat.SetMatWidth(minWidthForMat);
            discardMat.SetMatWidth(maxWidth - minWidthForMat);
        }
        else if(discard.Count == 0)
        {
            fateMat.SetMatWidth(maxWidth - minWidthForMat);
            discardMat.SetMatWidth(minWidthForMat);
        }
        else
        {
            float divSize = maxWidth / (float)(fate.Count + discard.Count);
            float fateWidth = (float)fate.Count * divSize;
            float discardWidth = (float)discard.Count * divSize;
            if(fateWidth < minWidthForMat)
            {
                fateWidth = minWidthForMat;
                discardWidth = maxWidth - minWidthForMat;
            }
            else if(discardWidth < minWidthForMat)
            {
                fateWidth = maxWidth - minWidthForMat;
                discardWidth = minWidthForMat;
            }
            fateMat.SetMatWidth(fateWidth);
            discardMat.SetMatWidth(discardWidth);
        }

        fateMat.SetCardsHidden(fateStartHidden);
        fateMat.AddCardsToMat(fate);

        discardMat.SetCardsHidden(discardStartHidden);
        discardMat.AddCardsToMat(discard);

        if (forceMinScale)
        {
            float minHeight = GetMinScale();

            handMat.ApplyHeightToCards(minHeight);
            fateMat.ApplyHeightToCards(minHeight);
            discardMat.ApplyHeightToCards(minHeight);
            aetherMat.ApplyHeightToCards(minHeight);
        }
    }

    public void UpdateFateDiscardSize()
    {
        float maxWidth = handMat.GetMatWidth();

        int fateCount = fateMat.GetCardCount();
        int discardCount = discardMat.GetCardCount();

        float divSize = maxWidth / ((float)fateCount + (float)discardCount);
        float fateWidth = (float)fateCount * divSize;
        float discardWidth = (float)discardCount * divSize;
        if (fateWidth < minWidthForMat)
        {
            fateWidth = minWidthForMat;
            discardWidth = maxWidth - minWidthForMat;
        }
        else if (discardWidth < minWidthForMat)
        {
            fateWidth = maxWidth - minWidthForMat;
            discardWidth = minWidthForMat;
        }
        fateMat.SetMatWidth(fateWidth);
        discardMat.SetMatWidth(discardWidth);
    }

    public void UpdateMidBorders()
    {
        //hand fate
        Vector4 fatePoints = fateMat.GetAnchorPoints();
        Vector4 discardPoints = discardMat.GetAnchorPoints();

        handFateMidBorder.position = new Vector2(0.5f * (fatePoints.y + fatePoints.x), fatePoints.w);
        fateAetherMidBorder.position = new Vector2(0.5f * (fatePoints.y + fatePoints.x), fatePoints.z);
        handDiscardMidBorder.position = new Vector2(0.5f * (discardPoints.y + discardPoints.x), discardPoints.w);
        handAetherMidBorder.position = new Vector2(0.5f * (discardPoints.y + discardPoints.x), discardPoints.z);

        fateDiscardMidBorder.position = new Vector2(fatePoints.y, (0.4f * (fatePoints.w - fatePoints.z)) + fatePoints.z);
        handAetherMidBorder.position = new Vector2(fatePoints.y, (0.6f * (fatePoints.w - fatePoints.z)) + fatePoints.z);
    }

    public float GetMinScale()
    {
        float minHeight = float.MaxValue;
        if (handMat.cardScaledHeight < minHeight && handMat.cardScaledHeight > 0f)
            minHeight = handMat.cardScaledHeight;
        if (fateMat.cardScaledHeight < minHeight && fateMat.cardScaledHeight > 0f)
            minHeight = fateMat.cardScaledHeight;
        if (discardMat.cardScaledHeight < minHeight && discardMat.cardScaledHeight > 0f)
            minHeight = discardMat.cardScaledHeight;
        if (aetherMat.cardScaledHeight < minHeight && aetherMat.cardScaledHeight > 0f)
            minHeight = aetherMat.cardScaledHeight;

        return minHeight;
    }
}
