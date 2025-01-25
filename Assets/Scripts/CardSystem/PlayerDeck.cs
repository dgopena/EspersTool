using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

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

    public bool deckDisplayed { get; private set; }

    [Header("UI")] 
    public Canvas baseCanvas;
    
    [Header("Confirm Panels")]
    [SerializeField] private GameObject shuffleConfirmPanel;
    [SerializeField] private GameObject coldExitConfirmPanel;

    [Header("Default Sizes")]
    [SerializeField] private int handSize = 5;
    [SerializeField] private bool handStartHidden = false;
    [SerializeField] private int fateSize = 15;
    [SerializeField] private bool fateStartHidden = true;
    [SerializeField] private int discardSize = 0;
    [SerializeField] private bool discardStartHidden = true;
    [SerializeField] private int aetherSize = 32;
    [SerializeField] private bool aetherStartHidden = true;

    private bool firstSetUp = true;
    public bool FirstSetUp => firstSetUp;

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

    public UnityEvent<bool> OnPanelClose;

    public struct IntDeck
    {
        public int[] handNumbers;
        public int[] handSuits;

        public int[] fateNumbers;
        public int[] fateSuits;

        public int[] discardNumbers;
        public int[] discardSuits;

        public int[] aetherNumbers;
        public int[] aetherSuits;
    }

    private void Awake()
    {
        //SetDeckUp();
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
        for (int i = 0; i < fateSize; i++)
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
        else if (discard.Count == 0)
        {
            fateMat.SetMatWidth(maxWidth - minWidthForMat);
            discardMat.SetMatWidth(minWidthForMat);
        }
        else
        {
            float divSize = maxWidth / (float)(fate.Count + discard.Count);
            float fateWidth = (float)fate.Count * divSize;
            float discardWidth = (float)discard.Count * divSize;
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

        firstSetUp = false;
    }

    public FateCard BuildNewCard(int suit, int number)
    {
        GameObject nuCard = Instantiate<GameObject>(fateCardPrefab, transform);
        FateCard auxFate = nuCard.GetComponent<FateCard>();
        auxFate.SetUpCard(number, suit, suitDefs[suit - 1].suitAction, suitDefs[suit - 1].suitIcon,
            suitDefs[suit - 1].suitColor);
        nuCard.SetActive(true);

        return auxFate;
    }

    public void BuildFromIntArrays(int[] handNumbers, int[] handSuits, int[] fateNumbers, int[] fateSuits,
        int[] discardNumbers, int[] discardSuits, int[] aetherNumbers, int[] aetherSuits)
    {
        IntDeck builtIntDeck = new IntDeck();
        builtIntDeck.handNumbers = handNumbers;
        builtIntDeck.handSuits = handSuits;
        builtIntDeck.fateNumbers = fateNumbers;
        builtIntDeck.fateSuits = fateSuits;
        builtIntDeck.discardNumbers = discardNumbers;
        builtIntDeck.discardSuits = discardSuits;
        builtIntDeck.aetherNumbers = aetherNumbers;
        builtIntDeck.aetherSuits = aetherSuits;
        
        BuildFromIntDeck(builtIntDeck);
    }
    
    public void BuildFromIntDeck(IntDeck givenDeck)
    {
        if (!firstSetUp)
        {
            handMat.ClearMat();
            fateMat.ClearMat();
            discardMat.ClearMat();
            aetherMat.ClearMat();
        }
        else
        {
            handMat.mainDeck = this;
            fateMat.mainDeck = this;
            discardMat.mainDeck = this;
            aetherMat.mainDeck = this;
        }

        List<FateCard> hand = BuildListFromIntSet(givenDeck.handNumbers, givenDeck.handSuits);
        List<FateCard> aether = BuildListFromIntSet(givenDeck.aetherNumbers, givenDeck.aetherSuits);
        List<FateCard> fate = BuildListFromIntSet(givenDeck.fateNumbers, givenDeck.fateSuits);
        List<FateCard> discard = BuildListFromIntSet(givenDeck.discardNumbers, givenDeck.discardSuits);

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
        else if (discard.Count == 0)
        {
            fateMat.SetMatWidth(maxWidth - minWidthForMat);
            discardMat.SetMatWidth(minWidthForMat);
        }
        else
        {
            float divSize = maxWidth / (float)(fate.Count + discard.Count);
            float fateWidth = (float)fate.Count * divSize;
            float discardWidth = (float)discard.Count * divSize;
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

    private List<FateCard> BuildListFromIntSet(int[] numbers, int[] suits)
    {
        List<FateCard> cardList = new List<FateCard>();

        for(int i = 0; i < numbers.Length; i++)
        {
            int s = suits[i];
            
            GameObject nuCard = Instantiate<GameObject>(fateCardPrefab, transform);
            FateCard auxFate = nuCard.GetComponent<FateCard>();
            auxFate.SetUpCard(numbers[i], s, suitDefs[s - 1].suitAction, suitDefs[s - 1].suitIcon, suitDefs[s - 1].suitColor);
            nuCard.SetActive(true);

            cardList.Add(auxFate);
        }

        return cardList;
    }

    public IntDeck BuildIntDeckFromMats()
    {
        IntDeck cardList = new IntDeck();

        List<FateCard> handCards = handMat.GetAllCards();
        int[] numbers = new int[handCards.Count];
        int[] suits = new int[handCards.Count];
        for(int i = 0; i < handCards.Count; i++)
        {
            numbers[i] = handCards[i].cardNumber;
            suits[i] = handCards[i].cardSuit;
        }
        cardList.handNumbers = numbers;
        cardList.handSuits = suits;

        List<FateCard> fateCards = fateMat.GetAllCards();
        numbers = new int[fateCards.Count];
        suits = new int[fateCards.Count];
        for (int i = 0; i < fateCards.Count; i++)
        {
            numbers[i] = fateCards[i].cardNumber;
            suits[i] = fateCards[i].cardSuit;
        }
        cardList.fateNumbers = numbers;
        cardList.fateSuits = suits;

        List<FateCard> discardCards = discardMat.GetAllCards();
        numbers = new int[discardCards.Count];
        suits = new int[discardCards.Count];
        for (int i = 0; i < discardCards.Count; i++)
        {
            numbers[i] = discardCards[i].cardNumber;
            suits[i] = discardCards[i].cardSuit;
        }
        cardList.discardNumbers = numbers;
        cardList.discardSuits = suits;

        List<FateCard> aetherCards = aetherMat.GetAllCards();
        numbers = new int[aetherCards.Count];
        suits = new int[aetherCards.Count];
        for (int i = 0; i < aetherCards.Count; i++)
        {
            numbers[i] = aetherCards[i].cardNumber;
            suits[i] = aetherCards[i].cardSuit;
        }
        cardList.aetherNumbers = numbers;
        cardList.aetherSuits = suits;

        return cardList;
    }

    public void ReShuffleCall()
    {
        shuffleConfirmPanel.SetActive(true);
    }

    public void ConfirmReshuffle()
    {
        handMat.ClearMat();
        aetherMat.ClearMat();
        discardMat.ClearMat();
        aetherMat.ClearMat();

        SetDeckUp();

        shuffleConfirmPanel.SetActive(false);
    }

    public void ColdExitCall()
    {
        coldExitConfirmPanel.SetActive(true);
    }

    public void ConfirmColdExit()
    {
        if(OnPanelClose != null)
            OnPanelClose.Invoke(false);
        
        coldExitConfirmPanel.SetActive(false);
        gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void SaveAndExit()
    {
        //return decks
        IntDeck currentCards = BuildIntDeckFromMats();

        if(OnPanelClose != null)
            OnPanelClose.Invoke(true);
        
        coldExitConfirmPanel.SetActive(false);
        gameObject.transform.parent.gameObject.SetActive(false);
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
        discardAetherMidBorder.position = new Vector2(0.5f * (discardPoints.y + discardPoints.x), discardPoints.z);

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

    public void GeneralSizeUpdate()
    {
        UpdateFateDiscardSize();

        handMat.ArrangeMat();
        fateMat.ArrangeMat();
        discardMat.ArrangeMat();
        aetherMat.ArrangeMat();
        
        UpdateMidBorders();

        float minScale = GetMinScale();
        handMat.ApplyHeightToCards(minScale);
        fateMat.ApplyHeightToCards(minScale);
        discardMat.ApplyHeightToCards(minScale);
        aetherMat.ApplyHeightToCards(minScale);
    }
    
    public void ShowDeck(bool show)
    {
        deckDisplayed = show;
    }
    
    public FateCard DrawCardToHand()
    {
        Tuple<int,int> coodSet = fateMat.SelectRandomCard();
        List<FateCard> cardsFromFate = fateMat.RemoveCardsFromMat();
        handMat.AddCardsToMat(cardsFromFate);

        if (deckDisplayed)
        {
            handMat.ArrangeMat();
            fateMat.ArrangeMat();
        }

        return cardsFromFate[0];
    }

    public CardMat GetHandMat()
    {
        return handMat;
    }

    public void DiscardSelectedCards()
    {
        List<FateCard> cardsFromHand = handMat.RemoveCardsFromMat();
        discardMat.AddCardsToMat(cardsFromHand);
        
        if (deckDisplayed)
        {
            handMat.ArrangeMat();
            discardMat.ArrangeMat();
        }
    }
}
